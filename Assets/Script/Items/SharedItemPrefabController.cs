using UnityEngine;

public enum SharedItemPrefabMode
{
    Ui,
    WorldPickup,
}

public class SharedItemPrefabController : MonoBehaviour
{
    [SerializeField] private SharedItemPrefabMode defaultMode = SharedItemPrefabMode.Ui;
    [SerializeField] [HideInInspector] private GameObject uiModeRoot;
    [SerializeField] [HideInInspector] private SpriteRenderer uiSpriteRenderer;
    [SerializeField] [HideInInspector] private Collider2D[] uiColliders;
    [SerializeField] [HideInInspector] private Rigidbody2D uiRigidbody;
    [SerializeField] [HideInInspector] private DraggableItem2D draggableItem;
    [SerializeField] [HideInInspector] private GameObject worldModeRoot;
    [SerializeField] [HideInInspector] private SpriteRenderer worldSpriteRenderer;
    [SerializeField] [HideInInspector] private Collider worldCollider;
    [SerializeField] private bool useItemIconForUi;
    [SerializeField] private bool useItemIconForWorld;

    private Camera cachedCamera;
    private SharedItemPrefabMode currentMode;

    public void InitializeForUi(ItemData itemData)
    {
        EnsureReferences();
        ApplyItemData(itemData);
        SetMode(SharedItemPrefabMode.Ui);
    }

    public void InitializeForWorld(ItemData itemData)
    {
        EnsureReferences();
        ApplyItemData(itemData);
        SetMode(SharedItemPrefabMode.WorldPickup);
    }

    public void ApplyItemData(ItemData itemData)
    {
        if (itemData == null)
        {
            return;
        }

        if (useItemIconForUi && uiSpriteRenderer != null)
        {
            uiSpriteRenderer.sprite = itemData.Icon;
        }

        if (useItemIconForWorld && worldSpriteRenderer != null)
        {
            worldSpriteRenderer.sprite = itemData.Icon;
        }
    }

    private void Awake()
    {
        EnsureReferences();
        SetMode(defaultMode);
    }

    private void OnValidate()
    {
        EnsureReferences();
        SetMode(defaultMode);
    }

    private void LateUpdate()
    {
        if (currentMode != SharedItemPrefabMode.WorldPickup || worldModeRoot == null || !worldModeRoot.activeSelf)
        {
            return;
        }

        Camera activeCamera = ResolveCamera();
        if (activeCamera == null)
        {
            return;
        }

        worldModeRoot.transform.rotation = activeCamera.transform.rotation;
    }

    private void SetMode(SharedItemPrefabMode mode)
    {
        currentMode = mode;

        bool uiActive = mode == SharedItemPrefabMode.Ui;
        if (uiModeRoot != null)
        {
            uiModeRoot.SetActive(uiActive);
        }

        if (worldModeRoot != null)
        {
            worldModeRoot.SetActive(!uiActive);
        }

        if (uiColliders != null)
        {
            for (int i = 0; i < uiColliders.Length; i++)
            {
                if (uiColliders[i] != null)
                {
                    uiColliders[i].enabled = uiActive;
                }
            }
        }

        if (uiRigidbody != null)
        {
            uiRigidbody.simulated = uiActive;
        }

        if (draggableItem != null)
        {
            draggableItem.enabled = uiActive;
        }

        if (worldCollider != null)
        {
            worldCollider.enabled = !uiActive;
        }
    }

    private void EnsureReferences()
    {
        if (uiModeRoot == null)
        {
            Transform uiTransform = transform.Find("UI Mode");
            if (uiTransform != null)
            {
                uiModeRoot = uiTransform.gameObject;
            }
        }

        if (worldModeRoot == null)
        {
            Transform worldTransform = transform.Find("World Mode");
            if (worldTransform != null)
            {
                worldModeRoot = worldTransform.gameObject;
            }
        }

        if (uiSpriteRenderer == null && uiModeRoot != null)
        {
            uiSpriteRenderer = uiModeRoot.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (worldSpriteRenderer == null && worldModeRoot != null)
        {
            worldSpriteRenderer = worldModeRoot.GetComponentInChildren<SpriteRenderer>(true);
        }

        if ((uiColliders == null || uiColliders.Length == 0))
        {
            uiColliders = uiModeRoot != null
                ? uiModeRoot.GetComponentsInChildren<Collider2D>(true)
                : GetComponentsInChildren<Collider2D>(true);
        }

        if (uiRigidbody == null)
        {
            uiRigidbody = GetComponent<Rigidbody2D>();
        }

        if (draggableItem == null)
        {
            draggableItem = GetComponent<DraggableItem2D>();
        }

        if (worldCollider == null)
        {
            worldCollider = worldModeRoot != null
                ? worldModeRoot.GetComponentInChildren<Collider>(true)
                : GetComponentInChildren<Collider>(true);
        }
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
