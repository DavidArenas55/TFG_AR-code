using System.Collections;
using UnityEngine;
using csDronLink;  

public class DronControl : MonoBehaviour
{
    [Header("Mapeo de sticks")]
    [Range(0, 500)] public int rcRange = 350;   
    [Range(0f, 1f)] public float expo = 0.3f;   
    public bool invertPitch = false;            
    public bool invertRoll = false;
    public bool invertYaw = false;

    [Header("Envío / Failsafe")]
    public int sendRateHz = 20;                 
    public float controlTimeout = 0.5f;         

    private Dron miDron;
    private bool rcEnabled = false;             

    private float curPitch, curRoll, curYaw;   
    private float lastStickTime;

    void Start()
    {
        if (DroneConnectionManager.Instance == null || !DroneConnectionManager.Instance.IsConnected)
        {
            Debug.LogError("DronControl: no hay conexión activa. Conecta desde el menú (CONNECT).");
        }
        else
        {
            miDron = DroneConnectionManager.Instance.GetDron();
            if (miDron == null)
                Debug.LogError("DronControl: conexión marcada como activa pero Dron es null.");
        }

        StartCoroutine(RCSendLoop());
    }

    private bool EnsureDron()
    {
        if (miDron != null) return true;
        if (DroneConnectionManager.Instance != null && DroneConnectionManager.Instance.IsConnected)
            miDron = DroneConnectionManager.Instance.GetDron();
        return miDron != null;
    }

    public void SetSticks(float pitch, float roll, float yaw)
    {
        curPitch = pitch;
        curRoll = roll;
        curYaw = yaw;
        lastStickTime = Time.time;
    }

    private float Expo(float v) => (1f - expo) * v + expo * v * v * v;

    private ushort ToPwm(float vNorm, bool invert)
    {
        if (invert) vNorm = -vNorm;
        float v = Mathf.Clamp(Expo(vNorm), -1f, 1f);
        int pwm = 1500 + Mathf.RoundToInt(v * rcRange);
        return (ushort)Mathf.Clamp(pwm, 1000, 2000);
    }

    private IEnumerator RCSendLoop()
    {
        var wait = new WaitForSeconds(1f / Mathf.Max(1, sendRateHz));
        while (true)
        {
            // Solo enviamos RC override en modo manual (tras Loiter) y con dron válido.
            if (rcEnabled && EnsureDron())
            {
                // Failsafe por si dejan de llegar sticks, neutralizar -> hover en sitio.
                bool stale = Time.time - lastStickTime > controlTimeout;
                float p = stale ? 0f : curPitch;
                float r = stale ? 0f : curRoll;
                float y = stale ? 0f : curYaw;

                ushort roll = ToPwm(r, invertRoll);
                ushort pitch = ToPwm(p, invertPitch);
                ushort yaw = ToPwm(y, invertYaw);
                ushort throttle = 1500; // throttle NO controlado (Loiter mantiene altitud)

                // Orden de la librería: SendRC(roll, pitch, throttle, yaw)
                miDron.SendRC(roll, pitch, throttle, yaw);
            }
            yield return wait;
        }
    }

    // Libera los canales RC override (0 = "ignorar canal" en MAVLink), para que
    // el modo automático (RTL/Land) recupere el control inmediatamente.
    private void ReleaseRC()
    {
        if (EnsureDron())
            miDron.SendRC(0, 0, 0, 0);
    }

    // ACCIONES (botones) 

    public void Despegar(int alt)
    {
        if (!EnsureDron()) return;
        rcEnabled = false;           // durante el despegue guiado NO pisamos con RC override
        ReleaseRC();
        miDron.Despegar(alt > 0 ? alt : 5, bloquear: false,
            f: (byte id, object o) => Debug.Log("✅ En el aire (dron " + id + ")"),
            param: null);
    }

    public void Loiter()
    {
        if (!EnsureDron()) return;
        miDron.PonModoLoiter();
        rcEnabled = true; // a partir de aquí, control manual por joysticks
        Debug.Log(" Modo manual (Loiter) activado");
    }

    public void RTL()
    {
        rcEnabled = false; // dejamos de pisar el RC
        ReleaseRC();
        if (EnsureDron()) miDron.RTL(bloquear: false);
    }

    public void SmartRTL()
    {
        rcEnabled = false;
        ReleaseRC();
        if (EnsureDron()) miDron.SmartRTL(bloquear: false);
    }

    public void Aterrizar()
    {
        rcEnabled = false;
        ReleaseRC();
        if (EnsureDron()) miDron.Aterrizar(bloquear: false);
    }
}