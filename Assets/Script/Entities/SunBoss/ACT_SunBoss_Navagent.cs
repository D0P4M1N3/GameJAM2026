using UnityEngine;
using UnityEngine.AI;

public class ACT_SunBoss_Navagent : MonoBehaviour
{
    [Header("References")]
    public BB_Sunboss_Master BB_Sunboss_Master;
    public NavMeshAgent agent;

    [Header("Runtime (View Only)")]
    public InterruptionRegistry intrREGIS;
    [SerializeField] private bool followThisFrameActive;
    [SerializeField] private Vector3 currentTarget;

    // NEW: cache last reachable point
    private Vector3 lastReachableTarget;
    private bool hasReachableTarget;

    void Awake()
    {
    }

    private void Update()
    {
        ApplyFacingMovementConstraint();
    }
    void LateUpdate()
    {
        // If GoToThisFrame wasn't called this frame → cancel
        if (!followThisFrameActive && agent.hasPath)
        {
            CancelPath();
        }

        // Reset per-frame flag
        followThisFrameActive = false;
    }



    // Path Commands ////////////////////////////////////////////

    //public void GoToOnce(Vector3 worldPos)
    //{
    //    STATS_UPDATE();
    //    currentTarget = worldPos;
    //    agent.isStopped = false;
    //    agent.SetDestination(worldPos);
    //}
    public void GoToThisFrame(Vector3 worldPos)
    {
        if (intrREGIS.isInterrupted)
        {
            CancelPath();
        }
        else
        {
            followThisFrameActive = true;

            // Avoid redundant SetDestination calls (small optimization)
            if (!agent.hasPath || Vector3.Distance(currentTarget, worldPos) > 0.05f)
            {
                STATS_UPDATE();
                currentTarget = worldPos;
                agent.isStopped = false;
                agent.SetDestination(worldPos);
            }
        }

        
    }

    public void CancelPath()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }



    // Utilities ////////////////////////////////////////////

    public bool HasPath()
    {
        return agent.hasPath;
    }

    public bool ReachedDestination(float threshold = 0.2f)
    {
        if (!agent.hasPath) return false;
        return agent.remainingDistance <= threshold;
    }



    // INTERNAL ////////////////////////////////////////////
    void STATS_UPDATE()
    {
        agent.speed = BB_Sunboss_Master.CharacterStats.finalSpeed;
        agent.angularSpeed = BB_Sunboss_Master.BB_SunbossCTX_Move.TurnSpeed;
    }

    private void ApplyFacingMovementConstraint()
    {
        if (agent == null || BB_Sunboss_Master == null)
        {
            return;
        }

        float baseSpeed = BB_Sunboss_Master.CharacterStats.finalSpeed;
        if (!agent.hasPath || agent.isStopped || agent.desiredVelocity.sqrMagnitude <= 0.0001f)
        {
            agent.speed = baseSpeed;
            return;
        }

        Transform facingTransform = BB_Sunboss_Master.BB_SunbossCTX_Body != null
            && BB_Sunboss_Master.BB_SunbossCTX_Body.WholeBody != null
            ? BB_Sunboss_Master.BB_SunbossCTX_Body.WholeBody
            : transform;

        Vector3 desiredDirection = agent.desiredVelocity;
        desiredDirection.y = 0f;
        if (desiredDirection.sqrMagnitude <= 0.0001f)
        {
            agent.speed = baseSpeed;
            return;
        }

        Vector3 forward = facingTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude <= 0.0001f)
        {
            agent.speed = baseSpeed;
            return;
        }

        float angleToPath = Vector3.Angle(forward.normalized, desiredDirection.normalized);
        float maxMoveAngle = Mathf.Max(0f, BB_Sunboss_Master.BB_SunbossCTX_Move.MaxMoveAngleFromFacing);
        agent.speed = angleToPath <= maxMoveAngle ? baseSpeed : 0f;
    }
}
