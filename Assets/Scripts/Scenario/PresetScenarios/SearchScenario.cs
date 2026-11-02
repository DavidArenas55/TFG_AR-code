
using UnityEngine;

public static class SearchScenario
{
    public static void Load(
        Transform parent,
        GameObject treeType1,
        GameObject treeType1Modified,
        GameObject treeType3Modified1,
        GameObject treeType3Modified2,
        GameObject fountain,
        GameObject gazebo,
        GameObject bench,
        GameObject lamp,
        GameObject table,
        GameObject safeTower)
    {
        Object.Instantiate(treeType1, new Vector3(21f, -0.1f - 1f, 0.7f), Quaternion.identity, parent);
        Object.Instantiate(treeType3Modified1, new Vector3(46.34401f, -0.1f - 1f, 9.111881f), Quaternion.Euler(0, 21f, 0), parent);
        Object.Instantiate(treeType3Modified2, new Vector3(29.74926f, -0.1f - 1f, -15.09937f), Quaternion.Euler(0, 208.454f, 0), parent);
        Object.Instantiate(treeType1, new Vector3(34.74667f, -0.1f - 1f, -17.05057f), Quaternion.identity, parent);
        Object.Instantiate(treeType1Modified, new Vector3(41.1f, -0.1f - 1f, -15.16579f), Quaternion.identity, parent);
        Object.Instantiate(treeType1Modified, new Vector3(46.19419f, -0.1f - 1f, -12.46974f), Quaternion.identity, parent);

        Object.Instantiate(fountain, new Vector3(38.92875f, 0f - 1f, -4.350088f), Quaternion.identity, parent);
        Object.Instantiate(gazebo, new Vector3(62.8f, 0f - 1f, 12f), Quaternion.Euler(0, -154.28f, 0), parent);

        Object.Instantiate(bench, new Vector3(40.42196f, 0f - 1f, -11.40582f), Quaternion.Euler(0, -9.05f, 0), parent);
        Object.Instantiate(bench, new Vector3(38.92283f, 0f - 1f, 2.530954f), Quaternion.Euler(0, 180.95f, 0), parent);
        Object.Instantiate(bench, new Vector3(31.985f, 0f - 1f, -3.914904f), Quaternion.Euler(0, 90.95f, 0), parent);
        Object.Instantiate(bench, new Vector3(45.63258f, 0f - 1f, -3.571131f), Quaternion.Euler(0, 260.95f, 0), parent);
        Object.Instantiate(bench, new Vector3(44.54998f, 0f - 1f, -8.543865f), Quaternion.Euler(0, -59.05f, 0), parent);
        Object.Instantiate(bench, new Vector3(33.84671f, 0f - 1f, 0.3948231f), Quaternion.Euler(0, 130.95f, 0), parent);
        Object.Instantiate(bench, new Vector3(43.38337f, 0f - 1f, 0.7167492f), Quaternion.Euler(0, 220.95f, 0), parent);
        Object.Instantiate(bench, new Vector3(35.14811f, 0f - 1f, -10.38824f), Quaternion.Euler(0, 35.95f, 0), parent);

        Object.Instantiate(lamp, new Vector3(65.11864f, 0f - 1f, -5.722569f), Quaternion.identity, parent);

        Object.Instantiate(table, new Vector3(67f, 0f - 1f, -3f), Quaternion.Euler(0, -17.05f, 0), parent);
        Object.Instantiate(table, new Vector3(68.38045f, 0f - 1f, -6.8468f), Quaternion.Euler(0, -17.05f, 0), parent);
        Object.Instantiate(table, new Vector3(63.42261f, 0f - 1f, -8.484831f), Quaternion.Euler(0, -17.05f, 0), parent);
        Object.Instantiate(table, new Vector3(62.11896f, 0f - 1f, -4.472668f), Quaternion.Euler(0, -17.05f, 0), parent);

        Object.Instantiate(safeTower, new Vector3(7.43573f, 1.8f, -2.405746f), Quaternion.Euler(0, -108f, 0), parent);
    }
}
