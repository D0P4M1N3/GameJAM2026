using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InventoryTriggerZone : MonoBehaviour
{
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private StashData stashData;
    [SerializeField] private Color normalBoundsColor = new(0.15f, 0.9f, 0.95f, 0.8f);
    [SerializeField] private Color overflowBoundsColor = new(1f, 0.3f, 0.2f, 0.9f);

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemWorldObject itemWorldObject = other.GetComponent<ItemWorldObject>();
        if (itemWorldObject == null || itemWorldObject.IsInInventory || itemWorldObject.ItemData == null)
        {
            return;
        }

        if (inventoryData != null)
        {
            inventoryData.AddItem(itemWorldObject.ItemData);
        }

        if (stashData != null)
        {
            stashData.RemoveItem(itemWorldObject.ItemData);
        }

        itemWorldObject.SetInventoryState(true);
        itemWorldObject.SuppressCollisionSound();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ItemWorldObject itemWorldObject = other.GetComponent<ItemWorldObject>();
        if (itemWorldObject == null || !itemWorldObject.IsInInventory || itemWorldObject.ItemData == null)
        {
            return;
        }

        if (inventoryData != null)
        {
            inventoryData.RemoveItem(itemWorldObject.ItemData);
        }

        if (stashData != null)
        {
            stashData.AddItem(itemWorldObject.ItemData);
        }

        itemWorldObject.SetInventoryState(false);
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
}
