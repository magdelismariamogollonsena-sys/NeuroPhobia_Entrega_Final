using UnityEngine;
using UnityEngine.UI;

public class BarraEstres : MonoBehaviour
{
    public Slider slider;

    public void SetProgress(float normalizedValue)
    {
        if (slider != null) slider.value = Mathf.Clamp01(normalizedValue);
    }
}
