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
    public Color ItemColor = Color.white;

    [Header("Modifier")]
    public float mSpeed = 0;
    public float mMaxHP = 0;
    public float mDamage = 0;
    public float mStorage = 0;


    [Header("Finals")]
    public float finalSpeed => (Speed / 100f) * mSpeed + Speed;
    public float finalMaxHP => (MaxHP / 100f) * mMaxHP + MaxHP;
    public float finalDamage => (Damage / 100f) * mDamage + Damage;
    public float finalStorage => (Storage / 100f) * mStorage + Storage;
}
