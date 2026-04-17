using System;
using UnityEngine;

[Serializable]
public struct ItemStats
{
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float Health { get; private set; }
    [field: SerializeField] public float Attack { get; private set; }
    [field: SerializeField] public float Value { get; private set; }

    public static ItemStats Zero => new(0f, 0f, 0f, 0f);

    public ItemStats(float speed, float health, float attack, float value)
    {
        Speed = speed;
        Health = health;
        Attack = attack;
        Value = value;
    }

    public static ItemStats operator +(ItemStats left, ItemStats right)
    {
        return new ItemStats(
            left.Speed + right.Speed,
            left.Health + right.Health,
            left.Attack + right.Attack,
            left.Value + right.Value);
    }

    public static ItemStats operator *(ItemStats stats, int multiplier)
    {
        return new ItemStats(
            stats.Speed * multiplier,
            stats.Health * multiplier,
            stats.Attack * multiplier,
            stats.Value * multiplier);
    }

    public bool IsZero()
    {
        return Mathf.Approximately(Speed, 0f)
            && Mathf.Approximately(Health, 0f)
            && Mathf.Approximately(Attack, 0f)
            && Mathf.Approximately(Value, 0f);
    }
}
