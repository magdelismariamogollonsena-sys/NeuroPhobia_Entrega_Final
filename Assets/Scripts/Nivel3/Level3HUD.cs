using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level3HUD : MonoBehaviour
{
    public TMP_Text instructionText;
    public TMP_Text checkpointText;
    public TMP_Text warningText;
    public TMP_Text completionText;
    public Button continueButton;
    public string nextSceneName = "Nivel4";
    private float warningTimer;

    private void Start()
    {
        if (instructionText != null)
            instructionText.text = "Nivel 3: Aproximación progresiva\nAcércate primero al punto de 3 metros y luego al punto de 1 metro.";
        SetCheckpointProgress(0, 2);
        ShowCompleted(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(GoToNextLevel);
        }
    }

    private void Update()
    {
        if (warningText == null || !warningText.gameObject.activeSelf) return;
        warningTimer -= Time.deltaTime;
        if (warningTimer <= 0f) warningText.gameObject.SetActive(false);
    }

    public void SetCheckpointProgress(int completed, int total)
    {
        if (checkpointText != null) checkpointText.text = $"Puntos alcanzados: {completed} / {total}";
    }

    public void ShowWarning(string message, float seconds = 3f)
    {
        if (warningText == null) return;
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        warningTimer = seconds;
    }

    public void ShowCompleted(bool show)
    {
        if (completionText != null)
        {
            completionText.gameObject.SetActive(show);
            if (show) completionText.text = "Nivel 3 completado\nPuedes continuar al Nivel 4.";
        }
        if (continueButton != null) continueButton.gameObject.SetActive(show);
    }

    public void GoToNextLevel() => SceneManager.LoadScene(nextSceneName);
}
