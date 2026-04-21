using System.Collections.Generic;
using UnityEngine;

public class ItemCollectionGridUI : MonoBehaviour
{
    private enum DataSourceType
    {
        Stash,
        Inventory,
        CollectBox,
    }

    [SerializeField] private DataSourceType dataSourceType = DataSourceType.Stash;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ItemUI itemUiPrefab;
    [SerializeField] private bool stackInventoryItems = true;
    [SerializeField] private HoveredItemStatsUI hoverStatsUi;

    private readonly List<ItemUI> spawnedItems = new();
    private StashData stashData;
    private InventoryData inventoryData;
    private CollectBoxData collectBoxData;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnEnable()
    {
        EnsureReferences();
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnValidate()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (contentRoot == null)
        {
            contentRoot = transform;
        }

        if (hoverStatsUi == null)
        {
            hoverStatsUi = FindFirstObjectByType<HoveredItemStatsUI>();
        }

        ResolveDataSources();
    }

    [ContextMenu("Refresh UI")]
    public void Refresh()
    {
        ClearSpawnedItems();

        if (contentRoot == null || itemUiPrefab == null)
        {
            return;
        }

        switch (dataSourceType)
        {
            case DataSourceType.Stash:
                BuildStashItems();
                break;

            case DataSourceType.Inventory:
                BuildInventoryItems();
                break;

            case DataSourceType.CollectBox:
                BuildCollectBoxItems();
                break;
        }
    }

    private void Subscribe()
    {
        ResolveDataSources();

        if (stashData != null)
        {
            stashData.Changed += Refresh;
        }

        if (inventoryData != null)
        {
            inventoryData.Changed += Refresh;
        }

        if (collectBoxData != null)
        {
            collectBoxData.Changed += Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (stashData != null)
        {
            stashData.Changed -= Refresh;
        }

        if (inventoryData != null)
        {
            inventoryData.Changed -= Refresh;
        }

        if (collectBoxData != null)
        {
            collectBoxData.Changed -= Refresh;
        }
    }

    private void ResolveDataSources()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            stashData = null;
            inventoryData = null;
            collectBoxData = null;
            return;
        }

        switch (dataSourceType)
        {
            case DataSourceType.Stash:
                stashData = gameManager.StashData;
                inventoryData = null;
                collectBoxData = null;
                break;

            case DataSourceType.Inventory:
                stashData = null;
                inventoryData = gameManager.InventoryData;
                collectBoxData = null;
                break;

            case DataSourceType.CollectBox:
                stashData = null;
                inventoryData = null;
                collectBoxData = gameManager.CollectBoxData;
                break;
        }
    }

    private void BuildStashItems()
    {
        if (stashData == null)
        {
            return;
        }

        IReadOnlyList<StashEntry> entries = stashData.Entries;
        for (int i = 0; i < entries.Count; i++)
        {
            StashEntry entry = entries[i];
            if (!entry.IsValid)
            {
                continue;
            }

            SpawnItem(entry.Item, entry.Quantity);
        }
    }

    private void BuildInventoryItems()
    {
        if (inventoryData == null)
        {
            return;
        }

        BuildInventoryStyleItems(inventoryData.Items);
    }

    private void BuildCollectBoxItems()
    {
        if (collectBoxData == null)
        {
            return;
        }

        BuildInventoryStyleItems(collectBoxData.Items);
    }

    private void BuildInventoryStyleItems(IReadOnlyList<InventoryEntry> entries)
    {
        if (entries == null)
        {
            return;
        }

        if (!stackInventoryItems)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                InventoryEntry entry = entries[i];
                if (!entry.IsValid)
                {
                    continue;
                }

                SpawnItem(entry.Item, 1);
            }

            return;
        }

        var itemCounts = new Dictionary<ItemData, int>();
        var orderedItems = new List<ItemData>();

        for (int i = 0; i < entries.Count; i++)
        {
            InventoryEntry entry = entries[i];
            if (!entry.IsValid)
            {
                continue;
            }

            if (!itemCounts.TryGetValue(entry.Item, out int currentCount))
            {
                itemCounts.Add(entry.Item, 1);
                orderedItems.Add(entry.Item);
                continue;
            }

            itemCounts[entry.Item] = currentCount + 1;
        }

        for (int i = 0; i < orderedItems.Count; i++)
        {
            ItemData item = orderedItems[i];
            SpawnItem(item, itemCounts[item]);
        }
    }

    private void SpawnItem(ItemData itemData, int amount)
    {
        ItemUI itemUi = Instantiate(itemUiPrefab, contentRoot);
        itemUi.Bind(itemData, amount);
        itemUi.SetHoverStatsUi(hoverStatsUi);
        spawnedItems.Add(itemUi);
    }

    private void ClearSpawnedItems()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] == null)
            {
                continue;
            }

            Destroy(spawnedItems[i].gameObject);
        }

        spawnedItems.Clear();
    }
}
