using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Level3CheckpointV2 : MonoBehaviour
{
    public int checkpointOrder = 1;
    public Level3ProgressControllerV2 progressController;
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
        bool player = root.name.Contains("XR Origin") || root.name.Contains("DesktopTestRig") || other.CompareTag("Player") || other.GetComponentInParent<CharacterController>() != null;
        if (!player) return;
        reached = true;
        if (progressController != null) progressController.RegisterCheckpoint(checkpointOrder);
    }
}
