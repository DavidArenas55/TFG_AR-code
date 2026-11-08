using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SavedScenariosController : MonoBehaviour
{
    [Header("Scenario List")]
    public Transform content;
    public GameObject scenarioItemPrefab;
    public Button loadButton;
    public Button deleteButton;

    [Header("Window")]
    public GameObject savedScenariosWindow;
    public Button backButton;

    private List<string> savedScenarioNames;
    private string selectedScenarioName;
    public event Action Closed;

    void Start()
    {
        loadButton.onClick.AddListener(OnLoadClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
        backButton.onClick.AddListener(OnBackClicked);

        savedScenariosWindow.SetActive(false);
    }
    public void Open()
    {
        ClearScenarioList();
        LoadScenarioList();
        savedScenariosWindow.SetActive(true);
    }
    public void LoadScenarioList()
    {
        savedScenarioNames = ScenarioPersistence.GetSavedScenarioNames();

        foreach (string scenarioName in savedScenarioNames)
        {
            GameObject item = Instantiate(scenarioItemPrefab, content);

            TMP_Text scenarioText = item.GetComponentInChildren<TMP_Text>();

            if (scenarioText != null)
            {
                scenarioText.text = scenarioName;
            }

            Button button = item.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(() => SelectScenario(scenarioName));
            }
        }
    }
    public void ClearScenarioList()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }
    void SelectScenario(string scenarioName)
    {
        selectedScenarioName = scenarioName;
    }
    void OnLoadClicked()
    {
        if (string.IsNullOrEmpty(selectedScenarioName))
        {
            return;
        }

        CustomScenarioSaveData data =
            ScenarioPersistence.LoadScenario(selectedScenarioName);

        if (data == null)
        {
            return;
        }

        ConnectionConfig.CustomScenarioObjects = data.objects;
        ConnectionConfig.CustomCoordinateSystem = data.coordinateSystem;

        foreach (var obj in ConnectionConfig.CustomScenarioObjects)
        {
            Debug.Log($"CARGADO: id={obj.id}, type={obj.type}, circuitTarget={obj.circuitTarget}");
        }
        ConnectionConfig.CustomHasStopwatch = data.hasStopwatch;
        ConnectionConfig.CustomHasCountdown = data.hasCountdown;
        ConnectionConfig.CustomCountdownTime = data.countdownTime;

        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.Custom;
        ConnectionConfig.CustomScenarioName = selectedScenarioName;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }
    void OnDeleteClicked()
    {
        if (string.IsNullOrEmpty(selectedScenarioName))
        {
            Debug.Log("No hay ningún escenario seleccionado.");
            return;
        }

        bool deleted = ScenarioPersistence.DeleteScenario(selectedScenarioName);

        if (deleted)
        {
            ClearScenarioList();
            LoadScenarioList();

            if (ConnectionConfig.CustomScenarioName == selectedScenarioName)
            {
                ConnectionConfig.CustomScenarioName = "";
                ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.None;
                ConnectionConfig.CustomCoordinateSystem = ConnectionConfig.CoordinateSystem.GPS;
            }

            selectedScenarioName = null;
        }
    }
    public void ClearSelection()
    {
        selectedScenarioName = null;
    }
    void OnBackClicked()
    {
        ClearSelection();
        savedScenariosWindow.SetActive(false);
        Closed?.Invoke();
    }
}