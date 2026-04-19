using UnityEngine;

public class DATA_Player : MonoBehaviour
{
    public CharacterStats CharacterStats;
    public static DATA_Player Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Kill duplicate
            return;
        }

        Instance = this;

        // Optional: persist across scenes
        DontDestroyOnLoad(gameObject);
    }

}