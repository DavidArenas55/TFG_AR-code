using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class WebCamDisplay : MonoBehaviour
{
    public RawImage rawImage;
    public AspectRatioFitter aspectFitter;
    public Material undistortMaterial;
    public string calibrationFile = "calibration_data.json";

    public Dropdown cameraDropdown;

    private WebCamTexture webCamTexture;
    private WebCamDevice[] devices;

    [System.Serializable]
    private class CalibrationData
    {
        public float fx, fy, cx, cy;  // Parámetros intrínsecos (distancia focal y centro óptico)
        public float k1, k2, k3, p1, p2;  // Coeficientes de distorsión radial (k) y tangencial (p)
        public int width, height;  // Resolución de la imagen usada en la calibración
    }

    void Start()
    {
        if (!rawImage)
        {
            Debug.LogError("WebCamDisplay: rawImage no asignado.");
            enabled = false;
            return;
        }

        devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No se encontraron dispositivos de cámara.");
            enabled = false;
            return;
        }

        SetupDropdown();

        // Simulacion -> primera cámara (webcam PC), Produccion -> segunda (receptor camara dron)
        int defaultIndex = ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Production ? 1 : 0;
        int index = Mathf.Clamp(defaultIndex, 0, devices.Length - 1);

        // Si hay dropdown, el valor del UI tiene prioridad
        if (cameraDropdown != null)
            index = Mathf.Clamp(cameraDropdown.value, 0, devices.Length - 1);

        StartCamera(index);
    }

    private void SetupDropdown()
    {
        if (cameraDropdown == null)
            return;

        cameraDropdown.onValueChanged.RemoveListener(OnCameraChanged);

        var options = new List<Dropdown.OptionData>();
        for (int i = 0; i < devices.Length; i++)
            options.Add(new Dropdown.OptionData(devices[i].name));

        cameraDropdown.ClearOptions();
        cameraDropdown.AddOptions(options);

        // Valor inicial por modo (pueden intercambiarse)
        int defaultIndex = ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Production ? 1 : 0;
        cameraDropdown.value = Mathf.Clamp(defaultIndex, 0, devices.Length - 1);
        cameraDropdown.RefreshShownValue();

        cameraDropdown.onValueChanged.AddListener(OnCameraChanged);
    }

    private void OnCameraChanged(int index)
    {
        if (index < 0 || index >= devices.Length)
            return;

        StartCamera(index);
    }

    private void StartCamera(int index)
    {
        if (webCamTexture != null)
        {
            if (webCamTexture.isPlaying)
                webCamTexture.Stop();

            Destroy(webCamTexture);
        }

        Debug.Log($"Modo: {ConnectionConfig.SelectedMode} → Cámara [{index}]: {devices[index].name}");

        webCamTexture = new WebCamTexture(devices[index].name);
        rawImage.texture = webCamTexture;

        bool isDroneCamera = index >= 1;

        // Aplicar corrección solo en Production (cámara real del dron)
        if (ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Production && isDroneCamera)
        { 
            ApplyUndistortion();
        }
        else
        {
            rawImage.material = null;
            if (ConnectionConfig.SelectedMode == ConnectionConfig.Mode.Production && !isDroneCamera)
                Debug.LogWarning("WebCamDisplay: Modo Production pero sin cámara externa del dron. " +
                                 "Usando la cámara del portátil SIN corrección de distorsión.");
        }
        webCamTexture.Play();
    }

    private void ApplyUndistortion()
    {
        if (undistortMaterial == null)
        {
            Debug.LogWarning("WebCamDisplay: undistortMaterial no asignado. Sin corrección.");
            return;
        }

        string path = Path.Combine(Application.streamingAssetsPath, calibrationFile);

        if (!File.Exists(path))
        {
            Debug.LogError($"WebCamDisplay: No se encontró {path}");
            return;
        }

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<CalibrationData>(json);

        undistortMaterial.SetFloat("_FxN", data.fx / data.width);
        undistortMaterial.SetFloat("_FyN", data.fy / data.height);
        undistortMaterial.SetFloat("_CxN", data.cx / data.width);
        undistortMaterial.SetFloat("_CyN", data.cy / data.height);
        undistortMaterial.SetFloat("_K1", data.k1);
        undistortMaterial.SetFloat("_K2", data.k2);
        undistortMaterial.SetFloat("_K3", data.k3);
        undistortMaterial.SetFloat("_P1", data.p1);
        undistortMaterial.SetFloat("_P2", data.p2);

        rawImage.material = undistortMaterial;
        Debug.Log("WebCamDisplay: Corrección de distorsión aplicada.");
    }

    void Update()
    {
        if (webCamTexture != null && webCamTexture.width > 100 && aspectFitter)
            aspectFitter.aspectRatio = (float)webCamTexture.width / webCamTexture.height;
    }

    // Detiene la cámara web y limpia la imagen en la UI.
    public void StopWebcam()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            Debug.Log("Deteniendo la cámara web.");
            webCamTexture.Stop();
        }
        if (rawImage != null)
        {
            rawImage.texture = null;
            rawImage.color = Color.black;
        }
    }

    // Se asegura de que la cámara se detenga cuando el objeto se desactiva o destruye.
    void OnDisable()
    {
        StopWebcam();
    }
}