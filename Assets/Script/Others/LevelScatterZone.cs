using UnityEngine;

public enum LevelScatterZoneType
{
    Buildings,
    Items,
    Enemies,
    PlayerStart,
}

public class LevelScatterZone : MonoBehaviour
{
    [SerializeField] private LevelScatterZoneType zoneType;
    [SerializeField] private Vector3 size = new(6f, 0f, 6f);
    [SerializeField] private Color gizmoColor = new(0.25f, 0.8f, 1f, 0.45f);

    public LevelScatterZoneType ZoneType => zoneType;
    public Vector3 Size => size;

    public Vector3 GetRandomPoint()
    {
        Vector3 halfSize = GetHalfSize();
        Vector3 localPoint = new(
            Random.Range(-halfSize.x, halfSize.x),
            0f,
            Random.Range(-halfSize.z, halfSize.z));

        return transform.TransformPoint(localPoint);
    }

    public Bounds GetWorldBounds()
    {
        Vector3 halfSize = GetHalfSize();
        Vector3[] corners =
        {
            transform.TransformPoint(new Vector3(-halfSize.x, -halfSize.y, -halfSize.z)),
            transform.TransformPoint(new Vector3(-halfSize.x, -halfSize.y, halfSize.z)),
            transform.TransformPoint(new Vector3(-halfSize.x, halfSize.y, -halfSize.z)),
            transform.TransformPoint(new Vector3(-halfSize.x, halfSize.y, halfSize.z)),
            transform.TransformPoint(new Vector3(halfSize.x, -halfSize.y, -halfSize.z)),
            transform.TransformPoint(new Vector3(halfSize.x, -halfSize.y, halfSize.z)),
            transform.TransformPoint(new Vector3(halfSize.x, halfSize.y, -halfSize.z)),
            transform.TransformPoint(new Vector3(halfSize.x, halfSize.y, halfSize.z)),
        };

        Bounds bounds = new(corners[0], Vector3.zero);
        for (int i = 1; i < corners.Length; i++)
        {
            bounds.Encapsulate(corners[i]);
        }

        return bounds;
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;
        Vector3 gizmoSize = GetGizmoSize();
        Gizmos.DrawWireCube(Vector3.zero, gizmoSize);
        Gizmos.DrawCube(Vector3.zero, gizmoSize);
        Gizmos.matrix = previousMatrix;
    }

    private void OnValidate()
    {
        size.y = 0f;
    }

    private Vector3 GetHalfSize()
    {
        return new Vector3(
            Mathf.Abs(size.x) * 0.5f,
            0f,
            Mathf.Abs(size.z) * 0.5f);
    }

    private Vector3 GetGizmoSize()
    {
        return new Vector3(
            Mathf.Abs(size.x),
            0.05f,
            Mathf.Abs(size.z));
    }
}
