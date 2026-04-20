using UnityEngine;

[CreateAssetMenu(fileName = "LevelBalanceData", menuName = "Gameplay/Level Balance Data")]
public class LevelBalanceData : ScriptableObject
{
    [SerializeField] private LevelLootTable lootTable;
    [SerializeField] [Min(0)] private int minBuildings = 3;
    [SerializeField] [Min(0)] private int maxBuildings = 6;
    [SerializeField] [Min(0)] private int minEnemies = 2;
    [SerializeField] [Min(0)] private int maxEnemies = 5;

    public LevelLootTable LootTable => lootTable;
    public int MinBuildings => minBuildings;
    public int MaxBuildings => maxBuildings;
    public int MinEnemies => minEnemies;
    public int MaxEnemies => maxEnemies;

    private void OnValidate()
    {
        if (maxBuildings < minBuildings)
        {
            maxBuildings = minBuildings;
        }

        if (maxEnemies < minEnemies)
        {
            maxEnemies = minEnemies;
        }
    }
}
