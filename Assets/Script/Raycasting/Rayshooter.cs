using UnityEngine;

[System.Serializable]
public class Rayshooter
{

    [Header("Configs")]
    public LayerMask LayerMask;
    public Transform Target;
    public Transform Shooter;
    public bool DrawGizmo;

    [Header("Event")]
    public bool IsObstructed { get; private set; }


    public void CheckObstruction()
    {
        if (Target == null || Shooter == null)
        {
            IsObstructed = true;
            return;
        }

        Vector3 direction = (Target.position - Shooter.position);
        float distance = direction.magnitude;

        direction.Normalize();

        Ray ray = new Ray(Shooter.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance, LayerMask))
        {
            // Hit something before reaching target → obstructed
            if (hit.transform != Target)
            {
                IsObstructed = true;
                return;
            }
        }

        IsObstructed = false;
    }

    /// <summary>
    /// Call this from a MonoBehaviour's OnDrawGizmos()
    /// </summary>
    public void DrawGizmos()
    {
        if (!DrawGizmo || Shooter == null || Target == null)
            return;

        Vector3 direction = (Target.position - Shooter.position);
        float distance = direction.magnitude;

        Gizmos.color = IsObstructed ? Color.red : Color.green;
        Gizmos.DrawLine(Shooter.position, Shooter.position + direction.normalized * distance);

        // Optional: draw hit point marker
        Gizmos.DrawSphere(Target.position, 0.1f);
    }
}