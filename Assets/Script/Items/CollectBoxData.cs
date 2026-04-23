using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectBoxData : MonoBehaviour
{
    [SerializeField] private List<InventoryEntry> items = new();
    [SerializeField] private int totalItemCount;
    [SerializeField] private ItemStats totalStats;

    public event Action Changed;

    public IReadOnlyList<InventoryEntry> Items => items;
    public int TotalItemCount => totalItemCount;
    public ItemStats TotalStats => totalStats;

    private void Awake()
    {
        RecalculateSummary();
    }

    private void OnValidate()
    {
        RecalculateSummary();
    }

    public bool AddItem(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        items.Add(new InventoryEntry(item));
        RecalculateSummary();
        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        if (item == null || items == null)
        {
            return false;
        }

        for (int i = 0; i < items.Count; i++)
        {
            InventoryEntry entry = items[i];
            if (entry.Item != item)
            {
                continue;
            }

            items.RemoveAt(i);
            RecalculateSummary();
            return true;
        }

        return false;
    }

    public bool ContainsItem(ItemData item)
    {
        if (item == null || items == null)
        {
            return false;
        }

        for (int i = 0; i < items.Count; i++)
        {
            InventoryEntry entry = items[i];
            if (entry.Item == item)
            {
                return true;
            }
        }

        return false;
    }

    public void SetItems(IEnumerable<ItemData> sourceItems)
    {
        items.Clear();

        if (sourceItems == null)
        {
            RecalculateSummary();
            return;
        }

        foreach (ItemData item in sourceItems)
        {
            if (item == null)
            {
                continue;
            }

            items.Add(new InventoryEntry(item));
        }

        RecalculateSummary();
    }

    public void RecalculateSummary()
    {
        totalItemCount = 0;
        totalStats = ItemStats.Zero;

        if (items == null)
        {
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            InventoryEntry entry = items[i];
            if (!entry.IsValid)
            {
                continue;
            }

            totalItemCount++;
            totalStats += entry.Item.Stats;
        }

        Changed?.Invoke();
    }
}
