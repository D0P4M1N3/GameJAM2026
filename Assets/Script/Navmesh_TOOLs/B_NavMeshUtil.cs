using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Guaranteed NavMesh projection utility.
/// Fast path via NavMesh.SamplePosition.
/// Fallback path via cached NavMesh triangulation (never fails).
/// </summary>
public static class B_NavMeshUtil
{

     
    // =============================
    // CONFIG
    // =============================

    /// <summary>
    /// Default search radius for SamplePosition.
    /// Keep small for speed; fallback guarantees result.
    /// </summary>
    private const float DEFAULT_SAMPLE_RADIUS = 2.0f;

    /// <summary>
    /// Area mask used for projection.
    /// </summary>
    private const int AREA_MASK = NavMesh.AllAreas;

    // =============================
    // CACHE
    // =============================

    private static bool _initialized;
    private static Vector3[] _navVertices;
    private static int _navVertexCount;

    // =============================
    // PUBLIC API
    // =============================

    /// <summary>
    /// Projects a world-space point onto the NavMesh.
    /// Always returns a valid position if a NavMesh exists.
    /// Never returns null.
    /// </summary>
    public static Vector3 Project(Vector3 worldPosition)
    {
        // Fast path
        if (NavMesh.SamplePosition(
                worldPosition,
                out NavMeshHit hit,
                DEFAULT_SAMPLE_RADIUS,
                AREA_MASK))
        {
            return hit.position;
        }

        // Guaranteed fallback
        EnsureCache();
        return FindClosestVertex(worldPosition);
    }






    /// <summary>
    /// Projects a world-space point onto the NavMesh,
    /// but guarantees the result is reachable by the given agent
    /// (i.e., same connected NavMesh island).
    /// </summary>
    public static bool ProjectOnConnected(
        NavMeshAgent agent,
        Vector3 worldPosition,
        out Vector3 result)
    {
        result = agent.transform.position;

        if (!agent.isOnNavMesh)
            return false;

        const float searchRadius = 2.0f;
        const int maxAttempts = 6;

        // ----------------------------------------
        // 1. Try fast sample first
        // ----------------------------------------
        if (NavMesh.SamplePosition(worldPosition, out NavMeshHit hit, searchRadius, agent.areaMask))
        {
            if (IsReachable(agent, hit.position))
            {
                result = hit.position;
                return true;
            }
        }

        // ----------------------------------------
        // 2. Expand search radius (progressive sampling)
        // ----------------------------------------
        for (int i = 1; i <= maxAttempts; i++)
        {
            float radius = searchRadius * (i + 1);

            if (NavMesh.SamplePosition(worldPosition, out NavMeshHit h, radius, agent.areaMask))
            {
                if (IsReachable(agent, h.position))
                {
                    result = h.position;
                    return true;
                }
            }
        }

        // ----------------------------------------
        // 3. Fallback: scan cached vertices
        // ----------------------------------------
        EnsureCache();

        float bestDist = float.MaxValue;
        Vector3 best = agent.transform.position;
        bool found = false;

        for (int i = 0; i < _navVertexCount; i++)
        {
            Vector3 v = _navVertices[i];

            float d = Vector3.SqrMagnitude(v - worldPosition);
            if (d >= bestDist)
                continue;

            if (!IsReachable(agent, v))
                continue;

            bestDist = d;
            best = v;
            found = true;
        }

        if (found)
        {
            result = best;
            return true;
        }

        // ----------------------------------------
        // 4. Absolute fallback (stay in place)
        // ----------------------------------------
        result = agent.transform.position;
        return false;
    }
                private static bool IsReachable(NavMeshAgent agent, Vector3 target)
                {
                    NavMeshPath path = new NavMeshPath();

                    if (!agent.CalculatePath(target, path))
                        return false;

                    return path.status == NavMeshPathStatus.PathComplete;
                }











    /// <summary>
    /// Force rebuild NavMesh cache.
    /// Call if NavMesh is rebuilt at runtime.
    /// </summary>
    public static void RebuildCache()
    {
        _initialized = false;
        EnsureCache();
    }

    // =============================
    // INTERNAL
    // =============================

