using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private string sceneName = "GameScene";

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
        GameSceneManager.Instance.LoadScene(sceneName);
    }
}