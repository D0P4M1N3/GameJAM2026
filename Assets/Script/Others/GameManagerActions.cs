using UnityEngine;

public class GameManagerActions : MonoBehaviour
{
    [SerializeField] private string loadSceneName = "ItemPrepare";

    public void MoveCollectBoxToStash()
    {
        GameManager.Instance?.MoveCollectBoxToStash();
    }

    public void MoveCollectBoxToStashIncrementProgressionAndLoadConfiguredScene()
    {
        Debug.Log(
            $"GameManagerActions.MoveCollectBoxToStashIncrementProgressionAndLoadConfiguredScene invoked on {gameObject.name}. " +
            $"Has GameManager={(GameManager.Instance != null)} Scene='{loadSceneName}'",
            this);

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("MoveCollectBoxToStashIncrementProgressionAndLoadConfiguredScene was invoked, but GameManager.Instance is null.", this);
            return;
        }

        GameManager.Instance.MoveCollectBoxToStashIncrementProgressionAndLoadScene(loadSceneName);
    }

    public void IncrementProgression()
    {
        GameManager.Instance?.IncrementProgression();
    }

    public void ResetProgression()
    {
        GameManager.Instance?.ResetProgression();
    }

    public void ResetCharacterStats()
    {
        GameManager.Instance?.ResetCharacterStats();
    }

    public void ApplyInventoryStatsToPlayer()
    {
        Debug.Log($"GameManagerActions.ApplyInventoryStatsToPlayer invoked on {gameObject.name}. Has GameManager={(GameManager.Instance != null)}", this);

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ApplyInventoryStatsToPlayer was invoked, but GameManager.Instance is null.", this);
            return;
        }

        GameManager.Instance?.ApplyInventoryStatsToPlayer();
    }

    public void DeleteInventoryItems()
    {
        Debug.Log($"GameManagerActions.DeleteInventoryItems invoked on {gameObject.name}. Has GameManager={(GameManager.Instance != null)}", this);

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("DeleteInventoryItems was invoked, but GameManager.Instance is null.", this);
            return;
        }

        GameManager.Instance.DeleteInventoryItems();
    }

    public void ApplyInventoryStatsAndDeleteInventoryItems()
    {
        Debug.Log($"GameManagerActions.ApplyInventoryStatsAndDeleteInventoryItems invoked on {gameObject.name}. Has GameManager={(GameManager.Instance != null)}", this);

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ApplyInventoryStatsAndDeleteInventoryItems was invoked, but GameManager.Instance is null.", this);
            return;
        }

        GameManager.Instance.ApplyInventoryStatsAndDeleteInventoryItems();
    }

    public void LoadConfiguredScene()
    {
        if (string.IsNullOrWhiteSpace(loadSceneName))
        {
            return;
        }

        GameManager.Instance?.LoadScene(loadSceneName);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        GameManager.Instance?.LoadScene(sceneName);
    }
}
