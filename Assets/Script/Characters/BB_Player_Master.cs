using System;
using UnityEngine;

public class BB_Player_Master : MonoBehaviour
{
    [Header("\n\n---- Stats")]
    [SerializeField] private CharacterStats characterStats;
    public CharacterStats CharacterStats => characterStats;

    [Header("\n\n---- Body")]
    public BB_PlayerCTX_Body BB_PlayerCTX_Body;

    [Header("\n\n---- Movement")]
    public BB_PlayerCTX_Move BB_PlayerCTX_Move;

    private void Awake()
    {
        EnsureCharacterStatsReference();
    }

    private void OnValidate()
    {
        EnsureCharacterStatsReference();
    }

    private void EnsureCharacterStatsReference()
    {
        if (characterStats == null)
        {
            characterStats = GetComponent<CharacterStats>();
        }

        if (characterStats == null)
        {
            characterStats = GetComponentInChildren<CharacterStats>();
        }
    }
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
