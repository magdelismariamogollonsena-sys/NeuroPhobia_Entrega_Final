using TMPro;
using UnityEngine;

public class CalmPulseUI : MonoBehaviour
{
    public RectTransform pulse;
    public TMP_Text instruction;
    public float inhaleSeconds = 4f;
    public float holdSeconds = 7f;
    public float exhaleSeconds = 8f;
    public float minScale = 0.55f;
    public float maxScale = 1.05f;

    private float timer;

    private void OnEnable()
    {
        timer = 0f;
        Apply(minScale, "INHALA");
    }

    private void Update()
    {
        float cycle = inhaleSeconds + holdSeconds + exhaleSeconds;
        timer = Mathf.Repeat(timer + Time.deltaTime, cycle);
        if (timer < inhaleSeconds)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / inhaleSeconds);
            Apply(Mathf.Lerp(minScale, maxScale, t), "INHALA");
        }
        else if (timer < inhaleSeconds + holdSeconds)
        {
            Apply(maxScale, "MANTÉN");
        }
        else
        {
            float t = Mathf.SmoothStep(0f, 1f, (timer - inhaleSeconds - holdSeconds) / exhaleSeconds);
            Apply(Mathf.Lerp(maxScale, minScale, t), "EXHALA");
        }
    }

    private void Apply(float scale, string phase)
    {
        if (pulse != null) pulse.localScale = Vector3.one * scale;
        if (instruction != null) instruction.text = $"PAUSA DE CALMA\n{phase}\n4 · 7 · 8";
    }
}
