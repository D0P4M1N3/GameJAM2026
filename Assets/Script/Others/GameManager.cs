using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Runtime Data")]
    [SerializeField] private StashData stashData;
    [SerializeField] private InventoryData inventoryData;

    private readonly List<ItemData> runtimeInventoryItems = new();
    private readonly List<StashEntry> runtimeStashEntries = new();

    private bool isApplyingSceneData;
    private bool hasInitializedRuntimeData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += HandleSceneLoaded;

        RebindSceneData();
        InitializeRuntimeDataFromScene();
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnsubscribeFromSceneData();
        Instance = null;
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        Time.timeScale = 1f;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        if (ScreenFading.Instance != null)
        {
            yield return ScreenFading.Instance.FadeToBlack();
        }

        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return null;

        if (ScreenFading.Instance != null)
        {
            yield return ScreenFading.Instance.FadeFromBlack();
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        RebindSceneData();
        InitializeRuntimeDataFromScene();
        ApplyRuntimeDataToScene();
    }

    public void AddItemsToInventory(IEnumerable<ItemData> items)
    {
        if (items == null)
        {
            return;
        }

        foreach (ItemData item in items)
        {
            if (item == null)
            {
                continue;
            }

            runtimeInventoryItems.Add(item);
        }

        ApplyRuntimeDataToScene();
    }

    public void AddItemsToStash(IEnumerable<ItemData> items)
    {
        if (items == null)
        {
            return;
        }

        foreach (ItemData item in items)
        {
            AddRuntimeStashItem(item);
        }

        ApplyRuntimeDataToScene();
    }

    private void InitializeRuntimeDataFromScene()
    {
        if (hasInitializedRuntimeData)
        {
            return;
        }

        HandleInventoryChanged();
        HandleStashChanged();
        hasInitializedRuntimeData = true;
    }

    private void ApplyRuntimeDataToScene()
    {
        isApplyingSceneData = true;

        if (inventoryData != null)
        {
            inventoryData.SetItems(runtimeInventoryItems);
        }

        if (stashData != null)
        {
            stashData.SetEntries(runtimeStashEntries);
        }

        isApplyingSceneData = false;
    }

    private void RebindSceneData()
    {
        UnsubscribeFromSceneData();

        stashData = FindFirstObjectByType<StashData>();
        inventoryData = FindFirstObjectByType<InventoryData>();

        SubscribeToSceneData();
    }

    private void SubscribeToSceneData()
    {
        if (stashData != null)
        {
            stashData.Changed += HandleStashChanged;
        }

        if (inventoryData != null)
        {
            inventoryData.Changed += HandleInventoryChanged;
        }
    }

    private void UnsubscribeFromSceneData()
    {
        if (stashData != null)
        {
            stashData.Changed -= HandleStashChanged;
        }

        if (inventoryData != null)
        {
            inventoryData.Changed -= HandleInventoryChanged;
        }
    }

    private void HandleInventoryChanged()
    {
        if (isApplyingSceneData || inventoryData == null)
        {
            return;
        }

        runtimeInventoryItems.Clear();
        IReadOnlyList<InventoryEntry> items = inventoryData.Items;
        for (int i = 0; i < items.Count; i++)
        {
            InventoryEntry entry = items[i];
            if (!entry.IsValid)
            {
                continue;
            }

            runtimeInventoryItems.Add(entry.Item);
        }
    }

    private void HandleStashChanged()
    {
        if (isApplyingSceneData || stashData == null)
        {
            return;
        }

        runtimeStashEntries.Clear();
        IReadOnlyList<StashEntry> entries = stashData.Entries;
        for (int i = 0; i < entries.Count; i++)
        {
            StashEntry entry = entries[i];
            if (!entry.IsValid)
            {
                continue;
            }

            runtimeStashEntries.Add(new StashEntry(entry.Item, entry.Quantity));
        }
    }

    private void AddRuntimeStashItem(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        for (int i = 0; i < runtimeStashEntries.Count; i++)
        {
            StashEntry entry = runtimeStashEntries[i];
            if (entry.Item != item)
            {
                continue;
            }

            runtimeStashEntries[i] = new StashEntry(item, entry.Quantity + 1);
            return;
        }

        runtimeStashEntries.Add(new StashEntry(item, 1));
    }
}
