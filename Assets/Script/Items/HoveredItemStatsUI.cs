using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private CanvasGroup panelCanvasGroup;
    private ItemData currentItem;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        EnsureContentRootReference();

        if (panelRoot != null)
        {
            panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panelRoot.AddComponent<CanvasGroup>();
            }
        }
    }

    private void OnValidate()
    {
        EnsureContentRootReference();
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
        RebuildLayout();
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
        ShowTitle();
        ShowDescription();
        ClearRows();
        RebuildLayout();
        SetPanelVisible(false);
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

        StatRowUI row = Instantiate(statRowPrefab, contentRoot, false);
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

    private void RebuildLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (contentRoot is RectTransform contentRectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);
        }
    }

    private void SetPanelVisible(bool isVisible)
    {
        if (panelCanvasGroup == null)
        {
            return;
        }

        panelCanvasGroup.alpha = isVisible ? 1f : 0f;
        panelCanvasGroup.interactable = isVisible;
        panelCanvasGroup.blocksRaycasts = isVisible;
    }

    private void EnsureContentRootReference()
    {
        if (contentRoot != null)
        {
            return;
        }

        Transform statContentRow = transform.Find("StatContentRow");
        if (statContentRow != null)
        {
            contentRoot = statContentRow;
            return;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<VerticalLayoutGroup>() != null || child.GetComponent<HorizontalLayoutGroup>() != null)
            {
                contentRoot = child;
                return;
            }
        }

        contentRoot = null;
    }
}
