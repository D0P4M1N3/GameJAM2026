using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionCone : MonoBehaviour
{
    [SerializeField] private int rayCount = 50;
    [SerializeField] private float fov = 120f;
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private float innerRadius = 1f;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private float maxDistance = 20f;
    [Range(0f, 1f)]
    [SerializeField] private float fadePercent = 0.2f;

    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "VisionConeMesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void LateUpdate()
    {
        float startingAngle = fov / 2f;
        float angleStep = fov / rayCount;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();

        List<int> innerIndex = new List<int>();
        List<int> midIndex = new List<int>();
        List<int> outerIndex = new List<int>();

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -startingAngle + i * angleStep;

            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            Ray ray = new Ray(transform.position, dir);
            RaycastHit hit;

            float hitDist = viewDistance;

            if (Physics.Raycast(ray, out hit, maxDistance, obstacleMask))
            {
                hitDist = hit.distance;
            }

            float innerDist = Mathf.Max(innerRadius, Mathf.Min(hitDist, maxDistance));
            float midDist = innerDist + fadePercent * (maxDistance - innerDist);

            Vector3 inner = dir * innerDist;
            Vector3 mid = dir * midDist;
            Vector3 outer = dir * maxDistance;

            innerIndex.Add(vertices.Count);
            vertices.Add(inner);
            uv.Add(new Vector2(0f, 0f));

            midIndex.Add(vertices.Count);
            vertices.Add(mid);
            uv.Add(new Vector2(1f, 0f));

            outerIndex.Add(vertices.Count);
            vertices.Add(outer);
            uv.Add(new Vector2(1f, 0f));
        }

        for (int i = 0; i < rayCount; i++)
        {
            int i0 = innerIndex[i];
            int i1 = innerIndex[i + 1];
            int m0 = midIndex[i];
            int m1 = midIndex[i + 1];
            int o0 = outerIndex[i];
            int o1 = outerIndex[i + 1];

            triangles.Add(i0); triangles.Add(i1); triangles.Add(m0);
            triangles.Add(m0); triangles.Add(i1); triangles.Add(m1);

            triangles.Add(m0); triangles.Add(m1); triangles.Add(o0);
            triangles.Add(o0); triangles.Add(m1); triangles.Add(o1);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uv);
        mesh.RecalculateNormals();
    }
}