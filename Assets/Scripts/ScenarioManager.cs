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
                List<GameObject> circuitTargets = new List<GameObject>();
                Instantiate(CircleTakeOff, new Vector3(12.65f, 0f-1f, -25.71f), Quaternion.Euler(0f, 70f, 0f), scenarioCircuit.transform);
                Instantiate(CircleLand, new Vector3(5f, 0f-1f, -3.17f), Quaternion.Euler(0f, 70f, 0f), scenarioCircuit.transform);

                circuitTargets.Add(Instantiate(Cylinder, new Vector3(67.96819f, 6f - 1f, -3.70529f), Quaternion.Euler(0f, 0.95f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Ring, new Vector3(43.34064f, 7f, -11.24994f), Quaternion.Euler(90f, 70f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Ring, new Vector3(38.56079f, 7f, -13.01093f), Quaternion.Euler(90f, 70f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Ring, new Vector3(33.75945f, 7f, -14.86158f), Quaternion.Euler(90f, 70f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Cylinder, new Vector3(20.86407f, 6f - 1f, -15.62595f), Quaternion.Euler(180f, 135.95f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Ring, new Vector3(28.31402f, 7f, -6.700171f), Quaternion.Euler(90f, 70f + 180f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Arc, new Vector3(44.10566f, 0f-1f, -1.599268f), Quaternion.Euler(0f, 90f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Arc, new Vector3(49.23645f, 0f-1f, 0.2159138f), Quaternion.Euler(0f, 90f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Arc, new Vector3(54.57055f, 0f-1f, 2.227753f), Quaternion.Euler(0f, 90f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Cylinder, new Vector3(59.22807f, 6f - 1f, 12.04179f), Quaternion.Euler(0f, -44.05f, 0f), scenarioCircuit.transform));
                circuitTargets.Add(Instantiate(Tunnel, new Vector3(32.18615f, 6f+1f, 4.85722f), Quaternion.Euler(90f, 70f, 0f), scenarioCircuit.transform));

                // Asignamos cantidad de Gates a superar y sus indices
                totalLevelTargets = circuitTargets.Count;
                for (int i = 0; i < circuitTargets.Count; i++)
                {
                    GateTrigger waypoint = circuitTargets[i].GetComponentInChildren<GateTrigger>();
                    if (waypoint == null) waypoint = circuitTargets[i].AddComponent<GateTrigger>();
                    waypoint.waypointIndex = i;
                }

                scenarioCircuit.SetActive(true);
                break;

            case ConnectionConfig.Scenario.Easy:
                // Pared 1
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 11, rotationY: -17.05f, basePosition: new Vector3(3.55f, 0.5f-1f, -9.13f), floorCount: 12, wallNumber: 1);
                // Pared 2
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 2, rotationY: 72.82f, basePosition: new Vector3(14.30245f, 0.5f-1f, -2.404995f), floorCount: 12, wallNumber: 2);
                // Pared 3
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 29, rotationY: -17.05f, basePosition: new Vector3(27.6054f, 0.5f-1f, 1.758993f), floorCount: 12, wallNumber: 3);
                // Pared 4
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 25, rotationY: -18f, basePosition: new Vector3(46.34001f, 0.5f-1f, -0.571003f), floorCount: 12, wallNumber: 4);
                // Pared 5
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 23, rotationY: 72.82f, basePosition: new Vector3(27.50053f, 0.5f-1f, 1.284662f), floorCount: 12, wallNumber: 5);
                // Pared 6
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 13, rotationY: -17.05f, basePosition: new Vector3(42.99652f, 0.5f-1f, -10.8419f), floorCount: 12, wallNumber: 6);
                // Pared 7
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 11, rotationY: -17.05f, basePosition: new Vector3(18.4781f, 0.5f-1f, -11.27147f), floorCount: 12, wallNumber: 7);
                // Pared 8  
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 8, rotationY: 72.82f, basePosition: new Vector3(18.23631f, 0.5f-1f, -11.62051f), floorCount: 12, wallNumber: 8);
                // Pared 9
                BuildWall(scenarioMazeEasy.transform, middleWallCount: 6, rotationY: 72.82f, basePosition: new Vector3(56.8027f, 0.5f-1f, -7.272335f), floorCount: 12, wallNumber: 9);
                scenarioMazeEasy.SetActive(true);
                    break;

            case ConnectionConfig.Scenario.Hard:
                // Pared 1
                BuildWall(scenarioMazeHard.transform, middleWallCount: 10, rotationY: -17.05f, basePosition: new Vector3(11.53328f, 0.5f-1f, -2.84897f), floorCount: 12, wallNumber: 1);
                // Pared 2
                BuildWall(scenarioMazeHard.transform, middleWallCount: 11, rotationY: 72.82f, basePosition: new Vector3(11.366f, 0.5f-1f, -3.161f), floorCount: 12, wallNumber: 2);
                // Pared 3
                BuildWall(scenarioMazeHard.transform, middleWallCount: 4, rotationY: 72.82f, basePosition: new Vector3(20.74113f, 0.5f-1f, 5.597328f), floorCount: 12, wallNumber: 3);
                // Pared 4
                BuildWall(scenarioMazeHard.transform, middleWallCount: 20, rotationY: -17.05f, basePosition: new Vector3(18.21164f, 0.5f-1f, -22.09454f), floorCount: 12, wallNumber: 4);
                // Pared 5
                BuildWall(scenarioMazeHard.transform, middleWallCount: 13, rotationY: 72.82f, basePosition: new Vector3(24.18879f, 0.5f-1f, -5.123785f), floorCount: 12, wallNumber: 5);
                // Pared 6
                BuildWall(scenarioMazeHard.transform, middleWallCount: 3, rotationY: 72.82f, basePosition: new Vector3(38.79459f, 0.5f-1f, -16.05497f), floorCount: 12, wallNumber: 6);
                // Pared 7
                BuildWall(scenarioMazeHard.transform, middleWallCount: 11, rotationY: 72.82f, basePosition: new Vector3(31.34671f, 0.5f-1f, 8.861938f), floorCount: 12, wallNumber: 7);
                // Pared 8
                BuildWall(scenarioMazeHard.transform, middleWallCount: 19, rotationY: 72.82f, basePosition: new Vector3(41.71032f, 0.5f-1f, 12.01054f), floorCount: 12, wallNumber: 8);
                // Pared 9
                BuildWall(scenarioMazeHard.transform, middleWallCount: 9, rotationY: -17.05f, basePosition: new Vector3(37.82368f, 0.5f-1f, -10.37057f), floorCount: 12, wallNumber: 9);
                // Pared 10
                BuildWall(scenarioMazeHard.transform, middleWallCount: 9, rotationY: -17.05f, basePosition: new Vector3(46.22995f, 0.5f-1f, -1.740282f), floorCount: 12, wallNumber: 10);
                // Pared 11
                BuildWall(scenarioMazeHard.transform, middleWallCount: 27, rotationY: -17.05f, basePosition: new Vector3(48.45221f, 0.5f-1f, -12.35706f), floorCount: 12, wallNumber: 11);
                // Pared 12
                BuildWall(scenarioMazeHard.transform, middleWallCount: 3, rotationY: 72.82f, basePosition: new Vector3(58.25583f, 0.5f-1f, -4.378506f), floorCount: 12, wallNumber: 12);
                // Pared 13
                BuildWall(scenarioMazeHard.transform, middleWallCount: 16, rotationY: -17.05f, basePosition: new Vector3(52.61269f, 0.5f-1f, 10.55711f), floorCount: 12, wallNumber: 13);
                // Pared 14
                BuildWall(scenarioMazeHard.transform, middleWallCount: 13, rotationY: 72.82f, basePosition: new Vector3(62.09842f, 0.5f-1f, 12.93264f), floorCount: 12, wallNumber: 14);
                scenarioMazeHard.SetActive(true);
                break;

            case ConnectionConfig.Scenario.Search:
                Instantiate(TreeType1, new Vector3(21f, -0.1f - 1f, 0.7f), Quaternion.identity, scenarioSearch.transform);
                Instantiate(TreeType3Modified1, new Vector3(46.34401f, -0.1f - 1f, 9.111881f), Quaternion.Euler(0, 21f, 0), scenarioSearch.transform);
                Instantiate(TreeType3Modified2, new Vector3(29.74926f, -0.1f - 1f, -15.09937f), Quaternion.Euler(0, 208.454f, 0), scenarioSearch.transform);
                Instantiate(TreeType1, new Vector3(34.74667f, -0.1f - 1f, -17.05057f), Quaternion.identity, scenarioSearch.transform);
                Instantiate(TreeType1Modified, new Vector3(41.1f, -0.1f-1f, -15.16579f), Quaternion.identity, scenarioSearch.transform);
                Instantiate(TreeType1Modified, new Vector3(46.19419f, -0.1f-1f, -12.46974f), Quaternion.identity, scenarioSearch.transform);
                Instantiate(Fountain, new Vector3(38.92875f, 0f-1f, -4.350088f), Quaternion.identity, scenarioSearch.transform);
                Instantiate(Gazebo, new Vector3(62.8f, 0f-1f, 12f), Quaternion.Euler(0, -154.28f, 0), scenarioSearch.transform);
                Instantiate(Bench, new Vector3(40.42196f, 0f-1f, -11.40582f), Quaternion.Euler(0, -9.05f, 0), scenarioSearch.transform);
                Instantiate(Bench, new Vector3(38.92283f, 0f-1f, 2.530954f), Quaternion.Euler(0, 180.95f, 0), scenarioSearch.transform);
                Instantiate(Bench, new Vector3(31.985f, 0f-1f, -3.914904f), Quaternion.Euler(0, 90.95f, 0), scenarioSearch.transform);
                Instantiate(Bench, new Vector3(45.63258f, 0f-1f, -3.571131f), Quaternion.Euler(0, 260.95f, 0), scenarioSearch.transform);
                Instantiate(Bench, new Vector3(44.54998f, 0f-1f, -8.543865f), Quaternion.Euler(0, -59.05f, 0), scenarioSearch.transform);
                Instantiate(Bench, new Vector3(33.84671f, 0f-1f, 0.3948231f), Quaternion.Euler(0, 130.95f, 0), scenarioSearch.transform);
                Instantiate(Bench, new Vector3(43.38337f, 0f-1f, 0.7167492f), Quaternion.Euler(0, 220.95f, 0), scenarioSearch.transform);
                Instantiate(Bench, new Vector3(35.14811f, 0f-1f, -10.38824f), Quaternion.Euler(0, 35.95f, 0), scenarioSearch.transform);
                Instantiate(Lamp, new Vector3(65.11864f, 0f - 1f, -5.722569f), Quaternion.identity, scenarioSearch.transform);
                Instantiate(Table, new Vector3(67f, 0f - 1f, -3f), Quaternion.Euler(0, -17.05f, 0), scenarioSearch.transform);
                Instantiate(Table, new Vector3(68.38045f, 0f - 1f, -6.8468f), Quaternion.Euler(0, -17.05f, 0), scenarioSearch.transform);
                Instantiate(Table, new Vector3(63.42261f, 0f - 1f, -8.484831f), Quaternion.Euler(0, -17.05f, 0), scenarioSearch.transform);
                Instantiate(Table, new Vector3(62.11896f, 0f - 1f, -4.472668f), Quaternion.Euler(0, -17.05f, 0), scenarioSearch.transform);
                Instantiate(SafeTower, new Vector3(7.43573f, 1.8f, -2.405746f), Quaternion.Euler(0, -108f, 0), scenarioSearch.transform);
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
                            float ringScale = obj.scale ?? 0.01f;

                            instantiatedObj.transform.localScale = new Vector3(
                                ringScale,
                                ringScale,
                                ringScale
                            );
                            Debug.Log(
                                $"RING SCALE: JSON={obj.scale}, " +
                                $"localScale={instantiatedObj.transform.localScale}, " +
                                $"lossyScale={instantiatedObj.transform.lossyScale}"
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
                            Vector3 cylinderPosition = position + new Vector3(0, 6f-1f, 0);
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
                            Vector3 basePosition = position + new Vector3(0, 0.5f-1f, 0);
                            int floorCount = obj.height.Value;
                            int wallCount = 0;
                            foreach (Transform child in scenarioCustom.transform)
                            {
                                if (child.name.StartsWith("Wall_"))
                                {
                                    wallCount++;
                                }
                            }
                            BuildWall(scenarioCustom.transform, middleWallCount, rotationY, basePosition, floorCount, wallNumber: wallCount + 1);
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
        // Limpiar los objetos de niveles anteriores (si hay)
        foreach (Transform child in scenarioDroneFlow.transform)
        {
            Destroy(child.gameObject);
        }

        // Nivel 5: se gestiona aparte porque mezcla gates (aquí) y gemas (GemaSpawner)
        if (level == 5)
        {
            LoadDroneFlowLevel5();
            return;
        }

        List<GameObject> currentLevelTargets = new List<GameObject>();

        switch (level)
        {
            case 1:
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(15.75526f, 4f, -5.571545f), Quaternion.Euler(90f, -18f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(17.62516f, 4f, -11.62338f), Quaternion.Euler(90f, -18f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(19.83f, 4f, -17.7557f), Quaternion.Euler(90f, -18, 0f), scenarioDroneFlow.transform));

                break;
            case 2:
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(26.7f, 4f, -19.2f), Quaternion.Euler(90f, 252f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(30f, 4f, -10.6f), Quaternion.Euler(90f, 162f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(28.1301f, 4f, -4.548166f), Quaternion.Euler(90f, 162f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(20.1f, 4f, 1.1f), Quaternion.Euler(90f, 72f, 0f), scenarioDroneFlow.transform));
                break;
            case 3:
                currentLevelTargets.Add(Instantiate(Square, new Vector3(21.15f, 2f, 1.35f), Quaternion.Euler(0f, 90f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Square, new Vector3(37.1f, 2f, 2.3f), Quaternion.Euler(0f, 90f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Square, new Vector3(54.4f, 2f, 1.5f), Quaternion.Euler(0f, 180f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Square, new Vector3(41.7f, 2f, -11.4f), Quaternion.Euler(0f, -90f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Square, new Vector3(25.6f, 2f, -20.3f), Quaternion.Euler(0f, -90f, 0f), scenarioDroneFlow.transform));
                break;

            case 4:
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(36.7f, 4f, -16f), Quaternion.Euler(90f, 252f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(41.9f, 4f, -10.7f), Quaternion.Euler(90f, 207f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(42.1f, 4f, -2.6f), Quaternion.Euler(90f, 162f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(37.2f, 4f, 2.9f), Quaternion.Euler(90f, 117f, 0f), scenarioDroneFlow.transform));
                currentLevelTargets.Add(Instantiate(Ring, new Vector3(29.98f, 4f, 4.35f), Quaternion.Euler(90f, 72f, 0f), scenarioDroneFlow.transform));
                break;

        }

        totalLevelTargets = currentLevelTargets.Count;
        for (int i = 0; i < currentLevelTargets.Count; i++)
        {
            GateTrigger waypoint = currentLevelTargets[i].GetComponent<GateTrigger>();
            if (waypoint == null)
            {
                waypoint = currentLevelTargets[i].AddComponent<GateTrigger>();
            }
            waypoint.waypointIndex = i;
        }

        scenarioDroneFlow.SetActive(true);
    }

    GameObject InstantiateDroneFlowGate(GameObject prefab, Vector3 pos, Quaternion rot, int waypointIndex)
    {
        GameObject obj = Instantiate(prefab, pos, rot, scenarioDroneFlow.transform);
        GateTrigger gate = obj.GetComponentInChildren<GateTrigger>();
        //if (gate == null) gate = obj.AddComponent<GateTrigger>();
        gate.waypointIndex = waypointIndex;
        return obj;
    }

    void LoadDroneFlowLevel5()
    {
        // ORDEN DEL NIVEL 5 
        // Cada objeto lleva su índice de orden. Los índices deben ir de 0 a (total-1), sin repetir. Los huecos que NO se pongan aquí son los que ocuparán las gemas.

        InstantiateDroneFlowGate(Ring, new Vector3(30f, 4f, 4f), Quaternion.Euler(90f, -18f, 0f), 0);
        InstantiateDroneFlowGate(Square, new Vector3(37.5f, 2f, 0.5f), Quaternion.Euler(0f, 90f, 0f), 2);
        InstantiateDroneFlowGate(Ring, new Vector3(44.6f, 4f, -3.8f), Quaternion.Euler(90f, -18f, 0f), 4);
        InstantiateDroneFlowGate(Square, new Vector3(41.7f, 2f, -11f), Quaternion.Euler(0f, -90f, 0f), 6);

        // Total de objetos ordenados del nivel = gates + gemas
        totalLevelTargets = 8; 

        // Pedimos a GemaSpawner que cree las gemas con sus índices
        GemaSpawner gemSpawner = FindFirstObjectByType<GemaSpawner>();
        if (gemSpawner != null)
            gemSpawner.SpawnDroneFlowLevel5Gems(scenarioDroneFlow.transform);
        else
            Debug.LogWarning("No se encontró GemaSpawner para las gemas del Nivel 5.");

        scenarioDroneFlow.SetActive(true);
    }

    void BuildWall(Transform mazeParent, int middleWallCount, float rotationY, Vector3 basePosition, int floorCount, int wallNumber)
    {
        int totalFloors = floorCount;

        GameObject wallParent = new GameObject($"Wall_{wallNumber}");
        wallParent.transform.SetParent(mazeParent);
        wallParent.transform.position = Vector3.zero;

        for (int j = 0; j < totalFloors; j++)
        {
            GameObject floorContainer = new GameObject($"Floor_{wallParent.transform.childCount + 1}");
            floorContainer.transform.SetParent(wallParent.transform);
            floorContainer.transform.localPosition = Vector3.zero;

            List<GameObject> walls = new List<GameObject>();
            walls.Add(Instantiate(WallLeft));
            for (int i = 0; i < middleWallCount; i++)
            {
                walls.Add(Instantiate(WallMiddle));
            }
            walls.Add(Instantiate(WallRight));

            foreach (GameObject wall in walls)
            {
                wall.transform.SetParent(floorContainer.transform);
            }

            float wallWidth = 1.0f;
            for (int i = 0; i < walls.Count; i++)
            {
                walls[i].transform.localPosition = Vector3.right * wallWidth * i;
            }

            floorContainer.transform.localPosition = new Vector3(0f, j, 0f);
        }

        wallParent.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        wallParent.transform.position = basePosition;
        // basePosition es la coordenada del punto donde empieza la pared, la base del primer "ladrillo" en la esquina inferior izquierda.
    }
}