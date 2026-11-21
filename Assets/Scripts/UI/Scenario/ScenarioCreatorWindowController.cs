using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioCreatorWindowController : MonoBehaviour
{
    public GameObject createScenarioWindow;
    public Button backButton;
    public ScenarioCreatorController scenarioCreatorController;
    private List<ScenarioObjectData> previousObjects;
    private ConnectionConfig.CoordinateSystem previousCoordinateSystem;

    private bool isCreating = false;

    public event System.Action Closed;

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(Close);

        if (createScenarioWindow != null)
            createScenarioWindow.SetActive(false);
    }

    public void Open()
    {
        if (isCreating)
            return;

        // Conservar los datos del escenario seleccionado.
        previousObjects = ConnectionConfig.CustomScenarioObjects;
        previousCoordinateSystem = ConnectionConfig.CustomCoordinateSystem;

        // Preparar un escenario nuevo sin modificar el anterior.
        ConnectionConfig.CustomScenarioObjects =
            new List<ScenarioObjectData>();

        ConnectionConfig.CustomCoordinateSystem =
            ConnectionConfig.CoordinateSystem.GPS;

        isCreating = true;

        if (scenarioCreatorController != null)
            scenarioCreatorController.ResetCreator();

        if (createScenarioWindow != null)
            createScenarioWindow.SetActive(true);
    }

    public void Close()
    {
        if (isCreating)
        {
            // Cancelar la creación y recuperar los datos anteriores.
            ConnectionConfig.CustomScenarioObjects = previousObjects;
            ConnectionConfig.CustomCoordinateSystem = previousCoordinateSystem;

            isCreating = false;
            previousObjects = null;
        }

        if (createScenarioWindow != null)
            createScenarioWindow.SetActive(false);

        Closed?.Invoke();
    }

    public void ConfirmSaved()
    {
        // El nuevo escenario sustituye al anterior.
        isCreating = false;
        previousObjects = null;

        if (createScenarioWindow != null)
            createScenarioWindow.SetActive(false);

        Closed?.Invoke();
    }
}