using System.Collections.Generic;

public static class ConnectionConfig
{
    public enum Mode { Simulation, Production }
    public enum Scenario { None, DroneFlow, Circuit, Easy, Hard, Search, Custom }

    public static Mode SelectedMode = Mode.Simulation;
    public static Scenario SelectedScenario = Scenario.None;

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
    public double latitude;
    public double longitude;
    public double altitude;
    public double heading;

    // Campos dependientes del "type" (Anulables)
    public int? width;
    public int? height;
    public float? scale; //Radio del anillo 0.01 = 2m
    public string rotation;
    public bool? scoreTarget;
    public bool? circuitTarget;
    public bool? endGameObject;
    public bool? gemsRandomPosition;
    public int? numberOfGems;
    public bool? stopwatchObject;
    public string destructionMethod;
}
