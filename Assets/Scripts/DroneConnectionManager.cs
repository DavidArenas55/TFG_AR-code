using UnityEngine;
using csDronLink;
using System;
using System.Net.Sockets;
//using System.IO.Ports;
//using System.Linq;

public class DroneConnectionManager : MonoBehaviour
{
    public static DroneConnectionManager Instance { get; private set; }
    public bool IsConnected { get; private set; } = false;
    public string LastError { get; private set; } = "";
    public ConnectionConfig.Mode Mode { get; private set; }
    public string ComPort { get; private set; }

    private Dron dron;

    void Awake()
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
                // Comprobar que el SITL es alcanzable
                using (var tcp = new TcpClient())
                {
                    tcp.Connect("127.0.0.1", 5763);
                    tcp.Close();
                }
            }
            else
            {
                // En producción, solo guardamos el puerto COM y validamos su formato básico.
                // La verificación real se hará en Connect() usando csDronLink, cuando se le de al botón PLAY.
                // System.IO.Ports no está disponible en .NET Standard 2.1 de Unity por defecto.
                // (MIRAR ESTO, prefiero que el botón conectar también verifique más aparte del formato,
                //  tal vez es mejor tener solo un botón en vez de 2, CONNECT y PLAY)

                if (string.IsNullOrEmpty(ComPort))
                {
                    LastError = "Puerto COM vacío. Escribe algo tipo COM5.";
                    return false;
                }

                if (!ComPort.StartsWith("COM") || !int.TryParse(ComPort.Substring(3), out _))
                {
                    LastError = "Invalid format. Use COM5, COM12...";
                    return false;
                }

                //string[] availablePorts = SerialPort.GetPortNames();
                //if (!availablePorts.Contains(ComPort))
                //{
                //    LastError = $"El puerto {ComPort} no existe. Disponibles: {string.Join(", ", availablePorts)}";
                //    return false;
                //}

                //// Verificar que el puerto no está ocupado (intento rápido de apertura)
                //using (var testPort = new SerialPort(ComPort))
                //{
                //    testPort.Open();
                //    testPort.Close();
                //}
            }

            return true;
        }
        //catch (UnauthorizedAccessException)
        //{
        //    LastError = $"El puerto {ComPort} está ocupado por otra aplicación.";
        //    return false;
        //}
        catch (Exception e)
        {
            LastError = e.Message;
            return false;
        }
    }

    // Establece la conexión real y comienza la comunicación MAVLink. Llamar solo cuando se vaya a usar Play.
    public bool Connect()
    {
        Disconnect();
        LastError = "";

        try
        {
            dron = new Dron();

            if (Mode == ConnectionConfig.Mode.Simulation)
            {
                dron.Conectar("simulacion",freq:20);
            }
            else
            {
                dron.Conectar("produccion", ComPort,freq:20);
            }

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
        IsConnected = false;
        LastError = "";
        dron = null;
    }

    public Dron GetDron()
    {
        return dron;
    }
}