using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Level3Checkpoint : MonoBehaviour
{
    public int checkpointOrder = 1;
    public Level3ProgressController progressController;
    private bool reached;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (reached) return;
        Transform root = other.transform.root;
        bool isPlayer = root.name.Contains("XR Origin") || other.CompareTag("Player") || other.GetComponentInParent<CharacterController>() != null;
        if (!isPlayer) return;
        reached = true;
        if (progressController != null) progressController.RegisterCheckpoint(checkpointOrder);
    }
}
