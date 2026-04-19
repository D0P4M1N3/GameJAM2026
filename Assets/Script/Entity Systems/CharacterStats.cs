using System;
using UnityEngine;


[Serializable]
public class CharacterStats
{
    [Header("Base")]
    public float Speed = 5;
    public float MaxHP = 20;
    public float Damage = 1;
    public float Storage = 10;

    [Header("Current")]
    public float HP = float.PositiveInfinity;
    public float Currency = 0;

    [Header("Modifier")]
    public float mSpeed = 1;
    public float mMaxHP = 1;
    public float mDamage = 1;
    public float mStorage = 0;


    [Header("Finals")]
    public float finalSpeed => Speed * mSpeed;
    public float finalMaxHP => MaxHP * mMaxHP;
    public float finalDamage => Damage * mDamage;
    public float finalStorage => Storage * mStorage;
}
