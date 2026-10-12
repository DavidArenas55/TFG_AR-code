using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GateTrigger : MonoBehaviour
{
    public Transform directionReference;
    public Transform visualRoot;

    public Material materialActivado; public Material materialDesactivado;
    public Material materialCorrecto; // verde brillante
    public Material materialIncorrecto; // amarillo brillante
    public Material materialCollision; // rojo brillante

    private Renderer[] rends;

    private bool inside = false;
    private bool completed = false;

    private Coroutine parpadeoCoroutine;

    public int waypointIndex = -1;

    public bool isFinalTarget = false;

    void Start()
    {
        rends = visualRoot.GetComponentsInChildren<Renderer>();
        SetMaterial(materialActivado);
    }

    private void SetMaterial(Material mat)
    {
        foreach (Renderer r in rends)
        {
            r.material = mat;
        }
    }

    public void OnEntry(Collider drone)
    {
        if (completed) return;

        // Comprobación de orden
        RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
        bool isCorrectOrder = (raceTimer != null && raceTimer.targetsCollected == waypointIndex);

        if (waypointIndex != -1 && !isCorrectOrder &&
            (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit ||
             ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow ||
             ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom))
        {
            Debug.Log($"Ignorando puerta {waypointIndex} porque el orden es incorrecto. El dron va por la {raceTimer.targetsCollected}");
            IniciarParpadeo(materialIncorrecto);
            return;
        }

        // Convertimos la posición global del dron al "espacio local" de directionReference
        Vector3 posicionLocal = directionReference.InverseTransformPoint(drone.transform.position);

        // Comprobamos en qué lado de la flecha azul (Eje Z) está el dron al entrar.
        // Asumimos que la flecha azul apunta hacia la salida de la puerta, por lo que 
        // una entrada correcta empieza por la parte trasera (Z negativo).
        if (posicionLocal.z < 0)
        {
            inside = true;
            Debug.Log("Entrada correcta ✅. Z local was: " + posicionLocal.z);
            IniciarParpadeo(materialCorrecto);
        }
        else
        {
            Debug.Log($"Entrada incorrecta ❌. Z local was: {posicionLocal.z}");
            IniciarParpadeo(materialIncorrecto);
        }
    }

    // DETECTAR SI SALE RETROCEDIENDO
    public void OnEntryExit(Collider drone)
    {
        if (completed) return;

        // Si salimos de la zona de entrada (hacia adelante o hacia atrás), detenemos el parpadeo
        DetenerParpadeo();
        SetMaterial(materialActivado);

        // Si estábamos en un intento válido pero salimos marcha atrás, cancelamos
        if (inside)
        {
            Vector3 posicionLocal = directionReference.InverseTransformPoint(drone.transform.position);
            if (posicionLocal.z < 0)
            {
                inside = false;
                Debug.Log("Entrada anulada: El dron ha retrocedido 🔙");
            }
        }
    }

    public void CancelRun()
    {
        if (completed || !inside) return;

        inside = false;
        Debug.Log("Pasada por puerta invalidada debido a choque 💥.");

        DetenerParpadeo();
        StartCoroutine(DestelloError());
    }

    public void OnExit(Collider drone)
    {
        if (!inside || completed) return;

        completed = true;
        inside = false;

        DetenerParpadeo();
        SetMaterial(materialDesactivado);
        Debug.Log("Gate completado 🎯");
        if (UIManager.Instance != null)
            UIManager.Instance.ShowNotification("Gate Completed!");

        RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
        if (raceTimer != null)
        {
            // SI ESTE ERA EL ÚLTIMO OBJETIVO ABSOLUTO, FORZA EL FINAL PARA GANAR
            if (isFinalTarget)
            {
                Debug.Log("¡End Game Object atravesado! Finalizando juego inmediatamente.");
                raceTimer.StopTimer();
                raceTimer.openResultsButton.gameObject.SetActive(true);
                raceTimer.GameOver.SetActive(true);
            }
            else
            {
                raceTimer.ValidateWaypoint(waypointIndex);
            }
        }
    }

    // Se ejecuta al entrar en la zona de aviso cercana a la puerta
    public void OnWarningEnter(Collider drone)
    {
        // Si ya está completada o ya ha entrado al trigger real válido, no hacemos nada
        if (completed || inside) return;

        RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
        bool isCorrectOrder = (raceTimer != null && raceTimer.targetsCollected == waypointIndex);

        // Si se aproximas a un aro que NO te toca ahora, le meto destello de error
        if (waypointIndex != -1 && !isCorrectOrder &&
            (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit ||
             ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow ||
             ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom))
        {
            IniciarParpadeo(materialIncorrecto);
            return;
        }

        Vector3 posicionLocal = directionReference.InverseTransformPoint(drone.transform.position);

        // Evaluamos en qué lado del aro espacialmente está el dron AHORA MISMO
        if (posicionLocal.z < 0)
        {
            // Viene por el lado bueno
            IniciarParpadeo(materialCorrecto);
        }
        else
        {
            // Viene por el lado malo (en contra de la flecha azul)
            IniciarParpadeo(materialIncorrecto);
        }
    }

    public void OnWarningExit()
    {
        if (completed || inside) return;

        // Se abortó la aproximación
        DetenerParpadeo();
        SetMaterial(materialActivado);
    }

    public void OnDangerEnter(Collider drone)
    {
        if (completed) return;

        // Un destello rojo más rápido
        DetenerParpadeo();
        parpadeoCoroutine = StartCoroutine(RutinaParpadeoRapido(materialCollision));
    }

    public void OnDangerExit(Collider drone)
    {
        if (completed) return;

        // Al salir de la zona de peligro sin chocar, vuelve a evaluar donde está usando la lógica de OnWarning 
        // o simplemente lo apagas y esperas que la próxima área le dé color.
        DetenerParpadeo();
        SetMaterial(materialActivado);

        // Si está inside (zona buena), la recuperamos
        if (inside) IniciarParpadeo(materialCorrecto);
    }


    // MÉTODOS DE EFECTOS VISUALES:

    private void IniciarParpadeo(Material materialEfecto)
    {
        DetenerParpadeo();
        parpadeoCoroutine = StartCoroutine(RutinaParpadeo(materialEfecto));
    }

    private void DetenerParpadeo()
    {
        if (parpadeoCoroutine != null)
        {
            StopCoroutine(parpadeoCoroutine);
            parpadeoCoroutine = null;
        }
    }

    private IEnumerator RutinaParpadeo(Material matDestello)
    {
        while (true) 
        {
            SetMaterial(matDestello);
            yield return new WaitForSeconds(0.15f); // Tiempo del flash
            SetMaterial(materialActivado);
            yield return new WaitForSeconds(0.15f); // Tiempo de la pausa
        }
    }

    private IEnumerator RutinaParpadeoRapido(Material matDestello)
    {
        while (true)
        {
            SetMaterial(matDestello);
            yield return new WaitForSeconds(0.05f); // 3x más rápido
            SetMaterial(materialActivado);
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator DestelloError()
    {
        SetMaterial(materialCollision);
        yield return new WaitForSeconds(0.5f);
        SetMaterial(materialActivado); // Vuelve a la normalidad (tiene que intentarlo de nuevo)
    }
}