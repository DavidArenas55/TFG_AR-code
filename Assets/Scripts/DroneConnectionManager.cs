using UnityEngine;
using csDronLink;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class DroneConnectionManager : MonoBehaviour
{
    public static DroneConnectionManager Instance { get; private set; }

    public bool IsConnected { get; private set; } = false;
    public string LastError { get; private set; } = "";
    public ConnectionConfig.Mode Mode { get; private set; }
    public string ComPort { get; private set; }

    private Dron dron;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Verifica que la conexión es posible sin iniciar telemetría.
    public bool Verify(ConnectionConfig.Mode mode, string comPortIfAny)
    {
        Mode = mode;
        ComPort = comPortIfAny ?? "";
        LastError = "";

        try
        {
            if (mode == ConnectionConfig.Mode.Simulation)
            {
                using (var tcp = new TcpClient())
                {
                    tcp.Connect("127.0.0.1", 5763);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ComPort))
                {
                    LastError = "Puerto COM vacío. Escribe algo tipo COM5.";
                    return false;
                }

                if (!ComPort.StartsWith("COM") ||
                    !int.TryParse(ComPort.Substring(3), out _))
                {
                    LastError = "Invalid format. Use COM5, COM12...";
                    return false;
                }
            }

            return true;
        }
        catch (Exception e)
        {
            LastError = e.Message;
            return false;
        }
    }

    public bool Connect()
    {
        Disconnect();
        LastError = "";

        try
        {
            dron = new Dron();

            if (Mode == ConnectionConfig.Mode.Simulation)
                dron.Conectar("simulacion", freq: 20);
            else
                dron.Conectar("produccion", ComPort, freq: 20);

            IsConnected = true;
            return true;
        }
        catch (Exception e)
        {
            IsConnected = false;
            LastError = e.Message;
            dron = null;
            return false;
        }
    }

    public void Disconnect()
    {
        StopTelemetry();
        StopBatteryTelemetry();

        IsConnected = false;
        LastError = "";
        dron = null;
    }

    private bool EnsureConnection()
    {
        if (IsConnected && dron != null)
            return true;

        Debug.LogWarning("No hay una conexión activa con el dron.");
        return false;
    }

    // TELEMETRÍA

    public void StartTelemetry(Action<byte, List<(string nombre, float valor)>> callback)
    {
        if (EnsureConnection())
            dron.EnviarDatosTelemetria(callback);
    }

    public void StartBatteryTelemetry(Action<byte, List<(string nombre, float valor)>> callback)
    {
        if (EnsureConnection())
            dron.EnviarDatosNivelBateria(callback);
    }

    public void StopTelemetry()
    {
        if (dron != null)
            dron.DetenerDatosTelemetria();
    }

    public void StopBatteryTelemetry()
    {
        if (dron != null)
            dron.DetenerDatosNivelBateria();
    }

    // CONTROL

    public void SendRC(ushort roll, ushort pitch, ushort throttle, ushort yaw)
    {
        if (EnsureConnection())
            dron.SendRC(roll, pitch, throttle, yaw);
    }

    public void TakeOff(
        int altitude,
        bool bloquear = true,
        Action<byte, object> callback = null,
        object param = null)
    {
        if (EnsureConnection())
            dron.Despegar(altitude, bloquear, callback, param);
    }

    public void SetLoiterMode()
    {
        if (EnsureConnection())
            dron.PonModoLoiter();
    }

    public void ReturnToLaunch(bool bloquear = true, Action<byte, object> callback = null, object param = null)
    {
        if (EnsureConnection())
            dron.RTL(bloquear, callback, param);
    }

    public void SmartReturnToLaunch(bool bloquear = true, Action<byte, object> callback = null, object param = null)
    {
        if (EnsureConnection())
            dron.SmartRTL(bloquear, callback, param);
    }

    public void Land(bool bloquear = true, Action<byte, object> callback = null, object param = null)
    {
        if (EnsureConnection())
            dron.Aterrizar(bloquear, callback, param);
    }

    // ESCENARIO / GEOCERCAS

    public void SetScenario(List<List<(double lat, double lon)>> scenario, Action callback = null)
    {
        if (EnsureConnection())
            dron.EstableceEscenario(scenario, callback);
    }
}
