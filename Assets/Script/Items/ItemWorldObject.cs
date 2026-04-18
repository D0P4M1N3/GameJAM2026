using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class ItemWorldObject : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool isInInventory;
    [SerializeField] private float collisionSoundThreshold = 1.5f;
    [SerializeField] private float collisionSoundCooldown = 0.1f;
    [SerializeField] private float pickupCollisionMuteDuration = 0.2f;
    [SerializeField] private Vector2 randomPitchRange = new(0.92f, 1.08f);

    private StashSpawner owningSpawner;
    private DraggableItem2D draggableItem;
    private float lastCollisionSoundTime = float.NegativeInfinity;
    private float collisionSoundSuppressedUntil = float.NegativeInfinity;

    public ItemData ItemData => itemData;
    public bool IsInInventory => isInInventory;
    public StashSpawner OwningSpawner => owningSpawner;

    private void Awake()
    {
        EnsureSpriteRendererReference();
        EnsureAudioSourceReference();
        RefreshVisuals();
    }

    private void OnValidate()
    {
        EnsureSpriteRendererReference();
        EnsureAudioSourceReference();
        ConfigureAudioSource();
        RefreshVisuals();
    }

    private void Reset()
    {
        EnsureSpriteRendererReference();
        EnsureAudioSourceReference();
        ConfigureAudioSource();
    }

    private void OnCollisionEnter2D(Collision2D collision)
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

    private void EnsureSpriteRendererReference()
    {
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

    private void RefreshVisuals()
    {
        if (spriteRenderer == null || itemData == null)
        {
            return;
        }

        spriteRenderer.sprite = itemData.Icon;
    }
}
