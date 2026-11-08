using System.Collections.Generic;
using UnityEngine;

public class DroneTelemetry : MonoBehaviour
{
    [Header("Arrastra aquí el DroneRig que tiene VirtualDroneTelemetry")]
    public VirtualDroneTelemetry virtualDron;

    private bool isActive = true;

    private void ProcesarTelemetria(byte source, List<(string nombre, float valor)> telemetria)
    {
        if (!isActive) return;

        if (virtualDron == null)
        {
            Debug.LogError("VirtualDroneTelemetry no asignado en el inspector. No se puede procesar la telemetría.");
            return;
        }

        float latitudRaw = telemetria.Find(t => t.nombre == "Lat").valor;
        float longitudRaw = telemetria.Find(t => t.nombre == "Lon").valor;
        float altitud = telemetria.Find(t => t.nombre == "Alt").valor;
        float headingRaw = telemetria.Find(t => t.nombre == "Heading").valor;

        double heading = headingRaw / 1e2;
        double latitud = latitudRaw / 1e7;
        double longitud = longitudRaw / 1e7;

        float modeId = telemetria.Find(t => t.nombre == "Mode").valor;
        string modeName = "";

        if (modeId == 0) modeName = "STABILIZE";
        else if (modeId == 1) modeName = "ACRO";
        else if (modeId == 2) modeName = "ALT_HOLD";
        else if (modeId == 3) modeName = "AUTO";
        else if (modeId == 4) modeName = "GUIDED";
        else if (modeId == 5) modeName = "LOITER";
        else if (modeId == 6) modeName = "RTL";
        else if (modeId == 7) modeName = "CIRCLE";
        else if (modeId == 9) modeName = "LAND";
        else if (modeId == 11) modeName = "DRIFT";
        else if (modeId == 13) modeName = "SPORT";
        else if (modeId == 14) modeName = "FLIP";
        else if (modeId == 15) modeName = "AUTOTUNE";
        else if (modeId == 16) modeName = "POSHOLD";
        else if (modeId == 17) modeName = "BRAKE";
        else if (modeId == 18) modeName = "THROW";
        else if (modeId == 19) modeName = "AVOID_ADSB";
        else if (modeId == 20) modeName = "GUIDED_NOGPS";
        else if (modeId == 21) modeName = "SMART_RTL";
        else if (modeId == 22) modeName = "FLOWHOLD";
        else if (modeId == 23) modeName = "FOLLOW";
        else if (modeId == 24) modeName = "ZIGZAG";
        else if (modeId == 25) modeName = "SYSTEMID";
        else if (modeId == 26) modeName = "AUTOROTATE";
        else if (modeId == 27) modeName = "AUTO_RTL";
        else if (modeId == 28) modeName = "TURTLE";
        else modeName = "UNKNOWN";

        float armStatus = telemetria.Find(t => t.nombre == "Armed").valor;
        float roll = telemetria.Find(t => t.nombre == "RollDeg").valor;
        float pitch = telemetria.Find(t => t.nombre == "PitchDeg").valor;

        virtualDron.SetTelemetry(
            latitud, longitud, altitud, (float)heading,
            modeName, armStatus, pitch, roll);
    }

    private void ProcesarTelemetriaBateria(byte source, List<(string nombre, float valor)> telemetria)
    {
        if (!isActive) return;

        if (virtualDron == null)
        {
            Debug.LogError("VirtualDroneTelemetry no asignado en el inspector. No se puede procesar la telemetría de batería.");
            return;
        }

        float battCurrent_A = telemetria.Find(t => t.nombre == "current").valor;
        float battVoltage_V = telemetria.Find(t => t.nombre == "voltage").valor;
        float battRemaining_pct = telemetria.Find(t => t.nombre == "remaining").valor;

        virtualDron.SetTelemetryBattery(
            battCurrent_A, battVoltage_V, battRemaining_pct);
    }

    private void Start()
    {
        if (virtualDron == null)
            virtualDron = FindFirstObjectByType<VirtualDroneTelemetry>();

        if (DroneConnectionManager.Instance == null ||
            !DroneConnectionManager.Instance.IsConnected)
        {
            Debug.LogError("DroneTelemetry: No hay conexión activa. Vuelve al menú y pulsa CONNECT.");
            return;
        }

        DroneConnectionManager.Instance.StartTelemetry(ProcesarTelemetria);
        DroneConnectionManager.Instance.StartBatteryTelemetry(ProcesarTelemetriaBateria);
    }

    private void OnDestroy()
    {
        if (DroneConnectionManager.Instance != null)
        {
            DroneConnectionManager.Instance.StopTelemetry();
            DroneConnectionManager.Instance.StopBatteryTelemetry();
        }
    }

    private void OnApplicationQuit()
    {
        if (DroneConnectionManager.Instance != null)
        {
            DroneConnectionManager.Instance.StopTelemetry();
            DroneConnectionManager.Instance.StopBatteryTelemetry();
        }
    }
}
