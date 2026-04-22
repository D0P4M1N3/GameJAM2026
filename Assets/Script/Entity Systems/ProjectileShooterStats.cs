using System;
using UnityEngine;



[Serializable]
public class ProjectileShooterStats 
{
    public float ProjectileCount_Max = 5;
    public float ProjectileCount_Current = float.PositiveInfinity;
    public bool Infinity = false;
}
