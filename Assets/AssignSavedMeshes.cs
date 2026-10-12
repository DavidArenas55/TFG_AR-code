#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class AssignSavedMeshes : MonoBehaviour
{
    [ContextMenu("Reasignar meshes")]
    void AssignMeshes()
    {
        foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
        {
            string path = "Assets/Fragments8/" + mf.name + ".asset";
            Mesh savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

            if (savedMesh != null)
                mf.sharedMesh = savedMesh;
        }
    }
}
#endif