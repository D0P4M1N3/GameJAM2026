using UnityEngine;


using UnityEngine;

[System.Serializable]
public class ConeBoxData
{
    [Header("Cone Settings")]
    public float Angle = 60f;
    public float Radius = 5f;
    public Vector3 PlaneNormal = Vector3.up;

    [Header("References")]
    public Rayshooter Ray;

    [Header("Debug")]
    public bool DrawGizmo = true;
    public int Segments = 30;

    [Header("Result")]
    public bool InsideCone;
    public bool ReachedTarget;
}



public class ConeBox : MonoBehaviour
{
    public ConeBoxData Data = new ConeBoxData();

    private void Start()
    {
        if (!Data.Ray.Shooter)
        {
            Data.Ray.Shooter = transform;
        }
    }

    private void Update()
    {
        if (Data.Ray == null || Data.Ray.Target == null || Data.Ray.Shooter == null)
        {
            Data.InsideCone = false;
            Data.ReachedTarget = false;
            return;
        }

        Data.InsideCone = CheckInsideCone();

        Data.Ray.CheckObstruction();

        Data.ReachedTarget = !Data.Ray.IsObstructed && Data.InsideCone;
    }

    bool CheckInsideCone()
    {
        Vector3 origin = Data.Ray.Shooter.position;
        Vector3 toTarget = Data.Ray.Target.position - origin;

        float distance = toTarget.magnitude;

        if (distance > Data.Radius)
            return false;

        Vector3 forward = Vector3.ProjectOnPlane(Data.Ray.Shooter.forward, Data.PlaneNormal).normalized;
        Vector3 dirToTarget = Vector3.ProjectOnPlane(toTarget, Data.PlaneNormal).normalized;

        if (forward == Vector3.zero || dirToTarget == Vector3.zero)
            return false;

        float dot = Vector3.Dot(forward, dirToTarget);
        float angleToTarget = Mathf.Acos(dot) * Mathf.Rad2Deg;

        return angleToTarget <= Data.Angle * 0.5f;
    }

    private void OnDrawGizmos()
    {
        if (!Data.DrawGizmo || Data.Ray == null || Data.Ray.Shooter == null)
            return;

        DrawFlatCone();

        if (Data.Ray.DrawGizmo)
            Data.Ray.DrawGizmos();
    }

    void DrawFlatCone()
    {
        Vector3 origin = Data.Ray.Shooter.position;
        Vector3 forward = Vector3.ProjectOnPlane(Data.Ray.Shooter.forward, Data.PlaneNormal).normalized;

        if (forward == Vector3.zero)
            return;

        float halfAngle = Data.Angle * 0.5f;

        Gizmos.color = Color.yellow;

        Quaternion startRot = Quaternion.AngleAxis(-halfAngle, Data.PlaneNormal);
        Vector3 prevDir = startRot * forward;
        Vector3 prevPoint = origin + prevDir * Data.Radius;

        for (int i = 1; i <= Data.Segments; i++)
        {
            float step = Data.Angle / Data.Segments;
            Quaternion rot = Quaternion.AngleAxis(-halfAngle + step * i, Data.PlaneNormal);
            Vector3 nextDir = rot * forward;
            Vector3 nextPoint = origin + nextDir * Data.Radius;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Vector3 left = Quaternion.AngleAxis(-halfAngle, Data.PlaneNormal) * forward;
        Vector3 right = Quaternion.AngleAxis(halfAngle, Data.PlaneNormal) * forward;

        Gizmos.DrawLine(origin, origin + left * Data.Radius);
        Gizmos.DrawLine(origin, origin + right * Data.Radius);

        if (Data.Ray != null && Data.Ray.Target != null)
        {
            Gizmos.color = Data.ReachedTarget ? Color.green :
                           Data.InsideCone ? Color.yellow : Color.red;

            Gizmos.DrawSphere(Data.Ray.Target.position, 0.15f);
        }
    }
}