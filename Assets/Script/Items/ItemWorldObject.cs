using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemWorldObject : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isInInventory;

    private StashSpawner owningSpawner;

    public ItemData ItemData => itemData;
    public bool IsInInventory => isInInventory;
    public StashSpawner OwningSpawner => owningSpawner;

    private void Awake()
    {
        EnsureSpriteRendererReference();
        RefreshVisuals();
    }

    private void OnValidate()
    {
        EnsureSpriteRendererReference();
        RefreshVisuals();
    }

    public void SetItemData(ItemData data)
    {
        itemData = data;
        EnsureSpriteRendererReference();
        RefreshVisuals();
    }

    public void Initialize(ItemData data, StashSpawner spawner)
    {
        owningSpawner = spawner;
        isInInventory = false;
        SetItemData(data);
    }

    public void SetInventoryState(bool inInventory)
    {
        isInInventory = inInventory;
    }

    private void EnsureSpriteRendererReference()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void RefreshVisuals()
    {
        if (spriteRenderer == null || itemData == null)
        {
            return;
        }

        spriteRenderer.sprite = itemData.Icon;
    }
}
