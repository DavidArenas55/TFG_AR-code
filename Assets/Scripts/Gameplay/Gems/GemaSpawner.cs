using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemaSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject gemaPrefab; public GameObject gemaFinishPrefab;
    public GameObject gem; public GameObject gem2; public GameObject gem3; public GameObject gem4; 
    public GameObject gem5; public GameObject gem6; public GameObject gem7; public GameObject gem8;
    public GameObject gem_rot; public GameObject gem3_rot; public GameObject gem4_rot; public GameObject gem5_rot; public GameObject gem6_rot;

    [Header("Scenario Search")]
    public GameObject scenarioSearch;

    [Header("Zona de spawn")]
    public Transform spawnArea;

    [Header("Gemas")]
    public int numberOfGems = 15;
    public Text gemsCollectedText;
    public GameObject Points;

    [Header("Drone")]
    public Transform drone;
    public float minDistanceFromDrone = 3f;

    [Header("Altura")]
    public float minHeight = 3f;
    public float maxHeight = 10f;

    [Header("Colisiones")]
    public LayerMask wallLayer;
    public float checkRadius = 1.0f; 

    [Header("Timing")]
    public float spawnDelay = 3f;

    [Header("Intentos")]
    public int maxAttempts = 200;

    List<Vector3> spawnedPositions = new List<Vector3>();

    [Header("Separación entre gemas")]
    public float minDistanceBetweenGems = 8f;

    Transform gemsParent;

    public static int numberOfSearchGems;
    public static int numberOfCustomScoreGems = 0;

    void Start()
    {
        GemProximityDestroy.ResetGemsCollected();

        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Circuit || ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.DroneFlow)
        {
            Points.SetActive(false);
            return;
        }
        Points.SetActive(true);

        GameObject parent = new GameObject("Gems");
        parent.transform.SetParent(transform, false);
        gemsParent = parent.transform;

        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Easy)
        {
            SpawnEasyMazeGem();
        }
        else if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Hard)
        {
            SpawnHardMazeGem();
        }


        if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Search)
        {
            SpawnSearchGems();
            numberOfSearchGems = gemsParent.childCount;
        }
        else if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Custom)
        {
            SpawnCustomScenarioGems();
        }
        else
        {
            StartCoroutine(DelayedSpawn());
        }
    }

    void Update()
    {
        UpdateGemsCollectedText();
    }

    void UpdateGemsCollectedText()
    {
        if (gemsCollectedText != null)
        {
            if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Search)
            {
                gemsCollectedText.text = $"{GemProximityDestroy.gemsCollected}/{numberOfSearchGems}";
                return;
            }
            else
            {
                gemsCollectedText.text = $"{GemProximityDestroy.gemsCollected}/{numberOfGems}";
            }
        }
    }

    void SpawnEasyMazeGem()
    {
        // Gema final que indica la salida del laberinto. Se coloca en una posición fija.
        Instantiate(gemaFinishPrefab, new Vector3(60.82829f, 4f, -11.18962f), Quaternion.identity);
    }

    void SpawnHardMazeGem()
    {
        // Gema final que indica la salida del laberinto. Se coloca en una posición fija.
        Instantiate(gemaFinishPrefab, new Vector3(68.20993f, 6f, 12.36673f), Quaternion.identity);
    }

    void SpawnSearchGems()
    {
        gemsParent.SetParent(scenarioSearch.transform);
        Instantiate(gem_rot, new Vector3(40.51151f, 4f, -3.93784f), Quaternion.identity, gemsParent);
        Instantiate(gem3_rot, new Vector3(65.11861f, 4f, -5.722569f), Quaternion.identity, gemsParent);
        Instantiate(gem4_rot, new Vector3(63.04151f, 4f, 11.74216f), Quaternion.identity, gemsParent);
        Instantiate(gem5_rot, new Vector3(50.4015f, 4f, -15.71784f), Quaternion.identity, gemsParent);
        Instantiate(gem6_rot, new Vector3(8.351517f, 4.1f, -2.187841f), Quaternion.identity, gemsParent);
    }

    public void SpawnDroneFlowLevel5Gems(Transform parent)
    {
        // Gemas a DISPARAR del nivel 5. Los índices deben encajar con los huecos que se dejaron en ScenarioManager
        SpawnShootGem(gemaPrefab, new Vector3(37.2f, 4f, -17.2f), 1, parent);
        SpawnShootGem(gemaPrefab, new Vector3(60.1f, 4f, 8f), 3, parent);
        SpawnShootGem(gemaPrefab, new Vector3(48.3f, 4f, -14.5f), 5, parent);
        SpawnShootGem(gemaFinishPrefab, new Vector3(19.5f, 4f, -18.7f), 7, parent);
    }

    void SpawnShootGem(GameObject prefab, Vector3 pos, int waypointIndex, Transform parent)
    {
        GameObject g = Instantiate(prefab, pos, Quaternion.identity, parent);
        GemProximityDestroy script = g.GetComponentInChildren<GemProximityDestroy>();
        if (script != null)
        {
            script.requireShooting = true;   // hay que dispararla, no colisionar
            script.isScoreTarget = false;  // NO suma puntos
            script.isDestructibleGem = true;   
            script.isFinalGem = false;
            script.waypointIndex = waypointIndex; // respeta el orden
        }
    }

    void SpawnCustomScenarioGems()
    {
        WorldAnchor worldAnchor = FindFirstObjectByType<WorldAnchor>();
        int scoreGemsCount = 0;

        for (int i = 0; i < ConnectionConfig.CustomScenarioObjects.Count; i++)
        {
            var obj = ConnectionConfig.CustomScenarioObjects[i];

            if (obj.type.Contains("Gem"))
            {
                GameObject prefab = GetGemPrefabType(obj.type);
                if (prefab == null) continue;

                // A) MODO RANDOM
                if (obj.gemsRandomPosition == true && obj.numberOfGems.HasValue)
                {
                    for (int j = 0; j < obj.numberOfGems.Value; j++)
                    {
                        Vector3 pos = GetValidRandomPosition();
                        if (pos != Vector3.zero)
                        {
                            GameObject g = Instantiate(prefab, pos, Quaternion.identity, gemsParent);
                            ConfigureGem(g, obj, -1); // Las Random no pueden ser circuit targets
                            if (obj.scoreTarget == true) scoreGemsCount++;
                        }
                        else
                        {
                            Debug.LogWarning("No se encontró espacio suficiente para spawnear todas las gemas aleatorias solicitadas.");
                        }
                    }
                }
                // B) MODO POSICIÓN FIJA (Coordenadas)
                else
                {
                    Vector3 pos = worldAnchor.GPSToUnity(obj.latitude, obj.longitude) + new Vector3(0, (float)obj.altitude, 0);
                    Vector3 gemPos = pos + new Vector3(0, -1f, 0);
                    Quaternion rot = Quaternion.Euler(0, (float)obj.heading - 18f, 0);

                    GameObject g = Instantiate(prefab, gemPos, rot, gemsParent);

                    // Calcular índice del circuito unificamente de forma idéntica
                    int myCircuitIndex = -1;
                    if (obj.circuitTarget == true)
                    {
                        myCircuitIndex = 0;
                        for (int k = 0; k < i; k++)
                        {
                            if (ConnectionConfig.CustomScenarioObjects[k].circuitTarget == true)
                                myCircuitIndex++;
                        }
                    }

                    ConfigureGem(g, obj, myCircuitIndex);
                    if (obj.scoreTarget == true) scoreGemsCount++;
                }
            }
        }

        numberOfGems = scoreGemsCount; // Actualiza el total para el Text UI
        numberOfCustomScoreGems = scoreGemsCount;
    }

    void ConfigureGem(GameObject gemObj, ScenarioObjectData data, int circuitIdx)
    {
        GemProximityDestroy script = gemObj.GetComponentInChildren<GemProximityDestroy>();
        if (script != null)
        {
            script.requireShooting = (data.destructionMethod == "Shoot");
            script.isScoreTarget = data.scoreTarget ?? false;  //ignora lo que se marque en el inspector del componente GemProximityDestroy de la gema.
            script.isFinalGem = data.endGameObject ?? false;  //ignora lo que se marque en el inspector del componente GemProximityDestroy de la gema.
            script.waypointIndex = circuitIdx;
        }
    }

    Vector3 GetValidRandomPosition()
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 pos = GetRandomPointInArea();
            pos.y = Random.Range(minHeight, maxHeight);
            if (IsValidPosition(pos))
            {
                spawnedPositions.Add(pos);
                return pos;
            }
        }
        return Vector3.zero;
    }

    GameObject GetGemPrefabType(string type)
    {
        switch (type)
        {
            case "BigGem1": return gemaPrefab;
            case "BigGem2": return gemaFinishPrefab;
            case "Gem1": return gem;
            case "Gem2": return gem2;
            case "Gem3": return gem3;
            case "Gem4": return gem4;
            case "Gem5": return gem5;
            case "Gem6": return gem6;
            case "Gem7": return gem7;
            case "Gem8": return gem8;
            default: return gem;
        }
    }

    IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnGems(gemaPrefab);
    }

    void SpawnGems(GameObject gemPrefab)
    {
        int spawned = 0;
        int attempts = 0;

        while (spawned < numberOfGems && attempts < maxAttempts)
        {
            attempts++;

            Vector3 pos = GetRandomPointInArea();
            if (ConnectionConfig.SelectedScenario == ConnectionConfig.Scenario.Easy)
            {
                pos.y = 4f;
            }
            else
            {
                pos.y = Random.Range(minHeight, maxHeight);
            }

            if (IsValidPosition(pos))
            {
                GameObject gema = Instantiate(gemPrefab, pos, Quaternion.identity);
                gema.transform.SetParent(gemsParent, true);
                spawnedPositions.Add(pos); // guardar posición
                spawned++;
            }
        }

        Debug.Log($"Gemas generadas: {spawned}");
    }

    Vector3 GetRandomPointInArea()
    {
        BoxCollider box = spawnArea.GetComponent<BoxCollider>();
        Vector3 size = box.size;

        float x = Random.Range(-size.x / 2f, size.x / 2f);
        float z = Random.Range(-size.z / 2f, size.z / 2f);

        Vector3 localPoint = new Vector3(x, 0, z);

        return spawnArea.TransformPoint(localPoint);
    }

    bool IsValidPosition(Vector3 pos)
    {
        // cerca del dron
        if (Vector3.Distance(pos, drone.position) < minDistanceFromDrone)
            return false;

        // colisión con paredes
        if (Physics.CheckSphere(pos, checkRadius, wallLayer))
            return false;

        // Colisión con cualquier cosa (anillos, arcs, gemas normales, etc.)
        Collider[] colliders = Physics.OverlapSphere(pos, checkRadius);
        foreach (Collider col in colliders)
        {
            // Ignoramos suelo, zonas invisibles de las gates o al propio dron
            if (col.CompareTag("Obstacle") || col.CompareTag("FinishPoint") || col.gameObject.name.Contains("Gem"))
            {
                return false;
            }
        }

        // cerca de otras gemas
        foreach (Vector3 existing in spawnedPositions)
        {
            float horizontalDist = Vector2.Distance(
                new Vector2(pos.x, pos.z),
                new Vector2(existing.x, existing.z)
            );

            float heightDiff = Mathf.Abs(pos.y - existing.y);

            if (horizontalDist < minDistanceBetweenGems)
            {
                return false;
            }
        }
        return true;
    }
}