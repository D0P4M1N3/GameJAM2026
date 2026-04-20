using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LevelLootEntry
{
    [SerializeField] private ItemData item;
    [SerializeField] [Min(0f)] private float weight;

    public ItemData Item => item;
    public float Weight => weight;
    public bool IsValid => item != null && weight > 0f;
}

[CreateAssetMenu(fileName = "LevelLootTable", menuName = "Gameplay/Level Loot Table")]
public class LevelLootTable : ScriptableObject
{
    [SerializeField] [Min(0)] private int minDrops = 1;
    [SerializeField] [Min(0)] private int maxDrops = 3;
    [SerializeField] private List<LevelLootEntry> entries = new();

    public int MinDrops => minDrops;
    public int MaxDrops => maxDrops;
    public IReadOnlyList<LevelLootEntry> Entries => entries;

    private void OnValidate()
    {
        if (maxDrops < minDrops)
        {
            maxDrops = minDrops;
        }
    }

    public List<ItemData> RollDrops()
    {
        int dropCount = UnityEngine.Random.Range(minDrops, maxDrops + 1);
        var drops = new List<ItemData>(dropCount);

        for (int i = 0; i < dropCount; i++)
        {
            if (!TryRollItem(out ItemData item))
            {
                break;
            }

            drops.Add(item);
        }

        return drops;
    }

    public bool TryRollItem(out ItemData item)
    {
        item = null;

        float totalWeight = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            LevelLootEntry entry = entries[i];
            if (!entry.IsValid)
            {
                continue;
            }

            totalWeight += entry.Weight;
        }

        if (totalWeight <= 0f)
        {
            return false;
        }

        float roll = UnityEngine.Random.value * totalWeight;
        for (int i = 0; i < entries.Count; i++)
        {
            LevelLootEntry entry = entries[i];
            if (!entry.IsValid)
            {
                continue;
            }

            roll -= entry.Weight;
            if (roll > 0f)
            {
                continue;
            }

            item = entry.Item;
            return true;
        }

        return false;
    }
}
