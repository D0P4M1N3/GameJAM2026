using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LevelBalanceData", menuName = "Gameplay/Level Balance Data")]
public class LevelBalanceData : ScriptableObject
{
    [SerializeField] private LevelLootTable lootTable;
    [SerializeField] private EnemyBalanceData enemyBalanceData;
    [FormerlySerializedAs("minBuildings")]
    [SerializeField] [Min(0)] private int startMinBuildings = 3;
    [FormerlySerializedAs("maxBuildings")]
    [SerializeField] [Min(0)] private int startMaxBuildings = 6;
    [SerializeField] private AnimationCurve minBuildingsProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] private AnimationCurve maxBuildingsProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [FormerlySerializedAs("minEnemies")]
    [SerializeField] [Min(0)] private int startMinEnemies = 2;
    [FormerlySerializedAs("maxEnemies")]
    [SerializeField] [Min(0)] private int startMaxEnemies = 5;
    [SerializeField] private AnimationCurve minEnemiesProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] private AnimationCurve maxEnemiesProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] [Min(0f)] private float levelSizeStart = 1f;
    [SerializeField] private AnimationCurve levelSizeProgressionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [Header("Sunboss AIs")]
    [Range(0f, 1f)]
    public float TimeoutPredictionAccuracy = 0.8f;


    public LevelLootTable LootTable => lootTable;
    public EnemyBalanceData EnemyBalanceData => enemyBalanceData;
    public int StartMinBuildings => startMinBuildings;
    public int StartMaxBuildings => startMaxBuildings;
    public int StartMinEnemies => startMinEnemies;
    public int StartMaxEnemies => startMaxEnemies;
    public float LevelSizeStart => levelSizeStart;

    public int EvaluateMinBuildings(int progression)
    {
        return EvaluateCount(startMinBuildings, minBuildingsProgressionCurve, progression);
    }

    public int EvaluateMaxBuildings(int progression)
    {
        return Mathf.Max(EvaluateMinBuildings(progression), EvaluateCount(startMaxBuildings, maxBuildingsProgressionCurve, progression));
    }

    public int EvaluateMinEnemies(int progression)
    {
        return EvaluateCount(startMinEnemies, minEnemiesProgressionCurve, progression);
    }

    public int EvaluateMaxEnemies(int progression)
    {
        return Mathf.Max(EvaluateMinEnemies(progression), EvaluateCount(startMaxEnemies, maxEnemiesProgressionCurve, progression));
    }

    public float EvaluateLevelSize(int progression)
    {
        return EvaluateScaledFloat(levelSizeStart, levelSizeProgressionCurve, progression);
    }

    private void OnValidate()
    {
        if (startMaxBuildings < startMinBuildings)
        {
            startMaxBuildings = startMinBuildings;
        }

        if (startMaxEnemies < startMinEnemies)
        {
            startMaxEnemies = startMinEnemies;
        }

        if (levelSizeStart < 0f)
        {
            levelSizeStart = 0f;
        }

        EnsureCurveStartsAtValue(ref minBuildingsProgressionCurve, startMinBuildings);
        EnsureCurveStartsAtValue(ref maxBuildingsProgressionCurve, startMaxBuildings);
        EnsureCurveStartsAtValue(ref minEnemiesProgressionCurve, startMinEnemies);
        EnsureCurveStartsAtValue(ref maxEnemiesProgressionCurve, startMaxEnemies);
        EnsureCurveStartsAtValue(ref levelSizeProgressionCurve, levelSizeStart);
    }


    private static int EvaluateCount(int baseValue, AnimationCurve progressionCurve, int progression)
    {
        float curveValue = EvaluateCurveClamped(progressionCurve, progression, baseValue);
        return Mathf.Max(0, Mathf.RoundToInt(curveValue));
    }

    private static float EvaluateScaledFloat(float baseValue, AnimationCurve progressionCurve, int progression)
    {
        return Mathf.Max(0f, EvaluateCurveClamped(progressionCurve, progression, baseValue));
    }

    private static float EvaluateCurveClamped(AnimationCurve curve, int progression, float fallbackValue)
    {
        if (curve == null || curve.length == 0)
        {
            return fallbackValue;
        }

        float level = Mathf.Max(1, progression);
        Keyframe[] keys = curve.keys;
        if (keys.Length == 0)
        {
            return fallbackValue;
        }

        if (level <= keys[0].time)
        {
            return keys[0].value;
        }

        int lastIndex = keys.Length - 1;
        if (level >= keys[lastIndex].time)
        {
            return keys[lastIndex].value;
        }

        return curve.Evaluate(level);
    }

    private static void EnsureCurveStartsAtValue(ref AnimationCurve curve, float startValue)
    {
        if (curve == null || curve.length == 0)
        {
            curve = new AnimationCurve(new Keyframe(1f, startValue));
            return;
        }

        Keyframe[] keys = curve.keys;
        int firstIndex = 0;
        float earliestTime = keys[0].time;
        for (int i = 1; i < keys.Length; i++)
        {
            if (keys[i].time >= earliestTime)
            {
                continue;
            }

            earliestTime = keys[i].time;
            firstIndex = i;
        }

        Keyframe firstKey = keys[firstIndex];
        firstKey.time = 1f;
        firstKey.value = startValue;
        keys[firstIndex] = firstKey;
        curve.keys = keys;
    }
}
