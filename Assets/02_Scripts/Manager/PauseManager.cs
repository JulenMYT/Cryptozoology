using System;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public bool isPaused { get; private set; }

    public event Action OnPause;
    public event Action OnResume;

    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0.0f;
        OnPause?.Invoke();
    }

    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1.0f;
        OnResume?.Invoke();
    }
}
