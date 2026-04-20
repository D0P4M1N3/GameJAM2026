using System.Collections.Generic;
using UnityEngine;

public class StarterPackManager : MonoBehaviour
{
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

        List<ItemData> grantedItems = DrawStarterItems();
        if (grantedItems.Count == 0)
        {
            return;
        }

        GameManager.Instance.TryGrantStarterPack(grantedItems);
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

        if (availableItems.Count == 0)
        {
            return grantedItems;
        }

        int itemsToGrant = starterItemCount;
        for (int i = 0; i < itemsToGrant; i++)
        {
            int index = Random.Range(0, availableItems.Count);
            grantedItems.Add(availableItems[index]);
        }

        return grantedItems;
    }
}
