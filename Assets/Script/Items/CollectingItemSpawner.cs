using UnityEngine;

public class CollectingItemSpawner : MonoBehaviour
{
    public enum PendingPlacement
    {
        None = 0,
        CollectBox = 1,
        TrashZone = 2
    }

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Camera popupDragCamera;

    private GameplayItemPickup pendingPickup;
    private ItemWorldObject spawnedUiItem;
    private PlayerCollectBoxPopUP ownerPopup;
    private PendingPlacement pendingPlacement;
    private Transform pendingCollectBoxTransform;

    public bool HasPendingItem => pendingPickup != null;
    public bool CanAcceptPendingItem => pendingPickup != null && pendingPlacement != PendingPlacement.None;
    public PendingPlacement CurrentPendingPlacement => pendingPlacement;

    private void Awake()
    {
        EnsureSpawnPoint();
    }

    private void OnValidate()
    {
        EnsureSpawnPoint();
    }

    public bool BeginCollecting(GameplayItemPickup pickup, PlayerCollectBoxPopUP popupOwner)
    {
        EnsureSpawnPoint();

        if (pickup == null || pickup.ItemData == null || HasPendingItem)
        {
            return false;
        }

        GameObject itemPrefab = pickup.ItemData.ItemPrefab;
        if (itemPrefab == null)
        {
            return false;
        }

        ownerPopup = popupOwner;
        pendingPickup = pickup;
        pendingPlacement = PendingPlacement.None;
        pendingCollectBoxTransform = null;

        Transform parent = spawnPoint != null ? spawnPoint : transform;
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        GameObject instance = Instantiate(itemPrefab, spawnPosition, spawnRotation, parent);
        spawnedUiItem = instance.GetComponent<ItemWorldObject>();
        if (spawnedUiItem == null)
        {
            Destroy(instance);
            pendingPickup = null;
            ownerPopup = null;
            return false;
        }

        spawnedUiItem.Initialize(pickup.ItemData, null);
        spawnedUiItem.SetInventoryState(false);

        DraggableItem2D draggableItem = instance.GetComponent<DraggableItem2D>();
        if (draggableItem == null)
        {
            draggableItem = instance.GetComponentInChildren<DraggableItem2D>(true);
        }

        if (draggableItem != null)
        {
            draggableItem.SetRequireExplicitDragCamera(true);
            draggableItem.SetDragCamera(popupDragCamera);
        }

        return true;
    }

    public bool TrySetSpawnedItemCollectBoxState(ItemWorldObject itemWorldObject, bool shouldBeInCollectBox, Transform collectBoxTransform)
    {
        if (pendingPickup == null || itemWorldObject == null || itemWorldObject != spawnedUiItem)
        {
            return false;
        }

        if (shouldBeInCollectBox)
        {
            pendingPickup.FinalizeCollection();
            itemWorldObject.SetCollectBoxState(true);
            pendingCollectBoxTransform = null;

            if (collectBoxTransform != null)
            {
                Transform targetParent = collectBoxTransform.parent != null
                    ? collectBoxTransform.parent
                    : collectBoxTransform;
                itemWorldObject.transform.SetParent(targetParent, true);
            }

            spawnedUiItem = null;
            pendingPickup = null;
            pendingPlacement = PendingPlacement.None;
            ownerPopup?.NotifyItemCollected();
            ownerPopup = null;
            return true;
        }

        return false;
    }

    public bool TrySetSpawnedItemTrashState(ItemWorldObject itemWorldObject, bool shouldBeInTrash)
    {
        if (pendingPickup == null || itemWorldObject == null || itemWorldObject != spawnedUiItem)
        {
            return false;
        }

        if (shouldBeInTrash)
        {
            pendingPlacement = PendingPlacement.TrashZone;
            pendingCollectBoxTransform = null;
            itemWorldObject.SetCollectBoxState(false);
            ownerPopup?.RefreshAcceptButtonState();
            return true;
        }

        if (pendingPlacement == PendingPlacement.TrashZone)
        {
            pendingPlacement = PendingPlacement.None;
            ownerPopup?.RefreshAcceptButtonState();
            return true;
        }

        return false;
    }

    public bool AcceptPendingItem()
    {
        if (!CanAcceptPendingItem)
        {
            return false;
        }

        if (pendingPlacement == PendingPlacement.CollectBox)
        {
            pendingPickup.FinalizeCollection();

            if (spawnedUiItem != null)
            {
                spawnedUiItem.SetCollectBoxState(true);

                if (pendingCollectBoxTransform != null)
                {
                    Transform targetParent = pendingCollectBoxTransform.parent != null
                        ? pendingCollectBoxTransform.parent
                        : pendingCollectBoxTransform;
                    spawnedUiItem.transform.SetParent(targetParent, true);
                }
            }
        }
        else
        {
            pendingPickup.CancelPendingCollection();

            if (pendingPickup != null)
            {
                Destroy(pendingPickup.gameObject);
            }

            if (spawnedUiItem != null)
            {
                Destroy(spawnedUiItem.gameObject);
            }
        }

        spawnedUiItem = null;
        pendingPickup = null;
        pendingPlacement = PendingPlacement.None;
        pendingCollectBoxTransform = null;
        ownerPopup?.NotifyItemCollected();
        ownerPopup = null;
        return true;
    }

    public void CancelPendingItem()
    {
        if (pendingPickup != null)
        {
            pendingPickup.CancelPendingCollection();
        }

        if (spawnedUiItem != null)
        {
            Destroy(spawnedUiItem.gameObject);
        }

        spawnedUiItem = null;
        pendingPickup = null;
        pendingPlacement = PendingPlacement.None;
        pendingCollectBoxTransform = null;
        ownerPopup = null;
    }

    private void EnsureSpawnPoint()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }
}
