using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EndingSellZone : MonoBehaviour
{
    [SerializeField] private TMP_Text totalValueText;
    [SerializeField] private GameObject valuePopupPrefab;
    [SerializeField] private string totalPrefix = "Total Value: ";
    [SerializeField] private string valuePrefix = "+";
    [SerializeField] private Vector3 popupOffset = new(0f, 0.6f, 0f);
    [SerializeField] [Min(0f)] private float destroySoldItemDelay = 0.05f;
    [SerializeField] private bool removeSoldItemFromStash = true;
    [SerializeField] private bool destroySoldItem = true;
    [Header("Total Text Pop")]
    [SerializeField] [Min(0f)] private float minTextPopScale = 0.12f;
    [SerializeField] [Min(0f)] private float valueToPopScaleMultiplier = 0.015f;
    [SerializeField] [Min(0f)] private float maxTextPopScale = 0.65f;
    [SerializeField] [Min(0.01f)] private float textPopDuration = 0.28f;
    [SerializeField] [Min(0f)] private float textPopOvershoot = 0.18f;

    private readonly HashSet<int> processedItemIds = new();
    private float totalValue;
    private Vector3 totalTextBaseScale = Vector3.one;
    private float currentTextPopStrength;
    private float textPopTime = float.MaxValue;

    private void Start()
    {
        CacheTotalTextBaseScale();
        UpdateTotalText();
    }

    private void Awake()
    {
        CacheTotalTextBaseScale();
    }

    private void Update()
    {
        UpdateTotalTextPopAnimation();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        Vector3 hitPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : collision.transform.position;

        TrySellItem(collision.collider, hitPoint);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        TrySellItem(other, other.transform.position);
    }

    private void TrySellItem(Component source, Vector3 hitPoint)
    {
        ItemWorldObject itemWorldObject = source != null ? source.GetComponentInParent<ItemWorldObject>() : null;
        if (itemWorldObject == null || itemWorldObject.ItemData == null)
        {
            return;
        }

        int itemId = itemWorldObject.GetInstanceID();
        if (!processedItemIds.Add(itemId))
        {
            return;
        }

        float soldValue = Mathf.Max(0f, itemWorldObject.ItemData.Stats.Value);
        totalValue += soldValue;
        UpdateTotalText();
        TriggerTotalTextPop(soldValue);
        SpawnValuePopup(itemWorldObject.ItemData, soldValue, hitPoint + popupOffset);

        if (removeSoldItemFromStash)
        {
            StashData stashData = GameManager.Instance != null ? GameManager.Instance.StashData : null;
            stashData?.RemoveItem(itemWorldObject.ItemData);
        }

        if (destroySoldItem)
        {
            DisableItemPhysics(itemWorldObject);
            Destroy(itemWorldObject.gameObject, destroySoldItemDelay);
        }
    }

    private void UpdateTotalText()
    {
        if (totalValueText == null)
        {
            return;
        }

        totalValueText.text = totalPrefix + FormatValue(totalValue);
    }

    private void TriggerTotalTextPop(float soldValue)
    {
        if (totalValueText == null)
        {
            return;
        }

        float popStrength = minTextPopScale + (Mathf.Max(0f, soldValue) * valueToPopScaleMultiplier);
        currentTextPopStrength = Mathf.Min(maxTextPopScale, currentTextPopStrength + popStrength);
        textPopTime = 0f;
    }

    private void UpdateTotalTextPopAnimation()
    {
        if (totalValueText == null)
        {
            return;
        }

        if (textPopTime == float.MaxValue)
        {
            totalValueText.transform.localScale = totalTextBaseScale;
            return;
        }

        textPopTime += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(textPopTime / textPopDuration);

        float scaleOffset;
        if (normalizedTime < 0.45f)
        {
            float expandBlend = normalizedTime / 0.45f;
            scaleOffset = Mathf.LerpUnclamped(0f, currentTextPopStrength, expandBlend);
        }
        else
        {
            float settleBlend = (normalizedTime - 0.45f) / 0.55f;
            float overshootTarget = -currentTextPopStrength * textPopOvershoot;
            scaleOffset = Mathf.LerpUnclamped(currentTextPopStrength, overshootTarget, settleBlend);
            scaleOffset = Mathf.LerpUnclamped(scaleOffset, 0f, settleBlend * settleBlend);
        }

        totalValueText.transform.localScale = totalTextBaseScale * (1f + scaleOffset);

        if (normalizedTime >= 1f)
        {
            totalValueText.transform.localScale = totalTextBaseScale;
            currentTextPopStrength = 0f;
            textPopTime = float.MaxValue;
        }
    }

    private void CacheTotalTextBaseScale()
    {
        if (totalValueText == null)
        {
            return;
        }

        totalTextBaseScale = totalValueText.transform.localScale;
    }

    private void SpawnValuePopup(ItemData itemData, float soldValue, Vector3 worldPosition)
    {
        GameObject popupInstance = valuePopupPrefab != null
            ? InstantiateValuePopupPrefab(worldPosition)
            : CreateFallbackPopup(worldPosition);

        if (popupInstance == null)
        {
            return;
        }

        TMP_Text popupText = popupInstance.GetComponentInChildren<TMP_Text>(true);
        if (popupText == null)
        {
            Destroy(popupInstance);
            popupInstance = CreateFallbackPopup(worldPosition);
            popupText = popupInstance != null ? popupInstance.GetComponentInChildren<TMP_Text>(true) : null;
        }

        if (popupText == null)
        {
            return;
        }

        popupText.text = valuePrefix + FormatValue(soldValue);

        if (itemData != null)
        {
            popupText.color = itemData.ItemColor;
        }

        FloatingValueText floatingValueText = popupInstance.GetComponent<FloatingValueText>();
        if (floatingValueText == null)
        {
            floatingValueText = popupInstance.AddComponent<FloatingValueText>();
        }

        floatingValueText.Play();
    }

    private GameObject InstantiateValuePopupPrefab(Vector3 worldPosition)
    {
        if (valuePopupPrefab == null)
        {
            return null;
        }

        return Instantiate(valuePopupPrefab, worldPosition, Quaternion.identity);
    }

    private static GameObject CreateFallbackPopup(Vector3 worldPosition)
    {
        GameObject popupObject = new("EndingSellValuePopup");
        popupObject.transform.position = worldPosition;

        TextMeshPro textMesh = popupObject.AddComponent<TextMeshPro>();
        textMesh.fontSize = 4f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        textMesh.raycastTarget = false;
        textMesh.color = Color.white;

        return popupObject;
    }

    private static void DisableItemPhysics(ItemWorldObject itemWorldObject)
    {
        Collider2D[] colliders = itemWorldObject.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        Rigidbody2D[] rigidbodies = itemWorldObject.GetComponentsInChildren<Rigidbody2D>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i] != null)
            {
                rigidbodies[i].simulated = false;
            }
        }
    }

    private static string FormatValue(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value))
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.##");
    }
}
