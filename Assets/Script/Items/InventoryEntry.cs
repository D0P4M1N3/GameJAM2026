using System;
using UnityEngine;

[Serializable]
public struct InventoryEntry
{
    [SerializeField] private ItemData item;

    public ItemData Item => item;
    public bool IsValid => item != null;
}
