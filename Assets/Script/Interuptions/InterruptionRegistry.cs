using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InterruptionRegistry
{
    [SerializeField]
    private List<string> activeKeys = new List<string>();

    public bool isInterrupted { get; private set; }

    public void Add(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (!activeKeys.Contains(key))
            activeKeys.Add(key);

        isInterrupted = activeKeys.Count > 0;
    }

    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        activeKeys.RemoveAll(k => k == key);

        isInterrupted = activeKeys.Count > 0;
    }



    public void Clear()
    {
        activeKeys.Clear();
    }

    public bool Contains(string key)
    {
        return activeKeys.Contains(key);
    }











    /////////////// REQUIRE MANUAL OPERATIONS ////////////////////////////////
    public bool OnInterruptGoing;
    public bool OnInterruptGoing_Prev;
    public bool OnInterruptEnter;
    public bool OnInterruptExit;
    public void __UpdateState()
    {
        // Rising edge
        OnInterruptEnter = OnInterruptGoing && !OnInterruptGoing_Prev;

        // Falling edge
        OnInterruptExit = !OnInterruptGoing && OnInterruptGoing_Prev;

        // Store state for next frame
        OnInterruptGoing_Prev = OnInterruptGoing;

        // Reset for next input sampling
        OnInterruptGoing = false;
    }
}