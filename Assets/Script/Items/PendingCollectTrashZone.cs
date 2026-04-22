using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PendingCollectTrashZone : MonoBehaviour
{
    [SerializeField] private CollectingItemSpawner collectingItemSpawner;

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collectingItemSpawner == null)
        {
            return;
        }

        ItemWorldObject itemWorldObject = other != null ? other.GetComponentInParent<ItemWorldObject>() : null;
        if (itemWorldObject == null)
        {
            return;
        }

        collectingItemSpawner.TryDeleteSpawnedItem(itemWorldObject);
    }

    public void SetCollectingItemSpawner(CollectingItemSpawner spawner)
    {
        collectingItemSpawner = spawner;
    }
}
