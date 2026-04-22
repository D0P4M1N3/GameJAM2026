using UnityEngine;

public class PlayerStorageVisual : MonoBehaviour
{
    [SerializeField] private Transform storageVisual;
    [SerializeField] private float currentStoragePercent;
    [SerializeField] private float currentScaleMultiplier = 1f;

    private Transform cachedStorageVisual;
    private Vector3 originalLocalScale = Vector3.one;
    private float lastAppliedStorage = float.NaN;

    private void Awake()
    {
        CacheStorageVisualScale();
        RefreshVisual();
    }

    private void OnEnable()
    {
        CacheStorageVisualScale();
        RefreshVisual();
    }

    private void LateUpdate()
    {
        if (DATA_Player.Instance == null || DATA_Player.Instance.CharacterStats == null || storageVisual == null)
        {
            return;
        }

        float currentStorage = DATA_Player.Instance.CharacterStats.Storage;
        if (!Mathf.Approximately(currentStorage, lastAppliedStorage))
        {
            ApplyStorageScale(currentStorage);
        }
    }

    private void OnValidate()
    {
        CacheStorageVisualScale();
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (storageVisual == null || DATA_Player.Instance == null || DATA_Player.Instance.CharacterStats == null)
        {
            return;
        }

        ApplyStorageScale(DATA_Player.Instance.CharacterStats.Storage);
    }

    public static void RefreshAll()
    {
        PlayerStorageVisual[] visuals = FindObjectsByType<PlayerStorageVisual>(FindObjectsSortMode.None);
        for (int i = 0; i < visuals.Length; i++)
        {
            visuals[i].RefreshVisual();
        }
    }

    private void CacheStorageVisualScale()
    {
        if (storageVisual == null)
        {
            cachedStorageVisual = null;
            return;
        }

        if (cachedStorageVisual == storageVisual)
        {
            return;
        }

        cachedStorageVisual = storageVisual;
        originalLocalScale = storageVisual.localScale;
        lastAppliedStorage = float.NaN;
    }

    private void ApplyStorageScale(float storagePercent)
    {
        if (storageVisual == null)
        {
            return;
        }

        float scaleMultiplier = Mathf.Max(0f, 1f + (storagePercent / 100f));
        currentStoragePercent = storagePercent;
        currentScaleMultiplier = scaleMultiplier;
        storageVisual.localScale = Vector3.Scale(originalLocalScale, Vector3.one * scaleMultiplier);
        lastAppliedStorage = storagePercent;
    }
}
