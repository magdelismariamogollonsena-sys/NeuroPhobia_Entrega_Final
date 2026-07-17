using UnityEngine;

public class BreathingGuide : MonoBehaviour
{
    [SerializeField] private float cycleSeconds = 19f;
    [SerializeField] private float minScale = 0.22f;
    [SerializeField] private float maxScale = 0.45f;

    private Vector3 baseScale;

    private void Start()
    {
        baseScale = Vector3.one;
        MetricsCollector metrics = FindFirstObjectByType<MetricsCollector>();
        if (metrics != null) metrics.RegisterBreathingActivation();
    }

    private void Update()
    {
        float t = Mathf.PingPong(Time.time / cycleSeconds, 1f);
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * scale;
    }
}
