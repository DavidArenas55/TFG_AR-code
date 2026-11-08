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
        Debug.Log($"[GATE {waypointIndex}] 🟢 EntryTrigger detectado");

        // Comprobación de orden
        RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
        bool isCorrectOrder = (raceTimer != null && raceTimer.targetsCollected == waypointIndex);

        if (waypointIndex != -1 && !isCorrectOrder &&
            (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit ||
             ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow ||
             ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom))
        {
            Debug.Log($"[GATE {waypointIndex}] ❌ Orden incorrecto. El dron va por la {raceTimer.targetsCollected}");
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
            Debug.Log($"[GATE {waypointIndex}] ✅ Entrada correcta. Z local: {posicionLocal.z}");
            IniciarParpadeo(materialCorrecto);
        }
        else
        {
            Debug.Log($"[GATE {waypointIndex}] ❌ Entrada incorrecta. Z local: {posicionLocal.z}");
            IniciarParpadeo(materialIncorrecto);
        }
    }

    // DETECTAR SI SALE RETROCEDIENDO
    public void OnEntryExit(Collider drone)
    {
        if (completed) return;
        Debug.Log($"[GATE {waypointIndex}] 🚪 EntryTrigger salido. inside = {inside}");

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
                Debug.Log($"[GATE {waypointIndex}] 🔙 Entrada anulada: el dron ha retrocedido");
            }
        }
    }

    public void CancelRun()
    {
        if (completed || !inside) return;

        inside = false;
        Debug.Log($"[GATE {waypointIndex}] 💥 Pasada invalidada debido a choque");

        DetenerParpadeo();
        StartCoroutine(DestelloError());
    }

    public void OnExit(Collider drone)
    {
        if (!inside || completed) return;

        Debug.Log($"[GATE {waypointIndex}] 🚪 ExitTrigger detectado. inside = {inside}");

        completed = true;
        inside = false;

        DetenerParpadeo();
        SetMaterial(materialDesactivado);
        Debug.Log($"[GATE {waypointIndex}] 🎯 Gate completado");
        if (UIManager.Instance != null)
            UIManager.Instance.ShowNotification("Gate Completed!");

        RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
        if (raceTimer != null)
        {
            // SI ESTE ERA EL ÚLTIMO OBJETIVO ABSOLUTO, FORZA EL FINAL PARA GANAR
            if (isFinalTarget)
            {
                Debug.Log($"[GATE {waypointIndex}] 🏁 Era el objetivo final. Finalizando juego");
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
        Debug.Log($"[GATE {waypointIndex}] 🟡 WarningTrigger detectado");

        RaceTimer raceTimer = FindFirstObjectByType<RaceTimer>();
        bool isCorrectOrder = (raceTimer != null && raceTimer.targetsCollected == waypointIndex);

        // Si se aproximas a un aro que NO te toca ahora, le meto destello de error
        if (waypointIndex != -1 && !isCorrectOrder &&
            (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit ||
             ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow ||
             ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom))
        {
            Debug.Log($"[GATE {waypointIndex}] ❌ Warning: orden incorrecto. El dron va por la {raceTimer.targetsCollected}");
            IniciarParpadeo(materialIncorrecto);
            return;
        }

        Vector3 posicionLocal = directionReference.InverseTransformPoint(drone.transform.position);
        Debug.Log($"[GATE {waypointIndex}] 🟡 Warning: Z local = {posicionLocal.z}");

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
        Debug.Log($"[GATE {waypointIndex}] 🟡 WarningTrigger salido");

        // Se abortó la aproximación
        DetenerParpadeo();
        SetMaterial(materialActivado);
    }

    public void OnDangerEnter(Collider drone)
    {
        if (completed) return;
        Debug.Log($"[GATE {waypointIndex}] 🔴 DangerTrigger detectado");
        // Un destello rojo más rápido
        DetenerParpadeo();
        parpadeoCoroutine = StartCoroutine(RutinaParpadeoRapido(materialCollision));
    }

    public void OnDangerExit(Collider drone)
    {
        if (completed) return;
        Debug.Log($"[GATE {waypointIndex}] 🔴 DangerTrigger salido");

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