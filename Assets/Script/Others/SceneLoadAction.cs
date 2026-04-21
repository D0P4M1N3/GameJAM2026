using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadAction : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool debugLogs;

    public void LoadConfiguredScene()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            if (debugLogs)
            {
                Debug.LogWarning("[SceneLoadAction] Scene name is empty.", this);
            }

            return;
        }

        if (GameSceneManager.Instance != null)
        {
            if (debugLogs)
            {
                Debug.Log($"[SceneLoadAction] Loading scene '{sceneName}' via GameSceneManager.", this);
            }

            GameSceneManager.Instance.LoadScene(sceneName);
            return;
        }

        if (GameManager.Instance != null)
        {
            if (debugLogs)
            {
                Debug.Log($"[SceneLoadAction] Loading scene '{sceneName}' via GameManager.", this);
            }

            GameManager.Instance.LoadScene(sceneName);
            return;
        }

        if (debugLogs)
        {
            Debug.Log($"[SceneLoadAction] Loading scene '{sceneName}' directly.", this);
        }

        SceneManager.LoadScene(sceneName);
    }
}
