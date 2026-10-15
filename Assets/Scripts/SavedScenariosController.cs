using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;
using UnityEngine.UI;

public class SavedScenariosController : MonoBehaviour
{
    [Header("Scenario List")]
    public Transform content;
    public GameObject scenarioItemPrefab;
    public Button loadButton;
    public Button deleteButton;

    private List<string> savedScenarioNames;
    private string selectedScenarioName;

    void Start()
    {
        LoadScenarioList();
        loadButton.onClick.AddListener(OnLoadClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    void LoadScenarioList()
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
    void SelectScenario(string scenarioName)
    {
        selectedScenarioName = scenarioName;

        Debug.Log("Escenario seleccionado: " + selectedScenarioName);
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
        ConnectionConfig.CustomHasStopwatch = data.hasStopwatch;
        ConnectionConfig.CustomHasCountdown = data.hasCountdown;
        ConnectionConfig.CustomCountdownTime = data.countdownTime;

        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.Custom;
        ConnectionConfig.CustomScenarioName = selectedScenarioName;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }
    void OnDeleteClicked()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        savedScenarioNames = ScenarioPersistence.GetSavedScenarioNames();

        if (string.IsNullOrEmpty(selectedScenarioName))
        {
            Debug.Log("No hay ningún escenario seleccionado.");
            return;
        }

        bool deleted = ScenarioPersistence.DeleteScenario(selectedScenarioName);

        if (deleted)
        {
            LoadScenarioList();
            selectedScenarioName = null;
        }
    }
    public void ClearSelection()
    {
        selectedScenarioName = null;
    }
}