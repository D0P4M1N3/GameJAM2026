using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public struct SceneButtonMapEntry
{
    public Button button;
    public string sceneName;
}

public class ButtonSceneMap : MonoBehaviour
{
    [SerializeField] private List<SceneButtonMapEntry> sceneButtons = new();

    private readonly List<(Button button, UnityAction action)> registeredActions = new();

    private void OnEnable()
    {
        RegisterButtons();
    }

    private void OnDisable()
    {
        UnregisterButtons();
    }

    private void RegisterButtons()
    {
        UnregisterButtons();

        for (int i = 0; i < sceneButtons.Count; i++)
        {
            SceneButtonMapEntry entry = sceneButtons[i];
            if (entry.button == null || string.IsNullOrWhiteSpace(entry.sceneName))
            {
                continue;
            }

            string targetSceneName = entry.sceneName;
            UnityAction action = () => LoadScene(targetSceneName);
            entry.button.onClick.AddListener(action);
            registeredActions.Add((entry.button, action));
        }
    }

    private void UnregisterButtons()
    {
        for (int i = 0; i < registeredActions.Count; i++)
        {
            (Button button, UnityAction action) = registeredActions[i];
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveListener(action);
        }

        registeredActions.Clear();
    }

    protected void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(sceneName);
            return;
        }

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadScene(sceneName);
        }
    }
}
