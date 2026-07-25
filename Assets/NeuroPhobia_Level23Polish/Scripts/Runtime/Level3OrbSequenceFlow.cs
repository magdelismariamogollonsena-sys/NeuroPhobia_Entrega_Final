using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Secuencia visible de dos orbes para los puntos de aproximación del Nivel 3.
/// </summary>
public sealed class Level3OrbSequenceFlow : MonoBehaviour
{
    [SerializeField] private Transform playerRoot;
    [SerializeField] private List<GameObject> orbs = new List<GameObject>();
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image progressFill;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private Level3SpiderPatrolV2 spiderPatrol;
    [SerializeField, Min(0.2f)] private float collectDistance = 0.85f;
    [SerializeField, Min(0f)] private float completionDelay = 5f;
    [SerializeField] private string nextSceneName = "Nivel4";

    private int currentOrb;
    private float messageUntil;
    private bool completed;
    private float previousTimeScale = 1f;

    private void Start()
    {
        ResolvePlayer();
        orbs = orbs.Where(o => o != null).Distinct().ToList();
        for (int i = 0; i < orbs.Count; i++) orbs[i].SetActive(i == 0);
        if (completionPanel != null) completionPanel.SetActive(false);
        ShowMessage("Acércate al primer orbe. La araña nunca irá hacia ti.", 5f);
        UpdateUI();
    }

    private void Update()
    {
        if (completed) return;
        if (playerRoot == null || !playerRoot.gameObject.activeInHierarchy) ResolvePlayer();
        if (playerRoot == null || currentOrb >= orbs.Count) return;

        GameObject orb = orbs[currentOrb];
        if (orb != null)
        {
            Vector3 a = playerRoot.position; a.y = 0f;
            Vector3 b = orb.transform.position; b.y = 0f;
            if (Vector3.Distance(a, b) <= collectDistance) CollectCurrentOrb();
        }

        if (messageText != null && messageText.gameObject.activeSelf && Time.unscaledTime >= messageUntil)
            messageText.gameObject.SetActive(false);
    }

    private void CollectCurrentOrb()
    {
        if (currentOrb < orbs.Count && orbs[currentOrb] != null) orbs[currentOrb].SetActive(false);
        currentOrb++;
        if (currentOrb < orbs.Count)
        {
            orbs[currentOrb].SetActive(true);
            if (spiderPatrol != null) spiderPatrol.SetReducedSpeed(true);
            ShowMessage("Primer punto alcanzado. Respira y avanza hacia el segundo orbe.", 5f);
            UpdateUI();
            return;
        }
        BeginCompletion();
    }

    private void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"NIVEL 3 · APROXIMACIÓN PROGRESIVA\nOrbes alcanzados: {currentOrb} / {orbs.Count}";
        if (progressFill != null)
            progressFill.fillAmount = orbs.Count == 0 ? 0f : (float)currentOrb / orbs.Count;
    }

    private void ShowMessage(string message, float seconds)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        messageUntil = Time.unscaledTime + seconds;
    }

    private void BeginCompletion()
    {
        completed = true;
        UpdateUI();
        if (spiderPatrol != null) spiderPatrol.PauseMovement();
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
                completionText.text = "¡Felicitaciones!\n\nCompletaste el Nivel 3\n\n" +
                                      $"El Nivel 4 comenzará en {Mathf.CeilToInt(remaining)} segundos.";
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }
        if (completionText != null)
            completionText.text = "¡Felicitaciones!\n\nCompletaste el Nivel 3\n\nCargando Nivel 4...";
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        SceneManager.LoadScene(nextSceneName);
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

    private void OnDestroy()
    {
        if (Time.timeScale == 0f) Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
    }
}
