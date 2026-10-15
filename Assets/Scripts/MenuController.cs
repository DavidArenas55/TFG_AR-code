using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private bool isUpdatingToggles = false;

    [Header("UI")]
    public Toggle simulationToggle; public Toggle productionToggle; public InputField comInputField; public Text debugText;

    public Button connectButton; public Button playButton;

    [Header("Scenario Creator")]
    public Button CreateScenarioButton;
    public GameObject CreateScenarioWindow;
    public Button SavedSenariosButton;
    public TMP_InputField scenarioNameInputField;
    public Toggle ringToggle; public Toggle arcToggle; public Toggle cylinderToggle; public Toggle tunnelToggle; public Toggle squareToggle; public Toggle wallToggle;
    public Toggle HtakeoffToggle; public Toggle HlandToggle;
    public Toggle bigGem1Toggle; public Toggle bigGem2Toggle;
    public Toggle gem1Toggle; public Toggle gem2Toggle; public Toggle gem3Toggle; public Toggle gem4Toggle; public Toggle gem5Toggle; public Toggle gem6Toggle; public Toggle gem7Toggle; public Toggle gem8Toggle;
    public InputField latitudeInputField; public InputField longitudeInputField; public InputField altitudeInputField; public InputField headingInputField;
    public InputField widthInputField; public InputField heightInputField;
    public Toggle clockwiseToggle; public Toggle counterclockwiseToggle;
    public Toggle scoreTargetToggle; public Toggle circuitTargetToggle; public Toggle endGameObjectToggle; public Toggle gemsRandomPositionToggle; public InputField numberOfGemsInputField;
    public Toggle stopwatchToggle; public Toggle countdownToggle; public InputField timeLimitInputField;
    public Toggle destroyShootToggle; public Toggle destroyCollideToggle;
    public Button addGameObjectButton;
    public Transform ScrollViewContent;
    public GameObject listItemPrefab;
    public Button removeGameObjectButton;
    public Text debugText2;
    public Button saveScenarioButton;
    public Button createScenarioBackButton;

    [Header("Saved Scenarios")]
    public Button savedScenariosBackButton;
    public GameObject SavedScenariosWindow;
    public TMP_Text selectedCustomScenarioText;
    public SavedScenariosController savedScenariosController;

    [Header("Place on Map")]
    public Button placeOnMapButton;    
    public GameObject mapPanel;        
    public RectTransform mapRect;      
    public RectTransform marker;        
    public Button savePositionButton;   

    // 4 esquinas del dronLab 
    public double latTopLeft = 41.2764282, lonTopLeft = 1.9882138;
    public double latTopRight = 41.2766189, lonTopRight = 1.9890228;
    public double latBottomRight = 41.2763519, lonBottomRight = 1.9891436;
    public double latBottomLeft = 41.2761573, lonBottomLeft = 1.9883241;

    private bool hasMapPoint = false;
    private double selectedLat, selectedLon;

    public int id = 0;

    [Header("Scenarios")]
    public Toggle droneFlowToggle; public Dropdown droneFlowDropdown; public Toggle circuitToggle; public Toggle easyToggle; public Toggle hardToggle; public Toggle searchToggle;

    [Header("Connection Status")]
    public GameObject connectionYESImage; public GameObject connectionNOImage;

    private GameObject selectedListItem = null;
    private int selectedItemId = -1;
    private Color normalColor = new Color(1f, 1f, 1f, 0f); // Transparente
    private Color selectedColor = new Color(0.7f, 0.7f, 1f, 0.5f); // Tono resaltado

    void Start()
    {
        if (ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Production)
        {
            productionToggle.isOn = true;
            simulationToggle.isOn = false;
        }
        else
        {
            simulationToggle.isOn = true;
            productionToggle.isOn = false;
            ConnectionConfig.SelectedMode = ConnectionConfig.Mode.Simulation;
        }

        comInputField.text = ConnectionConfig.ComPort; comInputField.interactable = productionToggle.isOn;

        droneFlowToggle.isOn = false; circuitToggle.isOn = false; easyToggle.isOn = false; hardToggle.isOn = false; searchToggle.isOn = false;
        //Comentamos estas dos lineas para mantener es escenario carrgado
        //ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.None;
        //ConnectionConfig.ConnectRequested = false;

        simulationToggle.onValueChanged.AddListener(OnSimulationToggle);
        productionToggle.onValueChanged.AddListener(OnProductionToggle);
        connectButton.onClick.AddListener(OnConnectClicked);
        playButton.onClick.AddListener(OnPlayClicked);

        droneFlowDropdown.gameObject.SetActive(false);
        circuitToggle.onValueChanged.AddListener((val) => { OnCircuitSelected(val); if (val) HideDroneDropdown(); });
        easyToggle.onValueChanged.AddListener((val) => { OnEasySelected(val); if (val) HideDroneDropdown(); });
        hardToggle.onValueChanged.AddListener((val) => { OnHardSelected(val); if (val) HideDroneDropdown(); });
        searchToggle.onValueChanged.AddListener((val) => { OnSearchSelected(val); if (val) HideDroneDropdown(); });
        droneFlowToggle.onValueChanged.AddListener(OnDroneFlowSelected);

        CreateScenarioWindow.SetActive(false);
        CreateScenarioButton.onClick.AddListener(OnCreateScenarioClicked);

        SavedScenariosWindow.SetActive(false);
        SavedSenariosButton.onClick.AddListener(onSavedScenariosClicked);

        InitInputsState();

        ringToggle.onValueChanged.AddListener((val) => { if (val) OnStandardObstacleSelected(ringToggle); });
        arcToggle.onValueChanged.AddListener((val) => { if (val) OnStandardObstacleSelected(arcToggle); });
        tunnelToggle.onValueChanged.AddListener((val) => { if (val) OnStandardObstacleSelected(tunnelToggle); });
        squareToggle.onValueChanged.AddListener((val) => { if (val) OnStandardObstacleSelected(squareToggle); });
        cylinderToggle.onValueChanged.AddListener((val) => { if (val) OnCylinderSelected(); });
        HtakeoffToggle.onValueChanged.AddListener((val) => { if (val) OnHTakeOffSelected(); });
        HlandToggle.onValueChanged.AddListener((val) => { if (val) OnHLand(); });
        wallToggle.onValueChanged.AddListener((val) => { if (val) OnWallSelected(); });

        bigGem1Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(bigGem1Toggle); });
        bigGem2Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(bigGem2Toggle); });
        gem1Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(gem1Toggle); });
        gem2Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(gem2Toggle); });
        gem3Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(gem3Toggle); });
        gem4Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(gem4Toggle); });
        gem5Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(gem5Toggle); });
        gem6Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(gem6Toggle); });
        gem7Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(gem7Toggle); });
        gem8Toggle.onValueChanged.AddListener((val) => { if (val) OnGemSelected(gem8Toggle); });

        clockwiseToggle.onValueChanged.AddListener(OnClockwiseSelected);
        counterclockwiseToggle.onValueChanged.AddListener(OnCounterclockwiseSelected);
        destroyShootToggle.onValueChanged.AddListener(OnShootSelected);
        destroyCollideToggle.onValueChanged.AddListener(OnCollideSelected);

        endGameObjectToggle.onValueChanged.AddListener(OnEndGameObjectChanged);
        gemsRandomPositionToggle.onValueChanged.AddListener(OnGemsRandomPositionChanged);
        countdownToggle.onValueChanged.AddListener((val) => { timeLimitInputField.interactable = val; });

        addGameObjectButton.onClick.AddListener(OnAddGameObjectClicked);
        removeGameObjectButton.onClick.AddListener(OnRemoveGameObjectClicked);
        saveScenarioButton.onClick.AddListener(OnSaveClicked);
        createScenarioBackButton.onClick.AddListener(onCreateScenarioBackClicked);

        savedScenariosBackButton.onClick.AddListener(onSavedScenariosBackClicked);

        if (placeOnMapButton != null) placeOnMapButton.onClick.AddListener(OpenMap);
        if (savePositionButton != null) savePositionButton.onClick.AddListener(SaveMapPosition);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (marker != null) marker.gameObject.SetActive(false);

        UpdateSelectedCustomScenarioText();
        ValidatePlay();
    }

    void InitInputsState()
    {
        // Enviar nulo apagará a todos los toggles visuales
        DeselectAllExcluding("None", null);

        countdownToggle.SetIsOnWithoutNotify(false);
        countdownToggle.interactable = true;
        timeLimitInputField.interactable = countdownToggle.isOn;
        stopwatchToggle.SetIsOnWithoutNotify(false);
        stopwatchToggle.interactable = false;
    }

    void OnSimulationToggle(bool on)
    {
        if (on)
        {
            productionToggle.isOn = false;

            ConnectionConfig.SelectedMode = ConnectionConfig.Mode.Simulation;
            comInputField.interactable = false;
            InvalidateConnect();
            ValidatePlay();
        }
        else if (!productionToggle.isOn)
        {
            simulationToggle.isOn = true;
        }
    }

    void OnProductionToggle(bool on)
    {
        if (on)
        {
            simulationToggle.isOn = false;
            ConnectionConfig.SelectedMode = ConnectionConfig.Mode.Production;
            comInputField.interactable = true;
            InvalidateConnect();
            ValidatePlay();
        }
        else if (!simulationToggle.isOn)
        {
            // Forzar a quedarse marcado si se intenta desmarcar y el otro no está activo
            productionToggle.isOn = true;
        }
    }

    void OnCreateScenarioClicked()
    {
        droneFlowToggle.isOn = false; circuitToggle.isOn = false; easyToggle.isOn = false; hardToggle.isOn = false; searchToggle.isOn = false;
        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.None;
        ValidatePlay();
        CreateScenarioWindow.SetActive(true);
    }

    void AddGameObjectToList(int id, string type, double latitude, double longitude, double altitude, double heading, int? width, int? height, string rotation, bool? scoreTarget, bool? circuitTarget, bool? endGameObject, bool? gemsRandomPosition, int? numberOfGems, bool? stopwatchObject, string destructionMethod)
    {
        ScenarioObjectData newObject = new ScenarioObjectData
        {
            id = id,
            type = type,
            latitude = latitude,
            longitude = longitude,
            altitude = altitude,
            heading = heading,
            width = width,
            height = height,
            rotation = rotation,
            scoreTarget = scoreTarget,
            circuitTarget = circuitTarget,
            endGameObject = endGameObject,
            gemsRandomPosition = gemsRandomPosition,
            numberOfGems = numberOfGems,
            stopwatchObject = stopwatchObject,
            destructionMethod = destructionMethod
        };

        ConnectionConfig.CustomScenarioObjects.Add(newObject);
    }

    bool HasValidCoordinates()
    {
        if (debugText2 != null) debugText2.text = "";

        // Verificación de comas (cultura español), bloqueando puntos de forma obligatoria
        string latStr = latitudeInputField.text;
        string lonStr = longitudeInputField.text;
        string altStr = altitudeInputField.text;
        string headStr = headingInputField.text;

        if (latStr.Contains(".") || lonStr.Contains(".") || altStr.Contains(".") || headStr.Contains("."))
        {
            if (debugText2 != null)
            {
                debugText2.text = "Error: Use coma ',' for decimals in coordinates, not points '.'.\nExample: 41,215";
                debugText2.color = Color.red;
            }
            return false;
        }
        return true;
    }

    void OnAddGameObjectClicked()
    {
        string objectType = GetSelectedObjectType();

        if (objectType == "Unknown")
        {
            if (debugText2 != null)
            {
                debugText2.text = "Error: You must select a Game Object first.";
                debugText2.color = Color.red;
            }
            return;
        }

        if (latitudeInputField.interactable && string.IsNullOrWhiteSpace(latitudeInputField.text))
        {
            if (debugText2 != null) { debugText2.text = "Error: The 'Latitude' field is obligatory."; debugText2.color = Color.red; }
            return;
        }
        if (longitudeInputField.interactable && string.IsNullOrWhiteSpace(longitudeInputField.text))
        {
            if (debugText2 != null) { debugText2.text = "Error: The 'Longitude' field is obligatory."; debugText2.color = Color.red; }
            return;
        }
        if (altitudeInputField.interactable && string.IsNullOrWhiteSpace(altitudeInputField.text))
        {
            if (debugText2 != null) { debugText2.text = "Error: The 'Altitude' field is obligatory."; debugText2.color = Color.red; }
            return;
        }
        if (headingInputField.interactable && string.IsNullOrWhiteSpace(headingInputField.text))
        {
            if (debugText2 != null) { debugText2.text = "Error: The 'Heading' field is obligatory."; debugText2.color = Color.red; }
            return;
        }

        // Comprobar campos opcionales específicos de ciertos objetos
        if (widthInputField.interactable && string.IsNullOrWhiteSpace(widthInputField.text))
        {
            if (debugText2 != null) { debugText2.text = "Error: The 'Width' field is obligatory for this object."; debugText2.color = Color.red; }
            return;
        }
        if (heightInputField.interactable && string.IsNullOrWhiteSpace(heightInputField.text))
        {
            if (debugText2 != null) { debugText2.text = "Error: The 'Height' field is obligatory for this object."; debugText2.color = Color.red; }
            return;
        }
        if (numberOfGemsInputField.interactable && string.IsNullOrWhiteSpace(numberOfGemsInputField.text))
        {
            if (debugText2 != null) { debugText2.text = "Error: Enter the number of gems."; debugText2.color = Color.red; }
            return;
        }

        if (!HasValidCoordinates()) return;
        // Campos obligatorios / comunes
        double lat = 0, lon = 0, alt = 0, heading = 0;

        if (latitudeInputField.interactable)
        {
            if (!double.TryParse(latitudeInputField.text, out lat))
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Latitude' contains letters o characters that are not valid."; debugText2.color = Color.red; }
                return;
            }
            if (lat < -90 || lat > 90)
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Latitude' must be a value between -90 and 90."; debugText2.color = Color.red; }
                return;
            }
        }

        if (longitudeInputField.interactable)
        {
            if (!double.TryParse(longitudeInputField.text, out lon))
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Longitude' contains letters o characters that are not valid."; debugText2.color = Color.red; }
                return;
            }
            if (lon < -180 || lon > 180)
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Longitude' must be a value between -180 and 180."; debugText2.color = Color.red; }
                return;
            }
        }

        if (altitudeInputField.interactable)
        {
            if (!double.TryParse(altitudeInputField.text, out alt))
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Altitude' contains letters o characters that are not valid."; debugText2.color = Color.red; }
                return;
            }
            if (alt > 15)
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Altitude'  cannot be more than 15 meters."; debugText2.color = Color.red; }
                return;
            }
        }

        if (headingInputField.interactable)
        {
            if (!double.TryParse(headingInputField.text, out heading))
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Heading' contains letters o characters that are not valid."; debugText2.color = Color.red; }
                return;
            }
        }

        int? width = null;
        if (widthInputField.interactable)
        {
            if (!int.TryParse(widthInputField.text, out int w))
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Width' must be a valid integer number (no letters)."; debugText2.color = Color.red; }
                return;
            }
            width = w;
        }

        int? height = null;
        if (heightInputField.interactable)
        {
            if (!int.TryParse(heightInputField.text, out int h))
            {
                if (debugText2 != null) { debugText2.text = "Error: 'Height' must be a valid integer number (no letters)."; debugText2.color = Color.red; }
                return;
            }
            if (objectType == "Wall" && h > 15)
            {
                if (debugText2 != null) { debugText2.text = "Error: The 'Height' (wall's height) cannot be over 15 meters."; debugText2.color = Color.red; }
                return;
            }
            height = h;
        }

        string rotation = null;
        if (clockwiseToggle.interactable || counterclockwiseToggle.interactable)
        {
            if (clockwiseToggle.isOn) rotation = "Clockwise";
            else if (counterclockwiseToggle.isOn) rotation = "Counterclockwise";
            else rotation = "None";
        }

        bool? isScoreTarget = scoreTargetToggle.interactable ? scoreTargetToggle.isOn : null;
        bool? isCircuitTarget = circuitTargetToggle.interactable ? circuitTargetToggle.isOn : null;
        bool? isEndGameObject = endGameObjectToggle.interactable ? endGameObjectToggle.isOn : null;
        bool? isGemsRandomPosition = gemsRandomPositionToggle.interactable ? gemsRandomPositionToggle.isOn : null;
        bool? isStopwatchObject = stopwatchToggle.isOn;

        int? numOfGems = null;
        if (numberOfGemsInputField.interactable)
        {
            if (!int.TryParse(numberOfGemsInputField.text, out int nGems))
            {
                if (debugText2 != null) { debugText2.text = "Error: The number of gems must be a numeric value."; debugText2.color = Color.red; }
                return;
            }

            if (nGems > 50)
            {
                if (debugText2 != null)
                {
                    debugText2.text = "Error: The maximum number of random gems is 50.";
                    debugText2.color = Color.red;
                }
                return;
            }
            numOfGems = nGems;
        }

        string destMethod = null;
        if (destroyShootToggle.interactable)
        {
            if (destroyShootToggle.isOn) destMethod = "Shoot";
            else if (destroyCollideToggle.isOn) destMethod = "Collide";
        }

        if (debugText2 != null) { debugText2.text = ""; debugText2.color = Color.white; }

        // ADVERTENCIA DE LÍMITE DE PAREDES PARA GEOFENCES
        if (objectType == "Wall")
        {
            int wallCount = 0;
            foreach (var obj in ConnectionConfig.CustomScenarioObjects)
            {
                if (obj.type == "Wall") wallCount++;
            }

            if (wallCount >= 15 && debugText2 != null)
            {
                debugText2.text = "";
                debugText2.text = "Warning: Limit of 15 geofences of walls exceeded.";
                debugText2.color = Color.yellow;
            }
        }

        AddGameObjectToList(id, objectType, lat, lon, alt, heading, width, height, rotation, isScoreTarget, isCircuitTarget, isEndGameObject, isGemsRandomPosition, numOfGems, isStopwatchObject, destMethod);

        RefreshListUI(); // Función que redibuja todo con sus IDs nuevos

        Debug.Log("Objeto agregado: " + objectType);
    }

    void RefreshListUI()
    {
        // 1. Limpiamos visualmente el panel 
        foreach (Transform child in ScrollViewContent)
        {
            Destroy(child.gameObject);
        }

        selectedListItem = null;
        selectedItemId = -1;

        // 2. Recreamos la interfaz basándonos en la lista de memoria
        for (int i = 0; i < ConnectionConfig.CustomScenarioObjects.Count; i++)
        {
            var obj = ConnectionConfig.CustomScenarioObjects[i];

            // FORZAMOS UN NUEVO ID SECUENCIAL
            obj.id = i;
            ConnectionConfig.CustomScenarioObjects[i] = obj; // Si 'ScenarioObjectData' es un struct, re-asignamos.

            GameObject listItem = Instantiate(listItemPrefab, ScrollViewContent);

            // Reconstruir el texto
            List<string> infoParts = new List<string> { $"ID: {obj.id}", $"{obj.type}" };

            if (obj.gemsRandomPosition != true)
            {
                infoParts.Add($"Pos: ({obj.latitude}, {obj.longitude}, {obj.altitude}m)");
                if (obj.heading != 0) infoParts.Add($"Head: {obj.heading}º");
            }

            if (obj.rotation != null && obj.rotation != "None") infoParts.Add($"Rot: {obj.rotation}");
            if (obj.width.HasValue) infoParts.Add($"W: {obj.width.Value}");
            if (obj.height.HasValue) infoParts.Add($"H: {obj.height.Value}");
            if (obj.scoreTarget == true) infoParts.Add("Score Target");
            if (obj.circuitTarget == true) infoParts.Add("Circuit Target");
            if (obj.endGameObject == true) infoParts.Add("End Game Obj");
            if (obj.gemsRandomPosition == true) infoParts.Add("Random Pos");
            if (obj.numberOfGems.HasValue) infoParts.Add($"Gems: {obj.numberOfGems.Value}");
            if (obj.destructionMethod != null) infoParts.Add($"Destroy: {obj.destructionMethod}");

            listItem.GetComponentInChildren<Text>().text = string.Join(" | ", infoParts);

            // Preparar botón para selección
            int currentId = obj.id;
            Button itemButton = listItem.GetComponent<Button>();
            if (itemButton == null) itemButton = listItem.AddComponent<Button>();
            itemButton.transition = Selectable.Transition.None;
            itemButton.onClick.AddListener(() => OnListItemSelected(listItem, currentId));
        }

        // Actualizamos nuestra ID global sumando 1 al total
        id = ConnectionConfig.CustomScenarioObjects.Count;
    }

    void OnListItemSelected(GameObject item, int itemId)
    {
        // Limpiamos los colores de todos los elements antes de marcar el nuevo
        foreach (Transform child in ScrollViewContent)
        {
            Image img = child.GetComponent<Image>();
            if (img != null) img.color = normalColor;
        }

        selectedListItem = item;
        selectedItemId = itemId;

        // Pintamos el seleccionado
        Image selectedImg = item.GetComponent<Image>();
        if (selectedImg != null) selectedImg.color = selectedColor;
    }

    void OnRemoveGameObjectClicked()
    {
        if (selectedItemId == -1)
        {
            if (debugText2 != null)
            {
                debugText2.text = "Error: Click on an object of the list below to eliminate it first.";
                debugText2.color = Color.red;
            }
            return;
        }

        // 1. Lo borramos de la memoria
        ConnectionConfig.CustomScenarioObjects.RemoveAll(obj => obj.id == selectedItemId);

        // 2. Volvemos a dibujar toda la interfaz gráfica desde 0 basándonos en la lista actualizada,
        // lo que automáticamente cerrará el hueco de los id lógicos
        RefreshListUI();

        if (debugText2 != null)
        {
            debugText2.text = "Object eliminated correctly.";
            debugText2.color = Color.green;
        }
    }

    string GetSelectedObjectType()
    {
        if (ringToggle.isOn) return "Ring";
        if (arcToggle.isOn) return "Arc";
        if (cylinderToggle.isOn) return "Cylinder";
        if (tunnelToggle.isOn) return "Tunnel";
        if (squareToggle.isOn) return "Square";
        if (wallToggle.isOn) return "Wall";
        if (HtakeoffToggle.isOn) return "HTakeoff";
        if (HlandToggle.isOn) return "HLand";
        if (bigGem1Toggle.isOn) return "BigGem1";
        if (bigGem2Toggle.isOn) return "BigGem2";
        if (gem1Toggle.isOn) return "Gem1";
        if (gem2Toggle.isOn) return "Gem2";
        if (gem3Toggle.isOn) return "Gem3";
        if (gem4Toggle.isOn) return "Gem4";
        if (gem5Toggle.isOn) return "Gem5";
        if (gem6Toggle.isOn) return "Gem6";
        if (gem7Toggle.isOn) return "Gem7";
        if (gem8Toggle.isOn) return "Gem8";
        return "Unknown";
    }

    void OnSaveClicked()
    {
        string scenarioName = scenarioNameInputField.text;

        if (string.IsNullOrWhiteSpace(scenarioName))
        {
            if (debugText2 != null)
            {
                debugText2.text = "Error: The 'Scenario Name' field is obligatory.";
                debugText2.color = Color.red;
            }
            return;
        }

        ConnectionConfig.CustomScenarioName = scenarioName;

        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.Custom;

        ConnectionConfig.CustomHasStopwatch = false;
        foreach (var obj in ConnectionConfig.CustomScenarioObjects)
        {
            if (obj.stopwatchObject == true)
                ConnectionConfig.CustomHasStopwatch = true;
        }

        ConnectionConfig.CustomHasCountdown = countdownToggle.isOn;

        if (countdownToggle.isOn)
        {
            if (float.TryParse(timeLimitInputField.text, out float t) && t > 0)
            {
                ConnectionConfig.CustomCountdownTime = t * 60f; // Pasar de minutos a segundos
                if (debugText2 != null) debugText2.text = ""; // Limpiar advertencias viejas
            }
            else
            {
                // Si estaba encendido pero el texto era inválido ("abc") o vacío
                if (debugText2 != null)
                {
                    debugText2.text = "Countdown time not valid. The default value of 6 minutes will be used.";
                    debugText2.color = Color.yellow;
                }
                ConnectionConfig.CustomCountdownTime = 360f;
            }
        }
        else
        {
            // Si el countdown NOT estaba encendido, no mostramos nada y dejamos el tiempo en 0.
            ConnectionConfig.CustomCountdownTime = 0f;
        }

        // Guardar el escenario
        ScenarioPersistence.SaveScenario(
            scenarioName,
            ConnectionConfig.CustomScenarioObjects,
            ConnectionConfig.CustomHasStopwatch,
            ConnectionConfig.CustomHasCountdown,
            ConnectionConfig.CustomCountdownTime
        );
        CreateScenarioWindow.SetActive(false);
        ValidatePlay();
    }

    void onCreateScenarioBackClicked()
    {
        savedScenariosController.ClearSelection();
        CreateScenarioWindow.SetActive(false);
    }

    void onSavedScenariosClicked()
    {
        SavedScenariosWindow.SetActive(true);
    }
    void onSavedScenariosBackClicked()
    {
        savedScenariosController.ClearSelection();
        SavedScenariosWindow.SetActive(false);
    }

    void DeselectAllExcluding(string groupType, Toggle activeToggle)
    {
        // Prevenir bucle infinito provocado por isOn = false
        if (isUpdatingToggles) return;
        isUpdatingToggles = true;

        Toggle[] allObjectToggles = new Toggle[] {
            ringToggle, arcToggle, tunnelToggle, squareToggle, cylinderToggle, wallToggle,
            HtakeoffToggle, HlandToggle, bigGem1Toggle, bigGem2Toggle, gem1Toggle, gem2Toggle,
            gem3Toggle, gem4Toggle, gem5Toggle, gem6Toggle, gem7Toggle, gem8Toggle
        };

        // Apagar todos menos el activo
        foreach (Toggle t in allObjectToggles)
        {
            if (t != activeToggle)
            {
                t.SetIsOnWithoutNotify(false);
            }
        }

        // Si se envió null (limpieza total en InitInputsState), apagamos el activo por si acaso
        if (activeToggle == null)
        {
            foreach (Toggle t in allObjectToggles) t.SetIsOnWithoutNotify(false);
        }

        // Estabilizamos configuraciones base de Input y Setup Toggles
        widthInputField.interactable = false; heightInputField.interactable = false;
        clockwiseToggle.interactable = false; clockwiseToggle.isOn = false; counterclockwiseToggle.interactable = false; counterclockwiseToggle.isOn = false;
        scoreTargetToggle.interactable = false; scoreTargetToggle.isOn = false;
        endGameObjectToggle.interactable = false; endGameObjectToggle.isOn = false;
        gemsRandomPositionToggle.interactable = false; gemsRandomPositionToggle.isOn = false; numberOfGemsInputField.interactable = false;
        destroyShootToggle.interactable = false; destroyShootToggle.isOn = false; destroyCollideToggle.interactable = false; destroyCollideToggle.isOn = false;
        circuitTargetToggle.isOn = false;

        // Si no hay ninguno activo, deshabilitamos las coordenadas también. 
        // Si hay uno activo, las habilitamos por defecto (se modificarán luego si es necesario)
        bool EnableCoords = (activeToggle != null);
        latitudeInputField.interactable = EnableCoords;
        longitudeInputField.interactable = EnableCoords;
        altitudeInputField.interactable = EnableCoords;
        headingInputField.interactable = EnableCoords;

        circuitTargetToggle.interactable = EnableCoords;

        // Aplicar configuraciones especiales según el grupo SOLO SI HAY ALGO ACTIVO
        if (activeToggle != null)
        {
            switch (groupType)
            {
                case "Standard":
                    // Setup básico + Permitir habilitar End Game Object
                    endGameObjectToggle.interactable = true;
                    break;

                case "Cylinder":
                    clockwiseToggle.interactable = true;
                    counterclockwiseToggle.interactable = true;
                    counterclockwiseToggle.isOn = true;
                    clockwiseToggle.isOn = false;
                    endGameObjectToggle.interactable = true;
                    break;

                case "HLand":
                    endGameObjectToggle.interactable = true;
                    endGameObjectToggle.isOn = true;
                    endGameObjectToggle.interactable = false; // Forzado a true
                    break;

                case "Wall":
                    widthInputField.interactable = true;
                    heightInputField.interactable = true;
                    circuitTargetToggle.interactable = false;
                    circuitTargetToggle.isOn = false;
                    break;

                case "Gem":
                    scoreTargetToggle.interactable = true;
                    endGameObjectToggle.interactable = true;
                    gemsRandomPositionToggle.interactable = true;

                    destroyShootToggle.interactable = true;
                    destroyCollideToggle.interactable = true;
                    destroyShootToggle.isOn = true; // Por defecto
                    break;
            }
        }

        // Liberar el seguro para que el siguiente clic funcione
        isUpdatingToggles = false;
    }

    void OnStandardObstacleSelected(Toggle caller)
    {
        DeselectAllExcluding("Standard", caller);
    }

    void OnCylinderSelected()
    {
        DeselectAllExcluding("Cylinder", cylinderToggle);

        clockwiseToggle.interactable = true; counterclockwiseToggle.interactable = true;
        counterclockwiseToggle.isOn = true; clockwiseToggle.isOn = false;
    }

    void OnClockwiseSelected (bool on)
    {
        if (!clockwiseToggle.interactable) return; // Ignorar si están deshabilitados

        if (on)
        {
            counterclockwiseToggle.isOn = false;
        }
        else if (!counterclockwiseToggle.isOn)
        {
            clockwiseToggle.isOn = true;
        }
    }

    void OnCounterclockwiseSelected (bool on)
    {
        if (!counterclockwiseToggle.interactable) return; // Ignorar si están deshabilitados

        if (on)
        {
            clockwiseToggle.isOn = false;
        }
        else if (!clockwiseToggle.isOn)
        {
            counterclockwiseToggle.isOn = true;
        }
    }

    void OnHTakeOffSelected()
    {
        DeselectAllExcluding("HTakeoff", HtakeoffToggle);
    }

    void OnHLand()
    {
        DeselectAllExcluding("HLand", HlandToggle);

        endGameObjectToggle.interactable = true;
        endGameObjectToggle.isOn = true;
        endGameObjectToggle.interactable = false;
    }

    void OnWallSelected()
    {
        DeselectAllExcluding("Wall", wallToggle);
        widthInputField.interactable = true; heightInputField.interactable = true;
        circuitTargetToggle.interactable = false; circuitTargetToggle.isOn = false;
    }

    void OnGemSelected(Toggle caller)
    {
        DeselectAllExcluding("Gem", caller);
        scoreTargetToggle.interactable = true;
        endGameObjectToggle.interactable = true;
        gemsRandomPositionToggle.interactable = true;
        destroyShootToggle.interactable = true;
        destroyCollideToggle.interactable = true;
        destroyShootToggle.isOn = true;
    }

    void OnGemsRandomPositionChanged(bool on)
    {
        numberOfGemsInputField.interactable = on;
        latitudeInputField.interactable = !on;
        longitudeInputField.interactable = !on;
        altitudeInputField.interactable = !on;
        headingInputField.interactable = !on;

        if (on)
        {
            endGameObjectToggle.isOn = false;
            endGameObjectToggle.interactable = false;

            circuitTargetToggle.isOn = false;
            circuitTargetToggle.interactable = false;

            // Reevaluar OnEndGameChanged al cambiar isOn a false
            OnEndGameObjectChanged(false);
        }
        else
        {
            endGameObjectToggle.interactable = true;
            circuitTargetToggle.interactable = true;
        }
    }

    void OnEndGameObjectChanged(bool on)
    {
        // El stopwatch estará encendido siempre que endGameObject lo esté y esté bloqueado temporalmente.
        if (on)
        {
            stopwatchToggle.isOn = true;
            stopwatchToggle.interactable = false; // Sólo informativo
        }
        else
        {
            stopwatchToggle.isOn = false;
            stopwatchToggle.interactable = false; // Por defecto apagado e inalterable si no es fin
        }
    }

    void OnShootSelected(bool on)
    {
        if (!destroyShootToggle.interactable) return;
        if (on) destroyCollideToggle.isOn = false;
        else if (!destroyCollideToggle.isOn) destroyShootToggle.isOn = true;
    }

    void OnCollideSelected(bool on)
    {
        if (!destroyCollideToggle.interactable) return;
        if (on) destroyShootToggle.isOn = false;
        else if (!destroyShootToggle.isOn) destroyCollideToggle.isOn = true;
    }

    void OnDroneFlowSelected(bool on)
    {
        if (!on) return;
        circuitToggle.isOn = false; easyToggle.isOn = false; hardToggle.isOn = false; searchToggle.isOn = false;
        droneFlowDropdown.gameObject.SetActive(true);
        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.DroneFlow;
        ValidatePlay();
    }

    void HideDroneDropdown()
    {
        droneFlowDropdown.gameObject.SetActive(false);
    }

    void OnCircuitSelected(bool on)
    {
        if (!on) return;
        droneFlowToggle.isOn = false; easyToggle.isOn = false; hardToggle.isOn = false; searchToggle.isOn = false;
        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.Circuit;
        ValidatePlay();
    }

    void OnEasySelected(bool on)
    {
        if (!on) return;
        droneFlowToggle.isOn = false; circuitToggle.isOn = false; hardToggle.isOn = false; searchToggle.isOn = false;
        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.Easy;
        ValidatePlay();
    }

    void OnHardSelected(bool on)
    {
        if (!on) return;
        droneFlowToggle.isOn = false; circuitToggle.isOn = false; easyToggle.isOn = false; searchToggle.isOn = false;
        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.Hard;
        ValidatePlay();
    }

    void OnSearchSelected(bool on)
    {
        if (!on) return;
        droneFlowToggle.isOn = false; circuitToggle.isOn = false; easyToggle.isOn = false; hardToggle.isOn = false;
        ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.Search;
        ValidatePlay();
    }

    void OnConnectClicked()
    {
        InvalidateConnect();

        if (DroneConnectionManager.Instance == null)
        {
            Debug.LogError("No existe DroneConnectionManager en la escena inicial.");
            return;
        }

        // Validación de COM solo en producción
        if (ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Production)
        {
            string port = comInputField.text.Trim().ToUpper();
            ConnectionConfig.ComPort = port;
        }

        // Intento real de conexión. En producción solo se valida el formato del puerto COM, no se abre el canal MAVLink todavía.
        bool ok = DroneConnectionManager.Instance.Verify(ConnectionConfig.SelectedMode, ConnectionConfig.ComPort);

        if (!ok)
        {
            Debug.LogError("CONNECT falló: " + DroneConnectionManager.Instance.LastError);
            debugText.text = "Error: " + DroneConnectionManager.Instance.LastError;
            debugText.color = Color.red;
            return;
        }

        connectionYESImage.SetActive(true);
        debugText.text = "Verification successful. You can click on PLAY.";
        debugText.color = Color.green;
        // Conectado OK => habilitar play
        ConnectionConfig.ConnectRequested = true;
        ValidatePlay();

        if (ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Simulation)
        {
            Debug.Log("VERIFY OK: Simulación (SITL)");
        }
        else
        {
            Debug.Log("VERIFY OK: Producción | " + ConnectionConfig.ComPort);
        }
    }

    void ValidatePlay()
    {
        bool modeSelected = simulationToggle.isOn || productionToggle.isOn;
        bool scenarioSelected = ConnectionConfig.SelectedScenario != ConnectionConfig.Scenario.None;
        playButton.interactable = ConnectionConfig.ConnectRequested && modeSelected && scenarioSelected;
    }

    void OnPlayClicked()
    {
        if (!ConnectionConfig.ConnectRequested)
        {
            Debug.LogWarning("PLAY bloqueado: primero CONNECT (y debe verificar bien).");
            return;
        }

        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow && droneFlowDropdown != null)
        {
            // Dropdown: Index 0 = Juego Secuencial, Index 1 = Nivel 1... etc.
            ConnectionConfig.DroneFlowLevel = droneFlowDropdown.value;
        }

        // Conexión real aquí: se abre el canal MAVLink justo antes de cargar la escena
        bool ok = DroneConnectionManager.Instance.Connect();

        if (!ok)
        {
            Debug.LogError("CONNECT falló al iniciar la conexión real: " + DroneConnectionManager.Instance.LastError);
            connectionYESImage.SetActive(false);
            debugText.text = "Error connecting: " + DroneConnectionManager.Instance.LastError; 
            debugText.color = Color.red;
            ConnectionConfig.ConnectRequested = false;
            playButton.interactable = false;
            return;
        }

        Debug.Log("CONNECT OK. Cargando GameScene...");
        SceneManager.LoadSceneAsync("GameScene");
    }

    void InvalidateConnect()
    {
        ConnectionConfig.ConnectRequested = false;
        playButton.interactable = false;
    }

    void OpenMap()
    {
        if (mapPanel != null) mapPanel.SetActive(true);
    }

    void CloseMap()
    {
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    void Update()
    {
        if (mapPanel == null || !mapPanel.activeSelf || mapRect == null) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 screenPos = Input.mousePosition;

        // Solo si el clic cae DENTRO del rectángulo del dronLab
        if (!RectTransformUtility.RectangleContainsScreenPoint(mapRect, screenPos, null))
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapRect, screenPos, null, out Vector2 localPoint))
            return;

        Rect r = mapRect.rect;
        float u = Mathf.Clamp01(Mathf.InverseLerp(r.xMin, r.xMax, localPoint.x)); // 0=izq, 1=der
        float v = Mathf.Clamp01(Mathf.InverseLerp(r.yMin, r.yMax, localPoint.y)); // 0=abajo, 1=arriba

        // Mover el marcador al punto clickado 
        if (marker != null)
        {
            marker.gameObject.SetActive(true);
            marker.anchorMin = marker.anchorMax = new Vector2(u, v);
            marker.anchoredPosition = Vector2.zero;
        }

        // Interpolación bilineal de las 4 esquinas GPS
        selectedLat = BilinearMap(latBottomLeft, latBottomRight, latTopLeft, latTopRight, u, v);
        selectedLon = BilinearMap(lonBottomLeft, lonBottomRight, lonTopLeft, lonTopRight, u, v);
        hasMapPoint = true;
    }

    double BilinearMap(double bl, double br, double tl, double tr, float u, float v)
    {
        double bottom = bl + (br - bl) * u;
        double top = tl + (tr - tl) * u;
        return bottom + (top - bottom) * v;
    }

    void SaveMapPosition()
    {
        if (!hasMapPoint)
        {
            Debug.LogWarning("No se ha seleccionado ninguna posición en el mapa.");
            return; // No cerramos para forzar a colocar un punto
        }

        // Mismo formato que espera tu double.TryParse (cultura actual)
        latitudeInputField.text = selectedLat.ToString("F8");
        longitudeInputField.text = selectedLon.ToString("F8");

        CloseMap();
    }
    void UpdateSelectedCustomScenarioText()
    {
        if (selectedCustomScenarioText == null)
            return;

        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom &&
            !string.IsNullOrEmpty(ConnectionConfig.CustomScenarioName))
        {
            selectedCustomScenarioText.text =
                "Selected: " + ConnectionConfig.CustomScenarioName;
        }
        else
        {
            selectedCustomScenarioText.text = "Selected: None";
        }
    }
}
