using UnityEngine;

public enum PlayerFaceVariant
{
    A = 0,
    B = 1,
    C = 2,
    D = 3,
    E = 4,
    F = 5,
    G = 6,
    H = 7
}

[System.Serializable]
public struct PlayerFaceState
{
    public PlayerFaceVariant Variant;

    public PlayerFaceState(PlayerFaceVariant variant)
    {
        Variant = variant;
    }
}

public class DATA_Player : MonoBehaviour
{
    public CharacterStats CharacterStats;
    public ProjectileShooterStats ProjectileShooterStats;
    public static DATA_Player Instance { get; private set; }
    public PlayerFaceState CurrentFaceState => currentFaceState;
    public event System.Action<PlayerFaceState> FaceChanged;

    [Header("Face")]
    [SerializeField] private PlayerFaceVariant defaultFace = PlayerFaceVariant.A;
    [SerializeField] private PlayerFaceState currentFaceState = new(PlayerFaceVariant.A);

    private CharacterStats initialCharacterStats;
    private Coroutine resetFaceRoutine;
    private Coroutine damageFaceRoutine;
    private float damageFaceEndTime;
    private bool hasTriggeredDefeat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Kill duplicate
            return;
        }

        Instance = this;
        CharacterStats?.RefreshInspectorFinals();
        CacheInitialStats();
        PlayerStorageVisual.RefreshAll();
        NotifyFaceChanged();

        // Optional: persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    public void ResetCharacterStats()
    {
        if (CharacterStats == null || initialCharacterStats == null)
        {
            return;
        }

        CharacterStats.CopyFrom(initialCharacterStats);

        if (float.IsPositiveInfinity(CharacterStats.HP) || CharacterStats.HP <= 0f)
        {
            CharacterStats.HP = CharacterStats.finalMaxHP;
        }

        CharacterStats.RefreshInspectorFinals();
        PlayerStorageVisual.RefreshAll();
        hasTriggeredDefeat = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgression();
        }
    }

    public void AddStoragePercent(float additionalPercent)
    {
        if (CharacterStats == null)
        {
            return;
        }

        CharacterStats.Storage = Mathf.Clamp(
            CharacterStats.Storage + additionalPercent,
            0f,
            Mathf.Max(0f, CharacterStats.MaxStorage));
        CharacterStats.RefreshInspectorFinals();
        PlayerStorageVisual.RefreshAll();
    }

    public void SetFace(PlayerFaceVariant variant)
    {
        CancelFaceReset();
        ApplyFace(variant);
    }

    public void SetFaceForDuration(PlayerFaceVariant variant, float duration)
    {
        CancelFaceReset();
        ApplyFace(variant);

        if (duration > 0f && isActiveAndEnabled)
        {
            resetFaceRoutine = StartCoroutine(ResetFaceAfterDelay(duration));
        }
    }

    public void ResetFaceToDefault()
    {
        CancelFaceReset();
        CancelDamageFace();
        ApplyFace(defaultFace);
    }

    public void SetFaceVariant(PlayerFaceVariant variant)
    {
        SetFace(variant);
    }

    public void SetFaceVariant(int variantIndex)
    {
        int clampedIndex = Mathf.Clamp(variantIndex, 0, System.Enum.GetValues(typeof(PlayerFaceVariant)).Length - 1);
        SetFaceVariant((PlayerFaceVariant)clampedIndex);
    }

    public void SetFaceState(PlayerFaceState faceState)
    {
        SetFace(faceState.Variant);
    }

    public void PlayDamageFaceSwap(float duration = 0.4f, float swapInterval = 0.2f)
    {
        if (duration <= 0f || swapInterval <= 0f)
        {
            return;
        }

        damageFaceEndTime = Time.time + duration;

        if (damageFaceRoutine != null)
        {
            return;
        }

        damageFaceRoutine = StartCoroutine(DamageFaceSwapRoutine(swapInterval));
    }

    private void ApplyFace(PlayerFaceVariant variant)
    {
        if (currentFaceState.Variant == variant)
        {
            return;
        }

        currentFaceState = new PlayerFaceState(variant);
        NotifyFaceChanged();
    }

    private void CancelFaceReset()
    {
        if (resetFaceRoutine == null)
        {
            return;
        }

        StopCoroutine(resetFaceRoutine);
        resetFaceRoutine = null;
    }

    private void CancelDamageFace()
    {
        if (damageFaceRoutine == null)
        {
            return;
        }

        StopCoroutine(damageFaceRoutine);
        damageFaceRoutine = null;
    }

    private System.Collections.IEnumerator ResetFaceAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        resetFaceRoutine = null;
        ApplyFace(defaultFace);

    }

    private System.Collections.IEnumerator DamageFaceSwapRoutine(float swapInterval)
    {
        CancelFaceReset();

        bool useFaceF = true;
        while (Time.time < damageFaceEndTime)
        {
            ApplyFace(useFaceF ? PlayerFaceVariant.F : PlayerFaceVariant.H);
            useFaceF = !useFaceF;
            yield return new WaitForSeconds(swapInterval);
        }

        damageFaceRoutine = null;
        ApplyFace(defaultFace);
    }

    private void CacheInitialStats()
    {
        if (CharacterStats == null)
        {
            return;
        }

        CharacterStats.RefreshInspectorFinals();
        initialCharacterStats = CharacterStats.Clone();
        if (float.IsPositiveInfinity(initialCharacterStats.HP) || initialCharacterStats.HP <= 0f)
        {
            initialCharacterStats.HP = initialCharacterStats.finalMaxHP;
        }
    }

    private void OnValidate()
    {
        CharacterStats?.RefreshInspectorFinals();
    }

    private void Update()
    {
        if (hasTriggeredDefeat || CharacterStats == null || CharacterStats.HP > 0f)
        {
            return;
        }

        hasTriggeredDefeat = true;
        GameManager.Instance?.HandlePlayerDefeated();
    }

    private void NotifyFaceChanged()
    {
        FaceChanged?.Invoke(currentFaceState);
    }
}
