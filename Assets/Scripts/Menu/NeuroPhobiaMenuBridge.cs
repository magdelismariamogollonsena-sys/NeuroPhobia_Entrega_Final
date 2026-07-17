using UnityEngine;
using UnityEngine.SceneManagement;

public class NeuroPhobiaMenuBridge : MonoBehaviour
{
    [SerializeField] private int defaultPhobiaGrade = 1;

    public void LoadNivel1()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.StartSession(defaultPhobiaGrade);
        }

        SceneManager.LoadScene("Nivel1");
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
