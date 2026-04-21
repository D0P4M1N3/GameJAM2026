using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelBalanceData", menuName = "Gameplay/Level Balance Data")]
public class LevelBalanceData : ScriptableObject
{
    [SerializeField] private LevelLootTable lootTable;
    [SerializeField] [Min(0)] private int minBuildings = 3;
    [SerializeField] [Min(0)] private int maxBuildings = 6;
    [SerializeField] private AnimationCurve minBuildingsProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] private AnimationCurve maxBuildingsProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] [Min(0)] private int minEnemies = 2;
    [SerializeField] [Min(0)] private int maxEnemies = 5;
    [SerializeField] private AnimationCurve minEnemiesProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] private AnimationCurve maxEnemiesProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [Header("Sunboss AIs")]
    [Range(0f, 1f)]
    public float TimeoutPredictionAccuracy = 0.8f;


    public LevelLootTable LootTable => lootTable;
    public int MinBuildings => minBuildings;
    public int MaxBuildings => maxBuildings;
    public int MinEnemies => minEnemies;
    public int MaxEnemies => maxEnemies;

    public int EvaluateMinBuildings(int progression)
    {
        return EvaluateCount(minBuildings, minBuildingsProgressionCurve, progression);
    }

    public int EvaluateMaxBuildings(int progression)
    {
        return Mathf.Max(EvaluateMinBuildings(progression), EvaluateCount(maxBuildings, maxBuildingsProgressionCurve, progression));
    }

    public int EvaluateMinEnemies(int progression)
    {
        return EvaluateCount(minEnemies, minEnemiesProgressionCurve, progression);
    }

    public int EvaluateMaxEnemies(int progression)
    {
        return Mathf.Max(EvaluateMinEnemies(progression), EvaluateCount(maxEnemies, maxEnemiesProgressionCurve, progression));
    }

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

    public void Begin()
    {
        UI_Timer.Instance.AddTimeOutListener(TimeOut);
    }

    private static int EvaluateCount(int baseValue, AnimationCurve progressionCurve, int progression)
    {
        float normalizedProgression = GetNormalizedProgression(progression);
        float multiplier = progressionCurve == null || progressionCurve.length == 0
            ? 1f
            : Mathf.Max(0f, progressionCurve.Evaluate(normalizedProgression));

        return Mathf.Max(0, Mathf.RoundToInt(baseValue * multiplier));
    }

    private static float GetNormalizedProgression(int progression)
    {
        float depth = Mathf.Max(0f, progression - 1);
        return 1f - Mathf.Exp(-0.12f * depth);
    }

    public void TimeOut()
    {
        BB_Sunboss_Master[] BSMs = FindObjectsOfType<BB_Sunboss_Master>();

        foreach (BB_Sunboss_Master BSM in BSMs)
        {
            BSM.BB_SunbossCTX_Brain.PredictionAccuracy = TimeoutPredictionAccuracy;
        }
    }
}
