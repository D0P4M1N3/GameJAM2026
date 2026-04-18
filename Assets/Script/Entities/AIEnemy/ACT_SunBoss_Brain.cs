using UnityEngine;

public class ACT_SunBoss_Brain : MonoBehaviour
{
    [Header("References")]
    public BB_SunbossCTX_Master BB_SunbossCTX_Master;

    void Update()
    {
        BB_SunbossCTX_Master.BB_SunbossCTX_Sense.ConeBox.Ray.Target = BB_SunbossCTX_Master.BB_SunbossCTX_Brain.PlayerOBJ.transform;



        BB_SunbossCTX_Master.BB_SunbossCTX_Debug.TextUI_Sight.text =
            "Target Reached: " + BB_SunbossCTX_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget.ToString();
    }
}
