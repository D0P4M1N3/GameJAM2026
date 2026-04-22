using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct LootRarityRule
{
    [SerializeField] private ItemRarity rarity;
    [SerializeField] [Min(0f)] private float spawnRate;
    [SerializeField] private AnimationCurve progressionCurve;

    public ItemRarity Rarity => rarity;
    public float SpawnRate => Mathf.Max(0f, spawnRate);

    public LootRarityRule(ItemRarity rarity, float spawnRate, AnimationCurve progressionCurve)
    {
        this.rarity = rarity;
        this.spawnRate = Mathf.Max(0f, spawnRate);
        this.progressionCurve = progressionCurve;
    }

    public float EvaluateProgressionMultiplier(float normalizedProgression)
    {
        if (progressionCurve == null || progressionCurve.length == 0)
        {
            return 1f;
        }

        return Mathf.Max(0f, progressionCurve.Evaluate(Mathf.Clamp01(normalizedProgression)));
    }
}

[CreateAssetMenu(fileName = "LevelLootTable", menuName = "Gameplay/Level Loot Table")]
public class LevelLootTable : ScriptableObject
{
    private const string ItemSearchRoot = "Assets/Items";

    [SerializeField] [Min(0)] private int minDrops = 1;
    [SerializeField] [Min(0)] private int maxDrops = 3;
    [SerializeField] private AnimationCurve minDropsProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] private AnimationCurve maxDropsProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] private List<LootRarityRule> rarityRules = new();
    [SerializeField] [HideInInspector] private List<ItemData> autoDiscoveredItems = new();

    public int MinDrops => minDrops;
    public int MaxDrops => maxDrops;
    public IReadOnlyList<LootRarityRule> RarityRules => rarityRules;
    public IReadOnlyList<ItemData> AutoDiscoveredItems => autoDiscoveredItems;

    public int EvaluateMinDrops(int progression)
    {
        return EvaluateCount(minDrops, minDropsProgressionCurve, progression);
    }

    public int EvaluateMaxDrops(int progression)
    {
        return Mathf.Max(EvaluateMinDrops(progression), EvaluateCount(maxDrops, maxDropsProgressionCurve, progression));
    }

    private void OnValidate()
    {
        if (maxDrops < minDrops)
        {
            maxDrops = minDrops;
        }

        EnsureDefaultRarityRules();
        RefreshItemsInEditor();
    }

    public List<ItemData> RollDrops()
    {
        return RollDrops(1);
    }

    public List<ItemData> RollDrops(int progression)
    {
        int resolvedProgression = Mathf.Max(1, progression);
        int minCount = EvaluateMinDrops(resolvedProgression);
        int maxCount = EvaluateMaxDrops(resolvedProgression);
        int dropCount = UnityEngine.Random.Range(minCount, maxCount + 1);
        var drops = new List<ItemData>(dropCount);

        for (int i = 0; i < dropCount; i++)
        {
            if (!TryRollItem(resolvedProgression, out ItemData item))
            {
                break;
            }

            drops.Add(item);
        }

        return drops;
    }

    public bool TryRollItem(out ItemData item)
    {
        return TryRollItem(1, out item);
    }

    public bool TryRollItem(int progression, out ItemData item)
    {
        item = null;
        int resolvedProgression = Mathf.Max(1, progression);

        float totalWeight = 0f;
        for (int i = 0; i < autoDiscoveredItems.Count; i++)
        {
            ItemData candidate = autoDiscoveredItems[i];
            if (candidate == null)
            {
                continue;
            }

            if (!IsEligibleForRandomRoll(candidate))
            {
                continue;
            }

            totalWeight += GetEffectiveWeight(candidate, resolvedProgression);
        }

        if (totalWeight <= 0f)
        {
            return false;
        }

        float roll = UnityEngine.Random.value * totalWeight;
        for (int i = 0; i < autoDiscoveredItems.Count; i++)
        {
            ItemData candidate = autoDiscoveredItems[i];
            if (candidate == null)
            {
                continue;
            }

            if (!IsEligibleForRandomRoll(candidate))
            {
                continue;
            }

            float effectiveWeight = GetEffectiveWeight(candidate, resolvedProgression);
            if (effectiveWeight <= 0f)
            {
                continue;
            }

            roll -= effectiveWeight;
            if (roll > 0f)
            {
                continue;
            }

            item = candidate;
            return true;
        }

        return false;
    }

    public bool TryRollSpecialItem(out ItemData item)
    {
        item = null;
        List<ItemData> specialItems = new();

        for (int i = 0; i < autoDiscoveredItems.Count; i++)
        {
            ItemData candidate = autoDiscoveredItems[i];
            if (candidate == null || candidate.Rarity != ItemRarity.Special)
            {
                continue;
            }

            specialItems.Add(candidate);
        }

        if (specialItems.Count == 0)
        {
            return false;
        }

        item = specialItems[UnityEngine.Random.Range(0, specialItems.Count)];
        return item != null;
    }

    private float GetEffectiveWeight(ItemData item, int progression)
    {
        if (item == null)
        {
            return 0f;
        }

        if (!TryGetRule(item.Rarity, out LootRarityRule rule))
        {
            return 0f;
        }

        float normalizedProgression = GetNormalizedProgression(progression);
        float curveMultiplier = rule.EvaluateProgressionMultiplier(normalizedProgression);
        return rule.SpawnRate * curveMultiplier;
    }

    private static bool IsEligibleForRandomRoll(ItemData item)
    {
        return item != null && item.Rarity != ItemRarity.Special;
    }

    private bool TryGetRule(ItemRarity rarity, out LootRarityRule rule)
    {
        for (int i = 0; i < rarityRules.Count; i++)
        {
            if (rarityRules[i].Rarity != rarity)
            {
                continue;
            }

            rule = rarityRules[i];
            return true;
        }

        rule = default;
        return false;
    }

    private static float GetNormalizedProgression(int progression)
    {
        float depth = Mathf.Max(0f, progression - 1);
        return 1f - Mathf.Exp(-0.12f * depth);
    }

    private static int EvaluateCount(int baseValue, AnimationCurve progressionCurve, int progression)
    {
        float normalizedProgression = GetNormalizedProgression(progression);
        float multiplier = progressionCurve == null || progressionCurve.length == 0
            ? 1f
            : Mathf.Max(0f, progressionCurve.Evaluate(normalizedProgression));

        return Mathf.Max(0, Mathf.RoundToInt(baseValue * multiplier));
    }

    private void EnsureDefaultRarityRules()
    {
        EnsureRule(ItemRarity.Common, 1.8f, AnimationCurve.EaseInOut(0f, 1.2f, 1f, 0.2f));
        EnsureRule(ItemRarity.Uncommon, 1f, AnimationCurve.EaseInOut(0f, 0.9f, 1f, 1.1f));
        EnsureRule(ItemRarity.Rare, 0.45f, AnimationCurve.EaseInOut(0f, 0.35f, 1f, 1.7f));
        EnsureRule(ItemRarity.Epic, 0.15f, AnimationCurve.EaseInOut(0f, 0.1f, 1f, 2.2f));
        EnsureRule(ItemRarity.Special, 0f, AnimationCurve.Linear(0f, 0f, 1f, 0f));
    }

    private void EnsureRule(ItemRarity rarity, float spawnRate, AnimationCurve progressionCurve)
    {
        for (int i = 0; i < rarityRules.Count; i++)
        {
            if (rarityRules[i].Rarity == rarity)
            {
                return;
            }
        }

        rarityRules.Add(new LootRarityRule(rarity, spawnRate, progressionCurve));
    }

    private void RefreshItemsInEditor()
    {
#if UNITY_EDITOR
        string[] assetGuids = AssetDatabase.FindAssets("t:ItemData", new[] { ItemSearchRoot });
        autoDiscoveredItems.Clear();

        for (int i = 0; i < assetGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (item == null)
            {
                continue;
            }

            autoDiscoveredItems.Add(item);
        }
#endif
    }
}
