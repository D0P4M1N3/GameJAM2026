using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ItemWorldObject : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] [HideInInspector] private SpriteRenderer spriteRenderer;
    [SerializeField] [HideInInspector] private AudioSource audioSource;
    [SerializeField] [HideInInspector] private HoveredItemStatsUI hoverStatsUi;
    [SerializeField] private bool isInInventory;
    [SerializeField] private bool isInCollectBox;
    [SerializeField] private float collisionSoundThreshold = 1.5f;
    [SerializeField] private float collisionSoundCooldown = 0.1f;
    [SerializeField] private float pickupCollisionMuteDuration = 0.2f;
    [SerializeField] private Vector2 randomPitchRange = new(0.92f, 1.08f);

    private StashSpawner owningSpawner;
    private DraggableItem2D draggableItem;
    private SharedItemPrefabController sharedPrefabController;
    private float lastCollisionSoundTime = float.NegativeInfinity;
    private float collisionSoundSuppressedUntil = float.NegativeInfinity;
    private bool keepHoverVisibleWhileDragging;

    public ItemData ItemData => itemData;
    public bool IsInInventory => isInInventory;
    public bool IsInCollectBox => isInCollectBox;
    public StashSpawner OwningSpawner => owningSpawner;

    private void Awake()
    {
        EnsureSpriteRendererReference();
        EnsureAudioSourceReference();
        EnsureHoverStatsUiReference();
        RefreshVisuals();
    }

    private void OnValidate()
    {
        EnsureSpriteRendererReference();
        EnsureAudioSourceReference();
        EnsureHoverStatsUiReference();
        ConfigureAudioSource();
        RefreshVisuals();
    }

    private void Reset()
    {
        EnsureSpriteRendererReference();
        EnsureAudioSourceReference();
        EnsureHoverStatsUiReference();
        ConfigureAudioSource();
    }

    private void OnMouseEnter()
    {
        if (sharedPrefabController != null)
        {
            return;
        }

        HandlePointerEnter();
    }

    public void HandlePointerEnter()
    {
        Debug.Log($"Pointer hit item: {itemData?.DisplayName ?? gameObject.name}", this);
        ShowHoverStats();
    }

    private void OnMouseExit()
    {
        if (sharedPrefabController != null)
        {
            return;
        }

        HandlePointerExit();
    }

    public void HandlePointerExit()
    {
        if (keepHoverVisibleWhileDragging)
        {
            return;
        }

        ClearHoverStats();
    }

    private void OnDisable()
    {
        ClearHoverStats();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (sharedPrefabController != null)
        {
            return;
        }

        HandleCollisionEnter2D(collision);
    }

    public void HandleCollisionEnter2D(Collision2D collision)
    {
        if (!CanPlayCollisionSound(collision))
        {
            return;
        }

        lastCollisionSoundTime = Time.time;
        PlayClipWithRandomPitch(itemData.CollisionClip, itemData.CollisionVolume);
    }

    public void SetItemData(ItemData data)
    {
        itemData = data;
        EnsureSpriteRendererReference();
        sharedPrefabController?.ApplyItemData(data);
        RefreshVisuals();
    }

    public void Initialize(ItemData data, StashSpawner spawner)
    {
        owningSpawner = spawner;
        isInInventory = false;
        isInCollectBox = false;
        SetItemData(data);
        sharedPrefabController?.InitializeForUi(data);
    }

    public void SetInventoryState(bool inInventory)
    {
        isInInventory = inInventory;
    }

    public void SetCollectBoxState(bool inCollectBox)
    {
        isInCollectBox = inCollectBox;
    }

    public void SuppressCollisionSound()
    {
        collisionSoundSuppressedUntil = Time.time + pickupCollisionMuteDuration;
    }

    public void PlayPickupSound()
    {
        if (itemData == null || itemData.PickupClip == null || audioSource == null)
        {
            return;
        }

        PlayClipWithRandomPitch(itemData.PickupClip, itemData.PickupVolume);
    }

    public void PlayCollisionSound()
    {
        if (itemData == null || itemData.CollisionClip == null || audioSource == null)
        {
            return;
        }

        lastCollisionSoundTime = Time.time;
        PlayClipWithRandomPitch(itemData.CollisionClip, itemData.CollisionVolume);
    }

    public void BeginDragHover()
    {
        keepHoverVisibleWhileDragging = true;
        ShowHoverStats();
    }

    public void EndDragHover()
    {
        keepHoverVisibleWhileDragging = false;
        ClearHoverStats();
    }

    private void EnsureSpriteRendererReference()
    {
        if (sharedPrefabController == null)
        {
            sharedPrefabController = GetComponent<SharedItemPrefabController>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void EnsureAudioSourceReference()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (draggableItem == null)
        {
            draggableItem = GetComponent<DraggableItem2D>();
        }
    }

    private void EnsureHoverStatsUiReference()
    {
        if (hoverStatsUi == null)
        {
            hoverStatsUi = FindFirstObjectByType<HoveredItemStatsUI>();
        }
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void PlayClipWithRandomPitch(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        float originalPitch = audioSource.pitch;
        audioSource.pitch = GetRandomPitch();
        audioSource.PlayOneShot(clip, volume);
        audioSource.pitch = originalPitch;
    }

    private float GetRandomPitch()
    {
        float minPitch = Mathf.Min(randomPitchRange.x, randomPitchRange.y);
        float maxPitch = Mathf.Max(randomPitchRange.x, randomPitchRange.y);

        if (Mathf.Approximately(minPitch, maxPitch))
        {
            return minPitch;
        }

        return Random.Range(minPitch, maxPitch);
    }

    private bool CanPlayCollisionSound(Collision2D collision)
    {
        if (itemData == null || itemData.CollisionClip == null || audioSource == null)
        {
            return false;
        }

        if (draggableItem != null && draggableItem.IsDragging)
        {
            return false;
        }

        if (Time.time < collisionSoundSuppressedUntil)
        {
            return false;
        }

        if (collision == null || collision.relativeVelocity.magnitude < collisionSoundThreshold)
        {
            return false;
        }

        return Time.time >= lastCollisionSoundTime + collisionSoundCooldown;
    }

    private void ShowHoverStats()
    {
        if (hoverStatsUi == null || itemData == null)
        {
            return;
        }

        hoverStatsUi.ShowItem(itemData);
    }

    private void ClearHoverStats()
    {
        if (hoverStatsUi == null || itemData == null)
        {
            return;
        }

        hoverStatsUi.ClearIfShowing(itemData);
    }

    private void RefreshVisuals()
    {
        if (itemData == null)
        {
            return;
        }

        if (sharedPrefabController != null)
        {
            sharedPrefabController.ApplyItemData(itemData);
            return;
        }

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = itemData.Icon;
    }
}
