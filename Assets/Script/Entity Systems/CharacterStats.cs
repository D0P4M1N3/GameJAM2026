using System;
using UnityEngine;


[Serializable]
public class CharacterStats
{
    [Header("Base")]
    public float Speed = 5;
    public float MaxHP = 20;
    public float Damage = 1;

    [Header("Multiplier")]
    public float mSpeed = 1;
    public float mMaxHP = 1;
    public float mDamage = 1;


    [Header("Finals")]
    public float finalSpeed => Speed * mSpeed;
    public float finalMaxHP => MaxHP * mMaxHP;
    public float finalDamage => Damage * mDamage;
}
