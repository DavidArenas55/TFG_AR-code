
using System.Collections.Generic;
using UnityEngine;

public static class CustomScenario
{
    public static int Load(
        Transform parent,
        WorldAnchor worldAnchor,
        List<ScenarioObjectData> objects,
        GameObject ring,
        GameObject arc,
        GameObject tunnel,
        GameObject square,
        GameObject cylinder,
        GameObject circleTakeOff,
        GameObject circleLand,
        GameObject wallLeft,
        GameObject wallRight,
        GameObject wallMiddle)
    {
        int globalCircuitIndexCounter = 0;

        foreach (var obj in objects)
        {
            Vector3 position = worldAnchor.GPSToUnity(
                obj.latitude, obj.longitude) +
                new Vector3(0, (float)obj.altitude, 0);

            Quaternion rotation = Quaternion.Euler(
                90, (float)obj.heading, 0);

            GameObject instantiatedObj = null;

            switch (obj.type)
            {
                case "Ring":
                    Vector3 ringPosition = position + new Vector3(0, -1f, 0);
                    instantiatedObj = Object.Instantiate(
                        ring, ringPosition, rotation, parent);

                    float ringDiameter = obj.diameter ?? 2f;
                    float ringScale = ringDiameter / 200f;
                    instantiatedObj.transform.localScale =
                        new Vector3(ringScale, ringScale, ringScale);
                    break;

                case "Arc":
                    rotation = Quaternion.Euler(
                        0, (float)obj.heading + 180f + 18f, 0);

                    Vector3 arcPosition = position + new Vector3(0, -1f, 0);
                    instantiatedObj = Object.Instantiate(
                        arc, arcPosition, rotation, parent);
                    break;

                case "Tunnel":
                    rotation = Quaternion.Euler(
                        90, (float)obj.heading + 90f, 0);

                    Vector3 tunnelPosition = position + new Vector3(0, -1f, 0);
                    instantiatedObj = Object.Instantiate(
                        tunnel, tunnelPosition, rotation, parent);
                    break;

                case "Square":
                    rotation = Quaternion.Euler(
                        0, (float)obj.heading + 180f + 18f, 0);

                    Vector3 squarePosition = position + new Vector3(0, -2f - 1f, 0);
                    instantiatedObj = Object.Instantiate(
                        square, squarePosition, rotation, parent);
                    break;

                case "Cylinder":
                    if (obj.rotation == "Clockwise")
                    {
                        rotation = Quaternion.Euler(
                            180, (float)obj.heading + 18f + 55f, 0);
                    }
                    else
                    {
                        rotation = Quaternion.Euler(
                            0, (float)obj.heading + 18f, 0);
                    }

                    Vector3 cylinderPosition = position + new Vector3(0, 6f - 1f, 0);
                    instantiatedObj = Object.Instantiate(
                        cylinder, cylinderPosition, rotation, parent);
                    break;

                case "HTakeoff":
                    rotation = Quaternion.Euler(0, (float)obj.heading, 0);

                    Vector3 hTakeOffPosition = position + new Vector3(0, -1f, 0);
                    instantiatedObj = Object.Instantiate(
                        circleTakeOff, hTakeOffPosition, rotation, parent);
                    break;

                case "HLand":
                    rotation = Quaternion.Euler(0, (float)obj.heading, 0);

                    Vector3 hLandPosition = position + new Vector3(0, -1f, 0);
                    instantiatedObj = Object.Instantiate(
                        circleLand, hLandPosition, rotation, parent);
                    break;

                case "Wall":
                    int middleWallCount = obj.width.Value - 2;
                    float rotationY = (float)obj.heading;
                    Vector3 basePosition = position + new Vector3(0, 0.5f - 1f, 0);
                    int floorCount = obj.height.Value;
                    int wallCount = 0;

                    foreach (Transform child in parent)
                    {
                        if (child.name.StartsWith("Wall_"))
                        {
                            wallCount++;
                        }
                    }

                    MazeScenario.BuildWall(
                        parent,
                        middleWallCount,
                        rotationY,
                        basePosition,
                        floorCount,
                        wallCount + 1,
                        wallLeft,
                        wallRight,
                        wallMiddle
                    );
                    break;
            }

            // Configuración del GateTrigger para objetos tipo gate.
            // Las gemas tienen instantiatedObj == null.
            if (instantiatedObj != null &&
                (obj.circuitTarget == true || obj.endGameObject == true))
            {
                GateTrigger waypoint =
                    instantiatedObj.GetComponentInChildren<GateTrigger>();

                if (waypoint == null)
                    waypoint = instantiatedObj.AddComponent<GateTrigger>();

                if (obj.circuitTarget == true)
                    waypoint.waypointIndex = globalCircuitIndexCounter;

                if (obj.endGameObject == true)
                    waypoint.isFinalTarget = true;
            }

            if (obj.circuitTarget == true)
                globalCircuitIndexCounter++;
        }

        return globalCircuitIndexCounter;
    }
}
