using UnityEngine;

public enum LevelScatterZoneType
{
    Buildings,
    Items,
    Enemies,
}

public class LevelScatterZone : MonoBehaviour
{
    [SerializeField] private LevelScatterZoneType zoneType;
    [SerializeField] private Vector3 size = new(6f, 0f, 6f);
    [SerializeField] private Color buildingColor = new(0.25f, 0.8f, 1f, 0.45f);
    [SerializeField] private Color itemColor = new(0.45f, 1f, 0.3f, 0.45f);
    [SerializeField] private Color enemyColor = new(1f, 0.45f, 0.3f, 0.45f);

    public LevelScatterZoneType ZoneType => zoneType;
    public Vector3 Size => size;

    public Vector3 GetRandomPoint()
    {
        Vector3 halfSize = GetHalfSize();
        Vector3 localPoint = new(
            Random.Range(-halfSize.x, halfSize.x),
            Random.Range(-halfSize.y, halfSize.y),
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
        Gizmos.color = GetZoneColor();
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.matrix = previousMatrix;
    }

    private Vector3 GetHalfSize()
    {
        return new Vector3(
            Mathf.Abs(size.x) * 0.5f,
            Mathf.Abs(size.y) * 0.5f,
            Mathf.Abs(size.z) * 0.5f);
    }

    private Color GetZoneColor()
    {
        return zoneType switch
        {
            LevelScatterZoneType.Buildings => buildingColor,
            LevelScatterZoneType.Items => itemColor,
            _ => enemyColor,
        };
    }
}
