using System.Collections.Generic;

public static class ConnectionConfig
{
    public enum Mode { Simulation, Production }
    public enum Scenario { None, DroneFlow, Circuit, Easy, Hard, Search, Custom }

    // Sistema de coordenadas utilizado por el escenario Custom.
    public enum CoordinateSystem
    {
        GPS = 0,
        Local = 1
    }

    public static Mode SelectedMode = Mode.Simulation;
    public static Scenario SelectedScenario = Scenario.None;

    // Sistema de coordenadas del escenario Custom actualmente seleccionado.
    // GPS es el valor predeterminado para mantener la compatibilidad.
    public static CoordinateSystem CustomCoordinateSystem = CoordinateSystem.GPS;

    // 0 = Secuencial, 1 = Nivel 1, 2 = Nivel 2, etc.
    public static int DroneFlowLevel = 0;

    public static string ComPort;
    public static bool ConnectRequested = false;

    public static List<ScenarioObjectData> CustomScenarioObjects = new List<ScenarioObjectData>();

    public static string CustomScenarioName = "";
    public static bool CustomHasStopwatch = false;
    public static bool CustomHasCountdown = false;
    public static float CustomCountdownTime = 0f; // Convertido en segundos
}

[System.Serializable]
public class ScenarioObjectData
{
    // Campos comunes u obligatorios
    public int id;
    public string type;

    // Coordenadas GPS
    public double latitude;
    public double longitude;
    public double altitude;

    // Coordenadas locales de Unity
    public float localX;
    public float localY;
    public float localZ;

    public double heading;

    // Campos dependientes del "type" (Anulables)
    public float? diameter; // Radio del anillo scale 0.01 = 2m de diametro
    public int? width;
    public int? height;
    public string rotation;
    public bool? scoreTarget;
    public bool? circuitTarget;
    public bool? endGameObject;
    public bool? gemsRandomPosition;
    public int? numberOfGems;
    public bool? stopwatchObject;
    public string destructionMethod;
}
