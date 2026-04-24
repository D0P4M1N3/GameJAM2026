using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class StashSpawner : MonoBehaviour
{
    [SerializeField] private float spawnRadius = 1.5f;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool clearExistingChildrenOnSpawn = true;
    [SerializeField] [Min(0f)] private float spawnDelaySeconds = 0.08f;
    [SerializeField] private GameObject spawningIndicatorObject;

    private readonly List<ItemWorldObject> spawnedItems = new();
    private Coroutine spawnRoutine;

    private void Awake()
    {
        SetSpawningIndicatorActive(false);
    }

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
        StashData stashData = GameManager.Instance != null ? GameManager.Instance.StashData : null;
        if (stashData == null)
        {
            SetSpawningIndicatorActive(false);
            return;
        }

        Transform parent = transform;

        StopSpawnRoutine();
        SetSpawningIndicatorActive(true);

        if (clearExisting)
        {
            ClearSpawnedItems(parent);
        }

        if (!Application.isPlaying || spawnDelaySeconds <= 0f)
        {
            SpawnItemsImmediately(parent, stashData);
            SetSpawningIndicatorActive(false);
            return;
        }

        spawnRoutine = StartCoroutine(SpawnItemsRoutine(parent, stashData));
    }

    private void SpawnItemsImmediately(Transform parent, StashData stashData)
    {
        int spawnIndex = 0;
        for (int entryIndex = 0; entryIndex < stashData.Entries.Count; entryIndex++)
        {
            StashEntry entry = stashData.Entries[entryIndex];
            GameObject itemPrefab = ResolveItemPrefab(entry.Item);
            if (!entry.IsValid || itemPrefab == null)
            {
                continue;
            }

            for (int quantityIndex = 0; quantityIndex < entry.Quantity; quantityIndex++)
            {
                SpawnSingleItem(parent, entry.Item, itemPrefab, spawnIndex);
                spawnIndex++;
            }
        }
    }

    private IEnumerator SpawnItemsRoutine(Transform parent, StashData stashData)
    {
        int spawnIndex = 0;
        for (int entryIndex = 0; entryIndex < stashData.Entries.Count; entryIndex++)
        {
            StashEntry entry = stashData.Entries[entryIndex];
            GameObject itemPrefab = ResolveItemPrefab(entry.Item);
            if (!entry.IsValid || itemPrefab == null)
            {
                continue;
            }

            for (int quantityIndex = 0; quantityIndex < entry.Quantity; quantityIndex++)
            {
                SpawnSingleItem(parent, entry.Item, itemPrefab, spawnIndex);
                spawnIndex++;
                yield return new WaitForSeconds(spawnDelaySeconds);
            }
        }

        spawnRoutine = null;
        SetSpawningIndicatorActive(false);
    }

    [ContextMenu("Reset Stash")]
    public void ResetStash()
    {
        StashData stashData = GameManager.Instance != null ? GameManager.Instance.StashData : null;
        if (stashData == null)
        {
            SetSpawningIndicatorActive(false);
            return;
        }

        SetSpawningIndicatorActive(true);
        StopSpawnRoutine();
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
        StopSpawnRoutine();

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

    private void SpawnSingleItem(Transform parent, ItemData itemData, GameObject itemPrefab, int spawnIndex)
    {
        Vector3 spawnPosition = GetSpawnPosition(parent, spawnIndex);
        Quaternion spawnRotation = GetSpawnRotation(parent, spawnIndex);
        GameObject instance = Instantiate(itemPrefab, spawnPosition, spawnRotation, parent);

        ItemWorldObject itemWorldObject = instance.GetComponent<ItemWorldObject>();
        if (itemWorldObject != null)
        {
            itemWorldObject.Initialize(itemData, this);
            spawnedItems.Add(itemWorldObject);
        }
    }

    private void StopSpawnRoutine()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
        SetSpawningIndicatorActive(false);
    }

    private void OnDisable()
    {
        SetSpawningIndicatorActive(false);
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
        float randomAngle = Random.Range(180f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(0f, spawnRadius);
        Vector2 randomOffset = new(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        randomOffset *= randomDistance;
        return parent.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }

    private Quaternion GetSpawnRotation(Transform parent, int spawnIndex)
    {
        return parent.rotation;
    }

    private static GameObject ResolveItemPrefab(ItemData item)
    {
        if (item == null)
        {
            return null;
        }

        return item.ItemPrefab;
    }

    private void SetSpawningIndicatorActive(bool isActive)
    {
        if (spawningIndicatorObject == null)
        {
            return;
        }

        spawningIndicatorObject.SetActive(isActive);
    }
}
