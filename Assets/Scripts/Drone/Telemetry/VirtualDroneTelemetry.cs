using UnityEngine;
using System;

public class VirtualDroneTelemetry : MonoBehaviour
{

    [Header("World Anchor (GPS fijo)")]
    public WorldAnchor worldAnchor;

    [Tooltip("Offset mundo real con virtual. Se suma a la posición Y.")]    
    public float offsetVisual = -1f; //Offset para que el suelo de Unity coincida con el suelo real visto en la imagen de la cámara. <-- MAL
    // Un offset NO soluciona el q los suelos no coincidan. Lo dejo porque he posicionado todos los objetos virtuales teniendo en cuenta que el suelo de Unity está a Y=-1.

    [Header("Smooth Movement")]
    public bool smoothMotion = true;
    public float positionLerpSpeed = 5f;
    public float rotationLerpSpeed = 5f;

    [Header("GPS actual (solo lectura)")]
    [SerializeField] private double currentLatitude;
    [SerializeField] private double currentLongitude;
    [SerializeField] private double currentAltitude;
    [SerializeField] private float currentHeading;

    [Header("Attitude (solo lectura)")]
    [SerializeField] private float currentPitch;
    //[SerializeField] private float currentYaw; // Se considera que el yaw es el mismo que el heading, pero lo guardamos aparte por si quieres mostrarlo o usarlo sin correcciones de offsets.
    [SerializeField] private float currentRoll;

    [Header("Modo de Vuelo (solo lectura)")]
    [SerializeField] private string armStatus; // "ARMED" o "DISARMED" 
    [SerializeField] private string flightMode;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    [Header("Battery Status")]
    [SerializeField] private float batteryCurrent_A;
    [SerializeField] private float batteryVoltage_V;
    [SerializeField] private float batteryRemaining_pct;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        if (ConnectionConfig.SelectedDrone == ConnectionConfig.DroneType.GPS
            && !worldAnchor)
        {
            Debug.LogError("VirtualDroneTelemetry: WorldAnchor no asignado.");
        }
    }

    void Update()
    {
        if (smoothMotion)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }
        else
        {
            transform.SetPositionAndRotation(targetPosition, targetRotation);
        }

        if (ConnectionConfig.SelectedDrone == ConnectionConfig.DroneType.GPS)
        {
            UpdateCurrentGPS();
        }
    }
    public void SetUnityPose(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = rotation;
    }
    // Actualiza la representación y el estado del dron a partir de datos Unity.
    // No realiza conversiones GPS.
    public void SetUnityTelemetry(
        Vector3 position,
        Quaternion rotation,
        string modeName,
        float armStatus,
        float pitch,
        float roll)
    {
        SetUnityPose(position, rotation);

        // Redondeamos a 2 decimales para mostrar los valores en el Inspector.
        currentPitch = Mathf.Round(pitch * 100f) / 100f;
        currentRoll = Mathf.Round(roll * 100f) / 100f;

        flightMode = modeName;

        // Estado de armado del dron.
        if (armStatus == 1f)
            this.armStatus = "ARMED";
        else
            this.armStatus = "DISARMED";
    }
    public void SetTelemetryBattery(float battCurrent_A, float battVoltage_V, float battRemaining_pct)
    {
        this.batteryCurrent_A = battCurrent_A;
        this.batteryVoltage_V = battVoltage_V;
        this.batteryRemaining_pct = battRemaining_pct;
    }
    private void UpdateCurrentGPS()
    {
        if (!worldAnchor) return;

        // Restamos offset para que la conversión inversa no infle la altitud GPS
        Vector3 posForGPS = transform.position;
        posForGPS.y -= offsetVisual;

        // Convertimos Unity -> GPS usando el anchor (solo para mostrar en Inspector)
        var gps = worldAnchor.UnityToGPS(posForGPS);

        currentLatitude = gps.latitude;
        currentLongitude = gps.longitude;
        currentAltitude = Math.Round(gps.altitude,2); // Redondeamos a 2 decimales (de forma diferente pq altitude es un double).

        // Heading desde rotación Unity
        float unityYaw = transform.eulerAngles.y;

        float heading = unityYaw;
        currentHeading = Mathf.Round(heading * 100f) / 100f; // Redondeamos a 2 decimales.
    }
    public (double latitude, double longitude, double altitude, float heading) GetCurrentGPS()
    {
        return (currentLatitude, currentLongitude, currentAltitude, currentHeading);
    }
    public float GetBatteryCurrent_A() => batteryCurrent_A;
    public float GetBatteryVoltage_V() => batteryVoltage_V;
    public float GetBatteryRemaining_pct() => batteryRemaining_pct;
    public string GetFlightMode() => flightMode;
    public string GetArmStatus() => armStatus;

}
