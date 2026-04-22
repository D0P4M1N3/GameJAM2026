using System;
using UnityEngine;

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
    [SerializeField] private EnemyBalanceStat timeoutPredictionAccuracy = EnemyBalanceStat.WithDefaults(0.75f, 1f);
    [SerializeField] private EnemyBalanceStat minPredictionErrorPosition = EnemyBalanceStat.WithDefaults(8f, 2f);
    [SerializeField] private EnemyBalanceStat maxPredictionErrorPosition = EnemyBalanceStat.WithDefaults(16f, 4f);

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

        float normalizedProgression = GetNormalizedProgression(progression);
        ApplyEvaluatedStats(target, normalizedProgression);
    }

    public void ApplyTimeoutTo(BB_Sunboss_Master target, int progression)
    {
        if (target == null || target.BB_SunbossCTX_Brain == null)
        {
            return;
        }

        float normalizedProgression = GetNormalizedProgression(progression);
        float timeoutAccuracy = Mathf.Clamp01(timeoutPredictionAccuracy.Evaluate(normalizedProgression));
        target.BB_SunbossCTX_Brain.PredictionAccuracy = timeoutAccuracy;
    }

    private void ApplyEvaluatedStats(BB_Sunboss_Master target, float normalizedProgression)
    {
        if (target.CharacterStats != null)
        {
            target.CharacterStats.Speed = speed.Evaluate(normalizedProgression);
            target.CharacterStats.MaxHP = maxHP.Evaluate(normalizedProgression);
            target.CharacterStats.Damage = damage.Evaluate(normalizedProgression);
            target.CharacterStats.RefreshInspectorFinals();
            target.CharacterStats.HP = target.CharacterStats.finalMaxHP;
        }

        if (target.BB_SunbossCTX_Sense != null && target.BB_SunbossCTX_Sense.ConeBox != null)
        {
            target.BB_SunbossCTX_Sense.ConeBox.Angle = coneAngle.Evaluate(normalizedProgression);
            target.BB_SunbossCTX_Sense.ConeBox.Radius = coneRadius.Evaluate(normalizedProgression);
            target.BB_SunbossCTX_Sense.ConeBox.PlaneNormal = conePlaneNormal;
        }

        if (target.BB_SunbossCTX_Brain != null)
        {
            target.BB_SunbossCTX_Brain.PredictionAccuracy = Mathf.Clamp01(predictionAccuracy.Evaluate(normalizedProgression));
            target.BB_SunbossCTX_Brain.MinPredictionError_Position = minPredictionErrorPosition.Evaluate(normalizedProgression);
            target.BB_SunbossCTX_Brain.MaxPredictionError_Position = Mathf.Max(
                target.BB_SunbossCTX_Brain.MinPredictionError_Position,
                maxPredictionErrorPosition.Evaluate(normalizedProgression));
            target.BB_SunbossCTX_Brain.ScanSpeed = scanSpeed.Evaluate(normalizedProgression);
            target.BB_SunbossCTX_Brain.ForgetTime = forgetTime.Evaluate(normalizedProgression);
        }

        if (target.BB_SunbossCTX_Move != null)
        {
            target.BB_SunbossCTX_Move.TurnSpeed = turnSpeed.Evaluate(normalizedProgression);
            target.BB_SunbossCTX_Move.TurnSpeedChase = turnSpeedChase.Evaluate(normalizedProgression);
            target.BB_SunbossCTX_Move.MaxMoveAngleFromFacing = maxMoveAngleFromFacing.Evaluate(normalizedProgression);
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
        timeoutPredictionAccuracy.Validate01();
        minPredictionErrorPosition.Validate();
        maxPredictionErrorPosition.Validate();
        scanSpeed.Validate();
        forgetTime.Validate();
        turnSpeed.Validate();
        turnSpeedChase.Validate();
        maxMoveAngleFromFacing.Validate();
    }

    private static float GetNormalizedProgression(int progression)
    {
        float depth = Mathf.Max(0f, progression - 1);
        return 1f - Mathf.Exp(-0.12f * depth);
    }
}

[Serializable]
public class EnemyBalanceStat
{
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue = 1f;
    [SerializeField] private AnimationCurve progressionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public float Evaluate(float normalizedProgression)
    {
        float resolvedProgression = Mathf.Clamp01(normalizedProgression);
        float curveValue = progressionCurve == null || progressionCurve.length == 0
            ? resolvedProgression
            : progressionCurve.Evaluate(resolvedProgression);

        return Mathf.Lerp(minValue, maxValue, Mathf.Clamp01(curveValue));
    }

    public void Validate()
    {
        if (progressionCurve == null || progressionCurve.length == 0)
        {
            progressionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }

    public void Validate01()
    {
        minValue = Mathf.Clamp01(minValue);
        maxValue = Mathf.Clamp01(maxValue);
        Validate();
    }

    public static EnemyBalanceStat WithDefaults(float minValue, float maxValue)
    {
        return new EnemyBalanceStat
        {
            minValue = minValue,
            maxValue = maxValue,
            progressionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
        };
    }
}
