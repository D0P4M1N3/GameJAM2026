using UnityEngine;
using System;

public class Pause3D : MonoBehaviour
{
    public static Pause3D Instance;

    public bool IsPaused { get; private set; }

    public static event Action<bool> OnPauseChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetPause(bool pause)
    {
        if (IsPaused == pause) return;

        IsPaused = pause;

        OnPauseChanged?.Invoke(IsPaused);
    }

    public void TogglePause()
    {
        SetPause(!IsPaused);
    }
}