using UnityEngine;
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
        }

        public override void Tick_Override()
        {
            BB_SunbossCTX_Master.BB_SunbossCTX_Debug.TextUI_State.text = GetCurrentState();

            BB_SunbossCTX_Master.BB_SunbossCTX_Sense.ConeBox.Ray.Target = BB_SunbossCTX_Master.BB_SunbossCTX_Brain.PlayerOBJ.transform;
            BB_SunbossCTX_Master.BB_SunbossCTX_Debug.TextUI_Sight.text =
                "Target Seen: " + BB_SunbossCTX_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget.ToString();
            if (BB_SunbossCTX_Master.BB_SunbossCTX_Sense.ConeBox.ReachedTarget)
            {
                BB_SunbossCTX_Master.BB_SunbossCTX_Brain.PlayerPosition_LastestKnown = BB_SunbossCTX_Master.BB_SunbossCTX_Brain.PlayerOBJ.transform.position;
            }
        }
    }

    public abstract class SunBossState : B_STATE
    {
        // QUICK CONSTRUCTOR
        protected BB_SunbossCTX_Master BB =>
            ((SM_SunBoss__BASE)stateMachine).BB_SunbossCTX_Master;
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
            PickNewPatrolPoint();
        }

        public override void OnTick()
        {
            var bb = BB;
            var sense = bb.BB_SunbossCTX_Sense.ConeBox;

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

            float baseMin = 2f;
            float baseMax = 5f;

            float radius = Random.Range(baseMin, baseMax);

            Vector2 offset = Random.insideUnitCircle * radius;


            Vector3 PointOfSpeculation = Vector3.Lerp(brain.PlayerOBJ.transform.position, brain.PlayerPosition_LastestKnown, brain.UncertainInPrediction);
            patrolTarget = PointOfSpeculation +
                           new Vector3(offset.x, 0, offset.y);
        }
    }

    public class STATE_CHASE : SunBossState
    {
        // Chase the player while keep tracking last seen position
        // SeePlayer -> STATE_CHASE
        // SeeNothing -> STATE_SEEK

        public override void OnEnter()
        {
            BB.BB_SunbossCTX_Brain.UncertainInPrediction = 1;
        }
        public override void OnTick()
        {
            var bb = BB;
            var sense = bb.BB_SunbossCTX_Sense.ConeBox;

            // Still see player → keep updating target
            if (sense.ReachedTarget)
            {
                bb.BB_SunbossCTX_Move.ACT_SunBoss_Navagent
                    .GoToThisFrame(
                        bb.BB_SunbossCTX_Brain.PlayerOBJ.transform.position);
            }
            else
            {
                // Lost player → SEEK
                stateMachine.SetState<STATE_SEEK>();
            }
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
        private float rotationSpeed = 180f; // degrees per second

        public override void OnEnter()
        {
            rotatedDegrees = 0f;
            BB.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.agent.updateRotation = true;
            BB.BB_SunbossCTX_Brain.UncertainInPrediction *= BB.BB_SunbossCTX_Brain.UncertainInPrediction_Reduction;
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
            float delta = rotationSpeed * Time.deltaTime;

            //BB.BB_SunbossCTX_Body.WholeBody.transform.Rotate(Vector3.up, delta);

            rotatedDegrees += delta;

            // --- FINISHED SCAN ---
            if (rotatedDegrees >= 360f)
            {
                stateMachine.SetState<STATE_PATROL>();
            }
        }

        public override void OnExit()
        {
            BB.BB_SunbossCTX_Move.ACT_SunBoss_Navagent.agent.updateRotation = true;
        }
    }









}
