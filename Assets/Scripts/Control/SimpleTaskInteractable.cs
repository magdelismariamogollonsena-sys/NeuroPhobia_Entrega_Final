using UnityEngine;

public class SimpleTaskInteractable : MonoBehaviour
{
    [SerializeField] private string taskId = "TASK";
    private bool completed;

    private void OnMouseDown()
    {
        Complete();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Camera>() != null || other.name.ToLowerInvariant().Contains("camera")) Complete();
    }

    public void Complete()
    {
        if (completed) return;
        completed = true;
        TaskManager manager = FindFirstObjectByType<TaskManager>();
        if (manager != null) manager.CompleteTask(taskId);
        gameObject.name = taskId + "_COMPLETED";
    }
}
