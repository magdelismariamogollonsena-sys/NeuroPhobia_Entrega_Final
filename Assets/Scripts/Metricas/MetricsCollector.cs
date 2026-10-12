using System.Collections.Generic;
using UnityEngine;

public class MetricsCollector : MonoBehaviour
{
    public int level = 1;
    public Transform player;
    public Transform spider;
    private AranaController[] level5Spiders;
    public string spiderLayerName = "Spider";

    private float levelStartTime;
    private float gazeTimeOnSpider;
    private int avoidanceAttempts;
    private float proximityMin = float.MaxValue;
    private int breathingActivations;
    private int levelAttempts;
    private int handWithdrawals;
    private float handContactDuration;
    private Quaternion lastHeadRotation;
    private float headMovementFrequency;
    private float sampleTimer;
    private Vector3 lastPlayerPosition;
    private readonly List<string> taskCompletions = new List<string>();

    private void Start()
    {
        levelStartTime = Time.time;
        if (player == null && Camera.main != null) player = Camera.main.transform;
        if (player != null)
        {
            lastHeadRotation = player.rotation;
            lastPlayerPosition = player.position;
        }
    }

    private void Update()
    {
        if (GameStateManager.CurrentState != GameState.PLAYING) return;
        TrackGaze();
        SampleMovement();
    }

    public void RegisterTaskCompletion(string taskId)
    {
        if (!taskCompletions.Contains(taskId)) taskCompletions.Add(taskId);
    }

    public void SetSpiders(AranaController[] activeSpiders) { level5Spiders = activeSpiders; }

    public void RegisterBreathingActivation() { breathingActivations++; }
    public void RegisterLevelAttempt() { levelAttempts++; }
    public void RegisterHandWithdrawal() { handWithdrawals++; }
    public void AddHandContactTime(float seconds) { handContactDuration += Mathf.Max(0f, seconds); }

    public LevelMetrics BuildMetrics(bool passed)
    {
        return new LevelMetrics
        {
            level = level,
            gazeTimeOnSpider = gazeTimeOnSpider,
            avoidanceAttempts = avoidanceAttempts,
            headMovementFrequency = headMovementFrequency,
            proximityMin = proximityMin == float.MaxValue ? -1f : proximityMin,
            taskCompletions = new List<string>(taskCompletions),
            breathingActivations = breathingActivations,
            levelPassed = passed,
            sessionDuration = Time.time - levelStartTime,
            levelAttempts = levelAttempts,
            handWithdrawals = handWithdrawals,
            handContactDuration = handContactDuration
        };
    }

    private void TrackGaze()
    {
        if (Camera.main == null) return;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            bool isSpider = hit.collider.gameObject.layer == LayerMask.NameToLayer(spiderLayerName)
                || hit.collider.name.ToLowerInvariant().Contains("spider")
                || hit.collider.name.ToLowerInvariant().Contains("arana")
                || hit.collider.name.ToLowerInvariant().Contains("araña");

            if (isSpider) gazeTimeOnSpider += Time.deltaTime;
        }
    }

    private void SampleMovement()
    {
        if (player == null) return;
        sampleTimer += Time.deltaTime;
        if (sampleTimer < 0.5f) return;

        float deltaAngle = Quaternion.Angle(lastHeadRotation, player.rotation);
        headMovementFrequency = deltaAngle / sampleTimer / 90f;

        Transform nearestSpider = spider;
        if (level5Spiders != null)
        {
            float nearestDistance = float.MaxValue;
            foreach (AranaController candidate in level5Spiders)
            {
                if (candidate == null) continue;
                float distanceToCandidate = Vector3.Distance(player.position, candidate.transform.position);
                if (distanceToCandidate < nearestDistance) { nearestDistance = distanceToCandidate; nearestSpider = candidate.transform; }
            }
        }
        if (nearestSpider != null)
        {
            float distance = Vector3.Distance(player.position, nearestSpider.position);
            if (distance < proximityMin) proximityMin = distance;

            float moved = Vector3.Distance(player.position, lastPlayerPosition);
            bool movedAwayFromSpider = Vector3.Distance(player.position, nearestSpider.position) > Vector3.Distance(lastPlayerPosition, nearestSpider.position);
            if (movedAwayFromSpider && moved > 2f && sampleTimer <= 1f) avoidanceAttempts++;
        }

        lastHeadRotation = player.rotation;
        lastPlayerPosition = player.position;
        sampleTimer = 0f;
    }
}
