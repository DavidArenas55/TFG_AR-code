using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptimalAndRealPath : MonoBehaviour
{
    [Header("Optimal Path")]
    public LineRenderer line;
    public int resolution = 20;
    public Button seeSuggestedButton;

    [Header("3D Path Volume")]
    public bool generate3DVolume = true;
    public bool generate3DVolumeDrone = true;
    public float volumeThickness = 0.5f;
    public float volumeHeight = 0.1f; // Grosor más pequeño en un eje perpendicular (plano/aplastado)
    public Material pathMaterial;
    public Material dronePathMaterial;

    [Header("Drone Path")]
    public LineRenderer dronePathLine;
    public Button seeRealButton;
    public Transform drone; 
    public float minDistance = 0.1f; // Distancia mínima para añadir un nuevo punto

    private List<Transform> points = new List<Transform>();
    private List<Vector3> dronePathPoints = new List<Vector3>();
    private bool isDronePathInitialized = false;
    private GameObject volumeParent;
    private GameObject droneVolumeParent;

    void Start()
    {
        // Detecta todos los hijos automáticamente
        for (int i = 0; i < transform.childCount; i++)
        {
            points.Add(transform.GetChild(i));
        }

        DrawPath();

        // Crear el contenedor del volumen 3D de la ruta real (vacío al inicio, se irá rellenando con segmentos a medida que el dron se mueve)
        if (generate3DVolumeDrone)
        {
            droneVolumeParent = new GameObject("3D_DronePath");
            droneVolumeParent.transform.SetParent(this.transform);
        }

        // Hacer los botones visibles
        if (seeSuggestedButton != null) seeSuggestedButton.gameObject.SetActive(true);
        if (seeRealButton != null) seeRealButton.gameObject.SetActive(true);

        // Desactivar los componentes LineRenderer al inicio
        if (line != null) line.enabled = false;
        if (dronePathLine != null) dronePathLine.enabled = false;
        if (volumeParent != null) volumeParent.SetActive(false);
        if (droneVolumeParent != null) droneVolumeParent.SetActive(false);

        // Configurar el botón de la ruta sugerida para alternar (activar/desactivar)
        if (seeSuggestedButton != null)
        {
            seeSuggestedButton.onClick.AddListener(() => {
                if (line != null) line.enabled = !line.enabled;
                if (volumeParent != null) volumeParent.SetActive(!volumeParent.activeSelf);
            });
        }

        // Configurar el botón de la ruta real para alternar (activar/desactivar)
        if (seeRealButton != null)
        {
            seeRealButton.onClick.AddListener(() => {
                if (dronePathLine != null) dronePathLine.enabled = !dronePathLine.enabled;
                if (droneVolumeParent != null) droneVolumeParent.SetActive(!droneVolumeParent.activeSelf);
            });
        }

        // Inicia la corrutina que esperará 3 segundos antes de empezar a dibujar
        StartCoroutine(InitializeDronePathAfterDelay(3f));

    }

    IEnumerator InitializeDronePathAfterDelay(float delay)
    {
        // Espera el tiempo especificado
        yield return new WaitForSeconds(delay);

        // Después del retardo, inicializa la ruta del dron en su posición actual
        if (dronePathLine != null && drone != null)
        {
            dronePathLine.positionCount = 1;
            dronePathLine.SetPosition(0, drone.position);
            dronePathPoints.Add(drone.position);
            isDronePathInitialized = true; // Permite que Update comience a dibujar
        }
    }

    void Update()
    {
        // No ejecuta ninguna lógica de dibujo hasta que la ruta se haya inicializado
        if (!isDronePathInitialized)
        {
            return;
        }

        // Actualización de la ruta del dron
        if (dronePathLine != null && drone != null)
        {
            // Comprueba si el dron se ha movido lo suficiente para añadir un nuevo punto
            if (Vector3.Distance(dronePathPoints[dronePathPoints.Count - 1], drone.position) > minDistance)
            {
                dronePathPoints.Add(drone.position);
                int count = dronePathPoints.Count;

                // Dibuja la línea usando todos los puntos de la ruta excepto el último. Esto evita que se dibuje el retroceso final al frenar.
                if (count > 1)
                {
                    dronePathLine.positionCount = dronePathPoints.Count - 1;

                    // Creamos un array temporal con los puntos a dibujar y lo asignamos.
                    Vector3[] positionsToDraw = new Vector3[dronePathPoints.Count - 1];
                    dronePathPoints.CopyTo(0, positionsToDraw, 0, dronePathPoints.Count - 1);

                    dronePathLine.SetPositions(positionsToDraw);
                }

                // VOLUMEN 3D DE LA RUTA REAL (Túnel cerrado)
                if (count >= 3)
                {
                    AddDronePathVolumeSegment(dronePathPoints[count - 3], dronePathPoints[count - 2]);
                }
            }
        }
    }


    void DrawPath()
    {
        if (points.Count < 4)
        {
            Debug.LogWarning("No hay suficientes puntos para dibujar la curva. Se necesitan al menos 4 hijos.");
            return;
        }

        List<Vector3> curve = new List<Vector3>();

        // cada 3 pasos (porque 4 puntos por curva, pero se solapan)
        for (int i = 0; i < points.Count - 3; i += 3)
        {
            Vector3 p0 = points[i].position;
            Vector3 p1 = points[i + 1].position;
            Vector3 p2 = points[i + 2].position;
            Vector3 p3 = points[i + 3].position;

            for (int j = 0; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                curve.Add(CubicBezier(p0, p1, p2, p3, t));
            }
        }

        if (line != null)
        {
            line.positionCount = curve.Count;
            line.SetPositions(curve.ToArray());
        }

        // OPCIÓN VOLUMEN 3D (Túnel cerrado)
        if (generate3DVolume && curve.Count > 1)
        {
            volumeParent = new GameObject("3D_OptimalPath");
            volumeParent.transform.SetParent(this.transform);

            for (int i = 0; i < curve.Count - 1; i++)
            {
                Vector3 startPoint = curve[i];
                Vector3 endPoint = curve[i + 1];
                float distance = Vector3.Distance(startPoint, endPoint);

                // 1. Segmento del túnel (prisma rectangular) entre los dos puntos
                CreateOrientedCube(
                    (startPoint + endPoint) / 2f,
                    endPoint - startPoint,
                    new Vector3(volumeThickness, volumeHeight, distance),
                    volumeParent.transform,
                    pathMaterial);

                // 2. Caja en la "articulación" para que las curvas se vean suaves y sin huecos
                CreateOrientedCube(
                    startPoint,
                    endPoint - startPoint,
                    new Vector3(volumeThickness, volumeHeight, volumeThickness),
                    volumeParent.transform,
                    pathMaterial);
            }

            // 3. Último bloque para cerrar la punta del final del tubo
            CreateOrientedCube(
                curve[curve.Count - 1],
                curve[curve.Count - 1] - curve[curve.Count - 2],
                new Vector3(volumeThickness, volumeHeight, volumeThickness),
                volumeParent.transform,
                pathMaterial);
        }
    }

    // Añade un segmento del túnel (prisma + articulación de cierre) a la ruta real. Se llama de forma incremental cada vez que el dron avanza lo suficiente.
    void AddDronePathVolumeSegment(Vector3 from, Vector3 to)
    {
        if (!generate3DVolumeDrone || droneVolumeParent == null) return;
        if (from == to) return;

        Vector3 dir = to - from;
        float distance = dir.magnitude;

        // 1. Segmento del túnel (prisma) entre los dos puntos
        CreateOrientedCube(
            (from + to) / 2f,
            dir,
            new Vector3(volumeThickness, volumeHeight, distance),
            droneVolumeParent.transform,
            dronePathMaterial);

        // 2. Caja en la articulación / cierre del extremo, para suavizar las curvas y cerrar la punta del tubo en el punto más reciente.
        CreateOrientedCube(
            to,
            dir,
            new Vector3(volumeThickness, volumeHeight, volumeThickness),
            droneVolumeParent.transform,
            dronePathMaterial);
    }

    // Crea un cubo orientado, le quita el collider y le asigna el material. Reutilizado por la ruta óptima y por la ruta real.
    GameObject CreateOrientedCube(Vector3 position, Vector3 forwardDir, Vector3 scale, Transform parent, Material mat)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(parent);
        cube.transform.position = position;

        if (forwardDir != Vector3.zero)
        {
            cube.transform.forward = forwardDir;
        }

        cube.transform.localScale = scale;

        // Destruir collider para que no interfiera físicamente con el dron
        Destroy(cube.GetComponent<Collider>());

        if (mat != null)
        {
            cube.GetComponent<Renderer>().material = mat;
        }

        return cube;
    }

    Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;

        return u * u * u * p0 +
               3 * u * u * t * p1 +
               3 * u * t * t * p2 +
               t * t * t * p3;
    }

}