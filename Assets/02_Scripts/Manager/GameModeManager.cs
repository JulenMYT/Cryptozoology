using UnityEngine;
using System;

public enum GameMode
{
    Normal,
    Building
}

public class GameModeManager : MonoBehaviour
{
    public GameMode CurrentMode { get; private set; } = GameMode.Normal;
    public event Action<GameMode> OnModeChanged;

    public void SetMode(GameMode mode)
    {
        if (CurrentMode == mode) return;
        CurrentMode = mode;
        OnModeChanged?.Invoke(mode);
    }
}
