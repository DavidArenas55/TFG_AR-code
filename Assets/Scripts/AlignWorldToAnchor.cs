using UnityEngine;

public class AlignWorldToAnchor : MonoBehaviour
{
    public Transform worldRoot;      // padre del mundo (red/escenario)
    public Transform anchorCorner;   // esquina del modelo que corresponde al anchor GPS
    private Vector3 anchorUnityPoint = new Vector3(0f, -1f, 0f);

    void Start()
    {
        if (!worldRoot || !anchorCorner)
        {
            Debug.LogError("AlignWorldToAnchor: asigna worldRoot y anchorCorner.");
            return;
        }
        
        Vector3 offset = anchorUnityPoint - anchorCorner.position;
        worldRoot.position += offset;
    }
}