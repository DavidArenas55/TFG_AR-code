using csDronLink;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class FenceGenerator : MonoBehaviour
{
    private Dron dron;

    [Header("Referencia a DroneTelemetry")]
    public DroneTelemetry droneTelemetry;

    [Header("World Anchor")]
    public WorldAnchor worldAnchor;

    [Header("Polígono de inclusión (lat, lon)")]
    public List<Vector2> inclusionGPS;

    [Header("Parámetros")]
    public float margen = 0.1f;

    void Start()
    {
        //Esperamos un poco para que DroneTelemetry tenga tiempo de inicializarse y detectar el dron, si es que está presente desde el inicio.
        Invoke(nameof(IntentarGenerarEscenario), 3f);
    }

    void IntentarGenerarEscenario()
    {
        if (droneTelemetry == null)
        {
            Debug.LogWarning("DroneTelemetry no asignado");
            return;
        }

        dron = droneTelemetry.GetDron();

        if (dron != null)
        {
            Debug.Log("Dron detectado → generando escenario");

            var scenario = ConstruirEscenario();

            Debug.Log("Enviando escenario al dron...");

            //new Thread(() =>
            //{
                Debug.Log("Entrando en EstableceEscenario...");
                dron.EstableceEscenario(scenario);
                Debug.Log("FENCES ENVIADOS AL DRON");
            //}).Start();
        }
    }

    List<List<(double lat, double lon)>> ConstruirEscenario()
    {
        List<List<(double lat, double lon)>> scenario =
            new List<List<(double lat, double lon)>>();

        // INCLUSIÓN
        List<(double, double)> inclusion = new List<(double, double)>();
        foreach (var p in inclusionGPS)
        {
            inclusion.Add((p.x, p.y));
        }
        scenario.Add(inclusion);

        // PAREDES MANUALES
        List<List<Vector3>> paredes = new List<List<Vector3>>();

        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Easy || 
            ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Hard ||
            ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom)
        {
            paredes = GetScenarioParedes();
        }

        if (paredes.Count > 15)
        {
            Debug.LogWarning($"Se encontraron {paredes.Count} paredes, pero el límite de Geofences de DroneLink es 15. Se descartarán las restantes.");
        }

        foreach (var pared in paredes.Take(15))
        {
            var expandida = ExpandirRectangulo(pared, margen);

            List<(double, double)> fence = new List<(double, double)>();

            foreach (var p in expandida)
            {
                var gps = worldAnchor.UnityToGPS(p);
                fence.Add((gps.latitude, gps.longitude));
            }

            scenario.Add(fence);
        }
        
        Debug.Log($"Escenario construido con {scenario.Count} fences");

        return scenario;
    }

    private List<List<Vector3>> GetScenarioParedes()
    {
        var walls = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(t => t.name.StartsWith("Wall_"))
            .Select(t => t.gameObject)
            .Distinct()
            .OrderBy(w => GetWallIndex(w.name))
            .ToList();

        List<List<Vector3>> paredes = new List<List<Vector3>>();

        foreach (var wall in walls)
        {
            if (TryGetWallVertices(wall, out var vertices))
            {
                paredes.Add(vertices);
            }
        }

        return paredes;

        bool TryGetWallVertices(GameObject wall, out List<Vector3> vertices)
        {
            var transforms = wall.GetComponentsInChildren<Transform>();
            Transform v_bl = transforms.FirstOrDefault(t => t.CompareTag("VertexBottomLeft"));
            Transform v_br = transforms.FirstOrDefault(t => t.CompareTag("VertexBottomRight"));
            Transform v_tl = transforms.FirstOrDefault(t => t.CompareTag("VertexTopLeft"));
            Transform v_tr = transforms.FirstOrDefault(t => t.CompareTag("VertexTopRight"));

            if (v_bl != null && v_br != null && v_tl != null && v_tr != null)
            {
                vertices = new List<Vector3>
                {
                    V(v_bl.position.x, v_bl.position.z),
                    V(v_br.position.x, v_br.position.z),
                    V(v_tr.position.x, v_tr.position.z),
                    V(v_tl.position.x, v_tl.position.z)
                };
                return true;
            }

            vertices = null;
            return false;
        }

        int GetWallIndex(string name)
        {
            const string prefix = "Wall_";
            if (name.StartsWith(prefix) && int.TryParse(name.Substring(prefix.Length), out var index))
            {
                return index;
            }

            return int.MaxValue;
        }
    }


    // Expande el rectángulo hacia fuera (margen de seguridad)
    List<Vector3> ExpandirRectangulo(List<Vector3> puntos, float margen)
    {
        // Centro
        Vector3 centro = Vector3.zero;
        foreach (var p in puntos) centro += p;
        centro /= puntos.Count;

        // Ejes del rectángulo (direcciones principales)
        Vector3 dir1 = (puntos[1] - puntos[0]).normalized;
        Vector3 dir2 = (puntos[3] - puntos[0]).normalized;

        List<Vector3> resultado = new List<Vector3>();

        foreach (var p in puntos)
        {
            Vector3 offset = Vector3.zero;

            // decidir hacia qué lado expandir en cada eje
            Vector3 local = p - centro;

            float sign1 = Mathf.Sign(Vector3.Dot(local, dir1));
            float sign2 = Mathf.Sign(Vector3.Dot(local, dir2));

            offset += dir1 * sign1 * margen;
            offset += dir2 * sign2 * margen;

            resultado.Add(p + offset);
        }

        return resultado;
    }

    Vector3 V(float x, float z) => new Vector3(x, 0, z);

    // Dibuja el polígono de inclusión y las paredes en la pestaña Scene del editor de Unity
    void OnDrawGizmos() 
    {
        if (worldAnchor == null) return;

        // INCLUSIÓN
        if (inclusionGPS != null && inclusionGPS.Count >= 3)
        {
            Gizmos.color = Color.blue;

            for (int i = 0; i < inclusionGPS.Count; i++)
            {
                Vector2 p1 = inclusionGPS[i];
                Vector2 p2 = inclusionGPS[(i + 1) % inclusionGPS.Count];

                Vector3 worldP1 = worldAnchor.GPSToUnity(p1.x, p1.y);
                Vector3 worldP2 = worldAnchor.GPSToUnity(p2.x, p2.y);

                worldP1.y = 0;
                worldP2.y = 0;

                Gizmos.DrawLine(worldP1, worldP2);
            }
        }

        // PAREDES
        Gizmos.color = Color.red;
        List<List<Vector3>> paredes = new List<List<Vector3>>();
        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Easy || 
            ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Hard ||
            ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom)
        {
            paredes = GetScenarioParedes();
        }

        foreach (var pared in paredes.Take(15))
        {
            var expandida = ExpandirRectangulo(pared, margen);

            for (int i = 0; i < expandida.Count; i++)
            {
                Vector3 p1 = expandida[i];
                Vector3 p2 = expandida[(i + 1) % expandida.Count];

                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}