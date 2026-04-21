using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string ItemPrepareSceneName = "ItemPrepare";
    private const string LevelSceneName = "Level";

    public static GameManager Instance { get; private set; }

    public int CurrentProgression => currentProgression;
    public StashData StashData => stashData;
    public InventoryData InventoryData => inventoryData;
    public CollectBoxData CollectBoxData => collectBoxData;
    
    [Header("Runtime Data")]
    [SerializeField] private StashData stashData;
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private CollectBoxData collectBoxData;
    [SerializeField] private bool hasGrantedStarterPack;
    [SerializeField] [Min(1)] private int currentProgression = 1;

    private readonly List<ItemData> runtimeInventoryItems = new();
    private readonly List<StashEntry> runtimeStashEntries = new();
    private readonly List<ItemData> runtimeCollectBoxItems = new();

    private bool isApplyingSceneData;
    private bool hasInitializedRuntimeData;
    private string lastLoadedSceneName;

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
        lastLoadedSceneName = SceneManager.GetActiveScene().name;

        RebindSceneData();
        InitializeRuntimeDataFromScene();
        UpdatePlayerCurrencyFromRuntimeStash();
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

        if (!isActiveAndEnabled)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

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
        bool refreshStashSpawn = HandleCompletedLevelReturn(scene.name);

        RebindSceneData();
        InitializeRuntimeDataFromScene();
        ApplyRuntimeDataToScene(refreshStashSpawn);
        lastLoadedSceneName = scene.name;
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

    public void AddItemToInventory(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        runtimeInventoryItems.Add(item);
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

        ApplyRuntimeDataToScene(refreshStashSpawn: true);
    }

    public void AddItemToStash(ItemData item)
    {
        AddRuntimeStashItem(item);
        ApplyRuntimeDataToScene(refreshStashSpawn: true);
    }

    public void AddItemsToCollectBox(IEnumerable<ItemData> items)
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

            runtimeCollectBoxItems.Add(item);
        }

        ApplyRuntimeDataToScene();
    }

    public void AddItemToCollectBox(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        runtimeCollectBoxItems.Add(item);
        ApplyRuntimeDataToScene();
    }

    public bool TryGrantStarterPack(IEnumerable<ItemData> items)
    {
        if (hasGrantedStarterPack || items == null)
        {
            return false;
        }

        hasGrantedStarterPack = true;
        AddItemsToStash(items);
        return true;
    }

    private void InitializeRuntimeDataFromScene()
    {
        if (hasInitializedRuntimeData)
        {
            return;
        }

        if (stashData == null && inventoryData == null && collectBoxData == null)
        {
            return;
        }

        if (inventoryData != null)
        {
            HandleInventoryChanged();
        }

        if (stashData != null)
        {
            HandleStashChanged();
        }

        if (collectBoxData != null)
        {
            HandleCollectBoxChanged();
        }

        hasInitializedRuntimeData = true;
    }

    private void ApplyRuntimeDataToScene(bool refreshStashSpawn = false)
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

        if (collectBoxData != null)
        {
            collectBoxData.SetItems(runtimeCollectBoxItems);
        }

        isApplyingSceneData = false;
        UpdatePlayerCurrencyFromRuntimeStash();

        if (refreshStashSpawn)
        {
            RefreshSceneStashSpawn();
        }
    }

    private void RebindSceneData()
    {
        UnsubscribeFromSceneData();

        stashData = FindFirstObjectByType<StashData>();
        inventoryData = FindFirstObjectByType<InventoryData>();
        collectBoxData = FindFirstObjectByType<CollectBoxData>();

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

        if (collectBoxData != null)
        {
            collectBoxData.Changed += HandleCollectBoxChanged;
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

        if (collectBoxData != null)
        {
            collectBoxData.Changed -= HandleCollectBoxChanged;
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

        UpdatePlayerCurrencyFromRuntimeStash();
    }

    private void HandleCollectBoxChanged()
    {
        if (isApplyingSceneData || collectBoxData == null)
        {
            return;
        }

        runtimeCollectBoxItems.Clear();
        IReadOnlyList<InventoryEntry> items = collectBoxData.Items;
        for (int i = 0; i < items.Count; i++)
        {
            InventoryEntry entry = items[i];
            if (!entry.IsValid)
            {
                continue;
            }

            runtimeCollectBoxItems.Add(entry.Item);
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

    private void MoveCollectBoxToStash()
    {
        if (runtimeCollectBoxItems.Count == 0)
        {
            return;
        }

        for (int i = 0; i < runtimeCollectBoxItems.Count; i++)
        {
            AddRuntimeStashItem(runtimeCollectBoxItems[i]);
        }

        runtimeCollectBoxItems.Clear();
    }

    private bool HandleCompletedLevelReturn(string loadedSceneName)
    {
        if (loadedSceneName != ItemPrepareSceneName || lastLoadedSceneName != LevelSceneName)
        {
            return false;
        }

        currentProgression = Mathf.Max(1, currentProgression + 1);
        MoveCollectBoxToStash();
        return true;
    }

    private void UpdatePlayerCurrencyFromRuntimeStash()
    {
        if (DATA_Player.Instance == null || DATA_Player.Instance.CharacterStats == null)
        {
            return;
        }

        float totalCurrency = 0f;
        for (int i = 0; i < runtimeStashEntries.Count; i++)
        {
            StashEntry entry = runtimeStashEntries[i];
            if (!entry.IsValid || entry.Item == null)
            {
                continue;
            }

            totalCurrency += entry.Item.Stats.Value * entry.Quantity;
        }

        DATA_Player.Instance.CharacterStats.Currency = totalCurrency;
    }

    private void RefreshSceneStashSpawn()
    {
        StashSpawner stashSpawner = FindFirstObjectByType<StashSpawner>();
        if (stashSpawner == null)
        {
            return;
        }

        stashSpawner.ResetStash();
    }
}
