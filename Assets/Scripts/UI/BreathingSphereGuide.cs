using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Guia visual de respiracion 4-7-8.
/// Azul: inhala y crece.
/// Amarillo: mantiene EXACTAMENTE el tamano grande.
/// Verde: exhala y se reduce.
/// </summary>
public class BreathingSphereGuide : MonoBehaviour
{
    [Header("Tiempos")]
    [SerializeField] private float inhaleSeconds = 4f;
    [SerializeField] private float holdSeconds = 7f;
    [SerializeField] private float exhaleSeconds = 8f;
    [SerializeField] private int cyclesToComplete = 3;

    [Header("Escala")]
    [SerializeField] private float minSize = 0.25f;
    [SerializeField] private float maxSize = 0.65f;

    [Header("Colores")]
    [SerializeField] private Color inhaleColor = new Color(0.25f, 0.55f, 1f, 1f);
    [SerializeField] private Color holdColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color exhaleColor = new Color(0.25f, 1f, 0.45f, 1f);
    [SerializeField] private Color completedColor = Color.white;

    [Header("Texto opcional")]
    [SerializeField] private TMP_Text instructionTMP;
    [SerializeField] private Text instructionText;

    private Renderer sphereRenderer;
    private MaterialPropertyBlock materialBlock;
    private Coroutine routine;
    private int completedCycles;
    private bool completed;

    private Vector3 MinScale => Vector3.one * minSize;
    private Vector3 MaxScale => Vector3.one * maxSize;

    private void Awake()
    {
        sphereRenderer = GetComponent<Renderer>();
        materialBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        StartBreathing();
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    public void StartBreathing()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        completedCycles = 0;
        completed = false;
        transform.localScale = MinScale;
        routine = StartCoroutine(BreathingRoutine());
    }

    public bool IsCompleted()
    {
        return completed;
    }

    public int GetCompletedCycles()
    {
        return completedCycles;
    }

    private IEnumerator BreathingRoutine()
    {
        while (completedCycles < cyclesToComplete)
        {
            SetVisual("INHALA", inhaleColor);
            yield return ScaleOverTime(MinScale, MaxScale, inhaleSeconds);

            // Clave: forzamos el tamano grande ANTES y DURANTE todo el tiempo amarillo.
            SetVisual("MANTEN", holdColor);
            transform.localScale = MaxScale;
            yield return HoldLarge(holdSeconds);

            SetVisual("EXHALA", exhaleColor);
            yield return ScaleOverTime(MaxScale, MinScale, exhaleSeconds);

            completedCycles++;
        }

        completed = true;
        transform.localScale = MinScale;
        SetVisual("RESPIRACION COMPLETADA", completedColor);
        routine = null;
    }

    private IEnumerator HoldLarge(float seconds)
    {
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            transform.localScale = MaxScale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = MaxScale;
    }

    private IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float seconds)
    {
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            float t = Mathf.Clamp01(elapsed / seconds);
            float smoothT = t * t * (3f - 2f * t);
            transform.localScale = Vector3.Lerp(from, to, smoothT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = to;
    }

    private void SetVisual(string text, Color color)
    {
        SetInstruction(text);
        SetColor(color);
    }

    private void SetInstruction(string text)
    {
        if (instructionTMP != null) instructionTMP.text = text;
        if (instructionText != null) instructionText.text = text;
    }

    private void SetColor(Color color)
    {
        if (sphereRenderer == null) return;

        sphereRenderer.GetPropertyBlock(materialBlock);
        materialBlock.SetColor("_BaseColor", color);
        materialBlock.SetColor("_Color", color);
        sphereRenderer.SetPropertyBlock(materialBlock);
    }
}
