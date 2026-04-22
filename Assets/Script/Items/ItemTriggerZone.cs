using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemTriggerZone : MonoBehaviour
{
    public enum ZoneMode
    {
        Inventory = 0,
        CollectBox = 1
    }

    [SerializeField] private ZoneMode zoneMode = ZoneMode.Inventory;
    [SerializeField] private CollectingItemSpawner collectingItemSpawner;
    [SerializeField] private Color boundsColor = new(0.15f, 0.9f, 0.95f, 0.8f);

    public ZoneMode CurrentMode => zoneMode;
    public bool CollectBoxExitRemovalEnabled { get; private set; } = true;

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (zoneMode == ZoneMode.CollectBox)
        {
            SetItemCollectBoxMembership(other, shouldBeInCollectBox: true);
            return;
        }

        SetItemInventoryMembership(other, shouldBeInInventory: true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (zoneMode == ZoneMode.CollectBox)
        {
            SetItemCollectBoxMembership(other, shouldBeInCollectBox: false);
            return;
        }

        if (zoneMode != ZoneMode.Inventory)
        {
            return;
        }

        SetItemInventoryMembership(other, shouldBeInInventory: false);
    }

    public void SetCollectBoxSpawner(CollectingItemSpawner spawner)
    {
        zoneMode = ZoneMode.CollectBox;
        collectingItemSpawner = spawner;
    }

    public void SetCollectBoxExitRemovalEnabled(bool isEnabled)
    {
        CollectBoxExitRemovalEnabled = isEnabled;
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
        InventoryData inventoryData = GameManager.Instance != null ? GameManager.Instance.InventoryData : null;
        StashData stashData = GameManager.Instance != null ? GameManager.Instance.StashData : null;

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

    private void SetItemCollectBoxMembership(Collider2D itemCollider, bool shouldBeInCollectBox)
    {
        CollectBoxData collectBoxData = GameManager.Instance != null ? GameManager.Instance.CollectBoxData : null;
        ItemWorldObject itemWorldObject = itemCollider != null ? itemCollider.GetComponentInParent<ItemWorldObject>() : null;
        if (itemWorldObject == null || itemWorldObject.ItemData == null || itemWorldObject.IsInCollectBox == shouldBeInCollectBox)
        {
            return;
        }

        if (shouldBeInCollectBox)
        {
            bool wasCollectedNow = collectingItemSpawner != null &&
                collectingItemSpawner.TryCollectSpawnedItem(itemWorldObject, transform);

            if (!wasCollectedNow && collectBoxData != null)
            {
                collectBoxData.AddItem(itemWorldObject.ItemData);
            }

            itemWorldObject.SetCollectBoxState(true);

            return;
        }

        if (!CollectBoxExitRemovalEnabled)
        {
            return;
        }

        collectBoxData?.RemoveItem(itemWorldObject.ItemData);
        itemWorldObject.SetCollectBoxState(false);
    }
}
