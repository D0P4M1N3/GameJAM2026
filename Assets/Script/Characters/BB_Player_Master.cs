using System;
using UnityEngine;

public class BB_Player_Master : MonoBehaviour
{
    [Header("\n\n---- Stats")]
    public CharacterStats CharacterStats;

    [Header("\n\n---- Body")]
    public BB_PlayerCTX_Body BB_PlayerCTX_Body;

    [Header("\n\n---- Movement")]
    public BB_PlayerCTX_Move BB_PlayerCTX_Move;

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