using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

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
