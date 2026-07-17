using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level4HUD : MonoBehaviour
{
    public TMP_Text instructionText;
    public TMP_Text contactProgressText;
    public TMP_Text warningText;
    public TMP_Text completionText;
    public Slider contactSlider;
    public Button continueButton;
    public string nextSceneName = "Nivel5";

    private float warningTimer;

    private void Start()
    {
        if (instructionText != null)
        {
            instructionText.text =
                "Nivel 4: Contacto virtual controlado\n" +
                "Abre el terrario y acerca lentamente la mano virtual a la araña.";
        }

        SetContactProgress(0f, 10f);
        ShowCompleted(false);

        if (warningText != null)
            warningText.gameObject.SetActive(false);

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
        if (warningTimer <= 0f)
            warningText.gameObject.SetActive(false);
    }

    public void SetContactProgress(float current, float required)
    {
        if (contactProgressText != null)
            contactProgressText.text = $"Contacto controlado: {current:0.0} / {required:0}s";

        if (contactSlider != null)
        {
            contactSlider.minValue = 0f;
            contactSlider.maxValue = required;
            contactSlider.value = Mathf.Clamp(current, 0f, required);
        }
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

            if (show)
                completionText.text = "Nivel 4 completado\nPuedes continuar al Nivel 5.";
        }

        if (continueButton != null)
            continueButton.gameObject.SetActive(show);
    }

    public void GoToNextLevel()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
