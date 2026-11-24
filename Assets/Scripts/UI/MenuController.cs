using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;

public class MenuController : MonoBehaviour
{
    private bool isUpdatingToggles = false;

    [Header("UI")]
    public Toggle simulationToggle;
    public Toggle productionToggle;
    public Toggle telloToggle;
    public InputField comInputField;
    public Text debugText;
    public Button connectButton; public Button playButton;

    [Header("Scenario Creator")]
    public Button CreateScenarioButton;
    public Button SavedSenariosButton;
    public TMP_InputField scenarioNameInputField;
    public Toggle ringToggle; public Toggle arcToggle; public Toggle cylinderToggle; public Toggle tunnelToggle; public Toggle squareToggle; public Toggle wallToggle;
    public Toggle HtakeoffToggle; public Toggle HlandToggle;
    public Toggle bigGem1Toggle; public Toggle bigGem2Toggle;
    public Toggle gem1Toggle; public Toggle gem2Toggle; public Toggle gem3Toggle; public Toggle gem4Toggle; public Toggle gem5Toggle; public Toggle gem6Toggle; public Toggle gem7Toggle; public Toggle gem8Toggle;
    public InputField latitudeInputField; public InputField longitudeInputField; public InputField altitudeInputField; public InputField headingInputField;
    public InputField diameterInputField;
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
    public MapPositionSelector mapPositionSelector;
    public ScenarioCreatorWindowController scenarioCreatorWindowController;

    [Header("Saved Scenarios")]
    public TMP_Text selectedCustomScenarioText;
    public SavedScenariosController savedScenariosController;

    [Header("Place on Map")]
    public int id = 0;

    [Header("Scenarios")]
    public Toggle droneFlowToggle; public Dropdown droneFlowDropdown; public Toggle circuitToggle; public Toggle easyToggle; public Toggle hardToggle; public Toggle searchToggle;

    [Header("Connection Status")]
    public GameObject connectionYESImage; 
    public GameObject connectionNOImage;
    private bool telloConnectionVerified;
    private GameObject selectedListItem = null;
    private int selectedItemId = -1;
    private Color normalColor = new Color(1f, 1f, 1f, 0f); // Transparente
    private Color selectedColor = new Color(0.7f, 0.7f, 1f, 0.5f); // Tono resaltado

    void Start()
    {
        if (ConnectionConfig.SelectedDrone == ConnectionConfig.DroneType.Tello)
        {
            telloToggle.SetIsOnWithoutNotify(true);
            simulationToggle.SetIsOnWithoutNotify(false);
            productionToggle.SetIsOnWithoutNotify(false);
        }
        else
        {
            telloToggle.SetIsOnWithoutNotify(false);

            bool isProduction =
                ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Production;

            productionToggle.SetIsOnWithoutNotify(isProduction);
            simulationToggle.SetIsOnWithoutNotify(!isProduction);
        }

        comInputField.text = ConnectionConfig.ComPort;
        comInputField.interactable = productionToggle.isOn;
        connectButton.interactable =
            ConnectionConfig.SelectedDrone == ConnectionConfig.DroneType.GPS || ConnectionConfig.SelectedDrone == ConnectionConfig.DroneType.Tello;

        droneFlowToggle.isOn = false;
        circuitToggle.isOn = false;
        easyToggle.isOn = false;
        hardToggle.isOn = false;
        searchToggle.isOn = false;
        //Comentamos estas dos lineas para mantener es escenario carrgado
        //ConnectionConfig.SelectedScenario = ConnectionConfig.Scenario.None;
        //ConnectionConfig.ConnectRequested = false;

        simulationToggle.onValueChanged.AddListener(OnSimulationToggle);
        productionToggle.onValueChanged.AddListener(OnProductionToggle);
        telloToggle.onValueChanged.AddListener(OnTelloToggle);
        connectButton.onClick.AddListener(OnConnectClicked);
        playButton.onClick.AddListener(OnPlayClicked);

        droneFlowDropdown.gameObject.SetActive(false);
        circuitToggle.onValueChanged.AddListener((val) => { OnCircuitSelected(val); if (val) HideDroneDropdown(); });
        easyToggle.onValueChanged.AddListener((val) => { OnEasySelected(val); if (val) HideDroneDropdown(); });
        hardToggle.onValueChanged.AddListener((val) => { OnHardSelected(val); if (val) HideDroneDropdown(); });
        searchToggle.onValueChanged.AddListener((val) => { OnSearchSelected(val); if (val) HideDroneDropdown(); });
        droneFlowToggle.onValueChanged.AddListener(OnDroneFlowSelected);

        CreateScenarioButton.onClick.AddListener(OnCreateScenarioClicked);
        SavedSenariosButton.onClick.AddListener(onSavedScenariosClicked);

        if (scenarioCreatorWindowController != null)
            scenarioCreatorWindowController.Closed += OnScenarioCreatorClosed;

        UpdateSelectedCustomScenarioText();
        ValidatePlay();
    }
    void OnSimulationToggle(bool on)
    {
        if (!on)
        {
            if (!productionToggle.isOn && !telloToggle.isOn)
                simulationToggle.SetIsOnWithoutNotify(true);

            return;
        }

        productionToggle.SetIsOnWithoutNotify(false);
        telloToggle.SetIsOnWithoutNotify(false);

        ConnectionConfig.SelectedDrone = ConnectionConfig.DroneType.GPS;
        ConnectionConfig.SelectedMode = ConnectionConfig.Mode.Simulation;

        comInputField.interactable = false;
        connectButton.interactable = true;

        InvalidateConnect();
        ValidatePlay();
    }
    void OnProductionToggle(bool on)
    {
        if (!on)
        {
            if (!simulationToggle.isOn && !telloToggle.isOn)
                productionToggle.SetIsOnWithoutNotify(true);

            return;
        }

        simulationToggle.SetIsOnWithoutNotify(false);
        telloToggle.SetIsOnWithoutNotify(false);

        ConnectionConfig.SelectedDrone = ConnectionConfig.DroneType.GPS;
        ConnectionConfig.SelectedMode = ConnectionConfig.Mode.Production;

        comInputField.interactable = true;
        connectButton.interactable = true;

        InvalidateConnect();
        ValidatePlay();
    }
    void OnTelloToggle(bool on)
    {
        if (!on)
        {
            if (!simulationToggle.isOn && !productionToggle.isOn)
                telloToggle.SetIsOnWithoutNotify(true);

            return;
        }

        simulationToggle.SetIsOnWithoutNotify(false);
        productionToggle.SetIsOnWithoutNotify(false);

        ConnectionConfig.SelectedDrone = ConnectionConfig.DroneType.Tello;

        comInputField.interactable = false;

        // La conexión Tello todavía no está implementada.
        InvalidateConnect();
        connectButton.interactable = true;

        ValidatePlay();
    }
    void OnCreateScenarioClicked()
    {
        scenarioCreatorWindowController.Open();
    }
    private void OnScenarioCreatorClosed()
    {
        savedScenariosController.ClearSelection();
        UpdateSelectedCustomScenarioText();
        ValidatePlay();
    }
    void onSavedScenariosClicked()
    {
        savedScenariosController.Open();
    }
    void OnSavedScenariosClosed()
    {
        UpdateSelectedCustomScenarioText();
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
        if (ConnectionConfig.SelectedDrone == ConnectionConfig.DroneType.Tello)
        {
            ConnectTello();
            return;
        }

        if (ConnectionConfig.SelectedDrone != ConnectionConfig.DroneType.GPS)
            return;

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
        bool modeSelected = simulationToggle.isOn ||
                            productionToggle.isOn ||
                            telloToggle.isOn;

        bool scenarioSelected =
            ConnectionConfig.SelectedScenario != ConnectionConfig.Scenario.None;

        bool connectionReady = ConnectionConfig.SelectedDrone switch
        {
            ConnectionConfig.DroneType.GPS => ConnectionConfig.ConnectRequested,
            ConnectionConfig.DroneType.Tello => telloConnectionVerified,
            _ => false
        };

        playButton.interactable = connectionReady && modeSelected && scenarioSelected;
    }
    void InvalidateConnect()
    {
        ConnectionConfig.ConnectRequested = false;
        telloConnectionVerified = false;
        playButton.interactable = false;
    }
    void OnPlayClicked()
    {
        if (ConnectionConfig.SelectedDrone == ConnectionConfig.DroneType.Tello)
        {
            if (!telloConnectionVerified ||
                ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.None)
                return;

            if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow &&
                droneFlowDropdown != null)
            {
                ConnectionConfig.DroneFlowLevel = droneFlowDropdown.value;
            }

            playButton.interactable = false;
            SceneManager.LoadSceneAsync("GameScene");
            return;
        }

