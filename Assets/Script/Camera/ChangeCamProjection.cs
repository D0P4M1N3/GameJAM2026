using UnityEngine;

public class ChangeCamProjection : MonoBehaviour
{
    public enum ProjectionType
    {
        Perspective,
        Orthographic
    }

    [SerializeField] private Camera cam;

    

    void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
    }

    public void SetProjectionType(ProjectionType type)
    {
        if (cam == null) return;

        switch (type)
        {
            case ProjectionType.Perspective:
                cam.orthographic = false;
                break;

            case ProjectionType.Orthographic:
                cam.orthographic = true;
                break;
        }
    }
}