using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HoveredItemStatsUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private StatRowUI statRowPrefab;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private bool hideZeroStats;
    [SerializeField] private string emptyTitle = "Hover Item";
    [SerializeField] [TextArea] private string emptyDescription = "";

    private readonly List<StatRowUI> spawnedRows = new();
    private ItemData currentItem;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (contentRoot == null)
        {
            contentRoot = transform;
        }
    }

    private void OnEnable()
    {
        Clear();
    }

    public void ShowItem(ItemData itemData)
    {
        currentItem = itemData;
        SetPanelVisible(true);
        ShowTitle();
        ShowDescription();
        RefreshRows(itemData != null ? itemData.Stats : ItemStats.Zero);
    }

    public void ClearIfShowing(ItemData itemData)
    {
        if (currentItem != itemData)
        {
            return;
        }

        Clear();
    }

    public void Clear()
    {
        currentItem = null;
        SetPanelVisible(false);
        ShowTitle();
        ShowDescription();
        ClearRows();
    }

    private void ShowTitle()
    {
        if (itemNameText == null)
        {
            return;
        }

        itemNameText.text = currentItem != null ? currentItem.DisplayName : emptyTitle;
    }

    private void ShowDescription()
    {
        if (descriptionText == null)
        {
            return;
        }

        descriptionText.text = currentItem != null ? currentItem.Description : emptyDescription;
    }

    private void RefreshRows(ItemStats stats)
    {
        ClearRows();

        if (contentRoot == null || statRowPrefab == null)
        {
            return;
        }

        TrySpawnRow("Speed", stats.Speed);
        TrySpawnRow("Health", stats.Health);
        TrySpawnRow("Attack", stats.Attack);
        TrySpawnRow("Value", stats.Value);
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

    private void SetPanelVisible(bool isVisible)
    {
        if (panelRoot == null)
        {
            return;
        }

        panelRoot.SetActive(isVisible);
    }
}
