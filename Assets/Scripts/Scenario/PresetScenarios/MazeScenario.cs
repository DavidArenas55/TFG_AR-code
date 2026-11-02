
using System.Collections.Generic;
using UnityEngine;

public static class MazeScenario
{
    public static void LoadEasy(
        Transform parent,
        GameObject wallLeft,
        GameObject wallRight,
        GameObject wallMiddle)
    {
        BuildWall(parent, 11, -17.05f, new Vector3(3.55f, 0.5f - 1f, -9.13f), 12, 1, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 2, 72.82f, new Vector3(14.30245f, 0.5f - 1f, -2.404995f), 12, 2, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 29, -17.05f, new Vector3(27.6054f, 0.5f - 1f, 1.758993f), 12, 3, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 25, -18f, new Vector3(46.34001f, 0.5f - 1f, -0.571003f), 12, 4, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 23, 72.82f, new Vector3(27.50053f, 0.5f - 1f, 1.284662f), 12, 5, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 13, -17.05f, new Vector3(42.99652f, 0.5f - 1f, -10.8419f), 12, 6, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 11, -17.05f, new Vector3(18.4781f, 0.5f - 1f, -11.27147f), 12, 7, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 8, 72.82f, new Vector3(18.23631f, 0.5f - 1f, -11.62051f), 12, 8, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 6, 72.82f, new Vector3(56.8027f, 0.5f - 1f, -7.272335f), 12, 9, wallLeft, wallRight, wallMiddle);
    }

    public static void LoadHard(
        Transform parent,
        GameObject wallLeft,
        GameObject wallRight,
        GameObject wallMiddle)
    {
        BuildWall(parent, 10, -17.05f, new Vector3(11.53328f, 0.5f - 1f, -2.84897f), 12, 1, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 11, 72.82f, new Vector3(11.366f, 0.5f - 1f, -3.161f), 12, 2, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 4, 72.82f, new Vector3(20.74113f, 0.5f - 1f, 5.597328f), 12, 3, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 20, -17.05f, new Vector3(18.21164f, 0.5f - 1f, -22.09454f), 12, 4, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 13, 72.82f, new Vector3(24.18879f, 0.5f - 1f, -5.123785f), 12, 5, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 3, 72.82f, new Vector3(38.79459f, 0.5f - 1f, -16.05497f), 12, 6, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 11, 72.82f, new Vector3(31.34671f, 0.5f - 1f, 8.861938f), 12, 7, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 19, 72.82f, new Vector3(41.71032f, 0.5f - 1f, 12.01054f), 12, 8, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 9, -17.05f, new Vector3(37.82368f, 0.5f - 1f, -10.37057f), 12, 9, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 9, -17.05f, new Vector3(46.22995f, 0.5f - 1f, -1.740282f), 12, 10, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 27, -17.05f, new Vector3(48.45221f, 0.5f - 1f, -12.35706f), 12, 11, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 3, 72.82f, new Vector3(58.25583f, 0.5f - 1f, -4.378506f), 12, 12, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 16, -17.05f, new Vector3(52.61269f, 0.5f - 1f, 10.55711f), 12, 13, wallLeft, wallRight, wallMiddle);
        BuildWall(parent, 13, 72.82f, new Vector3(62.09842f, 0.5f - 1f, 12.93264f), 12, 14, wallLeft, wallRight, wallMiddle);
    }

    public static void BuildWall(
        Transform mazeParent,
        int middleWallCount,
        float rotationY,
        Vector3 basePosition,
        int floorCount,
        int wallNumber,
        GameObject wallLeft,
        GameObject wallRight,
        GameObject wallMiddle)
    {
        int totalFloors = floorCount;

        GameObject wallParent = new GameObject($"Wall_{wallNumber}");
        wallParent.transform.SetParent(mazeParent);
        wallParent.transform.position = Vector3.zero;

        for (int j = 0; j < totalFloors; j++)
        {
            GameObject floorContainer = new GameObject($"Floor_{wallParent.transform.childCount + 1}");
            floorContainer.transform.SetParent(wallParent.transform);
            floorContainer.transform.localPosition = Vector3.zero;

            List<GameObject> walls = new List<GameObject>();
            walls.Add(Object.Instantiate(wallLeft));

            for (int i = 0; i < middleWallCount; i++)
            {
                walls.Add(Object.Instantiate(wallMiddle));
            }

            walls.Add(Object.Instantiate(wallRight));

            foreach (GameObject wall in walls)
            {
                wall.transform.SetParent(floorContainer.transform);
            }

            float wallWidth = 1.0f;

            for (int i = 0; i < walls.Count; i++)
            {
                walls[i].transform.localPosition = Vector3.right * wallWidth * i;
            }

            floorContainer.transform.localPosition = new Vector3(0f, j, 0f);
        }

        wallParent.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        wallParent.transform.position = basePosition;
    }
}
