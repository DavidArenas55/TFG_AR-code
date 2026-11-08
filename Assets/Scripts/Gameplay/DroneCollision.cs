using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DroneCollision : MonoBehaviour
{
    public BoxCollider box;

    public GameObject redFrame;

    void Start()
    {
        if (redFrame != null)
        {
            redFrame.SetActive(false); 
        }
    }

    void Update()
    {
        Vector3 center = box.transform.TransformPoint(box.center);

        Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, box.transform.rotation);

        bool crashed = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("ObstacleBody"))
            {
                crashed = true;
                Debug.Log(" Colisión REAL detectada contra el marco del objeto");
                StartCoroutine(ObstacleHitWarning());

                break;
            }
        }

        // Si chocó, anulamos la gate que se está intentado cruzar
        if (crashed)
        {
            foreach (var hit in hits)
            {
                GatePart gatePart = hit.GetComponent<GatePart>();
                if (gatePart != null)
                {
                    GateTrigger gate = gatePart.GetComponentInParent<GateTrigger>();
                    if (gate != null)
                    {
                        gate.CancelRun();
                    }
                }
            }
        }
    }

    IEnumerator ObstacleHitWarning()
    {

        if (redFrame != null)
        {
            redFrame.SetActive(true); // Mostrar frame rojo

            yield return new WaitForSeconds(2f); // Esperar 2 segundos

            redFrame.SetActive(false); // Ocultar frame rojo
        }
    }
}