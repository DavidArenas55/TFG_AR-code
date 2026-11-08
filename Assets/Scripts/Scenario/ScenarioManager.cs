using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    [Header("World Anchor")]
    public WorldAnchor worldAnchor;
    public GameObject scenarioDroneFlow;
    public GameObject scenarioCircuit;
    public GameObject scenarioMazeEasy;
    public GameObject scenarioMazeHard;
    public GameObject scenarioSearch;
    public GameObject scenarioCustom;

    [Header("Drone Flow Progress")]
    public int currentDroneFlowLevel = 1;
    public int totalLevelTargets = 0;
    public bool isSequential = true;

    [Header("Simulation Fences & Ground")]
    public GameObject fences;
    public GameObject ground;

    [Header("Scenario 0 & 1 Prefabs")]
    public GameObject CircleTakeOff; public GameObject CircleLand;
    public GameObject Ring;
    public GameObject Arc;
    public GameObject Square;
    public GameObject Cylinder;
    public GameObject Tunnel;

    [Header("Scenario 2 & 3 Prefabs")]
    public GameObject WallLeft;
    public GameObject WallRight;
    public GameObject WallMiddle;

    [Header("Scenario 4 Prefabs")]
    public GameObject TreeType1;
    public GameObject TreeType1Modified;
    public GameObject TreeType3;
    public GameObject TreeType3Modified1;
    public GameObject TreeType3Modified2;
    public GameObject Fountain;
    public GameObject Gazebo;
    public GameObject Bench;
    public GameObject Lamp;
    public GameObject Table;
    public GameObject SafeTower;

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
                totalLevelTargets = CustomScenario.Load(
                    scenarioCustom.transform,
                    worldAnchor,
                    ConnectionConfig.CustomScenarioObjects,
                    Ring,
                    Arc,
                    Tunnel,
                    Square,
                    Cylinder,
                    CircleTakeOff,
                    CircleLand,
                    WallLeft,
                    WallRight,
                    WallMiddle
                );

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