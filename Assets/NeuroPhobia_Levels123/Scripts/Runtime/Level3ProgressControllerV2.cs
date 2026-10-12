using UnityEngine;

public class Level3ProgressControllerV2 : MonoBehaviour
{
    public Level3HUD hud;
    public Level3SpiderPatrolV2 spiderPatrol;
    public MetricsCollector metricsCollector;
    private bool cp1;
    private bool cp2;
    private bool completed;

    private void Start()
    {
        if (metricsCollector == null) metricsCollector = FindFirstObjectByType<MetricsCollector>();
        UpdateHUD();
    }

    public void RegisterCheckpoint(int order)
    {
        if (completed) return;
        if (order == 1 && !cp1)
        {
            cp1 = true;
            if (hud != null) hud.ShowWarning("Primer punto alcanzado. Respira y continúa.", 4f);
            if (spiderPatrol != null) spiderPatrol.SetReducedSpeed(true);
        }
        else if (order == 2 && cp1 && !cp2)
        {
            cp2 = true;
            CompleteLevel();
        }
        else if (order == 2 && !cp1 && hud != null) hud.ShowWarning("Primero alcanza el punto situado a 3 metros.");
        UpdateHUD();
    }

    private void CompleteLevel()
    {
        completed = true;
        if (spiderPatrol != null) spiderPatrol.PauseMovement();
        if (hud != null) hud.ShowCompleted(true);
        if (metricsCollector != null && SessionManager.Instance != null)
        {
            LevelMetrics metrics = metricsCollector.BuildMetrics(true);
            metrics.level = 3;
            metrics.taskCompletions.Add("CP1_3M");
            metrics.taskCompletions.Add("CP2_1M");
            SessionManager.Instance.AddLevelMetrics(metrics);
        }
        GlobalSessionUI.CompleteLevel(3);
    }

    private void UpdateHUD()
    {
        int count = (cp1 ? 1 : 0) + (cp2 ? 1 : 0);
        if (hud != null) hud.SetCheckpointProgress(count, 2);
    }
}
