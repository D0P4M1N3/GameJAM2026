using UnityEngine;

public class DATA_Player : MonoBehaviour
{
    public CharacterStats CharacterStats;
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
        CacheInitialStats();

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
    }

    private void CacheInitialStats()
    {
        if (CharacterStats == null)
        {
            return;
        }

        initialCharacterStats = CharacterStats.Clone();
        if (float.IsPositiveInfinity(initialCharacterStats.HP) || initialCharacterStats.HP <= 0f)
        {
            initialCharacterStats.HP = initialCharacterStats.finalMaxHP;
        }
    }
}
