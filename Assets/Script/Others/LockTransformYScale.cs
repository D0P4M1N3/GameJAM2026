using UnityEngine;

public class LockTransformYScale : MonoBehaviour
{
    [SerializeField] private bool captureInitialYScaleOnAwake = true;
    [SerializeField] private float lockedYScale = 1f;
    [SerializeField] private bool applyInLateUpdate = true;

    private void Awake()
    {
        if (captureInitialYScaleOnAwake)
        {
            lockedYScale = transform.localScale.y;
        }

        ApplyLockedYScale();
    }

    private void Update()
    {
        if (!applyInLateUpdate)
        {
            ApplyLockedYScale();
        }
    }

    private void LateUpdate()
    {
        if (applyInLateUpdate)
        {
            ApplyLockedYScale();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ApplyLockedYScale();
        }
    }

    public void SetLockedYScale(float newLockedYScale)
    {
        lockedYScale = newLockedYScale;
        ApplyLockedYScale();
    }

    private void ApplyLockedYScale()
    {
        Vector3 localScale = transform.localScale;
        if (Mathf.Approximately(localScale.y, lockedYScale))
        {
            return;
        }

        localScale.y = lockedYScale;
        transform.localScale = localScale;
    }
}