        if (ConnectionConfig.SelectedDrone != ConnectionConfig.DroneType.GPS)
            return;

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
    private System.Diagnostics.Process telloConnectionProcess;
    private async void ConnectTello()
    {
        InvalidateConnect();
        connectButton.interactable = false;
        simulationToggle.interactable = false;
        productionToggle.interactable = false;
        telloToggle.interactable = false;
        debugText.text = "Conectando con Tello…";
        debugText.color = Color.white;
        connectionYESImage.SetActive(false);

        System.Diagnostics.Process process = null;

        try
        {
            string toolsPath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "Tools", "Tello"));

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = Path.Combine(toolsPath, ".venv", "Scripts", "python.exe"),
                Arguments = "-u connect_tello.py --unity",
                WorkingDirectory = toolsPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
                throw new System.InvalidOperationException("No se pudo iniciar Python.");

            telloConnectionProcess = process;
            string output = await ReadTelloConnectionAsync(process);

            if (this == null || telloConnectionProcess != process)
                return;

            telloConnectionVerified = true;
            debugText.text = int.TryParse(output, out int battery)
                ? $"Tello conectado · Batería: {battery} %"
                : "Tello conectado";
            debugText.color = Color.green;
            connectionYESImage.SetActive(true);
        }
        catch (System.Exception error)
        {
            if (this == null ||
                (process != null && telloConnectionProcess != process))
                return;

            debugText.text = "No se pudo conectar con Tello.";
            debugText.color = Color.red;
            Debug.LogWarning($"Tello: {error.Message}");
        }
        finally
        {
            process?.Dispose();

            if (this != null && (process == null || telloConnectionProcess == process))
            {
                telloConnectionProcess = null;
                RestoreTelloControls();
            }
        }
    }
    private static async Task<string> ReadTelloConnectionAsync(
        System.Diagnostics.Process process)
    {
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();

        bool exited = await Task.Run(() => process.WaitForExit(30000));
        if (!exited)
        {
            process.StandardInput.Close();
            throw new System.TimeoutException("El Tello no respondió a tiempo.");
        }

        if (process.ExitCode != 0)
            throw new System.InvalidOperationException((await error).Trim());

        return (await output).Trim();
    }

    private void RestoreTelloControls()
    {
        connectButton.interactable = true;
        simulationToggle.interactable = true;
        productionToggle.interactable = true;
        telloToggle.interactable = true;
        ValidatePlay();
    }

    private void OnDisable()
    {
        var process = telloConnectionProcess;
        if (process == null)
            return;

        telloConnectionProcess = null;
        process.StandardInput.Close();
        RestoreTelloControls();
    }
}
