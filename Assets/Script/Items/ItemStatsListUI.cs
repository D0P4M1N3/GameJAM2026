using System.Collections.Generic;
using UnityEngine;

public class ItemStatsListUI : MonoBehaviour
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
    [SerializeField] private StatRowUI statRowPrefab;
    [SerializeField] private bool hideZeroStats;

    private readonly List<StatRowUI> spawnedRows = new();

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

    [ContextMenu("Refresh Stats List UI")]
    public void Refresh()
    {
        ClearRows();

        if (contentRoot == null || statRowPrefab == null)
        {
            return;
        }

        ItemStats stats = GetStats();

        TrySpawnRow("Speed", stats.Speed);
        TrySpawnRow("Health", stats.Health);
        TrySpawnRow("Attack", stats.Attack);
        TrySpawnRow("Value", stats.Value);
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

    private ItemStats GetStats()
    {
        return dataSourceType switch
        {
            DataSourceType.Inventory when inventoryData != null => inventoryData.TotalStats,
            DataSourceType.Stash when stashData != null => stashData.TotalStats,
            _ => ItemStats.Zero,
        };
    }

    private void TrySpawnRow(string statName, float statValue)
    {
        if (hideZeroStats && Mathf.Approximately(statValue, 0f))
        {
            return;
        }

        StatRowUI row = Instantiate(statRowPrefab, contentRoot);
        row.Bind(statName, statValue);
        spawnedRows.Add(row);
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] == null)
            {
                continue;
            }

            Destroy(spawnedRows[i].gameObject);
        }

        spawnedRows.Clear();
    }
}
