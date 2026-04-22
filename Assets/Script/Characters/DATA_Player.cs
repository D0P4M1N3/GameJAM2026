using UnityEngine;

public class DATA_Player : MonoBehaviour
{
    public CharacterStats CharacterStats;
    public ProjectileShooterStats ProjectileShooterStats;
    public static DATA_Player Instance { get; private set; }

    private CharacterStats initialCharacterStats;

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

        CharacterStats.Storage = Mathf.Max(0f, CharacterStats.Storage + additionalPercent);
        CharacterStats.RefreshInspectorFinals();
        PlayerStorageVisual.RefreshAll();
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
}
