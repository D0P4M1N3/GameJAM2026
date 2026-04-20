using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InventoryTriggerZone : MonoBehaviour
{
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private StashData stashData;
    [SerializeField] private Color boundsColor = new(0.15f, 0.9f, 0.95f, 0.8f);

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SetItemInventoryMembership(other, shouldBeInInventory: true);
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

        Gizmos.color = boundsColor;

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

    private void SetItemInventoryMembership(Collider2D itemCollider, bool shouldBeInInventory)
    {
        ItemWorldObject itemWorldObject = itemCollider != null ? itemCollider.GetComponentInParent<ItemWorldObject>() : null;
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
}
