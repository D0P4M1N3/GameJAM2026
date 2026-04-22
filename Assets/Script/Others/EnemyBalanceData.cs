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
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue = 1f;
    [SerializeField] private AnimationCurve progressionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public float Evaluate(int progression)
    {
        if (progressionCurve == null || progressionCurve.length == 0)
        {
            return minValue;
        }

        float level = Mathf.Max(1, progression);
        Keyframe[] keys = progressionCurve.keys;
        if (keys.Length == 0)
        {
            return minValue;
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

        return progressionCurve.Evaluate(level);
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
