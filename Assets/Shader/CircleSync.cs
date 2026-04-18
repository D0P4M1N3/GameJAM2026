using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSync : MonoBehaviour
{
    [SerializeField] private static int PosID = Shader.PropertyToID("_Offset");
    [SerializeField] private static int SizeID = Shader.PropertyToID("_Size");

    [SerializeField] private Material WallMaterial;
    [SerializeField] private Camera Camera;
    [SerializeField] private float RayLength = 3000f;
    [SerializeField] private Transform Target;
    [SerializeField] private LayerMask Mask;
    [SerializeField] private float Size = 2f;
    [SerializeField] private Vector4 Offset = Vector4.zero;

    void Update()
    {
        var dir = Camera.transform.position - Target.position;
        var ray = new Ray(Target.position, dir.normalized);

        if (Physics.Raycast(ray, RayLength, Mask))
            WallMaterial.SetFloat(SizeID, Size);
        else
            WallMaterial.SetFloat(SizeID, 0f);

        var view = Camera.WorldToViewportPoint(Target.position);
        //WallMaterial.SetVector(PosID, view);
        WallMaterial.SetVector(PosID, Offset);//keep this center of the screen
    }
}
