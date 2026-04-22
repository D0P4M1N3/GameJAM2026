using UnityEngine;

public class ConeBox : MonoBehaviour
{
    [Header("Cone Settings")]
    public float Angle = 60f;
    public float Radius = 5f;
    public Vector3 PlaneNormal = Vector3.up; // defines the flat cone plane

    [Header("References")]
    public Rayshooter Ray;
    public InterruptionRegistry IntrREGIS;

    [Header("Debug")]
    public bool DrawGizmo = true;
    public int Segments = 30;

    [Header("Result")]
    public bool InsideCone;
    public bool ReachedTarget;

    private void Update()
    {
        if (IntrREGIS.isInterrupted) { return;  }

        if (Ray == null || Ray.Target == null || Ray.Shooter == null)
        {
            InsideCone = false;
            ReachedTarget = false;
            return;
        }

        InsideCone = CheckInsideCone();

        Ray.CheckObstruction();

        ReachedTarget = !Ray.IsObstructed && InsideCone;
    }

    bool CheckInsideCone()
    {
        Vector3 origin = Ray.Shooter.position;
        Vector3 toTarget = Ray.Target.position - origin;

        float distance = toTarget.magnitude;

        // Radius check
        if (distance > Radius)
            return false;

        // Flatten onto plane
        Vector3 forward = Vector3.ProjectOnPlane(Ray.Shooter.forward, PlaneNormal).normalized;
        Vector3 dirToTarget = Vector3.ProjectOnPlane(toTarget, PlaneNormal).normalized;

        // If projection collapses (edge case)
        if (forward == Vector3.zero || dirToTarget == Vector3.zero)
            return false;

        float dot = Vector3.Dot(forward, dirToTarget);
        float angleToTarget = Mathf.Acos(dot) * Mathf.Rad2Deg;

        return angleToTarget <= Angle * 0.5f;
    }

    private void OnDrawGizmos()
    {
        if (!DrawGizmo || Ray == null || Ray.Shooter == null)
            return;

        DrawFlatCone();

        if (Ray.DrawGizmo)
            Ray.DrawGizmos();
    }

    void DrawFlatCone()
    {
        Vector3 origin = Ray.Shooter.position;
        Vector3 forward = Vector3.ProjectOnPlane(Ray.Shooter.forward, PlaneNormal).normalized;

        if (forward == Vector3.zero)
            return;

        float halfAngle = Angle * 0.5f;

        Gizmos.color = Color.yellow;

        Quaternion startRot = Quaternion.AngleAxis(-halfAngle, PlaneNormal);
        Vector3 prevDir = startRot * forward;
        Vector3 prevPoint = origin + prevDir * Radius;

        for (int i = 1; i <= Segments; i++)
        {
            float step = Angle / Segments;
            Quaternion rot = Quaternion.AngleAxis(-halfAngle + step * i, PlaneNormal);
            Vector3 nextDir = rot * forward;
            Vector3 nextPoint = origin + nextDir * Radius;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        // Draw sides
        Vector3 left = Quaternion.AngleAxis(-halfAngle, PlaneNormal) * forward;
        Vector3 right = Quaternion.AngleAxis(halfAngle, PlaneNormal) * forward;

        Gizmos.DrawLine(origin, origin + left * Radius);
        Gizmos.DrawLine(origin, origin + right * Radius);

        // Color feedback
        if (Ray != null && Ray.Target != null)
        {
            Gizmos.color = ReachedTarget ? Color.green :
                           InsideCone ? Color.yellow : Color.red;

            Gizmos.DrawSphere(Ray.Target.position, 0.15f);
        }
    }
}