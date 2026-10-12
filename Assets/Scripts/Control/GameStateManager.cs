using System;
using UnityEngine;

public enum GameState
{
    MENU,
    PLAYING,
    PAUSED,
    BREATHING,
    LEVEL_COMPLETE,
    SESSION_END
}

public class GameStateManager : MonoBehaviour
{
    public static event Action<GameState> OnStateChanged;
    public static GameState CurrentState { get; private set; } = GameState.MENU;

    public static void SetState(GameState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
        OnStateChanged?.Invoke(CurrentState);

        if (state == GameState.BREATHING || state == GameState.PAUSED || state == GameState.LEVEL_COMPLETE || state == GameState.SESSION_END)
        {
            foreach (AranaController spider in UnityEngine.Object.FindObjectsByType<AranaController>(FindObjectsSortMode.None)) spider.Pause();
        }

        if (state == GameState.PLAYING)
        {
            foreach (AranaController spider in UnityEngine.Object.FindObjectsByType<AranaController>(FindObjectsSortMode.None)) spider.Resume();
        }
    }
}
