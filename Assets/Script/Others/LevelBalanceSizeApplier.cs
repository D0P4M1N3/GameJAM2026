using UnityEngine;

public class LevelBalanceSizeApplier : MonoBehaviour
{
    [SerializeField] private LevelBalanceData levelBalanceData;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool applyInEditMode = true;
    [SerializeField] [Min(1)] private int previewProgression = 1;
    [SerializeField] private float currentAppliedLevelSize = 1f;

    private Vector3 originalLocalScale = Vector3.one;
    private bool hasCachedOriginalScale;
    private Transform cachedScaleSource;

    private void Awake()
    {
        CacheOriginalScale();
    }

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyFromCurrentProgression();
        }
    }

    private void OnValidate()
    {
        if (previewProgression < 1)
        {
            previewProgression = 1;
        }

        CacheOriginalScale();

        if (!Application.isPlaying && applyInEditMode)
        {
            Apply(previewProgression);
        }
    }

    public void ApplyFromCurrentProgression()
    {
        int progression = GameManager.Instance != null ? GameManager.Instance.CurrentProgression : previewProgression;
        Apply(progression);
    }

    public void SetLevelBalanceData(LevelBalanceData balanceData)
    {
        levelBalanceData = balanceData;
    }

    public void Apply(int progression)
    {
        CacheOriginalScale();

        if (levelBalanceData == null || targetTransform == null)
        {
            return;
        }

        float levelSize = levelBalanceData.EvaluateLevelSize(progression);
        currentAppliedLevelSize = levelSize;
        targetTransform.localScale = Vector3.Scale(originalLocalScale, Vector3.one * levelSize);
    }

    private void CacheOriginalScale()
    {
        if (targetTransform == null)
        {
            targetTransform = transform;
        }

        if (targetTransform == null)
        {
            return;
        }

        if (cachedScaleSource != targetTransform)
        {
            hasCachedOriginalScale = false;
            cachedScaleSource = targetTransform;
        }

        if (hasCachedOriginalScale)
        {
            return;
        }

        originalLocalScale = targetTransform.localScale;
        hasCachedOriginalScale = true;
    }
}
