using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

public class WebRTCStreamer : MonoBehaviour
{
    public Camera cam; // Asignar StreamCamera. En realidas ya NO LA NECESITO, pero igualmente la desactivo en Start().
    //public int width = 960;
    //public int height = 540;

    public RenderTexture rt;
    private RenderTexture screenRT;

    private Dictionary<int, RTCPeerConnection> pcs = new Dictionary<int, RTCPeerConnection>();
    private Dictionary<int, VideoStreamTrack> tracks = new Dictionary<int, VideoStreamTrack>();

    private ClientWebSocket ws;
    private CancellationTokenSource cts;

    public DronControl dron;

    private static int shootTriggerFrame = -1;
    // Recuerda el último estado de disparo avisado a los controladores
    private bool lastShootState = false;

    public static bool ShootTriggered => Time.frameCount == shootTriggerFrame;

    void Start()
    {
        Application.targetFrameRate = 30;
        Application.runInBackground = true;

        //WebRTC.Initialize();

        // La RenderTexture ya viene asignada a la cámara desde el Inspector
        // Solo verificamos
        Debug.Log("RT asignada: " + (rt != null));
        Debug.Log("Cam target == rt: " + (cam.targetTexture == rt));

        cam.enabled = false;   // la Stream Camera ya no pinta rt, lo rellena la captura de pantalla

        StartCoroutine(WebSocketConnect());
        StartCoroutine(WebRTCUpdateLoop());
        StartCoroutine(CaptureScreenLoop());
        StartCoroutine(MonitorShootState());
    }

    IEnumerator CaptureScreenLoop()
    {
        var wait = new WaitForEndOfFrame();
        while (true)
        {
            yield return wait;
            if (rt == null) continue;

            // screenRT al tamaño REAL de la pantalla (si es más pequeña, recorta y da el "zoom")
            if (screenRT == null || screenRT.width != Screen.width || screenRT.height != Screen.height)
            {
                if (screenRT != null) screenRT.Release();
                screenRT = new RenderTexture(Screen.width, Screen.height, 0, rt.format);
                screenRT.Create();
            }

            ScreenCapture.CaptureScreenshotIntoRenderTexture(screenRT);
            // a rt (960x540) con flip vertical -> corrige el del revés y escala bien
            Graphics.Blit(screenRT, rt, new Vector2(1, -1), new Vector2(0, 1));
        }
    }

    IEnumerator WebSocketConnect()
    {
        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();

        Debug.Log("Intentando conectar a WebSocket...");

        var task = ws.ConnectAsync(
            //new System.Uri("ws://127.0.0.1:8108"), 
            new System.Uri("ws://127.0.0.1:8106/ws"), // servidor en el mismo portatil en el que corre Unity.
            //new System.Uri("wss://impatient-skinless-porous.ngrok-free.dev/ws"), // URL PÚBLICA (ngrok)
            cts.Token
        );

        while (!task.IsCompleted) yield return null;

        if (task.IsFaulted)
        {
            Debug.LogError("❌ Error al conectar: " + task.Exception?.GetBaseException().Message);
            yield break;
        }

        Debug.Log("✅ WS conectado. Estado: " + ws.State);

        yield return new WaitForSeconds(1f);

        Debug.Log("Tras espera. Estado WS: " + ws.State);

        if (ws.State != System.Net.WebSockets.WebSocketState.Open)
        {
            Debug.LogError("❌ WS ya no está abierto después de esperar. Estado: " + ws.State);
            yield break;
        }

        // REGISTRO CORRECTO 
        SendWS(new WSRegister { type = "registro", role = "emisor" });

        _ = ReceiveLoop();
    }

