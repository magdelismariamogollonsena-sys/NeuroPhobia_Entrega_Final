using UnityEngine;
using UnityEngine.SceneManagement;

public class NeuroPhobiaMenuBridge : MonoBehaviour
{
    public void LoadNivel1()
    {
        NeuroPhobiaMainMenuModern menu = FindFirstObjectByType<NeuroPhobiaMainMenuModern>();
        if (menu != null) menu.StartExperience();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
