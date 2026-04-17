using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InventoryTriggerZone : MonoBehaviour
{
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private StashData stashData;

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
}
