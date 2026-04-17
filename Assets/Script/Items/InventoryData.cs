using System.Collections.Generic;
using UnityEngine;

public class InventoryData : MonoBehaviour
{
    [SerializeField] private List<InventoryEntry> items = new();
    [SerializeField] private int totalItemCount;
    [SerializeField] private ItemStats totalStats;

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
    }
}
