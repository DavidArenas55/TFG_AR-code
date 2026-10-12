using UnityEngine;
using UnityEngine.UI;


public class ResultsDisplay : MonoBehaviour
{
    [Header("Siempre visibles")]
    public Text titleText;     
    public Text statusText;    
    public Text timeText;      

    [Header("Puntos (escenarios con gemas de puntuación: Search / Custom)")]
    public GameObject pointsRow;  
    public Text pointsText;

    [Header("Obstáculos / Puertas (Circuit / DroneFlow / Custom)")]
    public GameObject gatesRow;
    public Text gatesText;

    [Header("Nivel (solo Drone Flow)")]
    public GameObject levelRow;
    public Text levelText;

    void Start()
    {
        // Título con el nombre del escenario (siempre)
        if (titleText != null)
            titleText.text = GetScenarioName(GameResults.Scenario);

        // Estado: completado o sin tiempo (siempre)
        if (statusText != null)
        {
            if (GameResults.RanOutOfTime)
            {
                statusText.text = "¡Time Over!";
                statusText.color = Color.red;
            }
            else
            {
                statusText.text = "¡Game finished!";
                statusText.color = new Color(0.2f, 0.8f, 0.2f);
            }
        }

        // Tiempo de la partida (siempre)
        if (timeText != null)
            timeText.text = "Time: " + FormatTime(GameResults.CompletionTime);

        // Puntos: solo si el escenario tenía gemas de puntuación
        bool showPoints = GameResults.ScoreTotal > 0;
        if (pointsRow != null) pointsRow.SetActive(showPoints);
        if (showPoints && pointsText != null)
            pointsText.text = $"Points: {GameResults.Score} / {GameResults.ScoreTotal}";

        // gates solo si el escenario las tenía
        bool showGates = GameResults.TargetsTotal > 0;
        if (gatesRow != null) gatesRow.SetActive(showGates);
        if (showGates && gatesText != null)
            gatesText.text = $"Gates crossed: {GameResults.TargetsPassed} / {GameResults.TargetsTotal}";

        // Nivel, solo Drone Flow
        bool showLevel = GameResults.Scenario == ConnectionConfig.Scenario.DroneFlow;
        if (levelRow != null) levelRow.SetActive(showLevel);
        if (showLevel && levelText != null)
        {
            levelText.text = GameResults.DroneFlowSequential
                ? $"Levels completed: {GameResults.DroneFlowLevelsCompleted}/{GameResults.DroneFlowTotalLevels}"
                : $"Level played: {GameResults.DroneFlowLevel}";
        }
    }

    string GetScenarioName(ConnectionConfig.Scenario s)
    {
        switch (s)
        {
            case ConnectionConfig.Scenario.DroneFlow: return "Drone Flow";
            case ConnectionConfig.Scenario.Circuit: return "Practice Circuit";
            case ConnectionConfig.Scenario.Easy: return "Easy Maze";
            case ConnectionConfig.Scenario.Hard: return "Hard Maze";
            case ConnectionConfig.Scenario.Search: return "Search Gems";
            case ConnectionConfig.Scenario.Custom: return "Custom Scenario";
            default: return "Results";
        }
    }

    string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60);
        int seconds = Mathf.FloorToInt(t % 60);
        int milliseconds = Mathf.FloorToInt((t * 1000) % 1000);
        return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }
}