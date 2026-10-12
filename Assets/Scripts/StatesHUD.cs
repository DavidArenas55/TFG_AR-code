using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class StatesHUD : MonoBehaviour
{
    [Header("References")]
    public VirtualDroneTelemetry drone;

    [Header("UI Texts (Legacy)")]
    public Text telemetryText;  
    public Text batteryText;   
    public Text flightModeText;

    void Start()
    {
        if (!drone)
            drone = FindFirstObjectByType<VirtualDroneTelemetry>();

        if (!telemetryText || !batteryText || !flightModeText)
            Debug.LogWarning("TelemetryHUD: Asigna telemetryTextLeft, batteryTextRight y flightModeText en el Inspector.");
    }

    void Update()
    {
        if (drone == null) return;

        //Llamamos a la funcion GetCurrentGPS() para que actualice los valores de altitud.
        var (_, _, alt, heading) = drone.GetCurrentGPS();
        float voltage = drone.GetBatteryVoltage_V();
        float current = drone.GetBatteryCurrent_A();
        float remaining = drone.GetBatteryRemaining_pct();
        string mode = drone.GetFlightMode();
        string armStatus = drone.GetArmStatus();


        if (telemetryText)
        {
            telemetryText.text =
              $"ALT: {alt:0.0000} m\n" +
              $"HDG: {heading:0.00}°";
        }

        if (batteryText)
        {
            string voltText = voltage < 0f ? "--" : $"{voltage:0.00} V";
            string currText = current < 0f ? "--" : $"{current:0.00} A";
            string remText = remaining < 0f ? "--" : $"{remaining:0}%";

            batteryText.text = $"{voltText} | {currText} | {remText}";
        }

        if (flightModeText)
        {
            flightModeText.text = 
                $"Flight Mode: {mode} \n"+
                $"Status: {armStatus}";
        }

    }
}
