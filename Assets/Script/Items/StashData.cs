using System;
using System.Collections.Generic;
using UnityEngine;

public class StashData : MonoBehaviour
{
    [SerializeField] private List<StashEntry> entries = new();
    [SerializeField] private int totalItemCount;
    [SerializeField] private ItemStats totalStats;

    public event Action Changed;

    public IReadOnlyList<StashEntry> Entries => entries;
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

    public bool RemoveItem(ItemData item)
    {
        if (item == null || entries == null)
        {
            return false;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            StashEntry entry = entries[i];
            if (entry.Item != item || entry.Quantity <= 0)
            {
                continue;
            }

            int updatedQuantity = entry.Quantity - 1;
            if (updatedQuantity > 0)
            {
                entries[i] = new StashEntry(item, updatedQuantity);
            }
            else
            {
                entries.RemoveAt(i);
            }

            RecalculateSummary();
            return true;
        }

        return false;
    }

    public bool AddItem(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            StashEntry entry = entries[i];
            if (entry.Item != item)
            {
                continue;
            }

            entries[i] = new StashEntry(item, entry.Quantity + 1);
            RecalculateSummary();
            return true;
        }

        entries.Add(new StashEntry(item, 1));
        RecalculateSummary();
        return true;
    }

    public void RecalculateSummary()
    {
        totalItemCount = 0;
        totalStats = ItemStats.Zero;

        if (entries == null)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            StashEntry entry = entries[i];
            if (!entry.IsValid)
            {
                continue;
            }

            totalItemCount += entry.Quantity;
            totalStats += entry.Item.Stats * entry.Quantity;
        }

        Changed?.Invoke();
    }
}
