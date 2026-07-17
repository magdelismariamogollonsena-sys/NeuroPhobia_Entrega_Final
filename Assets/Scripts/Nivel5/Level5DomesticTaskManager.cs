using System.Collections.Generic;
using UnityEngine;

public class Level5DomesticTaskManager : MonoBehaviour
{
    public Level5HUD hud;
    public MetricsCollector metricsCollector;
    public int requiredTaskCount = 4;

    private readonly HashSet<string> completedTasks = new HashSet<string>();
    private bool completed;

    private void Start()
    {
        if (metricsCollector == null)
            metricsCollector = FindFirstObjectByType<MetricsCollector>();

        UpdateHUD();
    }

    public void RegisterTask(string taskId)
    {
        if (completed || string.IsNullOrWhiteSpace(taskId)) return;
        if (!completedTasks.Add(taskId)) return;

        if (metricsCollector != null)
            metricsCollector.RegisterTaskCompletion(taskId);

        UpdateHUD();

        if (completedTasks.Count >= requiredTaskCount)
            CompleteLevel();
    }

    private void CompleteLevel()
    {
        completed = true;

        if (hud != null)
            hud.ShowCompleted(true);

        if (metricsCollector != null && SessionManager.Instance != null)
        {
            LevelMetrics metrics = metricsCollector.BuildMetrics(true);
            metrics.level = 5;
            SessionManager.Instance.AddLevelMetrics(metrics);
        }

        Debug.Log("NeuroPhobia Nivel 5: tareas domésticas completadas.");
    }

    private void UpdateHUD()
    {
        if (hud != null)
            hud.SetTaskProgress(completedTasks.Count, requiredTaskCount);
    }
}
