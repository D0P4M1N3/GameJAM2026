using UnityEngine;
namespace SunBoss
{



    [CreateAssetMenu(menuName = "SunBoss/Statemachines/AI/SM_SunBoss_Brain")]

    public class SM_SunBoss_Brain : SM_SunBoss__BASE
    {
        public override void Tick_Override()
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




    public class STATE_PATROL : B_STATE
    {
        // Randomly pick a path to the player position within min and max radius
        // Overtime, The min and max Patrol Radius gets smaller to the Actual player position 
        // SeePlayer -> STATE_CHASE
        // SeeNothing -> STATE_PATROL
    }

    public class STATE_CHASE : B_STATE
    {
        // Chase the player while keep tracking last seen position
        // SeePlayer -> STATE_CHASE
        // SeeNothing -> STATE_SEEK
    }

    public class STATE_SEEK : B_STATE
    {
        // Go to the last seen player location
        // SeePlayer -> STATE_CHASE
        // SeeNothing -> STATE_SCAN
    }

    public class STATE_SCAN : B_STATE
    {
        // Go to the last seen player location
        // SeePlayer -> STATE_CHASE
        // SeeNothing -> STATE_PATROL
    }










}
