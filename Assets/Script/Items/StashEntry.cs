using System;
using UnityEngine;

[Serializable]
public struct StashEntry
{
    [SerializeField] private ItemData item;
    [SerializeField] [Min(0)] private int quantity;

    public StashEntry(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = Mathf.Max(0, quantity);
    }

    public ItemData Item => item;
    public int Quantity => Mathf.Max(0, quantity);
    public bool IsValid => item != null && Quantity > 0;
}
