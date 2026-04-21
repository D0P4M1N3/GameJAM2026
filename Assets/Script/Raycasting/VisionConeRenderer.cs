using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionConeRenderer : MonoBehaviour
{
    [Header("Vision Settings")]
    //[SerializeField] private float radius = 5f;
    //[SerializeField] private float angle = 90f;
    [SerializeField] private int rayCount = 50;

    [Header("Obstacle")]
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private ConeBox coneBox;

    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "VisionCone2D";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void LateUpdate()
    {
        GenerateCone();
    }

    private void GenerateCone()
    {
        float startAngle = -coneBox.Angle / 2f;
        float angleStep = coneBox.Angle / rayCount;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // center
        vertices.Add(Vector3.zero);

        // ===== RAYS =====
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;

            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            float dist = coneBox.Radius;

            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, coneBox.Radius, obstacleMask))
            {
                dist = hit.distance;
            }

            Vector3 localDir = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
            Vector3 point = localDir * dist;
            vertices.Add(point);
        }

        // ===== TRIANGLES =====
        for (int i = 1; i <= rayCount; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        // ===== APPLY =====
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}