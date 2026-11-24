using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

public class TelloVideoDisplay : MonoBehaviour
{
    private const string FrameUrl = "http://127.0.0.1:8765/frame.jpg";

    private RawImage target;
    private AspectRatioFitter aspectFitter;
    private Texture2D frameTexture;
    private HttpClient frameClient;
    private Process videoProcess;
    private Task<string> processErrors;
    private Task<bool> videoReady;
    private Coroutine frameLoop;

    public void StartStream(RawImage image, AspectRatioFitter fitter)
    {
        if (videoProcess != null)
            return;

        target = image;
        aspectFitter = fitter;

        try
        {
            string tools = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "Tools", "Tello"));
            videoProcess = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(tools, ".venv", "Scripts", "python.exe"),
                Arguments = "-B -u stream_tello.py --unity",
                WorkingDirectory = tools,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (videoProcess == null)
                throw new InvalidOperationException("No se pudo iniciar Python.");

            processErrors = videoProcess.StandardError.ReadToEndAsync();
            videoReady = WaitForVideoReadyAsync(videoProcess.StandardOutput);
            frameClient = new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
            frameTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            frameLoop = StartCoroutine(DisplayFrames());
        }
        catch (Exception error)
        {
            StopStream();
            Debug.LogError($"Vídeo Tello: {error.Message}", this);
        }
    }

    private IEnumerator DisplayFrames()
    {
        var interval = new WaitForSecondsRealtime(0.1f);
        double firstFrameDeadline = Time.realtimeSinceStartupAsDouble + 40;
        bool hasFrame = false;

        while (!videoProcess.HasExited)
        {
            if (!hasFrame && Time.realtimeSinceStartupAsDouble >= firstFrameDeadline)
            {
                Debug.LogError("Vídeo Tello: no se recibió ninguna imagen a tiempo.", this);
                break;
            }

            if (videoReady.Status != TaskStatus.RanToCompletion || !videoReady.Result)
            {
                yield return interval;
                continue;
            }

            Task<byte[]> request = frameClient.GetByteArrayAsync(FrameUrl);
            while (!request.IsCompleted)
                yield return null;

            if (request.Status == TaskStatus.RanToCompletion &&
                frameTexture.LoadImage(request.Result))
            {
                if (!hasFrame)
                {
                    target.material = null;
                    target.texture = frameTexture;
                    target.color = Color.white;
                    hasFrame = true;
                }

                if (aspectFitter != null)
                    aspectFitter.aspectRatio = (float)frameTexture.width / frameTexture.height;
            }
            else if (request.IsFaulted)
            {
                // Mientras Python arranca, el servidor puede rechazar las primeras peticiones.
                _ = request.Exception;
            }

            yield return interval;
        }

        if (videoProcess.HasExited)
        {
            while (!processErrors.IsCompleted)
                yield return null;

            string details = processErrors.Status == TaskStatus.RanToCompletion
                ? processErrors.Result.Trim()
                : string.Empty;
            Debug.LogError($"El proceso de vídeo Tello ha terminado. {details}", this);
        }

        frameLoop = null;
        StopStream();
    }

    private static async Task<bool> WaitForVideoReadyAsync(StreamReader output)
    {
        string line;
        while ((line = await output.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            if (line.Contains(FrameUrl))
                return true;
        }

        return false;
    }

    public void StopStream()
    {
        if (frameLoop != null)
        {
            StopCoroutine(frameLoop);
            frameLoop = null;
        }

        frameClient?.Dispose();
        frameClient = null;

        if (frameTexture != null)
        {
            if (target != null && target.texture == frameTexture)
                target.texture = null;
            Destroy(frameTexture);
            frameTexture = null;
        }

        Process process = videoProcess;
        videoProcess = null;
        if (process == null)
            return;

        // Python detecta el fin de la entrada y cierra la sesión en su bloque finally.
        process.StandardInput.Close();
        _ = Task.Run(() => FinishProcess(process));
    }

    private static void FinishProcess(Process process)
    {
        using (process)
        {
            if (process.WaitForExit(45000))
                return;

            // El ejecutable de un venv en Windows puede lanzar otro proceso Python.
            using var stopTree = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, "taskkill.exe"),
                Arguments = $"/PID {process.Id} /T /F",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            stopTree?.WaitForExit();
        }
    }

    private void OnDisable()
    {
        StopStream();
    }
}
