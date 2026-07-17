using System.Collections;
using UnityEngine;

public class AdaptiveCommandPoller : MonoBehaviour
{
    [SerializeField] private float pollingInterval = 5f;
    [SerializeField] private float pauseSeconds = 15f;

    private FastAPIClient apiClient;

    private void Start()
    {
        apiClient = FindFirstObjectByType<FastAPIClient>();
        StartCoroutine(PollLoop());
    }

    private IEnumerator PollLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(pollingInterval);

            if (apiClient == null || SessionManager.Instance == null || string.IsNullOrEmpty(SessionManager.Instance.SessionId)) continue;

            yield return apiClient.GetAdaptiveCommand(SessionManager.Instance.SessionId, HandleCommand);
        }
    }

    private void HandleCommand(string command)
    {
        if (command == "pause") StartCoroutine(PauseTemporarily());
        else if (command == "resume") GameStateManager.SetState(GameState.PLAYING);
    }

    private IEnumerator PauseTemporarily()
    {
        GameStateManager.SetState(GameState.BREATHING);
        yield return new WaitForSeconds(pauseSeconds);
        GameStateManager.SetState(GameState.PLAYING);
    }
}
