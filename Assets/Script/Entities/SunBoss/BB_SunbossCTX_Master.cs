using System;
using UnityEngine;
using UnityEngine.UI;

public class BB_Sunboss_Master : MonoBehaviour
{
    [Header("\n\n---- Stats")]
    public CharacterStats CharacterStats;

    [Header("\n\n---- Brain")]
    public BB_SunbossCTX_Brain BB_SunbossCTX_Brain;

    [Header("\n\n---- Body")]
    public BB_SunbossCTX_Body BB_SunbossCTX_Body;

    [Header("\n\n---- Sense")]
    public BB_SunbossCTX_Sense BB_SunbossCTX_Sense;

    [Header("\n\n---- Move")]
    public BB_SunbossCTX_Move BB_SunbossCTX_Move;
    
    [Header("\n\n---- Combat")]
    public BB_SunbossCTX_Combat BB_SunbossCTX_Combat;



    [Header("\n\n---- Debug")]
    public BB_SunbossCTX_Debug BB_SunbossCTX_Debug;

    private void Update()
    {
        ///Var Sync
        BB_SunbossCTX_Sense.ConeBox.Data = BB_SunbossCTX_Sense.ConeBoxData;
    }
}



[Serializable]
public class BB_SunbossCTX_Brain
{
    [Header("References")]
    public ACT_SunBoss_Brain ACT_SunBoss_Brain;
    //public GameObject PatrolPointOBJ;

    [Header("Patrol")]
    public float UncertainInPrediction_Reduction = 0.8f;
    public float MinPredictionError_Position = 5;
    public float MaxPredictionError_Position = 10;

    [Header("Scan")]
    public bool Rotation = true;
    public float ScanSpeed = 360;// degrees per second

    [Header("Chase")]
    public float ForgetTime = 1f;

    [Header("\nRuntime")]
    public GameObject PlayerOBJ;
    public Vector3 ActualPlayerPosition_NavmeshProjected;
    public Vector3 PlayerPosition_LastestKnown;
    public float UncertainInPrediction = 1f;
}

[Serializable]
public class BB_SunbossCTX_Body
{
    public Transform WholeBody;
    public Transform CharacterVisual;
}


[Serializable]
public class BB_SunbossCTX_Sense
{
    [Header("ConeBox 1")]
    public ConeBox ConeBox;
    [Header("ConeBox 1 - Settings")]
    public ConeBoxData ConeBoxData;
}

[Serializable]
public class BB_SunbossCTX_Move
{
    public ACT_SunBoss_Navagent ACT_SunBoss_Navagent;
    public float TurnSpeed = 360f;
    public float MaxMoveAngleFromFacing = 15f;
}

[Serializable]
public class BB_SunbossCTX_Combat
{
    public ACT_SunBoss_Combat ACT_SunBoss_Combat;
}

[Serializable]
public class BB_SunbossCTX_Debug
{
    public TextMesh TextUI_State;
    public TextMesh TextUI_Sight;
}




