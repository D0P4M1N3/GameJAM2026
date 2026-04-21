using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        Time.timeScale = 1f; // Ensure time scale is reset when loading a new scene

        if (!isActiveAndEnabled)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }


    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        if (ScreenFading.Instance != null)
            yield return ScreenFading.Instance.FadeToBlack();

        yield return SceneManager.LoadSceneAsync(sceneName);

        yield return null;

        if (ScreenFading.Instance != null)
            yield return ScreenFading.Instance.FadeFromBlack();
    }
}
