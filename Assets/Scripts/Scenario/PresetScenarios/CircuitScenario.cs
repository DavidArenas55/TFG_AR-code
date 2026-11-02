
using System.Collections.Generic;
using UnityEngine;

public static class CircuitScenario
{
    public static int Load(
        Transform parent,
        GameObject circleTakeOff,
        GameObject circleLand,
        GameObject cylinder,
        GameObject ring,
        GameObject arc,
        GameObject tunnel)
    {
        List<GameObject> circuitTargets = new List<GameObject>();

        Object.Instantiate(circleTakeOff,
            new Vector3(12.65f, 0f - 1f, -25.71f),
            Quaternion.Euler(0f, 70f, 0f), parent);

        Object.Instantiate(circleLand,
            new Vector3(5f, 0f - 1f, -3.17f),
            Quaternion.Euler(0f, 70f, 0f), parent);

        circuitTargets.Add(Object.Instantiate(cylinder,
            new Vector3(67.96819f, 6f - 1f, -3.70529f),
            Quaternion.Euler(0f, 0.95f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(ring,
            new Vector3(43.34064f, 7f, -11.24994f),
            Quaternion.Euler(90f, 70f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(ring,
            new Vector3(38.56079f, 7f, -13.01093f),
            Quaternion.Euler(90f, 70f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(ring,
            new Vector3(33.75945f, 7f, -14.86158f),
            Quaternion.Euler(90f, 70f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(cylinder,
            new Vector3(20.86407f, 6f - 1f, -15.62595f),
            Quaternion.Euler(180f, 135.95f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(ring,
            new Vector3(28.31402f, 7f, -6.700171f),
            Quaternion.Euler(90f, 70f + 180f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(arc,
            new Vector3(44.10566f, 0f - 1f, -1.599268f),
            Quaternion.Euler(0f, 90f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(arc,
            new Vector3(49.23645f, 0f - 1f, 0.2159138f),
            Quaternion.Euler(0f, 90f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(arc,
            new Vector3(54.57055f, 0f - 1f, 2.227753f),
            Quaternion.Euler(0f, 90f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(cylinder,
            new Vector3(59.22807f, 6f - 1f, 12.04179f),
            Quaternion.Euler(0f, -44.05f, 0f), parent));

        circuitTargets.Add(Object.Instantiate(tunnel,
            new Vector3(32.18615f, 6f + 1f, 4.85722f),
            Quaternion.Euler(90f, 70f, 0f), parent));

        // Asignamos cantidad de Gates a superar y sus índices
        for (int i = 0; i < circuitTargets.Count; i++)
        {
            GateTrigger waypoint =
                circuitTargets[i].GetComponentInChildren<GateTrigger>();

            if (waypoint == null)
                waypoint = circuitTargets[i].AddComponent<GateTrigger>();

            waypoint.waypointIndex = i;
        }

        return circuitTargets.Count;
    }
}
