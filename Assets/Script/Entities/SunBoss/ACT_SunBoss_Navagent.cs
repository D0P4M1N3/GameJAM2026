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

    [SerializeField] private Animator animator;

    // NEW: cache last reachable point
    private Vector3 lastReachableTarget;
    private bool hasReachableTarget;

    void Awake()
    {
        EnsureAgentReference();
    }

    private void OnEnable()
    {
        EnsureAgentReference();
        TryEnsureAgentOnNavMesh();

        Pause3D.OnPauseChanged += HandlePause;

    }

    private void Update()
    {
        if (paused) return;

        ApplyFacingMovementConstraint();
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null) return;

        float speed = agent.velocity.magnitude;

        float normalizedSpeed = speed / agent.speed;

        animator.SetFloat("Speed", normalizedSpeed);
    }

    void LateUpdate()
    {
        if (paused)
        {
            followThisFrameActive = false;
            return;
        }

        if (!CanUseAgent())
        {
            followThisFrameActive = false;
            return;
        }

        if (!followThisFrameActive && agent.hasPath)
        {
            CancelPath();
        }

        followThisFrameActive = false;
    }

    public void GoToThisFrame(Vector3 worldPos)
    {
        if (paused) return;

        if (!CanUseAgent())
        {
            return;
        }

        if (intrREGIS.isInterrupted)
        {
            CancelPath();
        }
        else
        {
            followThisFrameActive = true;

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
        if (!CanUseAgent())
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }




    public bool HasPath()
    {
        if (!CanUseAgent())
        {
            return false;
        }

        return agent.hasPath;
    }

    public bool ReachedDestination(float threshold = 0.2f)
    {
        if (!CanUseAgent())
        {
            return false;
        }

        if (!agent.hasPath) return false;
        return agent.remainingDistance <= threshold;
    }



    // INTERNAL ////////////////////////////////////////////
    void STATS_UPDATE()
    {
        if (!CanUseAgent() || BB_Sunboss_Master == null)
        {
            return;
        }

        agent.speed = BB_Sunboss_Master.CharacterStats.finalSpeed;
        agent.angularSpeed = BB_Sunboss_Master.BB_SunbossCTX_Move.TurnSpeed;
    }

    private void ApplyFacingMovementConstraint()
    {
        if (!CanUseAgent() || BB_Sunboss_Master == null)
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

    private bool CanUseAgent()
    {
        if (agent == null || !agent.isActiveAndEnabled)
        {
            return false;
        }

        if (agent.isOnNavMesh)
        {
            return true;
        }

        return TryEnsureAgentOnNavMesh();
    }

    private void EnsureAgentReference()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private bool TryEnsureAgentOnNavMesh()
    {
        if (agent == null || !agent.isActiveAndEnabled)
        {
            return false;
        }

        if (agent.isOnNavMesh)
        {
            return true;
        }

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, agent.areaMask))
        {
            return false;
        }

        return agent.Warp(hit.position);
    }



    private bool paused;


    private void OnDisable()
    {
        Pause3D.OnPauseChanged -= HandlePause;
    }

    void HandlePause(bool isPaused)
    {
        paused = isPaused;

        if (!CanUseAgent()) return;

        if (paused)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            agent.updatePosition = false;
            agent.updateRotation = false;
        }
        else
        {
            agent.isStopped = false;

            agent.updatePosition = true;
            agent.updateRotation = true;
        }
    }
}
