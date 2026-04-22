using System;
using UnityEngine;

public class BB_Player_Master : MonoBehaviour
{
    [Header("\n\n---- Stats")]
    public CharacterStats CharacterStats => DATA_Player.Instance.CharacterStats;
    public ProjectileShooterStats ProjectileShooterStats => DATA_Player.Instance.ProjectileShooterStats;

    [Header("\n\n---- Body")]
    public BB_PlayerCTX_Body BB_PlayerCTX_Body;

    [Header("\n\n---- Movement")]
    public BB_PlayerCTX_Move BB_PlayerCTX_Move;

    [Header("\n\n---- Combat")]
    public BB_PlayerCTX_Combat BB_PlayerCTX_Combat;


}

[Serializable]
public class BB_PlayerCTX_Body
{
    public Transform WholeBody;
}

[Serializable]
public class BB_PlayerCTX_Move
{
    public InputReader InputReader;
    public TopDownController TopDownController;
}

[Serializable]
public class BB_PlayerCTX_Combat
{
    public ACT_Player_Combat ACT_Player_Combat;
    public GameObject ProjectilePrefab;
    public float projectileSpeed = 10f;
    public float turnSpeed = 5f;
    public float BulletLifetime = 2f;
}