using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InventoryTriggerZone : MonoBehaviour
{
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private StashData stashData;
    [SerializeField] [Range(0f, 1f)] private float requiredOverlapRatio = 0.6f;
    [SerializeField] private Color normalBoundsColor = new(0.15f, 0.9f, 0.95f, 0.8f);
    [SerializeField] private Color overflowBoundsColor = new(1f, 0.3f, 0.2f, 0.9f);

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EvaluateItemOverlap(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        EvaluateItemOverlap(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        SetItemInventoryMembership(other, shouldBeInInventory: false);
    }

    private void OnDrawGizmos()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider == null)
        {
            return;
        }

        Gizmos.color = inventoryData != null && inventoryData.IsOverflowing
            ? overflowBoundsColor
            : normalBoundsColor;

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (zoneCollider is BoxCollider2D boxCollider)
        {
            Vector3 center = new(boxCollider.offset.x, boxCollider.offset.y, 0f);
            Vector3 size = new(boxCollider.size.x, boxCollider.size.y, 0.05f);
            Gizmos.DrawWireCube(center, size);
        }
        else if (zoneCollider is CircleCollider2D circleCollider)
        {
            Vector3 center = new(circleCollider.offset.x, circleCollider.offset.y, 0f);
            Gizmos.DrawWireSphere(center, circleCollider.radius);
        }

        Gizmos.matrix = previousMatrix;
    }

    private void EvaluateItemOverlap(Collider2D itemCollider)
    {
        if (itemCollider == null)
        {
            return;
        }

        SetItemInventoryMembership(itemCollider, GetOverlapRatio(itemCollider) >= requiredOverlapRatio);
    }

    private void SetItemInventoryMembership(Collider2D itemCollider, bool shouldBeInInventory)
    {
        ItemWorldObject itemWorldObject = itemCollider != null ? itemCollider.GetComponent<ItemWorldObject>() : null;
        if (itemWorldObject == null || itemWorldObject.ItemData == null || itemWorldObject.IsInInventory == shouldBeInInventory)
        {
            return;
        }

        if (shouldBeInInventory)
        {
            if (inventoryData != null)
            {
                inventoryData.AddItem(itemWorldObject.ItemData);
            }

            if (stashData != null)
            {
                stashData.RemoveItem(itemWorldObject.ItemData);
            }

            itemWorldObject.SuppressCollisionSound();
        }
        else
        {
            if (inventoryData != null)
            {
                inventoryData.RemoveItem(itemWorldObject.ItemData);
            }

            if (stashData != null)
            {
                stashData.AddItem(itemWorldObject.ItemData);
            }
        }

        itemWorldObject.SetInventoryState(shouldBeInInventory);
    }

    private float GetOverlapRatio(Collider2D itemCollider)
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider == null || itemCollider == null)
        {
            return 0f;
        }

        Bounds zoneBounds = zoneCollider.bounds;
        Bounds itemBounds = itemCollider.bounds;
        float itemArea = itemBounds.size.x * itemBounds.size.y;
        if (itemArea <= Mathf.Epsilon)
        {
            return 0f;
        }

        float overlapWidth = Mathf.Max(0f, Mathf.Min(zoneBounds.max.x, itemBounds.max.x) - Mathf.Max(zoneBounds.min.x, itemBounds.min.x));
        float overlapHeight = Mathf.Max(0f, Mathf.Min(zoneBounds.max.y, itemBounds.max.y) - Mathf.Max(zoneBounds.min.y, itemBounds.min.y));
        float overlapArea = overlapWidth * overlapHeight;
        return overlapArea / itemArea;
    }
}
