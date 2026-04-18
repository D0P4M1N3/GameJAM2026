using System.Collections.Generic;
using UnityEngine;

public class ItemCollectionGridUI : MonoBehaviour
{
    private enum DataSourceType
    {
        Stash,
        Inventory,
    }

    [SerializeField] private DataSourceType dataSourceType = DataSourceType.Stash;
    [SerializeField] private StashData stashData;
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ItemUI itemUiPrefab;
    [SerializeField] private bool stackInventoryItems = true;

    private readonly List<ItemUI> spawnedItems = new();

    private void Awake()
    {
        if (contentRoot == null)
        {
            contentRoot = transform;
        }
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnValidate()
    {
        if (contentRoot == null)
        {
            contentRoot = transform;
        }
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
        }
    }

    private void Subscribe()
    {
        if (stashData != null)
        {
            stashData.Changed += Refresh;
        }

        if (inventoryData != null)
        {
            inventoryData.Changed += Refresh;
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

        IReadOnlyList<InventoryEntry> entries = inventoryData.Items;
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
