using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Flujo definitivo del Nivel 2: HUD fijo, mirada continua, mejor intento,
/// zona segura, pantalla de felicitaciones y carga automática del Nivel 3.
/// </summary>
public sealed class Level2ScreenGazeFlow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform spiderTarget;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private Image progressFill;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text completionText;

    [Header("Mirada")]
    [SerializeField, Min(1f)] private float requiredSeconds = 30f;
    [SerializeField, Range(0.02f, 0.35f)] private float viewportRadius = 0.10f;
    [SerializeField, Min(0f)] private float lookAwayGraceSeconds = 0.15f;

    [Header("Distancia segura")]
    [SerializeField, Min(0.5f)] private float safeDistance = 8f;
    [SerializeField, Min(0f)] private float returnMargin = 0.40f;

    [Header("Final")]
    [SerializeField, Min(0f)] private float completionDelay = 5f;
    [SerializeField] private string nextSceneName = "Nivel3";

    private float continuousSeconds;
    private float bestAttemptSeconds;
    private float lastValidLookTime = float.NegativeInfinity;
    private float warningUntil;
    private bool completed;
    private float previousTimeScale = 1f;

    private void Start()
    {
        ResolveCamera();
        ResolvePlayer();
        if (completionPanel != null) completionPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
        UpdateUI(false);
    }

    private void Update()
    {
        if (completed) return;
        if (playerCamera == null || !playerCamera.isActiveAndEnabled) ResolveCamera();
        if (playerRoot == null || !playerRoot.gameObject.activeInHierarchy) ResolvePlayer();

        EnforceSafeDistance();

        bool looking = IsLookingAtSpider();
        if (looking)
        {
            lastValidLookTime = Time.unscaledTime;
            continuousSeconds += Time.unscaledDeltaTime;
            bestAttemptSeconds = Mathf.Max(bestAttemptSeconds, continuousSeconds);
        }
        else if (Time.unscaledTime - lastValidLookTime > lookAwayGraceSeconds)
        {
            continuousSeconds = 0f;
        }

        if (continuousSeconds >= requiredSeconds)
        {
            continuousSeconds = requiredSeconds;
            bestAttemptSeconds = Mathf.Max(bestAttemptSeconds, requiredSeconds);
            BeginCompletion();
        }

        if (warningText != null && warningText.gameObject.activeSelf && Time.unscaledTime >= warningUntil)
            warningText.gameObject.SetActive(false);

        UpdateUI(looking);
    }

    private bool IsLookingAtSpider()
    {
        if (playerCamera == null || spiderTarget == null || !spiderTarget.gameObject.activeInHierarchy) return false;
        Vector3 targetPoint = GetTargetPoint(spiderTarget.gameObject);
        Vector3 viewport = playerCamera.WorldToViewportPoint(targetPoint);
        if (viewport.z <= 0f) return false;
        Vector2 offset = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);
        return offset.magnitude <= viewportRadius;
    }

    private void EnforceSafeDistance()
    {
        if (playerRoot == null || spiderTarget == null) return;
        Vector3 playerFlat = new Vector3(playerRoot.position.x, 0f, playerRoot.position.z);
        Vector3 spiderFlat = new Vector3(spiderTarget.position.x, 0f, spiderTarget.position.z);
        float distance = Vector3.Distance(playerFlat, spiderFlat);
        if (distance >= safeDistance) return;

        Vector3 direction = (playerFlat - spiderFlat).normalized;
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.back;
        Vector3 safe = spiderFlat + direction * (safeDistance + returnMargin);
        Vector3 destination = new Vector3(safe.x, playerRoot.position.y, safe.z);

        CharacterController controller = playerRoot.GetComponent<CharacterController>();
        if (controller == null) controller = playerRoot.GetComponentInChildren<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            playerRoot.position = destination;
            controller.enabled = true;
        }
        else playerRoot.position = destination;

        ShowWarning("Mantén una distancia segura de 8 metros.", 2f);
    }

    private void ShowWarning(string message, float seconds)
    {
        if (warningText == null) return;
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        warningUntil = Time.unscaledTime + seconds;
    }

    private void UpdateUI(bool looking)
    {
        if (timerText != null)
        {
            timerText.text =
                "NIVEL 2 · DISTANCIA SEGURA\n" +
                $"Mirada continua: {continuousSeconds:0.0} / {requiredSeconds:0} s\n" +
                $"Mejor intento: {bestAttemptSeconds:0.0} s\n" +
                (looking ? "Mantén la mirada" : "Mira directamente a la araña");
        }
        if (progressFill != null)
            progressFill.fillAmount = requiredSeconds <= 0f ? 0f : Mathf.Clamp01(continuousSeconds / requiredSeconds);
    }

    private void BeginCompletion()
    {
        if (completed) return;
        completed = true;
        if (completionPanel != null) completionPanel.SetActive(true);
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        StartCoroutine(CompletionCountdown());
    }

    private IEnumerator CompletionCountdown()
    {
        float remaining = completionDelay;
        while (remaining > 0f)
        {
            if (completionText != null)
                completionText.text = "¡Felicitaciones!\n\nCompletaste el Nivel 2\n\n" +
                                      $"El Nivel 3 comenzará en {Mathf.CeilToInt(remaining)} segundos.";
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (completionText != null)
            completionText.text = "¡Felicitaciones!\n\nCompletaste el Nivel 2\n\nCargando Nivel 3...";

        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        SceneManager.LoadScene(nextSceneName);
    }

    private void ResolveCamera()
    {
        if (Camera.main != null && Camera.main.isActiveAndEnabled) { playerCamera = Camera.main; return; }
        playerCamera = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(c => c != null && c.isActiveAndEnabled);
    }

    private void ResolvePlayer()
    {
        string[] preferred = { "NP_DesktopPlayer_Normalizado", "DesktopTestRig", "XR Origin (XR Rig)" };
        foreach (string name in preferred)
        {
            GameObject candidate = GameObject.Find(name);
            if (candidate != null && candidate.activeInHierarchy) { playerRoot = candidate.transform; return; }
        }
        CharacterController controller = FindFirstObjectByType<CharacterController>();
        if (controller != null) playerRoot = controller.transform;
    }

    private static Vector3 GetTargetPoint(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return target.transform.position;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds.center;
    }

    private void OnDestroy()
    {
        if (Time.timeScale == 0f) Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
    }
}
