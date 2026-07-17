using UnityEngine;
using UnityEngine.UI;

public class IndicadorAnsiedad : MonoBehaviour
{
    public Image targetImage;
    public Color green = Color.green;
    public Color yellow = Color.yellow;
    public Color red = Color.red;

    public void UpdateIndicator(float headMovementFrequency, int recentAvoidanceAttempts)
    {
        if (targetImage == null) return;
        if (headMovementFrequency > 2.5f || recentAvoidanceAttempts >= 2) targetImage.color = red;
        else if (headMovementFrequency >= 1.0f || recentAvoidanceAttempts == 1) targetImage.color = yellow;
        else targetImage.color = green;
    }
}
