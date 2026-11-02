using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [Header("World Anchor")]
    public WorldAnchor worldAnchor;

    public GameObject scenarioDroneFlow; public GameObject scenarioCircuit; public GameObject scenarioMazeEasy; public GameObject scenarioMazeHard; public GameObject scenarioSearch; public GameObject scenarioCustom;

    [Header("Drone Flow Progress")]
    public int currentDroneFlowLevel = 1;
    public int totalLevelTargets = 0;
    public bool isSequential = true;

    [Header("Simulation Fences & Ground")]
    public GameObject fences;
    public GameObject ground;

    [Header("Scenario 0 & 1 Prefabs")]
    public GameObject CircleTakeOff; public GameObject CircleLand;
    public GameObject Ring; public GameObject Arc; public GameObject Square; public GameObject Cylinder; public GameObject Tunnel;

    [Header("Scenario 2 & 3 Prefabs")]
    public GameObject WallLeft; public GameObject WallRight; public GameObject WallMiddle;

    [Header("Scenario 4 Prefabs")]
    public GameObject TreeType1; public GameObject TreeType1Modified; public GameObject TreeType3; public GameObject TreeType3Modified1; public GameObject TreeType3Modified2;
    public GameObject Fountain; public GameObject Gazebo; public GameObject Bench; public GameObject Lamp; public GameObject Table; public GameObject SafeTower;

    void Start()
    {
        fences.SetActive(false);
        ground.SetActive(false);

        scenarioDroneFlow.SetActive(false);
        scenarioCircuit.SetActive(false);
        scenarioMazeEasy.SetActive(false);
        scenarioMazeHard.SetActive(false);
        scenarioSearch.SetActive(false);
        scenarioCustom.SetActive(false);

        if (ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Simulation)
        {
            fences.SetActive(true);
            ground.SetActive(true);
        }

        Debug.Log("Selected Scenario: " + ConnectionConfig.SelectedScenario);

        switch (ConnectionConfig.SelectedScenario)
        {
            case ConnectionConfig.Scenario.DroneFlow:
                if (ConnectionConfig.DroneFlowLevel == 0)
                {
                    isSequential = true;
                    currentDroneFlowLevel = 1;
                }
                else
                {
                    isSequential = false;
                    currentDroneFlowLevel = ConnectionConfig.DroneFlowLevel;
                }
                LoadDroneFlowLevel(currentDroneFlowLevel);
                break;

            case ConnectionConfig.Scenario.Circuit:
                totalLevelTargets = CircuitScenario.Load(
                    scenarioCircuit.transform,
                    CircleTakeOff,
                    CircleLand,
                    Cylinder,
                    Ring,
                    Arc,
                    Tunnel
                );

                scenarioCircuit.SetActive(true);
                break;

            case ConnectionConfig.Scenario.Easy:
                MazeScenario.LoadEasy(
                    scenarioMazeEasy.transform,
                    WallLeft,
                    WallRight,
                    WallMiddle
                );

                scenarioMazeEasy.SetActive(true);
                break;

            case ConnectionConfig.Scenario.Hard:
                MazeScenario.LoadHard(
                    scenarioMazeHard.transform,
                    WallLeft,
                    WallRight,
                    WallMiddle
                );

                scenarioMazeHard.SetActive(true);
                break;

            case ConnectionConfig.Scenario.Search:
                SearchScenario.Load(
                    scenarioSearch.transform,
                    TreeType1,
                    TreeType1Modified,
                    TreeType3Modified1,
                    TreeType3Modified2,
                    Fountain,
                    Gazebo,
                    Bench,
                    Lamp,
                    Table,
                    SafeTower
                );

                scenarioSearch.SetActive(true);
                break;

            case ConnectionConfig.Scenario.Custom:
                int globalCircuitIndexCounter = 0;
                foreach (var obj in ConnectionConfig.CustomScenarioObjects)
                {
                    Vector3 position = worldAnchor.GPSToUnity(obj.latitude, obj.longitude) + new Vector3(0, (float)obj.altitude, 0);
                    Quaternion rotation = Quaternion.Euler(90, (float)obj.heading, 0);
                    GameObject instantiatedObj = null;

                    switch (obj.type)
                    {
                        case "Ring":
                            Vector3 ringPosition = position + new Vector3(0, -1f, 0);
                            instantiatedObj = Instantiate(Ring, ringPosition, rotation, scenarioCustom.transform);
                            float ringDiameter = obj.diameter ?? 2f;
                            float ringScale = ringDiameter / 200f;

                            instantiatedObj.transform.localScale = new Vector3(
                                ringScale,
                                ringScale,
                                ringScale
                            );
                            break;

                        case "Arc":
                            rotation = Quaternion.Euler(0, (float)obj.heading + 180f + 18f, 0);
                            Vector3 arcPosition = position + new Vector3(0, -1f, 0);
                            instantiatedObj = Instantiate(Arc, arcPosition, rotation, scenarioCustom.transform);
                            break;

                        case "Tunnel":
                            rotation = Quaternion.Euler(90, (float)obj.heading + 90f, 0);
                            Vector3 tunnelPosition = position + new Vector3(0, -1f, 0);
                            instantiatedObj = Instantiate(Tunnel, tunnelPosition, rotation, scenarioCustom.transform);
                            break;

                        case "Square":
                            rotation = Quaternion.Euler(0, (float)obj.heading + 180f + 18f, 0);
                            Vector3 squarePosition = position + new Vector3(0, -2f - 1f, 0);
                            instantiatedObj = Instantiate(Square, squarePosition, rotation, scenarioCustom.transform);
                            break;

                        case "Cylinder":
                            if (obj.rotation == "Clockwise")
                            {
                                rotation = Quaternion.Euler(180, (float)obj.heading + 18f + 55f, 0);
                            }
                            else
                            {
                                rotation = Quaternion.Euler(0, (float)obj.heading + 18f, 0);
                            }
                            Vector3 cylinderPosition = position + new Vector3(0, 6f - 1f, 0);
                            instantiatedObj = Instantiate(Cylinder, cylinderPosition, rotation, scenarioCustom.transform);
                            break;

                        case "HTakeoff":
                            rotation = Quaternion.Euler(0, (float)obj.heading, 0);
                            Vector3 HTakeOffPosition = position + new Vector3(0, -1f, 0);
                            instantiatedObj = Instantiate(CircleTakeOff, HTakeOffPosition, rotation, scenarioCustom.transform);
                            break;

                        case "HLand":
                            rotation = Quaternion.Euler(0, (float)obj.heading, 0);
                            Vector3 HLandPosition = position + new Vector3(0, -1f, 0);
                            instantiatedObj = Instantiate(CircleLand, HLandPosition, rotation, scenarioCustom.transform);
                            break;

                        case "Wall":
                            int middleWallCount = obj.width.Value - 2;
                            float rotationY = (float)obj.heading;
                            Vector3 basePosition = position + new Vector3(0, 0.5f - 1f, 0);
                            int floorCount = obj.height.Value;
                            int wallCount = 0;
                            foreach (Transform child in scenarioCustom.transform)
                            {
                                if (child.name.StartsWith("Wall_"))
                                {
                                    wallCount++;
                                }
                            }
                            MazeScenario.BuildWall(
                                scenarioCustom.transform,
                                middleWallCount,
                                rotationY,
                                basePosition,
                                floorCount,
                                wallCount + 1,
                                WallLeft,
                                WallRight,
                                WallMiddle
                            );
                            break;
                    }

                    // Configuración del GateTrigger para objetos tipo gate (Ring, Arc, Cylinder...).
                    // Las gemas tienen instantiatedObj == null (las gestiona GemaSpawner).
                    if (instantiatedObj != null && (obj.circuitTarget == true || obj.endGameObject == true))
                    {
                        GateTrigger waypoint = instantiatedObj.GetComponentInChildren<GateTrigger>();
                        if (waypoint == null) waypoint = instantiatedObj.AddComponent<GateTrigger>();

                        // Solo los circuit targets reciben un índice de orden
                        if (obj.circuitTarget == true)
                            waypoint.waypointIndex = globalCircuitIndexCounter;

                        // El end game object termina la partida al cruzarlo, sea o no circuit target
                        if (obj.endGameObject == true)
                            waypoint.isFinalTarget = true;
                    }

                    // El contador de circuito avanza por cualquier objeto circuit target (gema o gate),
                    // para que GemaSpawner calcule los índices de forma coherente.
                    if (obj.circuitTarget == true)
                        globalCircuitIndexCounter++;
                }

                totalLevelTargets = globalCircuitIndexCounter;

                scenarioCustom.SetActive(true);
                break;
        }
    }

    public void AdvanceToNextLevel()
    {
        currentDroneFlowLevel++;

        if (currentDroneFlowLevel > 5)
        {
            RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
            if (raceTimer != null)
            {
                Debug.Log("¡Secuencia Drone Flow Superada por completo!");
                raceTimer.StopTimer();
                raceTimer.openResultsButton.gameObject.SetActive(true);
                raceTimer.GameOver.SetActive(true);
            }
            return;
        }

        Debug.Log("Cargando nivel: " + currentDroneFlowLevel);
        LoadDroneFlowLevel(currentDroneFlowLevel);

        RaceTimer timer = FindFirstObjectByType<RaceTimer>();
        if (timer != null) timer.SetDroneFlowTime(currentDroneFlowLevel);
    }
    public void LoadDroneFlowLevel(int level)
    {
        totalLevelTargets = DroneFlowScenario.LoadLevel(
            level,
            scenarioDroneFlow.transform,
            Ring,
            Square
        );

        Debug.Log("Drone Flow: " + totalLevelTargets + " objetos cargados");
        Debug.Log("Scenario activo: " + scenarioDroneFlow.activeSelf);
        Debug.Log("Hijos: " + scenarioDroneFlow.transform.childCount);

        scenarioDroneFlow.SetActive(true);
    }
}