    private static void EnsureCache()
    {
        if (_initialized)
            return;

        var triangulation = NavMesh.CalculateTriangulation();
        _navVertices = triangulation.vertices;
        _navVertexCount = _navVertices.Length;

        if (_navVertexCount == 0)
        {
            Debug.LogError(
                "[B_NavMeshUtil] NavMesh triangulation returned no vertices. " +
                "Ensure a NavMesh is baked.");
        }

        _initialized = true;
    }

    private static Vector3 FindClosestVertex(Vector3 position)
    {
        float bestDist = float.MaxValue;
        Vector3 best = position;

        // Linear scan — safe, deterministic
        for (int i = 0; i < _navVertexCount; i++)
        {
            Vector3 v = _navVertices[i];
            float d = Vector3.Distance(v,position);

            if (d < bestDist)
            {
                bestDist = d;
                best = v;
            }
        }

        return best;
    }












    public static class PathSpacing
    {
        public static bool FindPathShortest_TowardsTarget_Spacing(
    NavMeshAgent agent,
    Vector3 targetPos,
    float distToTargetNear,
    out NavMeshPath resultPath,
    out bool isInNearRange)
        {
            resultPath = new NavMeshPath();
            isInNearRange = false;

            // Reverse path: target → agent
            NavMeshPath reversePath = new NavMeshPath();
            if (!NavMesh.CalculatePath(
                    targetPos,
                    agent.transform.position,
                    NavMesh.AllAreas,
                    reversePath))
                return false;

            if (reversePath.status != NavMeshPathStatus.PathComplete)
                return false;

            float pathLength = GetPathLength(reversePath);
            isInNearRange = pathLength <= distToTargetNear;

            const float epsilon = 0.1f;

            // ---------- CASE 1: TOO FAR → MOVE IN ----------
            if (!isInNearRange)
            {
                float desiredDistance =
                    Mathf.Max(distToTargetNear - epsilon, 0f);

                Vector3 moveInPoint =
                    GetPointAlongPath(reversePath, desiredDistance);

                return agent.CalculatePath(moveInPoint, resultPath)
                       && resultPath.status == NavMeshPathStatus.PathComplete;
            }

            // ---------- CASE 2: TOO CLOSE → MOVE OUT ----------
            // Direction away from target (world hint)
            Vector3 awayDir =
                (agent.transform.position - targetPos).normalized;

            float pushOutDistance = distToTargetNear + epsilon;

            Vector3 escapeProbe =
                agent.transform.position + awayDir * pushOutDistance;

            // Snap to NavMesh
            if (!NavMesh.SamplePosition(
                    escapeProbe,
                    out NavMeshHit hit,
                    pushOutDistance,
                    NavMesh.AllAreas))
                return false;

            return agent.CalculatePath(hit.position, resultPath)
                   && resultPath.status == NavMeshPathStatus.PathComplete;
        }




        /////////////////////////////////////////////////////////
        static float GetPathLength(NavMeshPath path)
        {
            float length = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
                length += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            return length;
        }
        static Vector3 GetPointAlongPath(NavMeshPath path, float distance)
        {
            float remaining = distance;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Vector3 a = path.corners[i];
                Vector3 b = path.corners[i + 1];
                float segmentLength = Vector3.Distance(a, b);

                if (remaining <= segmentLength)
                {
                    float t = remaining / segmentLength;
                    return Vector3.Lerp(a, b, t);
                }

                remaining -= segmentLength;
            }

            return path.corners[^1];
        }















