using UnityEngine;
using UnityEngine.AI;

public class ACT_SunBoss_Navagent : MonoBehaviour
{
    [Header("References")]
    public BB_Sunboss_Master BB_Sunboss_Master;
    public NavMeshAgent agent;

    [Header("Runtime (View Only)")]
    [SerializeField] private bool followThisFrameActive;
    [SerializeField] private Vector3 currentTarget;

    // NEW: cache last reachable point
    private Vector3 lastReachableTarget;
    private bool hasReachableTarget;

    void Awake()
    {
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

    //    if (TryGetReachablePoint(worldPos, out var reachable))
    //    {
    //        currentTarget = reachable;

    //        lastReachableTarget = reachable;
    //        hasReachableTarget = true;

    //        agent.isStopped = false;
    //        agent.SetDestination(reachable);
    //    }
    //    else if (hasReachableTarget)
    //    {
    //        // fallback
    //        currentTarget = lastReachableTarget;

    //        agent.isStopped = false;
    //        agent.SetDestination(lastReachableTarget);
    //    }
    //}

    public void GoToThisFrame(Vector3 worldPos)
    {
        followThisFrameActive = true;

        Vector3 finalTarget = lastReachableTarget;

        if (TryGetReachablePoint(worldPos, out var reachable))
        {
            finalTarget = reachable;
            lastReachableTarget = reachable;
            hasReachableTarget = true;
        }

        // Avoid redundant SetDestination calls
        if (!agent.hasPath || Vector3.Distance(currentTarget, finalTarget) > 0.05f)
        {
            STATS_UPDATE();

            currentTarget = finalTarget;
            agent.isStopped = false;
            agent.SetDestination(finalTarget);
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
        agent.speed = BB_Sunboss_Master.CharacterStats.Speed;
    }
    bool TryGetReachablePoint(Vector3 input, out Vector3 result, float sampleRadius = 3f)
    {
        NavMeshHit hit;

        // Step 1: project onto NavMesh
        if (!NavMesh.SamplePosition(input, out hit, sampleRadius, NavMesh.AllAreas))
        {
            result = default;
            return false;
        }

        // Step 2: validate full path
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(hit.position, path) &&
            path.status == NavMeshPathStatus.PathComplete)
        {
            result = hit.position;
            return true;
        }

        result = default;
        return false;
    }
}