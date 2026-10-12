using UnityEngine;

public class GatePart : MonoBehaviour
{
    public enum Type { Entry, Exit, Warning, Danger }
    public Type type;

    private GateTrigger gate;

    void Start()
    {
        gate = GetComponentInParent<GateTrigger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Drone")) return;

        if (type == Type.Entry)
            gate.OnEntry(other);
        else if (type == Type.Exit)
            gate.OnExit(other);
        else if (type == Type.Warning)   
            gate.OnWarningEnter(other);
        else if (type == Type.Danger)
            gate.OnDangerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Drone")) return;

        // Avisamos a GateTrigger solo cuando salimos del aro de entrada
        if (type == Type.Entry)
            gate.OnEntryExit(other);
        else if (type == Type.Warning) 
            gate.OnWarningExit();
        else if (type == Type.Danger)
            gate.OnDangerExit(other);
    }
}