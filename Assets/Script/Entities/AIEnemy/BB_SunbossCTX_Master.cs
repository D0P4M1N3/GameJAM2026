using System;
using UnityEngine;
using UnityEngine.UI;

public class BB_SunbossCTX_Master : MonoBehaviour
{
    [Header("\n---- Brain")]
    public BB_SunbossCTX_Brain BB_SunbossCTX_Brain;

    [Header("\n---- Body")]
    public BB_SunbossCTX_Body BB_SunbossCTX_Body;

    [Header("\n---- Sense")]
    public BB_SunbossCTX_Sense BB_SunbossCTX_Sense;

    [Header("\n---- Move")]
    public BB_SunbossCTX_Move BB_SunbossCTX_Move;



    [Header("\n\n---- Debug")]
    public BB_SunbossCTX_Debug BB_SunbossCTX_Debug;
}



[Serializable]
public class BB_SunbossCTX_Brain
{
    [Header("References")]
    public ACT_SunBoss_Brain ACT_SunBoss_Brain;

    [Header("Runtime")]
    public GameObject PlayerOBJ;
    public Vector3 PlayerPosition_LastKnown;
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
    public ConeBox ConeBox;
}

[Serializable]
public class BB_SunbossCTX_Move
{
    public ACT_SunBoss_Navagent ACT_SunBoss_Navagent;
}

[Serializable]
public class BB_SunbossCTX_Debug
{
    public TextMesh TextUI_State;
    public TextMesh TextUI_Sight;
}




