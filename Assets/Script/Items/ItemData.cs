using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] [TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject worldPrefab;
    [SerializeField] private ItemStats stats;
    [SerializeField] private AudioClip collisionClip;
    [SerializeField] [Range(0f, 1f)] private float collisionVolume = 1f;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 1f;

    public string ItemId => itemId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public GameObject WorldPrefab => worldPrefab;
    public ItemStats Stats => stats;
    public AudioClip CollisionClip => collisionClip;
    public float CollisionVolume => collisionVolume;
    public AudioClip PickupClip => pickupClip;
    public float PickupVolume => pickupVolume;
}
