using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyBalanceData", menuName = "Gameplay/Enemy Balance Data")]
public class EnemyBalanceData : ScriptableObject
{
    [Header("Character Stats")]
    [SerializeField] private EnemyBalanceStat speed = EnemyBalanceStat.WithDefaults(1f, 5f);
    [SerializeField] private EnemyBalanceStat maxHP = EnemyBalanceStat.WithDefaults(10f, 60f);
    [SerializeField] private EnemyBalanceStat damage = EnemyBalanceStat.WithDefaults(1f, 12f);

    [Header("Sense")]
    [SerializeField] private EnemyBalanceStat coneAngle = EnemyBalanceStat.WithDefaults(25f, 120f);
    [SerializeField] private EnemyBalanceStat coneRadius = EnemyBalanceStat.WithDefaults(8f, 30f);
    [SerializeField] private Vector3 conePlaneNormal = Vector3.up;

    [Header("Prediction")]
    [SerializeField] private EnemyBalanceStat predictionAccuracy = EnemyBalanceStat.WithDefaults(0.1f, 0.95f);

    [Header("Brain")]
    [SerializeField] private EnemyBalanceStat scanSpeed = EnemyBalanceStat.WithDefaults(90f, 540f);
    [SerializeField] private EnemyBalanceStat forgetTime = EnemyBalanceStat.WithDefaults(0.5f, 4f);

    [Header("Movement")]
    [SerializeField] private EnemyBalanceStat turnSpeed = EnemyBalanceStat.WithDefaults(30f, 360f);
    [SerializeField] private EnemyBalanceStat turnSpeedChase = EnemyBalanceStat.WithDefaults(180f, 2048f);
    [SerializeField] private EnemyBalanceStat maxMoveAngleFromFacing = EnemyBalanceStat.WithDefaults(5f, 75f);

    public void ApplyTo(BB_Sunboss_Master target, int progression)
    {
        if (target == null)
        {
            return;
        }

        int resolvedProgression = Mathf.Max(1, progression);
        ApplyEvaluatedStats(target, resolvedProgression);
    }

    public void ApplyTimeoutTo(BB_Sunboss_Master target, int progression)
    {
        if (target == null || target.BB_SunbossCTX_Brain == null)
        {
            return;
        }

        ApplyPrediction(target, Mathf.Max(1, progression));
    }

    private void ApplyEvaluatedStats(BB_Sunboss_Master target, int progression)
    {
        if (target.CharacterStats != null)
        {
            target.CharacterStats.Speed = speed.Evaluate(progression);
            target.CharacterStats.MaxHP = maxHP.Evaluate(progression);
            target.CharacterStats.Damage = damage.Evaluate(progression);
            target.CharacterStats.RefreshInspectorFinals();
            target.CharacterStats.HP = target.CharacterStats.finalMaxHP;
        }

        if (target.BB_SunbossCTX_Sense != null && target.BB_SunbossCTX_Sense.ConeBox != null)
        {
            target.BB_SunbossCTX_Sense.ConeBox.Angle = coneAngle.Evaluate(progression);
            target.BB_SunbossCTX_Sense.ConeBox.Radius = coneRadius.Evaluate(progression);
            target.BB_SunbossCTX_Sense.ConeBox.PlaneNormal = conePlaneNormal;
        }

        if (target.BB_SunbossCTX_Brain != null)
        {
            ApplyPrediction(target, progression);
            target.BB_SunbossCTX_Brain.ScanSpeed = scanSpeed.Evaluate(progression);
            target.BB_SunbossCTX_Brain.ForgetTime = forgetTime.Evaluate(progression);
        }

        if (target.BB_SunbossCTX_Move != null)
        {
            target.BB_SunbossCTX_Move.TurnSpeed = turnSpeed.Evaluate(progression);
            target.BB_SunbossCTX_Move.TurnSpeedChase = turnSpeedChase.Evaluate(progression);
            target.BB_SunbossCTX_Move.MaxMoveAngleFromFacing = maxMoveAngleFromFacing.Evaluate(progression);
        }
    }

    private void OnValidate()
    {
        speed.Validate();
        maxHP.Validate();
        damage.Validate();
        coneAngle.Validate();
        coneRadius.Validate();
        predictionAccuracy.Validate01();
        scanSpeed.Validate();
        forgetTime.Validate();
        turnSpeed.Validate();
        turnSpeedChase.Validate();
        maxMoveAngleFromFacing.Validate();
    }

    private void ApplyPrediction(BB_Sunboss_Master target, int progression)
    {
        target.BB_SunbossCTX_Brain.PredictionAccuracy = Mathf.Clamp01(predictionAccuracy.Evaluate(progression));
    }
}

[Serializable]
public class EnemyBalanceStat
{
    [FormerlySerializedAs("minValue")]
    [SerializeField] private float startValue = 1f;
    [SerializeField] private AnimationCurve progressionCurve = AnimationCurve.Linear(1f, 1f, 2f, 1f);

    public float Evaluate(int progression)
    {
        return EvaluateCurveClamped(progressionCurve, progression, startValue);
    }

    public void Validate()
    {
        if (progressionCurve == null || progressionCurve.length == 0)
        {
            progressionCurve = new AnimationCurve(new Keyframe(1f, startValue));
            return;
        }

        EnsureCurveStartsAtValue(ref progressionCurve, startValue);
    }

    public void Validate01()
    {
        startValue = Mathf.Clamp01(startValue);
        Validate();
    }

    public static EnemyBalanceStat WithDefaults(float startValue, float unusedLegacyMaxValue)
    {
        return new EnemyBalanceStat
        {
            startValue = startValue,
            progressionCurve = new AnimationCurve(new Keyframe(1f, startValue)),
        };
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
