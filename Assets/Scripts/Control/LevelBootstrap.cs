using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelBootstrap : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int targetFrameRate = 72;

    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;

        if (SessionManager.Instance != null) SessionManager.Instance.CurrentLevel = level;
        GameStateManager.SetState(GameState.PLAYING);
    }

    private void Reset()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Nivel") && int.TryParse(sceneName.Replace("Nivel", ""), out int parsedLevel)) level = parsedLevel;
    }
}
