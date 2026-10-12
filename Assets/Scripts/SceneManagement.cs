using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
 
    public void LoadOpenGameScene()
    {
        SceneManager.LoadSceneAsync("OpenGameScene");
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadSceneAsync("MenuScene");
    }

    public void EndGameAndLoadResults()
    {
        // Detener el vídeo
        WebCamDisplay webcamDisplay = FindFirstObjectByType<WebCamDisplay>();
        if (webcamDisplay != null)
        {
            webcamDisplay.StopWebcam();
        }

        // Destruir los objetos persistentes de telemetría
        VirtualDroneTelemetry droneObject = FindFirstObjectByType<VirtualDroneTelemetry>();
        if (droneObject != null)
        {
            Destroy(droneObject.gameObject);
        }

        DroneTelemetry telemetryController = FindFirstObjectByType<DroneTelemetry>();
        if (telemetryController != null)
        {
            Destroy(telemetryController.gameObject);
        }

        // Cargar la escena de resultados
        LoadResultsScene();
    }

    public void LoadResultsScene()
    {
        SceneManager.LoadSceneAsync("ResultsScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
