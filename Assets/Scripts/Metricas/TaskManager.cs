using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public MetricsCollector metricsCollector;
    public List<string> requiredTasks = new List<string> { "TASK_LAMP", "TASK_DOOR", "TASK_FRIDGE", "TASK_WINDOW" };
    public int avoidanceThreshold = 5;

    private readonly HashSet<string> completedTasks = new HashSet<string>();

    private void Start()
    {
        if (metricsCollector == null) metricsCollector = FindFirstObjectByType<MetricsCollector>();
    }

    public void CompleteTask(string taskId)
    {
        if (completedTasks.Contains(taskId)) return;
        completedTasks.Add(taskId);
        if (metricsCollector != null) metricsCollector.RegisterTaskCompletion(taskId);

        if (completedTasks.Count >= requiredTasks.Count)
        {
            LevelMetrics metrics = metricsCollector != null ? metricsCollector.BuildMetrics(true) : new LevelMetrics { level = 5, levelPassed = true };
            if (LevelManager.Instance != null) LevelManager.Instance.OnLevelComplete(5, metrics);
        }
    }
}
