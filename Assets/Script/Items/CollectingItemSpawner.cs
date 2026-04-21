using UnityEngine;

public class CollectingItemSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Camera popupDragCamera;

    private GameplayItemPickup pendingPickup;
    private ItemWorldObject spawnedUiItem;
    private PlayerCollectBoxPopUP ownerPopup;

    public bool HasPendingItem => pendingPickup != null;

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

    public bool TryCollectSpawnedItem(ItemWorldObject itemWorldObject, Transform collectBoxTransform)
    {
        if (pendingPickup == null || itemWorldObject == null || itemWorldObject != spawnedUiItem)
        {
            return false;
        }

        pendingPickup.FinalizeCollection();

        if (spawnedUiItem != null)
        {
            if (collectBoxTransform != null)
            {
                Transform targetParent = collectBoxTransform.parent != null
                    ? collectBoxTransform.parent
                    : collectBoxTransform;
                spawnedUiItem.transform.SetParent(targetParent, true);
            }
        }

        spawnedUiItem = null;
        pendingPickup = null;
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
