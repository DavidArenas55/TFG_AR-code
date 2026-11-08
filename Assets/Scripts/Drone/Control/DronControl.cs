using System.Collections;
using UnityEngine;

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

    private bool rcEnabled = false;

    private float curPitch, curRoll, curYaw;
    private float lastStickTime;

    private DroneConnectionManager Connection => DroneConnectionManager.Instance;

    private void Start()
    {
        if (Connection == null || !Connection.IsConnected)
            Debug.LogError("DronControl: no hay conexión activa. Conecta desde el menú (CONNECT).");

        StartCoroutine(RCSendLoop());
    }

    private bool EnsureDron()
    {
        return Connection != null && Connection.IsConnected;
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
            if (rcEnabled && EnsureDron())
            {
                bool stale = Time.time - lastStickTime > controlTimeout;

                float p = stale ? 0f : curPitch;
                float r = stale ? 0f : curRoll;
                float y = stale ? 0f : curYaw;

                ushort roll = ToPwm(r, invertRoll);
                ushort pitch = ToPwm(p, invertPitch);
                ushort yaw = ToPwm(y, invertYaw);
                ushort throttle = 1500;

                Connection.SendRC(roll, pitch, throttle, yaw);
            }

            yield return wait;
        }
    }

    private void ReleaseRC()
    {
        if (EnsureDron())
            Connection.SendRC(0, 0, 0, 0);
    }

    public void Despegar(int alt)
    {
        if (!EnsureDron()) return;

        rcEnabled = false;
        ReleaseRC();

        Connection.TakeOff(
            alt > 0 ? alt : 5,
            bloquear: false,
            callback: (byte id, object o) =>
                Debug.Log("✅ En el aire (dron " + id + ")"),
            param: null);
    }

    public void Loiter()
    {
        if (!EnsureDron()) return;

        Connection.SetLoiterMode();
        rcEnabled = true;

        Debug.Log(" Modo manual (Loiter) activado");
    }

    public void RTL()
    {
        rcEnabled = false;
        ReleaseRC();

        if (EnsureDron())
            Connection.ReturnToLaunch(bloquear: false);
    }

    public void SmartRTL()
    {
        rcEnabled = false;
        ReleaseRC();

        if (EnsureDron())
            Connection.SmartReturnToLaunch(bloquear: false);
    }

    public void Aterrizar()
    {
        rcEnabled = false;
        ReleaseRC();

        if (EnsureDron())
            Connection.Land(bloquear: false);
    }
}
