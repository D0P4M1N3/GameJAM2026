using System.Collections.Generic;
using UnityEngine;

public class StashData : MonoBehaviour
{
    [SerializeField] private List<StashEntry> entries = new();
    [SerializeField] private int totalItemCount;
    [SerializeField] private ItemStats totalStats;

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
    }
}
