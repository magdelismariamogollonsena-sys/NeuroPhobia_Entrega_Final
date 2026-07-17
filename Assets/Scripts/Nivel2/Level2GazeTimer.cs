using UnityEngine;

public class Level2GazeTimer : MonoBehaviour
{
    [Header("Referencias")]
    public Camera playerCamera;
    public Transform spiderTarget;
    public Level2HUD hud;
    public MetricsCollector metricsCollector;

    [Header("Configuracion")]
    public float requiredGazeSeconds = 30f;
    public float gazeAngleThreshold = 10f;
    public bool requireLineOfSight = false;
    public float raycastDistance = 50f;

    [Header("Estado")]
    [SerializeField] private float continuousGazeSeconds;
    [SerializeField] private float bestAttemptSeconds;
    [SerializeField] private bool completed;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (metricsCollector == null)
        {
            metricsCollector = FindFirstObjectByType<MetricsCollector>();
        }

        UpdateHUD();
    }

    private void Update()
    {
        if (completed) return;
        if (playerCamera == null || spiderTarget == null) return;

        bool isLooking = IsLookingAtSpider();

        if (isLooking)
        {
            continuousGazeSeconds += Time.deltaTime;
            if (continuousGazeSeconds > bestAttemptSeconds)
            {
                bestAttemptSeconds = continuousGazeSeconds;
            }
        }
        else
        {
            continuousGazeSeconds = 0f;
        }

        if (continuousGazeSeconds >= requiredGazeSeconds)
        {
            CompleteLevel();
        }

        UpdateHUD();
    }

    public bool IsCompleted()
    {
        return completed;
    }

    public float GetBestAttemptSeconds()
    {
        return bestAttemptSeconds;
    }

    private bool IsLookingAtSpider()
    {
        Vector3 directionToSpider = spiderTarget.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToSpider);

        if (angle > gazeAngleThreshold)
        {
            return false;
        }

        if (!requireLineOfSight)
        {
            return true;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
        {
            return hit.transform == spiderTarget || hit.transform.IsChildOf(spiderTarget);
        }

        return false;
    }

    private void CompleteLevel()
    {
        completed = true;
        continuousGazeSeconds = requiredGazeSeconds;
        bestAttemptSeconds = Mathf.Max(bestAttemptSeconds, requiredGazeSeconds);

        if (hud != null)
        {
            hud.SetGazeProgress(continuousGazeSeconds, requiredGazeSeconds, bestAttemptSeconds);
            hud.ShowCompleted(true);
        }

        if (metricsCollector != null && SessionManager.Instance != null)
        {
            LevelMetrics metrics = metricsCollector.BuildMetrics(true);
            metrics.level = 2;
            metrics.gazeTimeOnSpider = requiredGazeSeconds;
            SessionManager.Instance.AddLevelMetrics(metrics);
        }

        Debug.Log("NeuroPhobia Nivel 2: mirada continua completada.");
    }

    private void UpdateHUD()
    {
        if (hud != null)
        {
            hud.SetGazeProgress(continuousGazeSeconds, requiredGazeSeconds, bestAttemptSeconds);
        }
    }
}
