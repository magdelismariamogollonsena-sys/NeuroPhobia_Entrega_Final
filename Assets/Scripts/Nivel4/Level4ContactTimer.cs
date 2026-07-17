using UnityEngine;

public class Level4ContactTimer : MonoBehaviour
{
    public Transform spiderTarget;
    public Transform leftHand;
    public Transform rightHand;
    public Level4TerrariumController terrarium;
    public Level4HUD hud;
    public MetricsCollector metricsCollector;

    public float contactDistance = 0.35f;
    public float requiredContactSeconds = 10f;

    [SerializeField] private float contactSeconds;
    [SerializeField] private bool completed;

    private void Start()
    {
        if (metricsCollector == null)
            metricsCollector = FindFirstObjectByType<MetricsCollector>();

        if (leftHand == null)
            leftHand = FindTransformByNames("Left Controller", "LeftHand Controller", "LeftHand");

        if (rightHand == null)
            rightHand = FindTransformByNames("Right Controller", "RightHand Controller", "RightHand");

        UpdateHUD();
    }

    private void Update()
    {
        if (completed || spiderTarget == null) return;

        if (terrarium != null && !terrarium.IsOpen())
        {
            contactSeconds = 0f;
            UpdateHUD();
            return;
        }

        bool handNear = IsNear(leftHand) || IsNear(rightHand);

        if (handNear)
            contactSeconds += Time.deltaTime;
        else
            contactSeconds = 0f;

        if (contactSeconds >= requiredContactSeconds)
            CompleteLevel();

        UpdateHUD();
    }

    private bool IsNear(Transform hand)
    {
        if (hand == null) return false;

        return Vector3.Distance(hand.position, spiderTarget.position) <= contactDistance;
    }

    private void CompleteLevel()
    {
        completed = true;
        contactSeconds = requiredContactSeconds;

        if (hud != null)
            hud.ShowCompleted(true);

        if (metricsCollector != null && SessionManager.Instance != null)
        {
            LevelMetrics metrics = metricsCollector.BuildMetrics(true);
            metrics.level = 4;
            metrics.taskCompletions.Add("TERRARIUM_OPENED");
            metrics.taskCompletions.Add("HAND_CONTACT_TIMER_COMPLETED");
            SessionManager.Instance.AddLevelMetrics(metrics);
        }

        Debug.Log("NeuroPhobia Nivel 4: contacto virtual completado.");
    }

    private void UpdateHUD()
    {
        if (hud != null)
            hud.SetContactProgress(contactSeconds, requiredContactSeconds);
    }

    private Transform FindTransformByNames(params string[] names)
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            foreach (string name in names)
            {
                if (t.name.Contains(name))
                    return t;
            }
        }

        return null;
    }
}
