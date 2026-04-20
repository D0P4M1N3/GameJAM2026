using UnityEngine;
using static UnityEngine.GraphicsBuffer;
namespace SunBoss
{



    [CreateAssetMenu(menuName = "SunBoss/Statemachines/AI/SM_SunBoss_Brain")]

    public class SM_SunBoss_Brain : SM_SunBoss__BASE
    {
        public override void Begin_Override()
        {
            AddStates(new B_STATE[]
            {
                new STATE_PATROL(),
                new STATE_CHASE(),
                new STATE_SEEK(),
                new STATE_SCAN()
            });

            SetState<STATE_PATROL>();

            BB_Sunboss_Master.BB_SunbossCTX_Brain.PlayerOBJ = FindObjectOfType<BB_Player_Master>().BB_PlayerCTX_Body.WholeBody.gameObject;
        }

        public override void Tick_Override()
        {
            BB_Sunboss_Master.BB_SunbossCTX_Debug.TextUI_State.text =
                GetCurrentState() +
                "\nUncertain: " + BB_Sunboss_Master.BB_SunbossCTX_Brain.UncertainInPrediction;
                ;

            BB_Sunboss_Master.BB_SunbossCTX_Sense.ConeBox.Ray.Target = BB_Sunboss_Master.BB_SunbossCTX_Brain.PlayerOBJ.transform;
            BB_Sunboss_Master.BB_SunbossCTX_Debug.TextUI_Sight.text =
                "Target Seen: " + BB_Sunboss_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget.ToString();


            BB_Sunboss_Master.BB_SunbossCTX_Brain.ActualPlayerPosition_NavmeshProjected = B_NavMeshUtil.Project( BB_Sunboss_Master.BB_SunbossCTX_Brain.PlayerOBJ.transform.position);

			if (BB_Sunboss_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget)
            {
                BB_Sunboss_Master.BB_SunbossCTX_Brain.PlayerPosition_LastestKnown = BB_Sunboss_Master.BB_SunbossCTX_Brain.ActualPlayerPosition_NavmeshProjected;
            }
        }
    }

    public abstract class SunBossState : B_STATE
    {
        // QUICK CONSTRUCTOR
        protected BB_Sunboss_Master BB =>
            ((SM_SunBoss__BASE)stateMachine).BB_Sunboss_Master;
    }







  
    public class STATE_PATROL : SunBossState
    {
        // Randomly pick a path to the player position within min and max radius
        // Each time PATROL enters SCAN, The min and max Patrol Radius gets smaller to the Actual player position by SearchRad_MULT
        // SearchRad_MULT gets reset once enter CHASE 

        // SeePlayer -> STATE_CHASE
        // Still walking && SeeNothing -> Stay in STATE_PATROL
        // Already Reached patrol destination && SeeNothing -> STATE_SCAN

        private Vector3 patrolTarget;

        public override void OnEnter()
        {
            RandomOffset();
        }

        public override void OnTick()
        {
            var bb = BB;
            var sense = bb.BB_SunbossCTX_Sense.ConeBox;

            PickNewPatrolPoint();

            // --- TRANSITION: SEE PLAYER ---
            if (sense.ReachedTarget)
            {
                stateMachine.SetState<STATE_CHASE>();
                return;
            }

            // --- MOVE ---
            bb.BB_SunbossCTX_Move.ACT_SunBoss_Navagent
                .GoToThisFrame(patrolTarget);

            // --- REACHED DESTINATION ---
            if (Vector3.Distance(bb.transform.position, patrolTarget) < 1.5f)
            {
                stateMachine.SetState<STATE_SCAN>();
            }
        }

        void PickNewPatrolPoint()
        {
            var brain = BB.BB_SunbossCTX_Brain;

            Vector3 PointOfSpeculation = Vector3.Lerp(brain.ActualPlayerPosition_NavmeshProjected, brain.PlayerPosition_LastestKnown, brain.UncertainInPrediction);
            patrolTarget = PointOfSpeculation +
                           new Vector3(offset.x, 0, offset.y);

            bool Success = false;
            Success = B_NavMeshUtil.ProjectOnConnected(BB.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.agent,patrolTarget, out Vector3 OUT );
            patrolTarget = OUT;
            //brain.PatrolPointOBJ.transform.position = patrolTarget;
        }
        Vector2 offset;
        void RandomOffset()
        {
            var brain = BB.BB_SunbossCTX_Brain;

            float baseMin = brain.MinPredictionError_Position;
            float baseMax = brain.MaxPredictionError_Position;

            float radius = Random.Range(baseMin, baseMax) * brain.UncertainInPrediction;

            offset = Random.insideUnitCircle * radius;

        }
    }

