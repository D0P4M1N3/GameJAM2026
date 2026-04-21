using System.Collections.Generic;
using UnityEngine;

public class ItemStatsListUI : MonoBehaviour
{
    private enum DataSourceType
    {
        Stash,
        Inventory,
        CollectBox,
    }

    [SerializeField] private DataSourceType dataSourceType = DataSourceType.Stash;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private StatRowUI statRowPrefab;
    [SerializeField] private bool hideZeroStats;

    private readonly List<StatRowUI> spawnedRows = new();
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

        ResolveDataSources();
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

    private ItemStats GetStats()
    {
        return dataSourceType switch
        {
            DataSourceType.Inventory when inventoryData != null => inventoryData.TotalStats,
            DataSourceType.Stash when stashData != null => stashData.TotalStats,
            DataSourceType.CollectBox when collectBoxData != null => collectBoxData.TotalStats,
            _ => ItemStats.Zero,
        };
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
