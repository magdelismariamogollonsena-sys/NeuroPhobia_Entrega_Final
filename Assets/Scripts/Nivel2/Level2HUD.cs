using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Level2HUD : MonoBehaviour
{
    [Header("Textos")]
    public TMP_Text instructionText;
    public TMP_Text gazeProgressText;
    public TMP_Text warningText;
    public TMP_Text completionText;

    [Header("Progreso")]
    public Slider gazeProgressSlider;

    [Header("Boton")]
    public Button continueButton;

    [Header("Escena siguiente")]
    public string nextSceneName = "Nivel3";

    private float warningTimer;

    private void Start()
    {
        SetInstruction("Nivel 2: Distancia segura\\nObserva la arana 3D durante 30 segundos continuos. Mantente fuera de la zona de exclusion.");
        SetGazeProgress(0f, 30f, 0f);
        ShowCompleted(false);

        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.RemoveListener(GoToNextLevel);
            continueButton.onClick.AddListener(GoToNextLevel);
        }
    }

    private void Update()
    {
        if (warningText != null && warningText.gameObject.activeSelf)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0f)
            {
                warningText.gameObject.SetActive(false);
            }
        }
    }

    public void SetInstruction(string message)
    {
        if (instructionText != null)
        {
            instructionText.text = message;
        }
    }

    public void SetGazeProgress(float currentSeconds, float requiredSeconds, float bestAttempt)
    {
        if (gazeProgressText != null)
        {
            gazeProgressText.text = $"Mirada continua: {currentSeconds:0.0} / {requiredSeconds:0}s\\nMejor intento: {bestAttempt:0.0}s";
        }

        if (gazeProgressSlider != null)
        {
            gazeProgressSlider.minValue = 0f;
            gazeProgressSlider.maxValue = requiredSeconds;
            gazeProgressSlider.value = Mathf.Clamp(currentSeconds, 0f, requiredSeconds);
        }
    }

    public void ShowWarning(string message, float seconds = 2f)
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
            if (show)
            {
                completionText.text = "Nivel 2 completado\\nPuedes continuar al Nivel 3.";
            }
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(show);
        }
    }

    public void GoToNextLevel()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
