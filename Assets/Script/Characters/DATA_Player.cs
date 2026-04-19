using UnityEngine;

public class DATA_Player : MonoBehaviour
{
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
    private void Start()
    {
        BB_Player_Master = FindObjectOfType<BB_Player_Master>();
    }





    public BB_Player_Master BB_Player_Master;
    public CharacterStats CharacterStats;
    

}