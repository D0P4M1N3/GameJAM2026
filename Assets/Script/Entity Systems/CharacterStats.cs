using System;
using UnityEngine;


[Serializable]
public class CharacterStats
{
    [Header("Base")]
    public float Speed = 1f;
    public float MaxHP = 5f;
    public float Damage = 1f;
    public float Storage = 10f;

    [Header("Current")]
    public float HP = float.PositiveInfinity;
    public float Currency = 0;

    [Header("Modifier")]
    public float mSpeed = 1;
    public float mMaxHP = 1;
    public float mDamage = 1;
    public float mStorage = 0;


    [Header("Finals")]
    public float finalSpeed => (Speed / 100f) * mSpeed;
    public float finalMaxHP => (MaxHP / 100f) * mMaxHP;
    public float finalDamage => (Damage / 100f) * mDamage;
    public float finalStorage => (Storage / 100f) * mStorage;
}
