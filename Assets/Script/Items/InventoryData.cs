using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryData : MonoBehaviour
{
    [SerializeField] private List<InventoryEntry> items = new();
    [SerializeField] private int totalItemCount;
    [SerializeField] private ItemStats totalStats;
    [SerializeField] private Color mixedColor = Color.clear;
    [SerializeField] [Min(0f)] private float totalColorWeight;

    public event Action Changed;

    public IReadOnlyList<InventoryEntry> Items => items;
    public int TotalItemCount => totalItemCount;
    public ItemStats TotalStats => totalStats;
    public Color MixedColor => mixedColor;
    public float TotalColorWeight => totalColorWeight;
    public int Capacity => GetCapacity();
    public bool IsOverflowing => totalItemCount > Capacity;

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
        mixedColor = Color.clear;
        totalColorWeight = 0f;
        Color accumulatedColor = Color.clear;

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

            float colorWeight = Mathf.Max(0f, entry.Item.SizeValue);
            if (colorWeight <= 0f)
            {
                continue;
            }

            accumulatedColor += entry.Item.ItemColor * colorWeight;
            totalColorWeight += colorWeight;
        }

        if (totalColorWeight > 0f)
        {
            mixedColor = accumulatedColor / totalColorWeight;
        }

        Changed?.Invoke();
    }


    private int GetCapacity()
    {
        if (DATA_Player.Instance == null || DATA_Player.Instance.CharacterStats == null)
        {
            return 0;
        }

        CharacterStats targetCharacterStats = DATA_Player.Instance.CharacterStats;
        float resolvedCapacity = targetCharacterStats.finalStorage;
        if (resolvedCapacity <= 0f)
        {
            resolvedCapacity = targetCharacterStats.Storage;
        }

        return Mathf.Max(0, Mathf.FloorToInt(resolvedCapacity));
    }
}
