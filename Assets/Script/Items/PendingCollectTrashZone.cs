using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PendingCollectTrashZone : MonoBehaviour
{
    [SerializeField] private CollectingItemSpawner collectingItemSpawner;
    [SerializeField] private PlayerCollectBoxPopUP ownerPopup;

    private void OnTriggerStay2D(Collider2D other)
    {
        ItemWorldObject itemWorldObject = other != null ? other.GetComponentInParent<ItemWorldObject>() : null;
        if (itemWorldObject == null)
        {
            return;
        }

        DraggableItem2D draggableItem = itemWorldObject.GetComponent<DraggableItem2D>();
        if (draggableItem != null && draggableItem.IsDragging)
        {
            return;
        }

        if (ownerPopup != null && ownerPopup.TryDeletePopupItem(itemWorldObject))
        {
            TriggerTrashFace();
        }
    }

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemWorldObject itemWorldObject = other != null ? other.GetComponentInParent<ItemWorldObject>() : null;
        if (itemWorldObject == null)
        {
            return;
        }

        if (ownerPopup != null && ownerPopup.TrySetCollectBoxItemTrashState(itemWorldObject, true))
        {
            TriggerTrashFace();
            return;
        }

        if (collectingItemSpawner != null && collectingItemSpawner.TrySetSpawnedItemTrashState(itemWorldObject, true))
        {
            TriggerTrashFace();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ItemWorldObject itemWorldObject = other != null ? other.GetComponentInParent<ItemWorldObject>() : null;
        if (itemWorldObject == null)
        {
            return;
        }

        if (ownerPopup != null && ownerPopup.TrySetCollectBoxItemTrashState(itemWorldObject, false))
        {
            return;
        }

        collectingItemSpawner?.TrySetSpawnedItemTrashState(itemWorldObject, false);
    }

    public void SetCollectingItemSpawner(CollectingItemSpawner spawner)
    {
        collectingItemSpawner = spawner;
    }

    public void SetOwnerPopup(PlayerCollectBoxPopUP popup)
    {
        ownerPopup = popup;
    }

    private static void TriggerTrashFace()
    {
        if (DATA_Player.Instance != null)
        {
            DATA_Player.Instance.SetFaceForDuration(PlayerFaceVariant.E, 0.5f);
        }
    }
}
