using UnityEngine;
using UnityEngine.AI;

public class ActEnemy_Navagent : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;

    [Header("Runtime (View Only)")]
    [SerializeField] private bool followThisFrameActive;
    [SerializeField] private Vector3 currentTarget;

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
    public void GoToOnce(Vector3 worldPos)
    {
        currentTarget = worldPos;
        agent.isStopped = false;
        agent.SetDestination(worldPos);
    }
    public void GoToThisFrame(Vector3 worldPos)
    {
        followThisFrameActive = true;

        // Avoid redundant SetDestination calls (small optimization)
        if (!agent.hasPath || Vector3.Distance(currentTarget, worldPos) > 0.05f)
        {
            currentTarget = worldPos;
            agent.isStopped = false;
            agent.SetDestination(worldPos);
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
}