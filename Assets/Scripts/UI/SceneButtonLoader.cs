using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script simple para botones de cambio de nivel.
/// Poner este componente en el mismo boton o en un objeto vacío.
/// </summary>
public class SceneButtonLoader : MonoBehaviour
{
    public void LoadNivel1()
    {
        SceneManager.LoadScene("Nivel1");
    }

    public void LoadNivel2()
    {
        SceneManager.LoadScene("Nivel2");
    }

    public void LoadNivel3()
    {
        SceneManager.LoadScene("Nivel3");
    }

    public void LoadNivel4()
    {
        SceneManager.LoadScene("Nivel4");
    }

    public void LoadNivel5()
    {
        SceneManager.LoadScene("Nivel5");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
