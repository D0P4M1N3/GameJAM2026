using UnityEngine;

public class ACT_SunBoss_Brain : MonoBehaviour
{
    [Header("References")]
    public BB_SunbossCTX_Master BB_SunbossCTX_Master;

    [Header("Configs")]
    [SerializeField] SM_SunBoss_Brain SM_SunBoss_Brain;

    [Header("Runtime")]
    public SM_SunBoss_Brain SM_SunBoss_Brain_INST;


    void Update()
    {
        BB_SunbossCTX_Master.BB_SunbossCTX_Sense.ConeBox.Ray.Target = BB_SunbossCTX_Master.BB_SunbossCTX_Brain.PlayerOBJ.transform;



        BB_SunbossCTX_Master.BB_SunbossCTX_Debug.TextUI_Sight.text =
            "Target Reached: " + BB_SunbossCTX_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget.ToString();




        if (BB_SunbossCTX_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget)
        {
            BB_SunbossCTX_Master.BB_SunbossCTX_Brain.PlayerPosition_LastestKnown = BB_SunbossCTX_Master.BB_SunbossCTX_Brain.PlayerOBJ.transform.position;
        }


        BB_SunbossCTX_Master.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.GoToThisFrame(BB_SunbossCTX_Master.BB_SunbossCTX_Brain.PlayerPosition_LastestKnown);
    }
}
