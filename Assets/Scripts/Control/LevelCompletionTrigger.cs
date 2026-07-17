using UnityEngine;

public class LevelCompletionTrigger : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private bool completeLevel = true;
    [SerializeField] private string playerTag = "MainCamera";

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !completeLevel) return;
        if (!IsPlayer(other)) return;
        Complete();
    }

    private void OnMouseDown()
    {
        if (triggered || !completeLevel) return;
        Complete();
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag(playerTag) || other.GetComponentInParent<Camera>() != null || other.name.ToLowerInvariant().Contains("camera");
    }

    private void Complete()
    {
        triggered = true;
        MetricsCollector collector = FindFirstObjectByType<MetricsCollector>();
        LevelMetrics metrics = collector != null ? collector.BuildMetrics(true) : new LevelMetrics { level = level, levelPassed = true };
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelComplete(level, metrics);
    }
}
