using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level5HUD : MonoBehaviour
{
    public TMP_Text instructionText;
    public TMP_Text taskProgressText;
    public TMP_Text completionText;
    public Button finishButton;
    public string finalSceneName = "MainMenu";

    private void Start()
    {
        if (instructionText != null)
        {
            instructionText.text =
                "Nivel 5: Entorno doméstico\n" +
                "Completa las tareas cotidianas mientras permaneces en el entorno.";
        }

        SetTaskProgress(0, 4);
        ShowCompleted(false);

        if (finishButton != null)
        {
            finishButton.gameObject.SetActive(false);
            finishButton.onClick.RemoveAllListeners();
            finishButton.onClick.AddListener(FinishSession);
        }
    }

    public void SetTaskProgress(int completed, int total)
    {
        if (taskProgressText != null)
            taskProgressText.text = $"Tareas completadas: {completed} / {total}";
    }

    public void ShowCompleted(bool show)
    {
        if (completionText != null)
        {
            completionText.gameObject.SetActive(show);

            if (show)
                completionText.text = "Sesión completada\nSe guardó el reporte local.";
        }

        if (finishButton != null)
            finishButton.gameObject.SetActive(show);
    }

    public void FinishSession()
    {
        if (SessionManager.Instance != null)
            SessionManager.Instance.EndSession();

        SceneManager.LoadScene(finalSceneName);
    }
}
