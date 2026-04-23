using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
    [SerializeField] private string playerDefeatSceneName = "StashSellEnding";

    public bool IsGameplayPaused { get; private set; }

    private readonly List<ItemData> runtimeInventoryItems = new();
    private readonly List<StashEntry> runtimeStashEntries = new();
    private readonly List<ItemData> runtimeCollectBoxItems = new();
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

        SubscribeToSceneData();
        InitializeRuntimeDataFromScene();
        UpdatePlayerCurrencyFromRuntimeStash();
    }

    private void Start()
    {
       
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
        bool addedAnyItem = false;

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
            addedAnyItem = true;
        }

        ApplyRuntimeDataToScene();

        if (addedAnyItem && DATA_Player.Instance != null)
        {
            DATA_Player.Instance.SetFaceForDuration(PlayerFaceVariant.B, 0.5f);
        }
    }

    public void AddItemToCollectBox(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        runtimeCollectBoxItems.Add(item);
        ApplyRuntimeDataToScene();

        if (DATA_Player.Instance != null)
        {
            DATA_Player.Instance.SetFaceForDuration(PlayerFaceVariant.B, 0.5f);
        }
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

    public void MoveCollectBoxToStash()
    {
        List<ItemData> itemsToMove = new();

        if (collectBoxData != null)
        {
            IReadOnlyList<InventoryEntry> collectBoxEntries = collectBoxData.Items;
            for (int i = 0; i < collectBoxEntries.Count; i++)
            {
                InventoryEntry entry = collectBoxEntries[i];
                if (!entry.IsValid)
                {
                    continue;
                }

                itemsToMove.Add(entry.Item);
            }
        }
        else
        {
            itemsToMove.AddRange(runtimeCollectBoxItems);
        }

        Debug.Log($"GameManager.MoveCollectBoxToStash called. CollectBox item count={itemsToMove.Count}", this);

        if (itemsToMove.Count == 0)
        {
            ApplyRuntimeDataToScene(refreshStashSpawn: true);
            return;
        }

        for (int i = 0; i < itemsToMove.Count; i++)
        {
            AddRuntimeStashItem(itemsToMove[i]);
        }

        runtimeCollectBoxItems.Clear();
        ApplyRuntimeDataToScene(refreshStashSpawn: true);
    }

    public void IncrementProgression()
    {
        currentProgression = Mathf.Max(1, currentProgression + 1);
    }

    public void MoveCollectBoxToStashIncrementProgressionAndLoadScene(string sceneName)
    {
        Debug.Log(
            $"GameManager.MoveCollectBoxToStashIncrementProgressionAndLoadScene called. Scene='{sceneName}' CurrentProgression={currentProgression}",
            this);

        MoveCollectBoxToStash();
        IncrementProgression();

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadScene(sceneName);
            return;
        }

        LoadScene(sceneName);
    }

    public void ResetProgression()
    {
        currentProgression = 1;
    }

    public void ResetCharacterStats()
    {
        if (DATA_Player.Instance != null)
        {
            DATA_Player.Instance.ResetCharacterStats();
        }
    }

    public void ResetAllRuntimeDataForNewGame()
    {
        hasGrantedStarterPack = false;
        currentProgression = 1;

        runtimeInventoryItems.Clear();
        runtimeStashEntries.Clear();
        runtimeCollectBoxItems.Clear();

        ApplyRuntimeDataToScene(refreshStashSpawn: true);
        DestroyInventoryWorldObjects();

        if (DATA_Player.Instance != null)
        {
            DATA_Player.Instance.ResetCharacterStats();
            DATA_Player.Instance.ResetFaceToDefault();
        }
    }

    public void TriggerEndingSellFaceFromTotalValue(float totalValue, float duration = 1f)
    {
        if (DATA_Player.Instance == null)
        {
            return;
        }

        DATA_Player.Instance.SetFaceForDuration(GetEndingSellFaceVariant(totalValue), duration);
    }

    public void HandlePlayerDefeated()
    {
        if (string.IsNullOrWhiteSpace(playerDefeatSceneName))
        {
            return;
        }

        LoadScene(playerDefeatSceneName);
    }

    public void ApplyInventoryStatsToPlayer()
    {
        Debug.Log(
            $"GameManager.ApplyInventoryStatsToPlayer called. " +
            $"HasInventoryData={(inventoryData != null)} " +
            $"HasDATA_Player={(DATA_Player.Instance != null)} " +
            $"HasCharacterStats={(DATA_Player.Instance != null && DATA_Player.Instance.CharacterStats != null)}",
            this);

        if (inventoryData == null)
        {
            Debug.LogWarning("ApplyInventoryStatsToPlayer aborted because inventoryData is null.", this);
            return;
        }

        if (inventoryData.IsOverflowing)
        {
            Debug.LogWarning($"Cannot apply inventory stats while overflowing. {inventoryData.TotalItemCount}/{inventoryData.Capacity} items.");
            return;
        }

        if (DATA_Player.Instance == null || DATA_Player.Instance.CharacterStats == null)
        {
            Debug.LogWarning("ApplyInventoryStatsToPlayer aborted because DATA_Player.Instance or CharacterStats is null.", this);
            return;
        }

        CharacterStats targetCharacterStats = DATA_Player.Instance.CharacterStats;
        ItemStats appliedStats = inventoryData.TotalStats;
        float baseSpeed = targetCharacterStats.Speed;
        float baseMaxHp = targetCharacterStats.MaxHP;
        float baseDamage = targetCharacterStats.Damage;
        float previousModifierSpeed = targetCharacterStats.mSpeed;
        float previousModifierMaxHp = targetCharacterStats.mMaxHP;
        float previousModifierDamage = targetCharacterStats.mDamage;
        float speedModifierDelta = appliedStats.Speed;
        float maxHpModifierDelta = appliedStats.Health;
        float damageModifierDelta = appliedStats.Attack;

        targetCharacterStats.mMaxHP += maxHpModifierDelta;
        targetCharacterStats.mDamage += damageModifierDelta;
        targetCharacterStats.mSpeed += speedModifierDelta;
        targetCharacterStats.HP = targetCharacterStats.finalMaxHP;
        targetCharacterStats.CharacterColor = inventoryData.MixedColor;
        targetCharacterStats.RefreshInspectorFinals();

        Debug.Log(
            "Applied inventory stats to player modifiers.\n" +
            $"Base Stats: Speed={baseSpeed:0.##}, MaxHP={baseMaxHp:0.##}, Damage={baseDamage:0.##}\n" +
            $"Previous Modifiers: mSpeed={previousModifierSpeed:0.####}, mMaxHP={previousModifierMaxHp:0.##}, mDamage={previousModifierDamage:0.##}\n" +
            $"Inventory Modifiers Applied: SpeedDelta={speedModifierDelta:0.####}, MaxHPDelta={maxHpModifierDelta:0.##}, DamageDelta={damageModifierDelta:0.##}\n" +
            $"New Modifiers: mSpeed={targetCharacterStats.mSpeed:0.####}, mMaxHP={targetCharacterStats.mMaxHP:0.##}, mDamage={targetCharacterStats.mDamage:0.##}\n" +
            $"New Finals: finalSpeed={targetCharacterStats.finalSpeed:0.##}, finalMaxHP={targetCharacterStats.finalMaxHP:0.##}, finalDamage={targetCharacterStats.finalDamage:0.##}, HP={targetCharacterStats.HP:0.##}",
            this);
    }

    public void DeleteInventoryItems()
    {
        runtimeInventoryItems.Clear();
        ApplyRuntimeDataToScene();
        DestroyInventoryWorldObjects();
    }

    public void ApplyInventoryStatsAndDeleteInventoryItems()
    {
        Debug.Log("ApplyInventoryStatsAndDeleteInventoryItems step 1: start", this);

        if (inventoryData == null)
        {
            Debug.LogWarning("ApplyInventoryStatsAndDeleteInventoryItems aborted: inventoryData is null.", this);
            return;
        }

        if (DATA_Player.Instance == null || DATA_Player.Instance.CharacterStats == null)
        {
            Debug.LogWarning("ApplyInventoryStatsAndDeleteInventoryItems aborted: DATA_Player.Instance or CharacterStats is null.", this);
            return;
        }

        ItemStats inventoryTotals = inventoryData.TotalStats;
        Debug.Log(
            $"ApplyInventoryStatsAndDeleteInventoryItems step 2: inventory totals Speed={inventoryTotals.Speed:0.##}, Health={inventoryTotals.Health:0.##}, Attack={inventoryTotals.Attack:0.##}, Value={inventoryTotals.Value:0.##}",
            this);

        CharacterStats targetCharacterStats = DATA_Player.Instance.CharacterStats;
        targetCharacterStats.mSpeed += inventoryTotals.Speed;
        targetCharacterStats.mMaxHP += inventoryTotals.Health;
        targetCharacterStats.mDamage += inventoryTotals.Attack;
        targetCharacterStats.HP = targetCharacterStats.finalMaxHP;
        targetCharacterStats.CharacterColor = inventoryData.MixedColor;
        targetCharacterStats.RefreshInspectorFinals();

        Debug.Log(
            $"ApplyInventoryStatsAndDeleteInventoryItems step 3: applied modifiers mSpeed={targetCharacterStats.mSpeed:0.##}, mMaxHP={targetCharacterStats.mMaxHP:0.##}, mDamage={targetCharacterStats.mDamage:0.##}, finalSpeed={targetCharacterStats.finalSpeed:0.##}, finalMaxHP={targetCharacterStats.finalMaxHP:0.##}, finalDamage={targetCharacterStats.finalDamage:0.##}",
            this);

        DeleteInventoryItems();
        Debug.Log("ApplyInventoryStatsAndDeleteInventoryItems step 4: deleted inventory items", this);
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

    private void DestroyInventoryWorldObjects()
    {
        ItemTriggerZone[] itemTriggerZones = FindObjectsByType<ItemTriggerZone>(FindObjectsSortMode.None);
        ItemWorldObject[] inventoryWorldObjects = FindObjectsByType<ItemWorldObject>(FindObjectsSortMode.None);
        for (int i = 0; i < inventoryWorldObjects.Length; i++)
        {
            ItemWorldObject itemWorldObject = inventoryWorldObjects[i];
            if (itemWorldObject == null)
            {
                continue;
            }

            bool shouldDestroy = itemWorldObject.IsInInventory || IsInsideInventoryZone(itemWorldObject, itemTriggerZones);
            if (!shouldDestroy)
            {
                continue;
            }

            itemWorldObject.SetInventoryState(false);

            Collider2D[] colliders = itemWorldObject.GetComponentsInChildren<Collider2D>(true);
            for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
            {
                if (colliders[colliderIndex] != null)
                {
                    colliders[colliderIndex].enabled = false;
                }
            }

            Rigidbody2D[] rigidbodies2D = itemWorldObject.GetComponentsInChildren<Rigidbody2D>(true);
            for (int bodyIndex = 0; bodyIndex < rigidbodies2D.Length; bodyIndex++)
            {
                if (rigidbodies2D[bodyIndex] != null)
                {
                    rigidbodies2D[bodyIndex].simulated = false;
                }
            }

            itemWorldObject.gameObject.SetActive(false);

            if (Application.isPlaying)
            {
                Destroy(itemWorldObject.gameObject);
            }
            else
            {
                DestroyImmediate(itemWorldObject.gameObject);
            }
        }
    }

    private static bool IsInsideInventoryZone(ItemWorldObject itemWorldObject, ItemTriggerZone[] triggerZones)
    {
        if (itemWorldObject == null || triggerZones == null)
        {
            return false;
        }

        Collider2D[] itemColliders = itemWorldObject.GetComponentsInChildren<Collider2D>(true);
        if (itemColliders == null || itemColliders.Length == 0)
        {
            return false;
        }

        for (int zoneIndex = 0; zoneIndex < triggerZones.Length; zoneIndex++)
        {
            ItemTriggerZone triggerZone = triggerZones[zoneIndex];
            if (triggerZone == null || triggerZone.CurrentMode != ItemTriggerZone.ZoneMode.Inventory)
            {
                continue;
            }

            Collider2D zoneCollider = triggerZone.GetComponent<Collider2D>();
            if (zoneCollider == null)
            {
                continue;
            }

            Bounds zoneBounds = zoneCollider.bounds;
            for (int itemColliderIndex = 0; itemColliderIndex < itemColliders.Length; itemColliderIndex++)
            {
                Collider2D itemCollider = itemColliders[itemColliderIndex];
                if (itemCollider == null || !itemCollider.enabled)
                {
                    continue;
                }

                if (zoneBounds.Intersects(itemCollider.bounds))
                {
                    return true;
                }
            }
        }

        return false;
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

    private static PlayerFaceVariant GetEndingSellFaceVariant(float totalValue)
    {
        if (totalValue > 1500f)
        {
            return PlayerFaceVariant.G;
        }

        if (totalValue > 1000f)
        {
            return PlayerFaceVariant.B;
        }

        if (totalValue > 500f)
        {
            return PlayerFaceVariant.D;
        }

        if (totalValue > 300f)
        {
            return PlayerFaceVariant.A;
        }

        if (totalValue > 100f)
        {
            return PlayerFaceVariant.E;
        }

        return PlayerFaceVariant.C;
    }





}
