using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);
    [SerializeField] private float followSpeed = 5f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    private float currentZoom = 10f;

    private void Start()
    {
        currentZoom = offset.magnitude;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();
        FollowTarget();
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset.normalized * currentZoom;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(target);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            currentZoom -= scroll * zoomSpeed * 10f;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }
}