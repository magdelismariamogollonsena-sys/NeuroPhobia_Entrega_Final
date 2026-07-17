using UnityEngine;

public class ExclusionZoneMessage : MonoBehaviour
{
    [SerializeField] private string message = "Mantén distancia segura.";

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Camera>() == null && !other.name.ToLowerInvariant().Contains("camera")) return;
        Debug.Log("NeuroPhobia: " + message);
    }
}
