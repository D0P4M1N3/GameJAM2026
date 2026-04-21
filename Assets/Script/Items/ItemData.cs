using UnityEngine.Serialization;
using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] [TextArea] private string description;
    [SerializeField] private ItemRarity rarity = ItemRarity.Common;
    [SerializeField] private Color itemColor = Color.white;
    [SerializeField] [Min(0f)] private float sizeValue = 1f;
    [SerializeField] private Sprite icon;
    [FormerlySerializedAs("uiPrefab")]
    [FormerlySerializedAs("worldPrefab")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private ItemStats stats;
    [SerializeField] private AudioClip collisionClip;
    [SerializeField] [Range(0f, 1f)] private float collisionVolume = 1f;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 1f;

    public string ItemId => itemId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public ItemRarity Rarity => rarity;
    public Color ItemColor => itemColor;
    public float SizeValue => sizeValue;
    public Sprite Icon => icon;
    public GameObject ItemPrefab => itemPrefab;
    public ItemStats Stats => stats;
    public AudioClip CollisionClip => collisionClip;
    public float CollisionVolume => collisionVolume;
    public AudioClip PickupClip => pickupClip;
    public float PickupVolume => pickupVolume;
}
