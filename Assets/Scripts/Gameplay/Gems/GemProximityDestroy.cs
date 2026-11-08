using System.Collections;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GemProximityDestroy : MonoBehaviour
{
    [Header("Tipode Objeto")]
    public bool isFinalGem = false;
    public bool isDestructibleGem = false;
    public bool isScoreTarget = false;

    [Header("Referencias")]
    public Transform drone;
    public GameObject resultsParent;

    [Header("Detección")]
    public float triggerDistance = 1.0f;

    [Header("Disparo (Search)")]
    public float shootDistance = 30.0f; 
    public KeyCode shootKey = KeyCode.Space; //tecla espacio
    public Camera MainCamera;

    [Header("Shrink")]
    public float shrinkDuration = 2.0f;

    private bool alreadyTriggered = false;

    private Renderer gemRenderer;
    private Collider gemCollider;

    private Renderer[] resultsParentRenderers;
    private Collider[] resultsParentColliders;

    public static int gemsCollected = 0;

    public int waypointIndex = -1;

    public bool requireShooting = false;

    private RaceTimer globalRaceTimer;

    public static void ResetGemsCollected()
    {
        gemsCollected = 0;
    }

    void Start()
    {
        // AUTO BUSCAR MAIN CAMERA
        if (MainCamera == null)
        {
            GameObject c = GameObject.FindWithTag("MainCamera");
            if (c != null)
                MainCamera = c.GetComponent<Camera>();
            else
                Debug.LogWarning("No se encontró ningún objeto con tag 'MainCamera'");
        }

        // AUTO BUSCAR DRON
        if (drone == null)
        {
            GameObject d = GameObject.FindWithTag("Drone");
            if (d != null)
                drone = d.transform;
            else
                Debug.LogWarning("No se encontró ningún objeto con tag 'Drone'");
        }

        gemRenderer = GetComponent<Renderer>();
        gemCollider = GetComponent<Collider>();

        if (isDestructibleGem || isFinalGem)
        {
            if (resultsParent != null)
            {
                // También buscamos el renderer y collider en los hijos de resultsParent:
                resultsParentRenderers = resultsParent.GetComponentsInChildren<Renderer>();
                resultsParentColliders = resultsParent.GetComponentsInChildren<Collider>();
                foreach (Renderer resultRenderer in resultsParentRenderers)
                    resultRenderer.enabled = false;
                foreach (Collider resultCollider in resultsParentColliders)
                    resultCollider.enabled = false;
            }
        }

        globalRaceTimer = FindFirstObjectByType<RaceTimer>();
        
    }

    void Update()
    {
        if (alreadyTriggered || drone == null)
            return;

        if (globalRaceTimer != null && globalRaceTimer.openResultsButton.gameObject.activeSelf)
            return;

        float distance = Vector3.Distance(transform.position, drone.position);
        bool shouldTrigger = false;

        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Search)
        {
            requireShooting = true; 
        }

        // Comprobación diferente según el escenario activo
        if (requireShooting)
        {
            // LÓGICA DE DISPARO 
            if (IsBeingAimedAt() && (Input.GetKeyDown(shootKey) || WebRTCStreamer.ShootTriggered))
            {
                Debug.Log("¡Disparo acertado contra la gema!");
                shouldTrigger = true;
            }
        }
        else
        {
            // LÓGICA DE PROXIMIDAD CLÁSICA (Easy Maze y Hard Maze)
            if (distance < triggerDistance)
            {
                shouldTrigger = true;
            }
        }

        if (shouldTrigger && waypointIndex != -1)
        {
            RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
            if (raceTimer != null)
            {
                if (raceTimer.targetsCollected != waypointIndex)
                {
                    Debug.Log($"Ignorando Gema {waypointIndex}: debes ir a la {raceTimer.targetsCollected}");
                    shouldTrigger = false; // Cancelamos la acción porque no es su turno
                }
                else
                {
                    // Si es el turno correcto, validamos en el Timer
                    raceTimer.ValidateWaypoint(waypointIndex);
                }
            }
        }

        if (shouldTrigger)
        {
            Debug.Log($"TRIGGER {name} | requireShooting={requireShooting} | aimed={IsBeingAimedAt()} | dist={Vector3.Distance(transform.position, drone.position):F1} | cam={(MainCamera ? MainCamera.name : "null")}");
            alreadyTriggered = true;
            bool triggersEndGame = false;

            if (isScoreTarget)
            {
                GemProximityDestroy.gemsCollected++;
                Debug.Log($"¡PUNTOS SUMADOS! Gemas recogidas: {GemProximityDestroy.gemsCollected}");
            }

            if (isDestructibleGem)
            {
                Debug.Log("¡GEMA DESTRUCTIBLE RECOGIDA!");
                StartCoroutine(DestroyGem());
                if (UIManager.Instance != null) UIManager.Instance.ShowNotification("Gem Collected!");
            }
            else if (isFinalGem)
            {
                StartCoroutine(DestroyGem());
            }

            if (isFinalGem)
            {
                Debug.Log("¡GEMA FINAL RECOGIDA! Fin del juego marcado.");
                triggersEndGame = true;
            }

            // Condición para terminar recolectando todas las gemas SOLO si es el escenario Search
            if (isScoreTarget && ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Search)
            {
                if (GemProximityDestroy.gemsCollected >= GemaSpawner.numberOfSearchGems)
                {
                    Debug.Log("¡Todas las gemas del escenario Search recogidas!");
                    triggersEndGame = true;
                }
            }

            // Condición unida para CUSTOM: Al sumar un punto, obligamos al RaceTimer a verificar si todo está completado
            if (isScoreTarget && ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom)
            {
                // Si existe un "End Game Object" configurado, IGNORAMOS esta validación
                // porque el fin del juego será forzado exclusivamente por el End Game Object verdadero.
                if (!ConnectionConfig.CustomHasStopwatch)
                {
                    RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
                    if (raceTimer != null)
                    {
                        raceTimer.CheckCustomWinCondition();
                    }
                }
            }

            // 4. DISPARADOR
            if (triggersEndGame)
            {
                RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
                if (raceTimer != null)
                {
                    raceTimer.StopTimer();
                    raceTimer.openResultsButton.gameObject.SetActive(true);
                    raceTimer.GameOver.SetActive(true);
                }
            }
        }
    }


    public bool IsBeingAimedAt()
    {
        if (alreadyTriggered || MainCamera == null || !requireShooting)
            return false;

        // Si la gema tiene un orden asignado, solo es "apuntable" en su turno. Evita que la mira se ponga verde (y que se pueda disparar) antes de tiempo.
        if (waypointIndex != -1)
        {
            if (globalRaceTimer == null)
                globalRaceTimer = FindFirstObjectByType<RaceTimer>();

            if (globalRaceTimer != null && globalRaceTimer.targetsCollected != waypointIndex)
                return false;
        }

        float distance = Vector3.Distance(transform.position, drone.position);

        if (distance > shootDistance)
            return false;

        // Rayo desde el centro de la pantalla, hacia donde apunta la cámara/cruz.
        // Usa el collider REAL de cada gema, así que vale para cualquier tamaño y cualquier distancia dentro de shootDistance.
        Ray ray = new Ray(MainCamera.transform.position, MainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            // El primer objeto que toca el rayo debe ser ESTA gema
            GemProximityDestroy hitGem = hit.collider.GetComponentInParent<GemProximityDestroy>();
            if (hitGem == null)
                hitGem = hit.collider.GetComponentInChildren<GemProximityDestroy>(); //por si acaso

            if (hitGem == this)
                return true;
        }
        return false;
    }

    IEnumerator DestroyGem()
    {
        if (gemRenderer != null) gemRenderer.enabled = false;
        if (gemCollider != null) gemCollider.enabled = false;

        // Ahora habilitamos los renderers y colliders de los puntos hijos de resultsParent:
        if (resultsParentRenderers != null)
        {
            foreach (Renderer resultRenderer in resultsParentRenderers)
                resultRenderer.enabled = true;
        }
        if (resultsParentColliders != null)
        {
            foreach (Collider resultCollider in resultsParentColliders)
                resultCollider.enabled = true;
        }

        // Encoger los puntos
        if (resultsParent != null)
        {
            foreach (Transform point in resultsParent.transform)
            {
                StartCoroutine(ShrinkPoint(point));
            }
        }

        yield return null;
    }

    IEnumerator ShrinkPoint(Transform point)
    {
        if (point == null) yield break;

        Vector3 initialScale = point.localScale;
        float t = 0f;

        while (t < shrinkDuration)
        {
            if (point == null) yield break;

            t += Time.deltaTime;
            float factor = Mathf.Pow(1 - (t / shrinkDuration), 2);
            point.localScale = initialScale * factor;

            yield return null;
        }

        if (point != null)
        {
            Destroy(point.gameObject);
        }
    }

}