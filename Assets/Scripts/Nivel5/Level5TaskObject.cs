using UnityEngine;

public enum Level5TaskType
{
    Lamp,
    Door,
    Fridge,
    Window
}

public class Level5TaskObject : MonoBehaviour
{
    public Level5TaskType taskType;
    public Level5DomesticTaskManager taskManager;

    [SerializeField] private bool completed;

    public void CompleteTask()
    {
        if (completed) return;

        completed = true;

        if (taskManager != null)
            taskManager.RegisterTask(taskType.ToString());

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = Color.green;
    }
}
