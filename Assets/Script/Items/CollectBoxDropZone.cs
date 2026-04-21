using UnityEngine;

public class CollectBoxDropZone : MonoBehaviour
{
    [SerializeField] private CollectingItemSpawner collectingItemSpawner;
    [SerializeField] private Collider2D triggerCollider;

    private void Awake()
    {
        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        EnsureTriggerCollider();
    }

    public void SetSpawner(CollectingItemSpawner spawner)
    {
        collectingItemSpawner = spawner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemWorldObject itemWorldObject = other != null ? other.GetComponentInParent<ItemWorldObject>() : null;
        if (itemWorldObject == null)
        {
            return;
        }

        collectingItemSpawner?.TryCollectSpawnedItem(itemWorldObject, transform);
    }

    private void EnsureTriggerCollider()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        if (triggerCollider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = ResolveLocalBoundsSize();
            triggerCollider = boxCollider;
        }

        triggerCollider.isTrigger = true;
    }

    private Vector2 ResolveLocalBoundsSize()
    {
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>(true);
        bool hasBounds = false;
        Bounds combinedBounds = default;

        for (int i = 0; i < childColliders.Length; i++)
        {
            Collider2D childCollider = childColliders[i];
            if (childCollider == null || childCollider.transform == transform)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = childCollider.bounds;
                hasBounds = true;
                continue;
            }

            combinedBounds.Encapsulate(childCollider.bounds);
        }

        if (!hasBounds)
        {
            return Vector2.one * 2f;
        }

        Vector3 localSize = transform.InverseTransformVector(combinedBounds.size);
        return new Vector2(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y));
    }
}
