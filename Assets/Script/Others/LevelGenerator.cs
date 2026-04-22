using System.Collections.Generic;
using System.Threading;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject baseLevelRoot;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] [FormerlySerializedAs("gameplayPickupPrefab")] private GameplayItemPickup fallbackGameplayPickupPrefab;
    [SerializeField] private LevelBalanceData levelBalanceData;
    [SerializeField] private LayerMask groundLayerMask = 1 << 6;
    [SerializeField] [Min(0.1f)] private float groundRaycastHeight = 50f;
    [SerializeField] [Min(0.1f)] private float groundRaycastDistance = 200f;
    [SerializeField] private List<GameObject> buildingPrefabs = new();
    [SerializeField] private List<GameObject> enemyPrefabs = new();
    [SerializeField] private GameObject exitGatePrefab;
    [SerializeField] [Min(1)] private int maxBuildingPlacementAttempts = 12;
    [SerializeField] [Min(1)] private int maxItemPlacementAttempts = 8;
    [SerializeField] [Min(1)] private int maxEnemyPlacementAttempts = 8;
    [SerializeField] [Min(1)] private int maxPlayerPlacementAttempts = 12;
    [SerializeField] [Min(1)] private int maxExitGatePlacementAttempts = 12;
    [SerializeField] [Min(0f)] private float minBuildingSpacing = 1f;
    [SerializeField] [Min(0f)] private float minItemBuildingSpacing = 2f;
    [SerializeField] [Min(0f)] private float minItemPlayerSpacing = 3f;
    [SerializeField] [Min(0f)] private float minExitGateBuildingSpacing = 2f;
    [SerializeField] private float itemSpawnYOffset = 0.5f;
    [SerializeField] private bool debugEnemySpawning;
    [SerializeField] private bool randomizeBuildingYaw = true;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] [HideInInspector] private LevelLootTable levelLootTable;
    [SerializeField] [HideInInspector] [Min(0)] private int minBuildings = 3;
    [SerializeField] [HideInInspector] [Min(0)] private int maxBuildings = 6;

    private readonly List<GameObject> spawnedBuildings = new();
    private readonly List<GameplayItemPickup> spawnedPickups = new();
    private readonly List<GameObject> spawnedEnemies = new();
    private GameObject spawnedExitGate;
    private readonly List<Bounds> placedBuildingBounds = new();
    private readonly List<Collider> groundColliders = new();
    private Transform generatedRoot;
    private Transform generatedBuildingsRoot;
    private Transform generatedItemsRoot;
    private Transform generatedEnemiesRoot;
    private Transform generatedExitGateRoot;




    private void Start()
    {
        
        if (generateOnStart)
        {
            Generate();
        }

        UI_Timer.Instance.AddTimeOutListener(TimeOut);
        Debug.Log("Bind TimeOut Event");
    }

    private void OnValidate()
    {
        EnsurePlayerReference();
        EnsureNavMeshSurfaceReference();
    }

    public void SetLevelBalanceData(LevelBalanceData balanceData)
    {
        levelBalanceData = balanceData;
    }

    [ContextMenu("Generate Level")]
    public void Generate()
    {
        ClearGenerated();

        if (baseLevelRoot == null)
        {
            Debug.LogWarning("LevelGenerator requires a base level root object in the scene.", this);
            return;
        }

        CacheGroundColliders();
        EnsurePlayerReference();
        EnsureNavMeshSurfaceReference();

        LevelScatterZone[] scatterZones = baseLevelRoot.GetComponentsInChildren<LevelScatterZone>(true);

        var buildingZones = new List<LevelScatterZone>();
        var itemZones = new List<LevelScatterZone>();
        var enemyZones = new List<LevelScatterZone>();
        var playerZones = new List<LevelScatterZone>();
        var exitGateZones = new List<LevelScatterZone>();
        for (int i = 0; i < scatterZones.Length; i++)
        {
            LevelScatterZone zone = scatterZones[i];
            if (zone.ZoneType == LevelScatterZoneType.Buildings)
            {
                buildingZones.Add(zone);
            }
            else if (zone.ZoneType == LevelScatterZoneType.Items)
            {
                itemZones.Add(zone);
            }
            else if (zone.ZoneType == LevelScatterZoneType.Enemies)
            {
                enemyZones.Add(zone);
            }
            else if (zone.ZoneType == LevelScatterZoneType.PlayerStart)
            {
                playerZones.Add(zone);
            }
            else
            {
                exitGateZones.Add(zone);
            }
        }

        SpawnBuildings(buildingZones, baseLevelRoot.transform);
        RebuildNavMesh();
        PositionPlayer(playerZones);
        SpawnItems(itemZones, baseLevelRoot.transform);
        SpawnExitGate(exitGateZones, baseLevelRoot.transform);
        SpawnEnemies(enemyZones, baseLevelRoot.transform);
    }

    [ContextMenu("Clear Generated Level")]
    public void ClearGenerated()
    {
        for (int i = spawnedPickups.Count - 1; i >= 0; i--)
        {
            if (spawnedPickups[i] != null)
            {
                DestroyGenerated(spawnedPickups[i].gameObject);
            }
        }

        for (int i = spawnedBuildings.Count - 1; i >= 0; i--)
        {
            if (spawnedBuildings[i] != null)
            {
                DestroyGenerated(spawnedBuildings[i]);
            }
        }

        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] != null)
            {
                DestroyGenerated(spawnedEnemies[i]);
            }
        }

        if (spawnedExitGate != null)
        {
            DestroyGenerated(spawnedExitGate);
        }

        spawnedPickups.Clear();
        spawnedBuildings.Clear();
        spawnedEnemies.Clear();
        spawnedExitGate = null;
        placedBuildingBounds.Clear();
        groundColliders.Clear();

        DestroyGeneratedRoots();
    }

    private void SpawnBuildings(List<LevelScatterZone> buildingZones, Transform parent)
    {
        if (buildingZones.Count == 0)
        {
            Debug.LogWarning("LevelGenerator found no building scatter zones.", this);
            return;
        }

        if (buildingPrefabs.Count == 0)
        {
            Debug.LogWarning("LevelGenerator has no building prefabs assigned.", this);
            return;
        }

        if (!TryGetBuildingCountRange(out int minCount, out int maxCount))
        {
            Debug.LogWarning("LevelGenerator has no building count source. Assign LevelBalanceData or keep legacy min/max building values.", this);
            return;
        }

        int buildingCount = Random.Range(minCount, maxCount + 1);
        Transform buildingsParent = GetOrCreateGeneratedCategoryRoot("Buildings", ref generatedBuildingsRoot);
        for (int i = 0; i < buildingCount; i++)
        {
            TrySpawnBuilding(buildingZones, buildingsParent != null ? buildingsParent : parent);
        }
    }

    private void TrySpawnBuilding(List<LevelScatterZone> buildingZones, Transform parent)
    {
        for (int attempt = 0; attempt < maxBuildingPlacementAttempts; attempt++)
        {
            GameObject prefab = GetRandomBuildingPrefab();
            if (prefab == null)
            {
                return;
            }

            LevelScatterZone zone = buildingZones[Random.Range(0, buildingZones.Count)];
            if (!TryGetGroundPlacementPosition(zone, out Vector3 position))
            {
                continue;
            }

            Quaternion rotation = randomizeBuildingYaw
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * prefab.transform.rotation
                : prefab.transform.rotation;

            GameObject instance = Instantiate(prefab, position, rotation, parent);
            if (!TryGetCombinedBounds(instance, out Bounds bounds))
            {
                spawnedBuildings.Add(instance);
                return;
            }

            if (!Contains(zone.GetWorldBounds(), bounds) || IntersectsPlacedBuilding(bounds))
            {
                DiscardGeneratedCandidate(instance);
                continue;
            }

            spawnedBuildings.Add(instance);
            placedBuildingBounds.Add(bounds);
            return;
        }
    }

    private void SpawnItems(List<LevelScatterZone> itemZones, Transform parent)
    {
        if (itemZones.Count == 0)
        {
            Debug.LogWarning("LevelGenerator found no item scatter zones.", this);
            return;
        }

        LevelLootTable activeLootTable = GetActiveLootTable();
        if (activeLootTable == null)
        {
            Debug.LogWarning("LevelGenerator has no loot table. Assign LevelBalanceData or a legacy LevelLootTable.", this);
            return;
        }

        int progression = GameManager.Instance != null ? GameManager.Instance.CurrentProgression : 1;
        List<ItemData> rolledItems = activeLootTable.RollDrops(progression);
        Transform itemsParent = GetOrCreateGeneratedCategoryRoot("Items", ref generatedItemsRoot);

        if (activeLootTable.TryRollSpecialItem(out ItemData specialItem) && specialItem != null)
        {
            TrySpawnItem(itemZones, specialItem, itemsParent != null ? itemsParent : parent);
        }

        for (int i = 0; i < rolledItems.Count; i++)
        {
            ItemData item = rolledItems[i];
            if (item == null)
            {
                continue;
            }

            TrySpawnItem(itemZones, item, itemsParent != null ? itemsParent : parent);
        }
    }

    private void TrySpawnItem(List<LevelScatterZone> itemZones, ItemData item, Transform parent)
    {
        for (int attempt = 0; attempt < maxItemPlacementAttempts; attempt++)
        {
            GameObject pickupPrefab = ResolvePickupPrefab(item);
            if (pickupPrefab == null)
            {
                return;
            }

            LevelScatterZone zone = itemZones[Random.Range(0, itemZones.Count)];
            Vector3 position = zone.GetRandomPoint() + Vector3.up * itemSpawnYOffset;
            GameObject instanceObject = Instantiate(pickupPrefab, position, Quaternion.identity, parent);
            GameplayItemPickup instance = instanceObject.GetComponent<GameplayItemPickup>();
            if (instance == null)
            {
                Debug.LogWarning($"World pickup prefab for item '{item.DisplayName}' is missing GameplayItemPickup.", instanceObject);
                DestroyGenerated(instanceObject);
                return;
            }

            instance.Initialize(item);

            if (!TryGetCombinedBounds(instanceObject, out Bounds bounds))
            {
                if (IsTooCloseToPlayer(instanceObject.transform.position) || IsTooCloseToBuilding(instanceObject.transform.position, minItemBuildingSpacing))
                {
                    DiscardGeneratedCandidate(instanceObject);
                    continue;
                }

                spawnedPickups.Add(instance);
                return;
            }

            if (!Contains(zone.GetWorldBounds(), bounds)
                || IntersectsPlacedBuilding(bounds, minItemBuildingSpacing)
                || IsTooCloseToPlayer(bounds.center))
            {
                DiscardGeneratedCandidate(instanceObject);
                continue;
            }

            spawnedPickups.Add(instance);
            return;

        }
    }

    private void SpawnEnemies(List<LevelScatterZone> enemyZones, Transform parent)
    {
        if (enemyZones.Count == 0)
        {
            LogEnemyDebug("No enemy zones found.");
            return;
        }

        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("LevelGenerator found enemy zones but no enemy prefabs are assigned.", this);
            LogEnemyDebug("Enemy prefabs list is empty.");
            return;
        }

        if (!TryGetEnemyCountRange(out int minCount, out int maxCount))
        {
            Debug.LogWarning("LevelGenerator found enemy zones but no LevelBalanceData enemy counts are assigned.", this);
            LogEnemyDebug("Enemy count range is unavailable from level balance data.");
            return;
        }

        if (navMeshSurface == null)
        {
            Debug.LogWarning("LevelGenerator could not find a NavMeshSurface for enemy spawning.", this);
            LogEnemyDebug("NavMeshSurface is missing.");
            return;
        }

        int enemyCount = Random.Range(minCount, maxCount + 1);
        Transform enemiesParent = GetOrCreateGeneratedCategoryRoot("Enemies", ref generatedEnemiesRoot);
        LogEnemyDebug($"Trying to spawn {enemyCount} enemies across {enemyZones.Count} zones.");
        for (int i = 0; i < enemyCount; i++)
        {
            TrySpawnEnemy(enemyZones, enemiesParent != null ? enemiesParent : parent, i + 1);
        }
    }

    private void SpawnExitGate(List<LevelScatterZone> exitGateZones, Transform parent)
    {
        if (exitGatePrefab == null || exitGateZones.Count == 0)
        {
            return;
        }

        Transform exitGateParent = GetOrCreateGeneratedCategoryRoot("ExitGate", ref generatedExitGateRoot);
        for (int attempt = 0; attempt < maxExitGatePlacementAttempts; attempt++)
        {
            LevelScatterZone zone = exitGateZones[Random.Range(0, exitGateZones.Count)];
            if (!TryGetGroundPlacementPosition(zone, out Vector3 position))
            {
                continue;
            }

            Quaternion rotation = randomizeBuildingYaw
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * exitGatePrefab.transform.rotation
                : exitGatePrefab.transform.rotation;

            GameObject instance = Instantiate(exitGatePrefab, position, rotation, exitGateParent != null ? exitGateParent : parent);
            if (TryGetCombinedBounds(instance, out Bounds bounds) &&
                IntersectsPlacedBuilding(bounds, minExitGateBuildingSpacing))
            {
                DiscardGeneratedCandidate(instance);
                continue;
            }

            spawnedExitGate = instance;
            return;
        }

        Debug.LogWarning("LevelGenerator could not place exit gate after all attempts.", this);
    }

    private void TrySpawnEnemy(List<LevelScatterZone> enemyZones, Transform parent, int enemyIndex)
    {
        string failureReason = "No attempts were made.";

        for (int attempt = 0; attempt < maxEnemyPlacementAttempts; attempt++)
        {
            GameObject prefab = GetRandomEnemyPrefab();
            if (prefab == null)
            {
                LogEnemyDebug($"Enemy {enemyIndex}: no valid enemy prefab found.");
                return;
            }

            LevelScatterZone zone = enemyZones[Random.Range(0, enemyZones.Count)];
            if (!TryGetNavMeshPlacementPosition(zone, out Vector3 position))
            {
                failureReason = $"attempt {attempt + 1}: no valid navmesh point in zone '{zone.name}'.";
                continue;
            }

            Quaternion rotation = randomizeBuildingYaw
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * prefab.transform.rotation
                : prefab.transform.rotation;

            GameObject instance = Instantiate(prefab, position, rotation, parent);
            ApplyEnemyBalance(instance);
            if (TryGetEnemyFootprintBounds(instance, out Bounds bounds) && IntersectsPlacedBuilding(bounds))
            {
                failureReason = $"attempt {attempt + 1}: spawned '{prefab.name}' overlapped a building at {position}.";
                DiscardGeneratedCandidate(instance);
                continue;
            }

            spawnedEnemies.Add(instance);
            LogEnemyDebug($"Enemy {enemyIndex}: spawned '{prefab.name}' at {position} on attempt {attempt + 1}.");
            return;
        }

        LogEnemyDebug($"Enemy {enemyIndex}: failed after {maxEnemyPlacementAttempts} attempts. Last reason: {failureReason}");
    }

    private GameObject ResolvePickupPrefab(ItemData item)
    {
        if (item != null && item.ItemPrefab != null)
        {
            return item.ItemPrefab;
        }

        return fallbackGameplayPickupPrefab != null ? fallbackGameplayPickupPrefab.gameObject : null;
    }

    private void CacheGroundColliders()
    {
        groundColliders.Clear();

        Collider[] colliders = baseLevelRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || !collider.enabled)
            {
                continue;
            }

            if (((1 << collider.gameObject.layer) & groundLayerMask.value) == 0)
            {
                continue;
            }

            groundColliders.Add(collider);
        }
    }

    private void EnsureNavMeshSurfaceReference()
    {
        if (navMeshSurface != null)
        {
            return;
        }

        navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface != null)
        {
            return;
        }

        if (baseLevelRoot != null)
        {
            navMeshSurface = baseLevelRoot.GetComponentInChildren<NavMeshSurface>(true);
        }
    }

    private void EnsurePlayerReference()
    {
        if (playerRoot != null)
        {
            return;
        }

        TopDownController playerController = FindFirstObjectByType<TopDownController>();
        if (playerController != null)
        {
            playerRoot = playerController.transform;
        }
    }

    private Transform GetOrCreateGeneratedRoot()
    {
        if (baseLevelRoot == null)
        {
            return null;
        }

        if (generatedRoot != null)
        {
            ApplyGeneratedRootCompensation();
            return generatedRoot;
        }

        Transform existingRoot = baseLevelRoot.transform.Find("Generated");
        if (existingRoot != null)
        {
            generatedRoot = existingRoot;
            ApplyGeneratedRootCompensation();
            return generatedRoot;
        }

        GameObject rootObject = new("Generated");
        generatedRoot = rootObject.transform;
        generatedRoot.SetParent(baseLevelRoot.transform, false);
        ApplyGeneratedRootCompensation();
        return generatedRoot;
    }

    private Transform GetOrCreateGeneratedCategoryRoot(string categoryName, ref Transform categoryRoot)
    {
        Transform parentRoot = GetOrCreateGeneratedRoot();
        if (parentRoot == null)
        {
            return null;
        }

        if (categoryRoot != null)
        {
            return categoryRoot;
        }

        Transform existingRoot = parentRoot.Find(categoryName);
        if (existingRoot != null)
        {
            categoryRoot = existingRoot;
            return categoryRoot;
        }

        GameObject categoryObject = new(categoryName);
        categoryRoot = categoryObject.transform;
        categoryRoot.SetParent(parentRoot, false);
        return categoryRoot;
    }

    private void ApplyGeneratedRootCompensation()
    {
        if (generatedRoot == null || baseLevelRoot == null)
        {
            return;
        }

        generatedRoot.localPosition = Vector3.zero;
        generatedRoot.localRotation = Quaternion.identity;

        Vector3 parentScale = baseLevelRoot.transform.lossyScale;
        generatedRoot.localScale = new Vector3(
            GetInverseScale(parentScale.x),
            GetInverseScale(parentScale.y),
            GetInverseScale(parentScale.z));
    }

    private void RebuildNavMesh()
    {
        if (navMeshSurface == null)
        {
            return;
        }

        navMeshSurface.BuildNavMesh();
        B_NavMeshUtil.RebuildCache();
    }

    private void DestroyGeneratedRoots()
    {
        generatedBuildingsRoot = null;
        generatedItemsRoot = null;
        generatedEnemiesRoot = null;
        generatedExitGateRoot = null;

        if (generatedRoot == null && baseLevelRoot != null)
        {
            generatedRoot = baseLevelRoot.transform.Find("Generated");
        }

        if (generatedRoot == null)
        {
            return;
        }

        Transform rootToDestroy = generatedRoot;
        generatedRoot = null;
        DestroyGenerated(rootToDestroy.gameObject);
    }

    private void PositionPlayer(List<LevelScatterZone> playerZones)
    {
        if (playerRoot == null || playerZones.Count == 0)
        {
            return;
        }

        for (int attempt = 0; attempt < maxPlayerPlacementAttempts; attempt++)
        {
            LevelScatterZone zone = playerZones[Random.Range(0, playerZones.Count)];
            if (!TryGetPlayerSpawnPosition(zone, out Vector3 position))
            {
                continue;
            }

            if (TryGetShiftedBounds(playerRoot.gameObject, position, out Bounds bounds) && IntersectsPlacedBuilding(bounds))
            {
                continue;
            }

            ForceMovePlayer(position);
            return;
        }

        Debug.LogWarning("LevelGenerator could not find a valid player spawn point.", this);
    }

    private bool TryGetPlayerSpawnPosition(LevelScatterZone zone, out Vector3 position)
    {
        position = default;
        if (zone == null)
        {
            return false;
        }

        for (int attempt = 0; attempt < maxPlayerPlacementAttempts; attempt++)
        {
            Vector3 candidatePoint = zone.GetRandomPoint();
            Vector3 projected = navMeshSurface != null ? B_NavMeshUtil.Project(candidatePoint) : candidatePoint;

            if (navMeshSurface != null && NavMesh.SamplePosition(projected, out NavMeshHit navMeshHit, 1.5f, NavMesh.AllAreas))
            {
                position = navMeshHit.position;
                return true;
            }

            if (TryGetGroundPlacementPosition(zone, out Vector3 groundPosition))
            {
                position = groundPosition;
                return true;
            }
        }

        return false;
    }

    private void LogEnemyDebug(string message)
    {
        if (!debugEnemySpawning)
        {
            return;
        }

        Debug.Log($"[LevelGenerator EnemyDebug] {message}", this);
    }

    private void ForceMovePlayer(Vector3 position)
    {
        Rigidbody playerRigidbody = playerRoot.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.position = position;
        }

        playerRoot.position = position;
    }

    private bool TryGetGroundPlacementPosition(LevelScatterZone zone, out Vector3 position)
    {
        position = default;
        if (zone == null)
        {
            return false;
        }

        Vector3 candidatePoint = zone.GetRandomPoint();
        Vector3 rayOrigin = candidatePoint + Vector3.up * groundRaycastHeight;
        Ray ray = new(rayOrigin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, groundRaycastDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null && groundColliders.Contains(hit.collider))
            {
                position = hit.point;
                return true;
            }
        }

        float closestDistance = float.PositiveInfinity;
        for (int i = 0; i < groundColliders.Count; i++)
        {
            Collider groundCollider = groundColliders[i];
            if (groundCollider == null)
            {
                continue;
            }

            Bounds bounds = groundCollider.bounds;
            if (candidatePoint.x < bounds.min.x || candidatePoint.x > bounds.max.x
                || candidatePoint.z < bounds.min.z || candidatePoint.z > bounds.max.z)
            {
                continue;
            }

            float distance = Mathf.Abs(rayOrigin.y - bounds.max.y);
            if (distance >= closestDistance)
            {
                continue;
            }

            closestDistance = distance;
            position = new Vector3(candidatePoint.x, bounds.max.y, candidatePoint.z);
        }

        return closestDistance < float.PositiveInfinity;
    }

    private bool TryGetNavMeshPlacementPosition(LevelScatterZone zone, out Vector3 position)
    {
        position = default;
        if (zone == null)
        {
            return false;
        }

        for (int attempt = 0; attempt < maxEnemyPlacementAttempts; attempt++)
        {
            Vector3 candidatePoint = zone.GetRandomPoint();
            Vector3 projected = B_NavMeshUtil.Project(candidatePoint);

            if (!NavMesh.SamplePosition(projected, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                continue;
            }

            position = hit.position;
            return true;
        }

        return false;
    }

    private GameObject GetRandomBuildingPrefab()
    {
        var availablePrefabs = new List<GameObject>();
        for (int i = 0; i < buildingPrefabs.Count; i++)
        {
            if (buildingPrefabs[i] != null)
            {
                availablePrefabs.Add(buildingPrefabs[i]);
            }
        }

        if (availablePrefabs.Count == 0)
        {
            return null;
        }

        return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
    }

    private GameObject GetRandomEnemyPrefab()
    {
        var availablePrefabs = new List<GameObject>();
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (enemyPrefabs[i] != null)
            {
                availablePrefabs.Add(enemyPrefabs[i]);
            }
        }

        if (availablePrefabs.Count == 0)
        {
            return null;
        }

        return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
    }

    private LevelLootTable GetActiveLootTable()
    {
        if (levelBalanceData != null && levelBalanceData.LootTable != null)
        {
            return levelBalanceData.LootTable;
        }

        return levelLootTable;
    }

    private bool TryGetBuildingCountRange(out int minCount, out int maxCount)
    {
        if (levelBalanceData != null)
        {
            int progression = GameManager.Instance != null ? GameManager.Instance.CurrentProgression : 1;
            minCount = levelBalanceData.EvaluateMinBuildings(progression);
            maxCount = levelBalanceData.EvaluateMaxBuildings(progression);
            return true;
        }

        minCount = minBuildings;
        maxCount = Mathf.Max(minBuildings, maxBuildings);
        return maxCount >= minCount;
    }

    private bool TryGetEnemyCountRange(out int minCount, out int maxCount)
    {
        if (levelBalanceData == null)
        {
            minCount = 0;
            maxCount = 0;
            return false;
        }

        int progression = GameManager.Instance != null ? GameManager.Instance.CurrentProgression : 1;
        minCount = levelBalanceData.EvaluateMinEnemies(progression);
        maxCount = levelBalanceData.EvaluateMaxEnemies(progression);
        return maxCount >= minCount;
    }

    private bool IntersectsPlacedBuilding(Bounds candidateBounds)
    {
        return IntersectsPlacedBuilding(candidateBounds, minBuildingSpacing);
    }

    private bool IntersectsPlacedBuilding(Bounds candidateBounds, float spacing)
    {
        for (int i = 0; i < placedBuildingBounds.Count; i++)
        {
            if (GetExpandedXZBounds(placedBuildingBounds[i], spacing).Intersects(candidateBounds))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsTooCloseToPlayer(Vector3 candidatePosition)
    {
        if (playerRoot == null || minItemPlayerSpacing <= 0f)
        {
            return false;
        }

        return GetXZDistance(candidatePosition, playerRoot.position) < minItemPlayerSpacing;
    }

    private bool IsTooCloseToBuilding(Vector3 candidatePosition, float spacing)
    {
        if (spacing <= 0f)
        {
            return false;
        }

        for (int i = 0; i < placedBuildingBounds.Count; i++)
        {
            Bounds expandedBounds = GetExpandedXZBounds(placedBuildingBounds[i], spacing);
            if (candidatePosition.x >= expandedBounds.min.x
                && candidatePosition.x <= expandedBounds.max.x
                && candidatePosition.z >= expandedBounds.min.z
                && candidatePosition.z <= expandedBounds.max.z)
            {
                return true;
            }
        }

        return false;
    }

    private static Bounds GetExpandedXZBounds(Bounds sourceBounds, float spacing)
    {
        if (spacing <= 0f)
        {
            return sourceBounds;
        }

        Bounds expandedBounds = sourceBounds;
        expandedBounds.Expand(new Vector3(spacing * 2f, 0f, spacing * 2f));
        return expandedBounds;
    }

    private static bool TryGetShiftedBounds(GameObject target, Vector3 centerPosition, out Bounds bounds)
    {
        if (!TryGetCombinedBounds(target, out bounds))
        {
            return false;
        }

        Vector3 delta = centerPosition - target.transform.position;
        bounds.center += delta;
        return true;
    }

    private static bool TryGetEnemyFootprintBounds(GameObject target, out Bounds bounds)
    {
        if (target == null)
        {
            bounds = default;
            return false;
        }

        NavMeshAgent navMeshAgent = target.GetComponentInChildren<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            Vector3 center = navMeshAgent.transform.position + Vector3.up * (navMeshAgent.height * 0.5f);
            Vector3 size = new(navMeshAgent.radius * 2f, navMeshAgent.height, navMeshAgent.radius * 2f);
            bounds = new Bounds(center, size);
            return true;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || !collider.enabled || collider.isTrigger)
            {
                continue;
            }

            bounds = collider.bounds;
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Collider extraCollider = colliders[j];
                if (extraCollider == null || !extraCollider.enabled || extraCollider.isTrigger)
                {
                    continue;
                }

                bounds.Encapsulate(extraCollider.bounds);
            }

            return true;
        }

        return TryGetCombinedBounds(target, out bounds);
    }

    private static bool Contains(Bounds container, Bounds target)
    {
        return target.min.x >= container.min.x
            && target.max.x <= container.max.x
            && target.min.z >= container.min.z
            && target.max.z <= container.max.z;
    }

    private static float GetXZDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private static bool TryGetCombinedBounds(GameObject target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return true;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return true;
        }

        bounds = new Bounds(target.transform.position, Vector3.zero);
        return false;
    }

    private static void DestroyGenerated(GameObject target)
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


    public void TimeOut()
    {
        Debug.Log("Timeout triggered.");

        BB_Sunboss_Master[] BSMs = FindObjectsOfType<BB_Sunboss_Master>();
        int progression = GameManager.Instance != null ? GameManager.Instance.CurrentProgression : 1;

        foreach (BB_Sunboss_Master BSM in BSMs)
        {
            if (levelBalanceData != null && levelBalanceData.EnemyBalanceData != null)
            {
                levelBalanceData.EnemyBalanceData.ApplyTimeoutTo(BSM, progression);
                continue;
            }

            if (levelBalanceData != null)
            {
                BSM.BB_SunbossCTX_Brain.PredictionAccuracy = levelBalanceData.TimeoutPredictionAccuracy;
            }
        }
    }

    private static void DiscardGeneratedCandidate(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        target.SetActive(false);
        DestroyGenerated(target);
    }

    private static float GetInverseScale(float value)
    {
        return Mathf.Approximately(value, 0f) ? 1f : 1f / value;
    }

    private void ApplyEnemyBalance(GameObject enemyInstance)
    {
        if (enemyInstance == null || levelBalanceData == null || levelBalanceData.EnemyBalanceData == null)
        {
            return;
        }

        int progression = GameManager.Instance != null ? GameManager.Instance.CurrentProgression : 1;
        BB_Sunboss_Master[] masters = enemyInstance.GetComponentsInChildren<BB_Sunboss_Master>(true);
        for (int i = 0; i < masters.Length; i++)
        {
            levelBalanceData.EnemyBalanceData.ApplyTo(masters[i], progression);
        }
    }
}
