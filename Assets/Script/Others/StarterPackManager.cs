using System.Collections.Generic;
using UnityEngine;

public class StarterPackManager : MonoBehaviour
{
    [SerializeField] private StashData stashData;
    [SerializeField] private List<ItemData> starterItemPool = new();
    [SerializeField] [Min(1)] private int starterItemCount = 3;

    private void Start()
    {
        TryGrantStarterPack();
    }

    [ContextMenu("Grant Starter Pack If Needed")]
    public void TryGrantStarterPack()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (stashData == null)
        {
            stashData = FindFirstObjectByType<StashData>();
        }

        if (stashData != null && stashData.TotalItemCount > 0)
        {
            return;
        }

        List<ItemData> grantedItems = DrawStarterItems();
        if (grantedItems.Count == 0)
        {
            return;
        }

        GameManager.Instance.AddItemsToStash(grantedItems);
    }

    private List<ItemData> DrawStarterItems()
    {
        var grantedItems = new List<ItemData>();
        var availableItems = new List<ItemData>();

        for (int i = 0; i < starterItemPool.Count; i++)
        {
            ItemData item = starterItemPool[i];
            if (item != null)
            {
                availableItems.Add(item);
            }
        }

        int itemsToGrant = Mathf.Min(starterItemCount, availableItems.Count);
        for (int i = 0; i < itemsToGrant; i++)
        {
            int index = Random.Range(0, availableItems.Count);
            grantedItems.Add(availableItems[index]);
            availableItems.RemoveAt(index);
        }

        return grantedItems;
    }
}
