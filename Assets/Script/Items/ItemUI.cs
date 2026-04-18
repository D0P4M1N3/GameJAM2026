using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

    private ItemData boundItemData;
    private HoveredItemStatsUI hoverStatsUi;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnValidate()
    {
        EnsureReferences();
    }

    public void Bind(ItemData itemData, int amount)
    {
        EnsureReferences();
        boundItemData = itemData;

        Sprite icon = itemData != null ? itemData.Icon : null;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (amountText != null)
        {
            amountText.text = Mathf.Max(0, amount).ToString();
        }
    }

    public void SetHoverStatsUi(HoveredItemStatsUI targetHoverStatsUi)
    {
        hoverStatsUi = targetHoverStatsUi;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverStatsUi == null)
        {
            hoverStatsUi = FindFirstObjectByType<HoveredItemStatsUI>();
        }

        if (hoverStatsUi == null || boundItemData == null)
        {
            return;
        }

        hoverStatsUi.ShowItem(boundItemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverStatsUi == null)
        {
            hoverStatsUi = FindFirstObjectByType<HoveredItemStatsUI>();
        }

        if (hoverStatsUi == null)
        {
            return;
        }

        hoverStatsUi.ClearIfShowing(boundItemData);
    }

    private void OnDisable()
    {
        if (hoverStatsUi == null)
        {
            return;
        }

        hoverStatsUi.ClearIfShowing(boundItemData);
    }

    private void EnsureReferences()
    {
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("ItemIcon");
            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }
        }

        if (amountText == null)
        {
            Transform amountTransform = transform.Find("Amount");
            if (amountTransform != null)
            {
                amountText = amountTransform.GetComponent<TMP_Text>();
            }
        }
    }
}
