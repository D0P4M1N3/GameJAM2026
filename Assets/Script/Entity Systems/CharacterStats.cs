using System;
using UnityEngine;


[Serializable]
public class CharacterStats
{
    [Header("Base")]
    public float Speed = 10f;
    public float MaxHP = 5f;
    public float Damage = 1f;
    public float Storage = 0f;

    [Header("Current")]
    public float HP = float.PositiveInfinity;
    public float Currency = 0;
    public Color CharacterColor = Color.white;

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

    [Header("Inspector Finals")]
    [SerializeField] private float inspectorFinalSpeed;
    [SerializeField] private float inspectorFinalMaxHP;
    [SerializeField] private float inspectorFinalDamage;
    [SerializeField] private float inspectorFinalStorage;

    public float InspectorFinalSpeed => inspectorFinalSpeed;
    public float InspectorFinalMaxHP => inspectorFinalMaxHP;
    public float InspectorFinalDamage => inspectorFinalDamage;
    public float InspectorFinalStorage => inspectorFinalStorage;

    public void RefreshInspectorFinals()
    {
        inspectorFinalSpeed = finalSpeed;
        inspectorFinalMaxHP = finalMaxHP;
        inspectorFinalDamage = finalDamage;
        inspectorFinalStorage = finalStorage;
    }

    public CharacterStats Clone()
    {
        CharacterStats clone = new CharacterStats
        {
            Speed = Speed,
            MaxHP = MaxHP,
            Damage = Damage,
            Storage = Storage,
            HP = HP,
            Currency = Currency,
            CharacterColor = CharacterColor,
            mSpeed = mSpeed,
            mMaxHP = mMaxHP,
            mDamage = mDamage,
            mStorage = mStorage,
        };

        clone.RefreshInspectorFinals();
        return clone;
    }

    public void CopyFrom(CharacterStats source)
    {
        if (source == null)
        {
            return;
        }

        Speed = source.Speed;
        MaxHP = source.MaxHP;
        Damage = source.Damage;
        Storage = source.Storage;
        HP = source.HP;
        Currency = source.Currency;
        CharacterColor = source.CharacterColor;
        mSpeed = source.mSpeed;
        mMaxHP = source.mMaxHP;
        mDamage = source.mDamage;
        mStorage = source.mStorage;
        RefreshInspectorFinals();
    }
}
