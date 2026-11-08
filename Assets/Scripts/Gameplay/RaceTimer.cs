using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RaceTimer : MonoBehaviour
{
    [Header("UI")]
    public Text timerText;
    public Text countdownText;
    public Button openResultsButton;
    public GameObject GameOver;
    public GameObject nextLevelText;
    public Button startButton;

    [Header("Referencias")]
    public Transform drone;
    private Transform finishPoint; 

    [Header("Configuración")]
    public Vector3 stopDistance = new Vector3(1f, 0.5f, 1f);
    public float countdownStartTime = 360f;  // 6 min

    private float stopwatchTime = 0f;
    private float countdownTime = 0f;
    private bool running = false;

    private bool useStopwatch = false;
    private bool useCountdown = false;

    private ScenarioManager scenarioManager;
    public int targetsCollected = 0;

    private float totalPlayTime = 0f;
    private bool ranOutOfTime = false;

    public GameObject explanationPanel;  
    public Text explanationText;
    public GameObject searchImages;

    public GameObject levelBox;          
    public Text levelText;               
    public int droneFlowTotalLevels = 5; 
    private int droneFlowLevelsCompleted = 0;

    private int lastShownLevel = -1;

    void Start()
    {
        nextLevelText.SetActive(false);
        GameOver.SetActive(false);
        openResultsButton.gameObject.SetActive(false);
        openResultsButton.onClick.AddListener(OnOpenResultsClicked);

        scenarioManager = FindFirstObjectByType<ScenarioManager>();
        targetsCollected = 0;

        // Determinar qué temporizadores usar según el escenario activo
        switch (ConnectionConfig.SelectedScenario)
        {
            case ConnectionConfig.Scenario.Search:
            case ConnectionConfig.Scenario.Circuit:
                if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit) FindFinishPoint();
                useStopwatch = true;
                break;

            case ConnectionConfig.Scenario.DroneFlow:
                useCountdown = true;
                SetDroneFlowTime(scenarioManager.currentDroneFlowLevel);
                break;

            case ConnectionConfig.Scenario.Easy:
            case ConnectionConfig.Scenario.Hard:
                useCountdown = true;
                countdownTime = countdownStartTime;
                break;

            case ConnectionConfig.Scenario.Custom:
                useStopwatch = ConnectionConfig.CustomHasStopwatch;
                useCountdown = ConnectionConfig.CustomHasCountdown;

                if (!useStopwatch && !useCountdown)
                {
                    useStopwatch = true; // Forzamos stopwatch para circuitos por defecto
                }

                if (useCountdown)
                {
                    countdownTime = ConnectionConfig.CustomCountdownTime;
                }
                break;
        }

        // Apagamos ambos primero 
        if (timerText != null) timerText.transform.parent.gameObject.SetActive(false);
        if (countdownText != null) countdownText.transform.parent.gameObject.SetActive(false);

        // Los encendemos SOLO si les corresponde a nivel lógico real.
        if (useStopwatch && timerText != null) timerText.transform.parent.gameObject.SetActive(true);
        if (useCountdown && countdownText != null) countdownText.transform.parent.gameObject.SetActive(true);

        // Prevenir solapamiento visual
        if (useStopwatch && useCountdown && timerText != null && countdownText != null)
        {
            RectTransform cdRect = countdownText.transform.parent.GetComponent<RectTransform>();
            if (cdRect != null)
            {
                cdRect.anchoredPosition = new Vector2(cdRect.anchoredPosition.x, cdRect.anchoredPosition.y - 50f);
            }
        }

        running = false;  // No arrancamos hasta pulsar Start

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(StartRace);
        }

        // Mostrar el tiempo inicial en pantalla aunque aún no corra
        if (useCountdown && countdownText != null)
            countdownText.text = FormatTime(countdownTime);
        if (useStopwatch && timerText != null)
            timerText.text = FormatTime(0f);

        ShowScenarioExplanation();
    }

    public void StartRace()
    {
        if (running) return;          
        running = true;
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        if (explanationPanel != null)
            explanationPanel.SetActive(false);
    }

    void Update()
    {
        UpdateLevelDisplay();

        if (!running) return;

        totalPlayTime += Time.deltaTime;

        // Lógica de Countdown (resta)
        if (useCountdown)
        {
            countdownTime -= Time.deltaTime;

            if (countdownTime <= 0)
            {
                countdownTime = 0;
                Debug.Log("¡TIME OVER!");
                running = false;
                ranOutOfTime = true;
                openResultsButton.gameObject.SetActive(true);
                GameOver.SetActive(true);
            }

            if (countdownText != null)
            {
                countdownText.text = FormatTime(countdownTime);

                // Cambiar el color basado en el tiempo restante
                if (countdownTime <= 30f)
                {
                    countdownText.color = Color.red;
                }
                else if (countdownTime <= 60f)
                {
                    countdownText.color = Color.yellow;
                }
                else
                {
                    countdownText.color = Color.white; // Color por defecto si queda más de 1 minuto
                }
            }
        }

        // Lógica de Stopwatch (suma)
        if (useStopwatch)
        {
            stopwatchTime += Time.deltaTime;
            if (timerText != null) timerText.text = FormatTime(stopwatchTime);

            if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit)
            {
                if (scenarioManager != null && targetsCollected >= scenarioManager.totalLevelTargets)
                {
                    CheckDistanceFinish();
                }
            }
        }
    }

    string FormatTime(float timeToFormat)
    {
        int minutes = Mathf.FloorToInt(timeToFormat / 60);
        int seconds = Mathf.FloorToInt(timeToFormat % 60);
        int milliseconds = Mathf.FloorToInt((timeToFormat * 1000) % 1000);
        return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }

    void CheckDistanceFinish()
    {
        if (drone == null || finishPoint == null) return;

        Vector3 distanceVector = drone.position - finishPoint.position;
        if (Mathf.Abs(distanceVector.x) <= stopDistance.x &&
            Mathf.Abs(distanceVector.y) <= stopDistance.y &&
            Mathf.Abs(distanceVector.z) <= stopDistance.z)
        {
            Debug.Log("Meta alcanzada 🏁 Tiempo final: " + FormatTime(useStopwatch ? stopwatchTime : countdownTime));
            running = false;
            openResultsButton.gameObject.SetActive(true);
            GameOver.SetActive(true);
        }
    }

    void FindFinishPoint()
    {
        GameObject finishObject = GameObject.FindGameObjectWithTag("FinishPoint");
        if (finishObject != null)
        {
            finishPoint = finishObject.transform;
            Debug.Log("Punto de meta detectado exitosamente para el Circuito.");
        }
        else
        {
            Debug.LogWarning("No se encontró ningún objeto con la etiqueta 'FinishPoint'.");
        }
    }

    public void StopTimer()
    {
        running = false;
    }

    public void TriggerEndGame()
    {
        CaptureResults();

        SceneManagement sceneManager = FindFirstObjectByType<SceneManagement>();
        if (sceneManager != null)
        {
            sceneManager.EndGameAndLoadResults();
        }
        else
        {
            Debug.LogError("No se encontró el componente SceneManagement en la escena.");
        }
    }

    public void OnOpenResultsClicked()
    {
        TriggerEndGame();
    }

    public void SetDroneFlowTime(int level)
    {
        targetsCollected = 0; // Reiniciamos contador
        switch (level)
        {
            case 1: countdownTime = 300f; break;
            case 2: countdownTime = 300f; break;
            case 3: countdownTime = 300f; break;
            case 4: countdownTime = 300f; break;
            case 5: countdownTime = 300f; break;
            default: countdownTime = 360f; break;
        }
    }

    void UpdateLevelDisplay()
    {
        // Solo en Drone Flow jugado en modo secuencial
        bool show = ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow
                    && scenarioManager != null
                    && scenarioManager.isSequential;

        GameObject box = (levelBox != null) ? levelBox
                       : (levelText != null ? levelText.gameObject : null);
        if (box != null && box.activeSelf != show)
            box.SetActive(show);

        if (!show || levelText == null) return;

        int current = scenarioManager.currentDroneFlowLevel;
        if (current != lastShownLevel)   // solo reescribe el texto si cambió el nivel
        {
            lastShownLevel = current;
            levelText.text = $"{current}/{droneFlowTotalLevels}";
        }
    }

    IEnumerator NextLevelNotify()
    {

        if (nextLevelText != null)
            nextLevelText.SetActive(true);
        {
            nextLevelText.GetComponent<Text>().text = "Level Completed!"; // Mostrar el texto

            yield return new WaitForSeconds(3f); // Esperar 3 segundos

            nextLevelText.GetComponent<Text>().text = ""; // Ocultar el texto
            nextLevelText.SetActive(false);
        }
    }

    public void ValidateWaypoint(int waypointIndex)
    {
        if (!running) return;

        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow || 
            ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit ||
            ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom)
        {
            if (waypointIndex == targetsCollected)
            {
                targetsCollected++;
                Debug.Log($"✅ Objetivo {targetsCollected} de {scenarioManager.totalLevelTargets} cruzado.");

                if (targetsCollected >= scenarioManager.totalLevelTargets)
                {
                    if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow)
                    {
                        if (!scenarioManager.isSequential)
                        {
                            running = false;
                            openResultsButton.gameObject.SetActive(true);
                            GameOver.SetActive(true);
                        }
                        else
                        {
                            droneFlowLevelsCompleted++;
                            StartCoroutine(NextLevelNotify());
                            scenarioManager.AdvanceToNextLevel();

                        }
                    }
                    else if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit)
                    {
                        Debug.Log($"🏁 CIRCUITO COMPLETADO en {FormatTime(useStopwatch ? stopwatchTime : countdownTime)}");
                    }
                    else if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom)
                    {
                        if (!ConnectionConfig.CustomHasStopwatch)
                        {
                            CheckCustomWinCondition();
                        }
                    }
                }
            }
        }
    }

    // Comprueba las condiciones de victoria específicas de un escenario Custom y que hacen que la partida acabe
    public void CheckCustomWinCondition()
    {
        if (ConnectionConfig.SelectedScenario != ConnectionConfig.Scenario.Custom) return;

        // Si tenemos un End Object termina la partida destruyendolo.
        if (ConnectionConfig.CustomHasStopwatch) return;

        bool circuitCompleted = targetsCollected >= scenarioManager.totalLevelTargets;
        bool scoreCompleted = GemProximityDestroy.gemsCollected >= GemaSpawner.numberOfCustomScoreGems;

        if (circuitCompleted && scoreCompleted)
        {
            Debug.Log($"🏁 CUSTOM ESCENARIO COMPLETADO. Circuitos: {targetsCollected}/{scenarioManager.totalLevelTargets} | Gemas: {GemProximityDestroy.gemsCollected}/{GemaSpawner.numberOfCustomScoreGems}");
            running = false;
            openResultsButton.gameObject.SetActive(true);
            GameOver.SetActive(true);
        }
    }

    public void CaptureResults()
    {
        GameResults.Scenario = ConnectionConfig.SelectedScenario;
        GameResults.CompletionTime = totalPlayTime;
        GameResults.TimeWasCountdown = useCountdown;
        GameResults.RanOutOfTime = ranOutOfTime;

        // gates atravesadas.
        GameResults.TargetsPassed = targetsCollected;
        GameResults.TargetsTotal = (scenarioManager != null) ? scenarioManager.totalLevelTargets : 0;

        // Puntos (gemas de puntuación). El denominador depende del escenario.
        GameResults.Score = GemProximityDestroy.gemsCollected;
        GemaSpawner spawner = FindFirstObjectByType<GemaSpawner>();
        switch (ConnectionConfig.SelectedScenario)
        {
            case ConnectionConfig.Scenario.Search:
                GameResults.ScoreTotal = GemaSpawner.numberOfSearchGems;
                break;
            case ConnectionConfig.Scenario.Custom:
                GameResults.ScoreTotal = GemaSpawner.numberOfCustomScoreGems;
                break;
            case ConnectionConfig.Scenario.Easy:
            case ConnectionConfig.Scenario.Hard:
                // Los laberintos generan gemas de puntuación aleatorias (numberOfGems), igual que muestra el contador en pantalla durante el juego.
                GameResults.ScoreTotal = (spawner != null) ? spawner.numberOfGems : 0;
                break;
            default:
                // Circuit / DroneFlow no puntúan por gemas (Points desactivado).
                GameResults.ScoreTotal = 0;
                break;
        }

        // Info extra para Drone Flow.
        if (scenarioManager != null)
        {
            GameResults.DroneFlowLevel = scenarioManager.currentDroneFlowLevel;
            GameResults.DroneFlowSequential = scenarioManager.isSequential;
            GameResults.DroneFlowLevelsCompleted = droneFlowLevelsCompleted;
            GameResults.DroneFlowTotalLevels = droneFlowTotalLevels;
        }
    }

    void ShowScenarioExplanation()
    {
        if (explanationPanel == null) return;

        if (searchImages != null) searchImages.SetActive(false);

        string explanation;
        switch (ConnectionConfig.SelectedScenario)
        {
            case ConnectionConfig.Scenario.Easy:
                explanation = "EASY MAZE:\n\nFly through the maze to find the RED GEM and on your way collect as many pink gems as you can before time runs out. Crash against a gem to pick it up. You have 6 minutes!\n\nFly in LOITER mode, so the walls of the maze will be detected as exclusion geofences and you won't crash against them.";
                break;
            case ConnectionConfig.Scenario.Hard:
                explanation = "HARD MAZE:\n\nNavigate through a tougher maze to find the RED GEM and on your way collect as many pink gems as possible before time runs out. Crash against a gem to pick it up. You have 6 minutes!\n\nFly in LOITER mode, so the walls of the maze will be detected as exclusion geofences and you won't crash against them.";
                break;
            case ConnectionConfig.Scenario.Circuit:
                explanation = "CIRCUIT:\n\nBefore you start,place the drone near the corner of the DronLab that is the most close to the Southwest.\n\nFly through all the gates in order as fast as you can and reach the landing 'H' for the time to stop. Try your best time!\n\nIf you crash, a red frame will appear.\n\nIf the gate blinks GREEN, you are on the right path.If the gate blinks RED, you are about to crash against it!If the gate blinks YELLOW, you are about to pass through the wrong gate or you are entering the gate on the wrong side.\n\nClick the button 'See Suggested Path' to see the right path you should follow.\n\nClick the button 'See Real Path' to see the path you have done so far.";
                break;
            case ConnectionConfig.Scenario.Search:
                explanation = "SEARCH:\n\nExplore the area and find and destroy all the hidden gems as fast as you can!. AIM at the gems and SHOOT them pressing Space on your keyboard or pressing the red button on your phone screen.\n\nThese are the gems you should find:";
                if (searchImages != null) searchImages.SetActive(true);
                break;
            case ConnectionConfig.Scenario.DroneFlow:
                explanation = "DRONE FLOW:\n\nBefore you start:\n\nIf you are in Sequential, Level 1, Level 3 or Level 5, place the drone near the corner of the DronLab that is the most close to the Northwest.\n\nIf you are in Level 2 or Level 4, place the drone near the corner of the DronLab that is the most close to the Southwest.\n\nFly through the gates in order before time runs out!\n\nIf you crash, a red frame will appear.\n\nIf the gate blinks GREEN, you are on the right path.If the gate blinks RED, you are about to crash against it!If the gate blinks YELLOW, you are about to pass through the wrong gate or you are entering the gate on the wrong side.\n\nWhen you reach LEVEL 5, each time you pass through a gate you will see a gem on your sight which you will have to AIM and SHOOT pressing Space on your keyboard or pressing the red button on your phone screen.";
                break;
            default:
                // Custom u otros: no mostramos cuadro
                explanationPanel.SetActive(false);
                return;
        }

        if (explanationText != null) explanationText.text = explanation;
        explanationPanel.SetActive(true);
    }
}