    public class STATE_CHASE : SunBossState
    {
        // Chase the player while keep tracking last seen position
        // SeePlayer -> STATE_CHASE
        // SeeNothing -> STATE_SEEK

        float GoSeekTimer = 0;

        public override void OnEnter()
        {
            BB.BB_SunbossCTX_Brain.UncertainInPrediction = 1;
            BB.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.agent.updateRotation = false;

        }   
        public override void OnTick()
        {
            var bb = BB;
            var sense = bb.BB_SunbossCTX_Sense.ConeBox;


            //GOTO TARGET
            bb.BB_SunbossCTX_Move.ACT_SunBoss_Navagent
                    .GoToThisFrame(
                        bb.BB_SunbossCTX_Brain.ActualPlayerPosition_NavmeshProjected);


            if (sense.ReachedTarget)
            {
                GoSeekTimer = BB.BB_SunbossCTX_Brain.ForgetTime;


                //FACE TARGET
                Vector3 flatTarget = new Vector3(
                    bb.BB_SunbossCTX_Brain.ActualPlayerPosition_NavmeshProjected.x,
                    bb.BB_SunbossCTX_Body.WholeBody.transform.position.y,
                    bb.BB_SunbossCTX_Brain.ActualPlayerPosition_NavmeshProjected.z
                );
                bb.BB_SunbossCTX_Body.WholeBody.LookAt(flatTarget);
            }
            else
            {
                // Lost player → SEEK
                GoSeekTimer -= Time.deltaTime;
                if (GoSeekTimer<0){
                    stateMachine.SetState<STATE_SEEK>();
                }
                
            }
        }

        public override void OnExit()
        {
            BB.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.agent.updateRotation = true;
        }
    }




    public class STATE_SEEK : SunBossState
    {
        // Go to the last seen player location
        // SeePlayer -> STATE_CHASE
        // SeeNothing -> STATE_SCAN
        public override void OnTick()
        {
            var bb = BB;
            var sense = bb.BB_SunbossCTX_Sense.ConeBox;

            // Found player again
            if (sense.ReachedTarget)
            {
                stateMachine.SetState<STATE_CHASE>();
                return;
            }

            // Move to last known
            var target = bb.BB_SunbossCTX_Brain.PlayerPosition_LastestKnown;

            bb.BB_SunbossCTX_Move.ACT_SunBoss_Navagent
                .GoToThisFrame(target);

            // Reached last known → SCAN
            if (Vector3.Distance(bb.transform.position, target) < 2f)
            {
                stateMachine.SetState<STATE_SCAN>();
            }
        }
    }



    public class STATE_SCAN : SunBossState
    {
        private float rotatedDegrees;
        bool Clockwise;

        public override void OnEnter()
        {
            rotatedDegrees = 0f;
            BB.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.agent.updateRotation = true;
        }

        public override void OnTick()
        {
            var sense = BB.BB_SunbossCTX_Sense.ConeBox;

            // Transition → CHASE
            if (sense.ReachedTarget)
            {
                stateMachine.SetState<STATE_CHASE>();
                return;
            }

            // --- ROTATE ---
            float delta = BB.BB_SunbossCTX_Brain.ScanSpeed * Time.deltaTime;


            bool goPATROL = false;
            if (Clockwise){
                if (BB.BB_SunbossCTX_Brain.Rotation) 
                    BB.BB_SunbossCTX_Body.WholeBody.transform.Rotate(Vector3.up, delta);
                rotatedDegrees += delta;
                goPATROL = (rotatedDegrees >= 360f);
            }
            else {
                if (BB.BB_SunbossCTX_Brain.Rotation)
                    BB.BB_SunbossCTX_Body.WholeBody.transform.Rotate(Vector3.up, -delta);
                rotatedDegrees -= delta;
                goPATROL = (rotatedDegrees <= -360f);
            }
            

            // --- FINISHED SCAN ---
            if (goPATROL)
            {
                stateMachine.SetState<STATE_PATROL>();
            }
        }

        public override void OnExit()
        {
            BB.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.agent.updateRotation = true;

            BB.BB_SunbossCTX_Brain.UncertainInPrediction -= BB.BB_SunbossCTX_Brain.UncertainInPrediction_Reduction;
            BB.BB_SunbossCTX_Brain.UncertainInPrediction = Mathf.Clamp(BB.BB_SunbossCTX_Brain.UncertainInPrediction, 0, 1);

            Clockwise = !Clockwise;
        }
    }









}
