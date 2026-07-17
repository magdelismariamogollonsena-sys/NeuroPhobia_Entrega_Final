using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneName;  // Ahora editable desde el Inspector

    [Header("Optional Panel")]
    [SerializeField] private GameObject panel;

    // Botón para iniciar juego
    public void StartGame()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el nombre de la escena en el Inspector.");
        }
    }

    // Botón para salir del juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // Botón para activar/desactivar un panel
    public void TogglePanel()
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }

    // Botón simple
    public void PrintMessage()
    {
        Debug.Log("Botón presionado correctamente");
    }
}
