using System.Collections.Generic;
using UnityEngine;

public class Level3AdaptiveCalmV2 : MonoBehaviour
{
    public Transform playerRoot;
    public Transform spiderTarget;
    public Level3SpiderPatrolV2 spiderPatrol;
    public Level3HUD hud;
    public GameObject breathingOverlay;
    public MetricsCollector metricsCollector;
    public AudioSource calmAudio;

    public float sampleInterval = 0.2f;
    public float headTurnEventDegrees = 22f;
    public int headEventsForTrigger = 8;
    public float headWindowSeconds = 3f;
    public float retreatMeters = 2f;
    public float retreatWindowSeconds = 1f;
    public float calmSeconds = 10f;

    private readonly Queue<float> headEvents = new Queue<float>();
    private float timer;
    private Vector3 lastPlayerPosition;
    private float lastPositionTime;
    private Quaternion lastHeadRotation;
    private float cooldownUntil;

    private void OnEnable() => GameStateManager.OnStateChanged += HandleState;
    private void OnDisable() => GameStateManager.OnStateChanged -= HandleState;

    private void Start()
    {
        ResolvePlayer();
        if (Camera.main != null) lastHeadRotation = Camera.main.transform.rotation;
        if (playerRoot != null) lastPlayerPosition = playerRoot.position;
        lastPositionTime = Time.time;
        if (breathingOverlay != null) breathingOverlay.SetActive(false);
        if (metricsCollector == null) metricsCollector = FindFirstObjectByType<MetricsCollector>();
    }

    private void Update()
    {
        if (Time.time < cooldownUntil) return;
        timer += Time.deltaTime;
        if (timer < sampleInterval) return;
        timer = 0f;
        if (playerRoot == null || !playerRoot.gameObject.activeInHierarchy) ResolvePlayer();
        if (playerRoot == null || spiderTarget == null || Camera.main == null) return;

        float headDelta = Quaternion.Angle(lastHeadRotation, Camera.main.transform.rotation);
        lastHeadRotation = Camera.main.transform.rotation;
        if (headDelta >= headTurnEventDegrees) headEvents.Enqueue(Time.time);
        while (headEvents.Count > 0 && Time.time - headEvents.Peek() > headWindowSeconds) headEvents.Dequeue();
        if (headEvents.Count >= headEventsForTrigger)
        {
            TriggerCalm("Movimiento de cabeza elevado. Haz una pausa y respira.");
            headEvents.Clear();
            return;
        }

        float elapsed = Time.time - lastPositionTime;
        if (elapsed >= retreatWindowSeconds)
        {
            float oldDistance = Vector3.Distance(lastPlayerPosition, spiderTarget.position);
            float newDistance = Vector3.Distance(playerRoot.position, spiderTarget.position);
            if (newDistance - oldDistance >= retreatMeters)
            {
                TriggerCalm("Detectamos un alejamiento rápido. Respira antes de continuar.");
                return;
            }
            lastPlayerPosition = playerRoot.position;
            lastPositionTime = Time.time;
        }
    }

    private void TriggerCalm(string message)
    {
        cooldownUntil = Time.time + calmSeconds;
        if (spiderPatrol != null) spiderPatrol.PauseFor(calmSeconds);
        if (hud != null) hud.ShowWarning(message, calmSeconds);
        if (metricsCollector != null) metricsCollector.RegisterBreathingActivation();
        if (breathingOverlay != null)
        {
            breathingOverlay.SetActive(true);
            CancelInvoke(nameof(HideOverlay));
            Invoke(nameof(HideOverlay), calmSeconds);
        }
        if (calmAudio != null)
        {
            calmAudio.loop = true;
            if (!calmAudio.isPlaying) calmAudio.Play();
        }
    }

    private void HideOverlay()
    {
        if (breathingOverlay != null) breathingOverlay.SetActive(false);
        if (calmAudio != null && calmAudio.isPlaying) calmAudio.Stop();
    }

    private void HandleState(GameState state)
    {
        if (state == GameState.BREATHING)
        {
            if (breathingOverlay != null) breathingOverlay.SetActive(true);
            if (calmAudio != null && !calmAudio.isPlaying) { calmAudio.loop = true; calmAudio.Play(); }
        }
        else if (state == GameState.PLAYING) HideOverlay();
    }

    private void ResolvePlayer()
    {
        GameObject desktop = GameObject.Find("DesktopTestRig");
        if (desktop != null && desktop.activeInHierarchy) { playerRoot = desktop.transform; return; }
        GameObject xr = GameObject.Find("XR Origin (XR Rig)");
        if (xr != null) playerRoot = xr.transform;
    }
}
