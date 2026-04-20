using UnityEngine;

public enum GameplayPickupDestination
{
    Inventory,
    Stash,
    CollectBox,
}

public class GameplayItemPickup : MonoBehaviour
{
    [SerializeField] [HideInInspector] private ItemData itemData;
    [SerializeField] [HideInInspector] private SpriteRenderer iconRenderer;
    [SerializeField] private GameplayPickupDestination destination = GameplayPickupDestination.CollectBox;
    [SerializeField] [Min(0f)] private float spriteScale = 1f;

    private bool hasCollected;
    private Camera cachedCamera;
    private SharedItemPrefabController sharedPrefabController;

    public ItemData ItemData => itemData;

    private void Awake()
    {
        EnsureReferences();
        ConfigureCollider();
        RefreshVisuals();
    }

    private void OnValidate()
    {
        EnsureReferences();
        ConfigureCollider();
        RefreshVisuals();
    }

    private void Reset()
    {
        EnsureReferences();
        ConfigureCollider();
        transform.localScale = Vector3.one;
    }

    private void LateUpdate()
    {
        if (sharedPrefabController != null)
        {
            return;
        }

        Camera activeCamera = ResolveCamera();
        if (activeCamera == null)
        {
            return;
        }

        transform.rotation = activeCamera.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sharedPrefabController != null)
        {
            return;
        }

        HandleTriggerEnter(other);
    }

    public void HandleTriggerEnter(Collider other)
    {
        if (hasCollected || itemData == null || other == null)
        {
            return;
        }

        if (other.GetComponentInParent<TopDownController>() == null)
        {
            return;
        }

        Collect();
    }

    public void Initialize(ItemData data)
    {
        itemData = data;
        sharedPrefabController?.InitializeForWorld(data);
        RefreshVisuals();
    }

    private void Collect()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        hasCollected = true;

        switch (destination)
        {
            case GameplayPickupDestination.Inventory:
                GameManager.Instance.AddItemToInventory(itemData);
                break;
            case GameplayPickupDestination.Stash:
                GameManager.Instance.AddItemToStash(itemData);
                break;
            default:
                GameManager.Instance.AddItemToCollectBox(itemData);
                break;
        }

        if (itemData.PickupClip != null)
        {
            AudioSource.PlayClipAtPoint(itemData.PickupClip, transform.position, itemData.PickupVolume);
        }

        Destroy(gameObject);
    }

    private void EnsureReferences()
    {
        if (sharedPrefabController == null)
        {
            sharedPrefabController = GetComponent<SharedItemPrefabController>();
        }

        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void ConfigureCollider()
    {
        SphereCollider pickupCollider = sharedPrefabController != null
            ? GetComponentInChildren<SphereCollider>(true)
            : GetComponent<SphereCollider>();
        if (pickupCollider == null)
        {
            return;
        }

        pickupCollider.isTrigger = true;
        pickupCollider.radius = 0.5f;
    }

    private void RefreshVisuals()
    {
        if (sharedPrefabController != null)
        {
            transform.localScale = Vector3.one * Mathf.Max(0.01f, spriteScale);
            return;
        }

        if (iconRenderer == null)
        {
            return;
        }

        transform.localScale = Vector3.one * Mathf.Max(0.01f, spriteScale);
    }

    private Camera ResolveCamera()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        return cachedCamera;
    }
}