        public static bool FindPathComplex_AwayfromTarget_Spacing(
    NavMeshAgent agent,
    Vector3 targetToMoveAwayFrom,
    float distToTargetFar,
    out NavMeshPath resultPath,
    out bool isOutRange,
    int samples = 32,
    int areaMask = NavMesh.AllAreas)
        {
            resultPath = new NavMeshPath();
            isOutRange = false;

            Vector3 agentPos = agent.transform.position;

            // --- 1. Check NavMesh distance (target → agent) ---
            NavMeshPath reverse = new NavMeshPath();
            if (!NavMesh.CalculatePath(targetToMoveAwayFrom, agentPos, areaMask, reverse) ||
                reverse.status != NavMeshPathStatus.PathComplete)
                return false;

            float navDist = GetPathLength(reverse);

            // --- 2. OUTSIDE FAR → move back IN using shortest solver ---
            if (navDist > distToTargetFar)
            {
                isOutRange = true;
                return FindPathShortest_TowardsTarget_Spacing(
                    agent,
                    targetToMoveAwayFrom,
                    distToTargetFar,
                    out resultPath,
                    out _);
            }

            // --- 3. INSIDE FAR → find MOST COMPLEX escape destination ---
            isOutRange = false;

            Vector3 awayDir = (agentPos - targetToMoveAwayFrom).normalized;
            if (awayDir.sqrMagnitude < 0.0001f)
                awayDir = Random.onUnitSphere;

            float bestScore = float.MinValue;
            NavMeshPath bestPath = null;

            for (int i = 0; i < samples; i++)
            {
                Vector3 dir = Vector3.Slerp(
                    awayDir,
                    Random.onUnitSphere,
                    0.35f);

                Vector3 candidate =
                    agentPos + dir * Random.Range(distToTargetFar * 0.5f, distToTargetFar);

                candidate.y = agentPos.y;

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, areaMask))
                    continue;

                NavMeshPath path = new NavMeshPath();
                if (!agent.CalculatePath(hit.position, path))
                    continue;

                if (path.status != NavMeshPathStatus.PathComplete)
                    continue;

                float score = ComputeTurnScore(path);

                // Distance reward: prefer actually moving away
                score += Vector3.Distance(hit.position, targetToMoveAwayFrom) * 0.25f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPath = path;
                }
            }

            if (bestPath == null)
                return false;

            resultPath = bestPath;
            return true;
        }

        /////////////////////////////////////////////////////////
        static float ComputeTurnScore(NavMeshPath path)
        {
            if (path.corners.Length < 3)
                return 0f;

            float score = 0f;

            for (int i = 1; i < path.corners.Length - 1; i++)
            {
                Vector3 a = (path.corners[i] - path.corners[i - 1]).normalized; 
                Vector3 b = (path.corners[i + 1] - path.corners[i]).normalized;

                float angle = Vector3.Angle(a, b);
                score += angle * angle; // emphasize sharp turns
            }

            return score;
        }




    }

































public static class PathAround
    {
        /// <summary>
        /// Generates a path that makes the agent orbit around TargetWorldPos at a given radius.
        /// SidewaySpeed > 0  = Clockwise
        /// SidewaySpeed < 0  = Counter-Clockwise
        /// The magnitude of SidewaySpeed controls how far along the circle the next point is.
        /// </summary>
        public static NavMeshPath FindPathAround(
            Vector3 TargetWorldPos,
            NavMeshAgent agent,
            float Radius,
            bool clockwise)
        {
            NavMeshPath path = new NavMeshPath();

            if (!agent.isOnNavMesh)
                return path;

            Vector3 agentPos = agent.transform.position;

            // Radial vector (target → agent)
            Vector3 radial = agentPos - TargetWorldPos;
            radial.y = 0f;

            if (radial.sqrMagnitude < 0.001f)
                radial = agent.transform.forward;

            radial.Normalize();

            // Exact orbit point at radius
            Vector3 orbitPoint = TargetWorldPos + radial * Radius;

            // Tangent direction
            Vector3 tangent = Vector3.Cross(Vector3.up, radial).normalized;

            if (!clockwise)
                tangent = -tangent;

            // Small step forward along tangent
            float angleStep = 20f * Mathf.Deg2Rad; // 20 degrees ahead
            float step = angleStep;

            Vector3 nextPoint = orbitPoint + tangent * step;

            // Reproject to exact radius
            Vector3 correctedRadial = nextPoint - TargetWorldPos;
            correctedRadial.y = 0f;
            correctedRadial.Normalize();
            nextPoint = TargetWorldPos + correctedRadial * Radius;

            // Project to NavMesh
            if (NavMesh.SamplePosition(nextPoint, out NavMeshHit hit, 2f, agent.areaMask))
            {
                NavMesh.CalculatePath(agentPos, hit.position, agent.areaMask, path);
            }
            else
            {
                NavMesh.CalculatePath(agentPos, agentPos, agent.areaMask, path);
            }

            return path;
        }
    }

}