    async Task ReceiveLoop()
    {
        byte[] buffer = new byte[8192];

        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(
                    new System.ArraySegment<byte>(buffer),
                    cts.Token
                );
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.LogError("Servidor cerró la conexión. Razón: "
                        + result.CloseStatus + " - " + result.CloseStatusDescription);
                    break;
                }
                string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                HandleMessage(msg);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ ReceiveLoop excepción: " + e.Message);
        }
        Debug.Log("ReceiveLoop terminado. Estado WS: " + ws.State);

    }

    void HandleMessage(string msg)
    {
        var data = JsonUtility.FromJson<WSMessage>(msg);

        // NO loggear los sticks: llegan a ~25 Hz y el flood de Debug.Log satura el hilo principal de Unity, retrasando el envío de SendRC.
        if (data.type != "control")
            Debug.Log("📩 MSG: " + msg);

        if (data.type == "peticion")
        {
            StartCoroutine(CreateOffer(data.id));
        }
        else if (data.type == "sdp")
        {
            StartCoroutine(SetRemote(data.id, data.sdp));
        }
        else if (data.type == "ice")
        {
            if (!pcs.ContainsKey(data.id)) return;

            // Ignorar candidato null (señal de fin de candidatos)
            if (data.candidate == null || string.IsNullOrEmpty(data.candidate.candidate))
            {
                Debug.Log("Fin de candidatos ICE para cliente " + data.id);
                return;
            }

            RTCIceCandidate cand = new RTCIceCandidate(
                new RTCIceCandidateInit
                {
                    candidate = data.candidate.candidate,
                    sdpMid = data.candidate.sdpMid,
                    sdpMLineIndex = data.candidate.sdpMLineIndex
                });

            pcs[data.id].AddIceCandidate(cand);
        }
        // Sticks de los joysticks -> al dron
        else if (data.type == "control")
        {
            if (dron != null) dron.SetSticks(data.pitch, data.roll, data.yaw);
        }
        // Botones de acción (despegar / loiter / rtl / aterrizar / disparar)
        else if (data.type == "action")
        {
            // Disparo: acción de juego, no necesita el dron
            if (data.action == "shoot") { shootTriggerFrame = Time.frameCount + 1; return; }

            if (dron == null) return;
            switch (data.action)
            {
                case "takeoff": dron.Despegar(data.alt); break;
                case "loiter": dron.Loiter(); break;
                case "rtl": dron.RTL(); break;
                case "smartrtl": dron.SmartRTL(); break;
                case "land": dron.Aterrizar(); break;
            }
        }
        else if (data.type == "client-disconnect")
        {
            CleanupClient(data.id);
        }
    }

    bool ShootScenarioEnabled()
    {
        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Search)
            return true;
        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom)
        {
            foreach (var obj in ConnectionConfig.CustomScenarioObjects)
                if (obj.destructionMethod == "Shoot") return true;
        }

        // Cualquier escenario con gemas a disparar en escena (p. ej. Drone Flow Nivel 5)
        var gems = FindObjectsByType<GemProximityDestroy>(FindObjectsSortMode.None);
        foreach (var g in gems)
            if (g.requireShooting) return true;

        return false;
    }

    // Vigila si el disparo pasa a estar disponible (p. ej. al cargar el Nivel 5 de Drone Flow) y avisa en vivo a los controladores ya conectados.
    IEnumerator MonitorShootState()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            bool current = ShootScenarioEnabled();
            if (current != lastShootState)
            {
                lastShootState = current;
                if (pcs.Count > 0) SendShootState(current);
            }
            yield return wait;
        }
    }

    void SendShootState(bool enabled)
    {
        SendWS(new WSShoot { type = "shoot", role = "emisor", shoot = enabled });
    }

    IEnumerator CreateOffer(int id)
    {
        RTCConfiguration config = default;
        config.iceServers = new[]
        {
            new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } },
            new RTCIceServer
            {
                urls = new[] { "turn:dronseetac.upc.edu:3478" },
                username = "dronseetac",
                credential = "Mimara00."
            },
            new RTCIceServer
            {
                urls = new[] { "turn:standard.relay.metered.ca:80" },
                username = "337f189c0bf26e1022e19f05",
                credential = "pSwi01maZzQZTUAf"
            }
        };

        var pc = new RTCPeerConnection(ref config);

        pc.OnIceConnectionChange = state =>
        {
            Debug.Log("🧊 ICE Connection State [" + id + "]: " + state);
        };

        pc.OnConnectionStateChange = state =>
        {
            Debug.Log("🔗 Connection State [" + id + "]: " + state);
        };

        var track = new VideoStreamTrack(rt);
        pc.AddTrack(track);

        pcs[id] = pc;
        tracks[id] = track;

        var allVideoCodecs = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs;
        var preferredCodecs = allVideoCodecs.Where(c => c.mimeType == "video/H264")
            .Concat(allVideoCodecs.Where(c => c.mimeType == "video/VP8"))
            .Concat(allVideoCodecs.Where(c => c.mimeType == "video/rtx"))
            .ToArray();


        // Activar el envío explícitamente
        foreach (var transceiver in pc.GetTransceivers())
        {
            transceiver.Direction = RTCRtpTransceiverDirection.SendOnly;
            var codecErr = transceiver.SetCodecPreferences(preferredCodecs);
            if (codecErr != RTCErrorType.None)
                Debug.LogError("❌ SetCodecPreferences falló: " + codecErr);
            else
                Debug.Log("Transceiver SendOnly (H.264 preferido, VP8 de reserva)");
        }

        pc.OnIceCandidate = candidate =>
        {
            if (candidate == null) return;

            Debug.Log("Candidato local: " + candidate.Candidate);

            SendWS(new WSICE
            {
                type = "ice",
                role = "emisor",
                id = id,
                candidate = new ICE
                {
                    candidate = candidate.Candidate,
                    sdpMid = candidate.SdpMid,
                    sdpMLineIndex = candidate.SdpMLineIndex ?? 0
                }
            });
        };

        var op = pc.CreateOffer();
        yield return op;

        var desc = op.Desc;
        Debug.Log("SDP Offer generado:\n" + desc.sdp);

        var op2 = pc.SetLocalDescription(ref desc);
        yield return op2;

        //Forzar bitrate alto para mejor calidad
        foreach (var sender in pc.GetSenders())
        {
            var parameters = sender.GetParameters();
            foreach (var encoding in parameters.encodings)
            {
                encoding.maxBitrate = 3000000; // 3 Mbps
                encoding.minBitrate = 1000000; // 1 Mbps
            }
            sender.SetParameters(parameters);
        }

        SendWS(new WSSDP
        {
            type = "sdp",
            role = "emisor",
            id = id,
            sdp = desc.sdp,
            sdp_type = desc.type.ToString(),
            shoot = ShootScenarioEnabled()
        });
    }

    IEnumerator SetRemote(int id, string sdp)
    {
        if (!pcs.ContainsKey(id)) yield break;

        RTCSessionDescription desc = new RTCSessionDescription
        {
            sdp = sdp,
            type = RTCSdpType.Answer
        };

        var op = pcs[id].SetRemoteDescription(ref desc);
        yield return op;

        if (op.IsError)
            Debug.LogError("❌ SetRemoteDescription error: " + op.Error.message);
        else
            Debug.Log("Respuesta recibida de cliente " + id);
    }

    void CleanupClient(int id)
    {
        if (pcs.ContainsKey(id))
        {
            pcs[id].Close();
            pcs[id].Dispose();
            pcs.Remove(id);
        }

        if (tracks.ContainsKey(id))
        {
            tracks[id].Dispose();
            tracks.Remove(id);
        }
    }

    async void SendWS(object obj)
    {
        if (ws == null || ws.State != WebSocketState.Open)
        {
            Debug.LogError("❌ SendWS fallido: WS no está abierto. Estado: " + (ws?.State.ToString() ?? "null"));
            return;
        }

        string json = JsonUtility.ToJson(obj);

        Debug.Log("Enviando: " + json);

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await ws.SendAsync(
            new System.ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            cts.Token
        );
    }

    IEnumerator WebRTCUpdateLoop()
    {
        while (true)
        {
            yield return StartCoroutine(WebRTC.Update());
        }
    }

    void Update()
    {

        if (Time.frameCount % 300 == 0)
        {
            Debug.Log("📷 RT activa: " + rt.IsCreated() +
                      " | Cam activa: " + cam.enabled +
                      " | Cam target: " + (cam.targetTexture == rt));

            foreach (var kvp in tracks)
            {
                Debug.Log("🎥 Track [" + kvp.Key + "] enabled: " + kvp.Value.Enabled +
                          " | readyState: " + kvp.Value.ReadyState);
            }
        }
    }

    void OnDestroy()
    {
        foreach (var pc in pcs.Values)
        {
            pc.Close();
            pc.Dispose();
        }

        foreach (var track in tracks.Values)
        {
            track.Dispose();
        }

        //WebRTC.Dispose();
    }
}

#region MENSAJES

[System.Serializable]
public class WSRegister
{
    public string type;
    public string role;
}

[System.Serializable]
public class WSSDP
{
    public string type;
    public string role;
    public int id;
    public string sdp;
    public string sdp_type;
    public bool shoot;
}

[System.Serializable]
public class WSICE
{
    public string type;
    public string role;
    public int id;
    public ICE candidate;
}

[System.Serializable]
public class WSShoot
{
    public string type;
    public string role;
    public bool shoot;
}

[System.Serializable]
public class WSMessage
{
    public string type;
    public int id;
    public string sdp;
    public string sdp_type;
    public ICE candidate;

    // Comandos del controlador. JsonUtility deja a 0/null los campos ausentes, así que no afecta a los mensajes de sdp/ice.
    public float pitch;
    public float roll;
    public float yaw;
    public string action;   // "takeoff" | "loiter" | "rtl" | "smartrtl" | "land"
    public int alt;         // altitud de despegue (m)
}

[System.Serializable]
public class ICE
{
    public string candidate;
    public string sdpMid;
    public int sdpMLineIndex;
}

#endregion