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
        intrREGIS.__UpdateState();
        if (intrREGIS.OnInterruptEnter)
        {
            CancelPath();
        }
        else if (intrREGIS.OnInterruptExit)
        {

        }
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
    }
    
}