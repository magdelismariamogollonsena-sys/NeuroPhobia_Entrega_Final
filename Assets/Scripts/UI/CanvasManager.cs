using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public GameObject infoPanel;
    public GameObject breathingPanel;
    public GameObject levelCompletePanel;
    public GameObject sessionSummaryPanel;

    public void ShowInfo(float seconds = 3f)
    {
        if (infoPanel == null) return;
        StopAllCoroutines();
        StartCoroutine(ShowThenHide(infoPanel, seconds));
    }

    public void ShowBreathing(bool show)
    {
        if (breathingPanel != null) breathingPanel.SetActive(show);
    }

    public void ShowLevelComplete(bool show)
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(show);
    }

    public void ShowSessionSummary(bool show)
    {
        if (sessionSummaryPanel != null) sessionSummaryPanel.SetActive(show);
    }

    private System.Collections.IEnumerator ShowThenHide(GameObject target, float seconds)
    {
        target.SetActive(true);
        yield return new WaitForSeconds(seconds);
        target.SetActive(false);
    }
}
