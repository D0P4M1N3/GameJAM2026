using System.Collections.Generic;
using UnityEngine;

public class StashSpawner : MonoBehaviour
{
    [SerializeField] private StashData stashData;
    [SerializeField] private float spawnRadius = 1.5f;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool clearExistingChildrenOnSpawn = true;

    private readonly List<ItemWorldObject> spawnedItems = new();

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnItems();
        }
    }

    [ContextMenu("Spawn Items")]
    public void SpawnItems()
    {
        SpawnItems(clearExistingChildrenOnSpawn);
    }

    private void SpawnItems(bool clearExisting)
    {
        if (stashData == null)
        {
            return;
        }

        Transform parent = transform;

        if (clearExisting)
        {
            ClearSpawnedItems(parent);
        }

        int spawnIndex = 0;
        for (int entryIndex = 0; entryIndex < stashData.Entries.Count; entryIndex++)
        {
            StashEntry entry = stashData.Entries[entryIndex];
            if (!entry.IsValid || entry.Item.WorldPrefab == null)
            {
                continue;
            }

            for (int quantityIndex = 0; quantityIndex < entry.Quantity; quantityIndex++)
            {
                Vector3 spawnPosition = GetSpawnPosition(parent, spawnIndex);
                Quaternion spawnRotation = GetSpawnRotation(parent, spawnIndex);
                GameObject instance = Instantiate(entry.Item.WorldPrefab, spawnPosition, spawnRotation, parent);

                ItemWorldObject itemWorldObject = instance.GetComponent<ItemWorldObject>();
                if (itemWorldObject != null)
                {
                    itemWorldObject.Initialize(entry.Item, this);
                    spawnedItems.Add(itemWorldObject);
                }
                spawnIndex++;
            }
        }
    }

    [ContextMenu("Reset Stash")]
    public void ResetStash()
    {
        if (stashData == null)
        {
            return;
        }

        DestroyNonInventorySpawnedItems();
        SpawnItems(false);
    }

    [ContextMenu("Clear Spawned Items")]
    public void ClearSpawnedItems()
    {
        ClearSpawnedItems(transform);
    }

    public void UnregisterSpawnedItem(ItemWorldObject itemWorldObject)
    {
        if (itemWorldObject == null)
        {
            return;
        }

        spawnedItems.Remove(itemWorldObject);
    }

    private void ClearSpawnedItems(Transform parent)
    {
        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            if (spawnedItems[i] != null)
            {
                DestroyTrackedObject(spawnedItems[i].gameObject);
            }
        }

        spawnedItems.Clear();

        if (!clearExistingChildrenOnSpawn)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyTrackedObject(parent.GetChild(i).gameObject);
        }
    }

    private void DestroyNonInventorySpawnedItems()
    {
        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            ItemWorldObject itemWorldObject = spawnedItems[i];
            if (itemWorldObject == null)
            {
                spawnedItems.RemoveAt(i);
                continue;
            }

            if (itemWorldObject.IsInInventory)
            {
                continue;
            }

            DestroyTrackedObject(itemWorldObject.gameObject);
            spawnedItems.RemoveAt(i);
        }
    }

    private static void DestroyTrackedObject(GameObject target)
    {
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    private Vector3 GetSpawnPosition(Transform parent, int spawnIndex)
    {
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        return parent.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }

    private Quaternion GetSpawnRotation(Transform parent, int spawnIndex)
    {
        return parent.rotation;
    }
}
