#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SaveMeshesEditor : MonoBehaviour
{
    [ContextMenu("Guardar meshes de hijos")]
    void SaveMeshes()
    {
        foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;

            string path = "Assets/Fragments8/" + mf.name + ".asset";

            Mesh meshCopy = Instantiate(mf.sharedMesh);
            AssetDatabase.CreateAsset(meshCopy, path);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Meshes guardadas");
    }
}
#endif
