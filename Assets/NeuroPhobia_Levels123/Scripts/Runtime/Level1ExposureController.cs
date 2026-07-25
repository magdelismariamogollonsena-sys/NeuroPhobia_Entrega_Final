using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level1ExposureController : MonoBehaviour
{
    [Header("Referencias")]
    public Camera playerCamera;
    public BreathingSphereGuide breathingGuide;
    public List<GameObject> spiderCards = new List<GameObject>();
    public TMP_Text statusText;
    public Button continueButton;

    [Header("Tiempos reales")]
    public float baselineSeconds = 30f;
    public float requiredGazePerCard = 5f;
    public int requiredCards = 3;
    public float gazeAngleThreshold = 12f;

    [Header("Prueba rápida en computador")]
    public bool fastTimingsInEditor = true;
    public float editorBaselineSeconds = 3f;
    public float editorGazeSeconds = 2f;

    private float baselineTimer;
    private float currentGaze;
    private int currentCard;
    private int completedCards;
    private bool exposureStarted;
    private bool completed;

    private float ActiveBaseline => Application.isEditor && fastTimingsInEditor ? editorBaselineSeconds : baselineSeconds;
    private float ActiveGaze => Application.isEditor && fastTimingsInEditor ? editorGazeSeconds : requiredGazePerCard;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.RemoveListener(GoToLevel2);
            continueButton.onClick.AddListener(GoToLevel2);
        }
        SetAllCards(false);
        UpdateStatus();
    }

    private void Update()
    {
        if (completed) return;
        if (playerCamera == null) playerCamera = Camera.main;

        if (!exposureStarted)
        {
            baselineTimer += Time.deltaTime;
            if (baselineTimer >= ActiveBaseline)
            {
                exposureStarted = true;
                ShowCurrentCard();
            }
            UpdateStatus();
            return;
        }

        if (currentCard < spiderCards.Count && IsLookingAt(spiderCards[currentCard]))
        {
            currentGaze += Time.deltaTime;
            if (currentGaze >= ActiveGaze) CompleteCurrentCard();
        }
        else currentGaze = 0f;

        bool breathingDone = breathingGuide == null || breathingGuide.IsCompleted();
        if (completedCards >= requiredCards && breathingDone) CompleteLevel();
        UpdateStatus();
    }

    private bool IsLookingAt(GameObject card)
    {
        if (card == null || playerCamera == null || !card.activeInHierarchy) return false;
        Vector3 direction = card.transform.position - playerCamera.transform.position;
        return Vector3.Angle(playerCamera.transform.forward, direction) <= gazeAngleThreshold;
    }

    private void CompleteCurrentCard()
    {
        completedCards++;
        currentGaze = 0f;
        if (currentCard < spiderCards.Count && spiderCards[currentCard] != null) spiderCards[currentCard].SetActive(false);
        currentCard++;
        if (currentCard < spiderCards.Count) ShowCurrentCard();
    }

    private void ShowCurrentCard()
    {
        SetAllCards(false);
        if (currentCard < spiderCards.Count && spiderCards[currentCard] != null) spiderCards[currentCard].SetActive(true);
    }

    private void SetAllCards(bool value)
    {
        foreach (GameObject card in spiderCards) if (card != null) card.SetActive(value);
    }

    private void CompleteLevel()
    {
        completed = true;
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        if (statusText != null) statusText.text = "Nivel 1 completado. Pulsa Continuar.";
    }

    private void UpdateStatus()
    {
        if (statusText == null || completed) return;
        if (!exposureStarted)
        {
            statusText.text = $"Respira con calma. Preparación: {Mathf.CeilToInt(Mathf.Max(0f, ActiveBaseline - baselineTimer))} s";
            return;
        }
        bool breathingDone = breathingGuide == null || breathingGuide.IsCompleted();
        statusText.text = $"Imágenes observadas: {completedCards}/{requiredCards}\nFijación actual: {currentGaze:0.0}/{ActiveGaze:0}s\nRespiración: {(breathingDone ? "completada" : "en progreso")}";
    }

    public void GoToLevel2() => SceneManager.LoadScene("Nivel2");
}
