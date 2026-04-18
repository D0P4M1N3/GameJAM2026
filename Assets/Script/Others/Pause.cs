using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    public bool isPaused = false;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
                Debug.Log("Game Resumed");
            }
            else if (!isPaused)
            {
                PauseGame();
                Debug.Log("Game Paused");
            }
        }

    }


    public void PauseGame()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void BackToMainMenu()
    {
        GameSceneManager.Instance.LoadScene("MainMenu");
    }
}
