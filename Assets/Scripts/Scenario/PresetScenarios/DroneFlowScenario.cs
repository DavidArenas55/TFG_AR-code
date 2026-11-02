using System.Collections.Generic;
using UnityEngine;

public class DroneFlowScenario
{
    public static int LoadLevel(
        int level,
        Transform parent,
        GameObject ring,
        GameObject square)
    {
        // Limpiar los objetos del nivel anterior.
        foreach (Transform child in parent)
        {
            Object.Destroy(child.gameObject);
        }

        // El nivel 5 se gestiona aparte porque combina gates y gemas.
        if (level == 5)
        {
            LoadLevel5(parent, ring, square);
            return 8;
        }

        List<GameObject> targets = new List<GameObject>();

        switch (level)
        {
            case 1:
                targets.Add(Object.Instantiate(ring, new Vector3(15.75526f, 4f, -5.571545f), Quaternion.Euler(90f, -18f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(17.62516f, 4f, -11.62338f), Quaternion.Euler(90f, -18f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(19.83f, 4f, -17.7557f), Quaternion.Euler(90f, -18f, 0f), parent));
                break;

            case 2:
                targets.Add(Object.Instantiate(ring, new Vector3(26.7f, 4f, -19.2f), Quaternion.Euler(90f, 252f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(30f, 4f, -10.6f), Quaternion.Euler(90f, 162f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(28.1301f, 4f, -4.548166f), Quaternion.Euler(90f, 162f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(20.1f, 4f, 1.1f), Quaternion.Euler(90f, 72f, 0f), parent));
                break;

            case 3:
                targets.Add(Object.Instantiate(square, new Vector3(21.15f, 2f, 1.35f), Quaternion.Euler(0f, 90f, 0f), parent));
                targets.Add(Object.Instantiate(square, new Vector3(37.1f, 2f, 2.3f), Quaternion.Euler(0f, 90f, 0f), parent));
                targets.Add(Object.Instantiate(square, new Vector3(54.4f, 2f, 1.5f), Quaternion.Euler(0f, 180f, 0f), parent));
                targets.Add(Object.Instantiate(square, new Vector3(41.7f, 2f, -11.4f), Quaternion.Euler(0f, -90f, 0f), parent));
                targets.Add(Object.Instantiate(square, new Vector3(25.6f, 2f, -20.3f), Quaternion.Euler(0f, -90f, 0f), parent));
                break;

            case 4:
                targets.Add(Object.Instantiate(ring, new Vector3(36.7f, 4f, -16f), Quaternion.Euler(90f, 252f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(41.9f, 4f, -10.7f), Quaternion.Euler(90f, 207f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(42.1f, 4f, -2.6f), Quaternion.Euler(90f, 162f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(37.2f, 4f, 2.9f), Quaternion.Euler(90f, 117f, 0f), parent));
                targets.Add(Object.Instantiate(ring, new Vector3(29.98f, 4f, 4.35f), Quaternion.Euler(90f, 72f, 0f), parent));
                break;

            default:
                Debug.LogWarning("Nivel Drone Flow no válido: " + level);
                return 0;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            GateTrigger gate = targets[i].GetComponent<GateTrigger>();
            if (gate == null)
                gate = targets[i].AddComponent<GateTrigger>();

            gate.waypointIndex = i;
        }

        return targets.Count;
    }

    private static void LoadLevel5(
        Transform parent,
        GameObject ring,
        GameObject square)
    {
        InstantiateDroneFlowGate(ring, new Vector3(30f, 4f, 4f), Quaternion.Euler(90f, -18f, 0f), 0, parent);
        InstantiateDroneFlowGate(square, new Vector3(37.5f, 2f, 0.5f), Quaternion.Euler(0f, 90f, 0f), 2, parent);
        InstantiateDroneFlowGate(ring, new Vector3(44.6f, 4f, -3.8f), Quaternion.Euler(90f, -18f, 0f), 4, parent);
        InstantiateDroneFlowGate(square, new Vector3(41.7f, 2f, -11f), Quaternion.Euler(0f, -90f, 0f), 6, parent);

        GemaSpawner gemSpawner = Object.FindFirstObjectByType<GemaSpawner>();

        if (gemSpawner != null)
            gemSpawner.SpawnDroneFlowLevel5Gems(parent);
        else
            Debug.LogWarning("No se encontró GemaSpawner para las gemas del Nivel 5.");
    }

    private static void InstantiateDroneFlowGate(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        int waypointIndex,
        Transform parent)
    {
        GameObject obj = Object.Instantiate(prefab, position, rotation, parent);
        GateTrigger gate = obj.GetComponentInChildren<GateTrigger>();
        gate.waypointIndex = waypointIndex;
    }
}
