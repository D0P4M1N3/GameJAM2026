using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private string sceneName = "ItemPrepare";

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }
    }

    private void OnPlayClicked()
    {
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
