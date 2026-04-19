using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0); 

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;
    private float currentZoom = 10f;

    [Header("Rotation")]
    private float yaw = 0f;
    [SerializeField] private float pitch = 45f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float snapAngle = 45f;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 10f;

    private float targetYaw;

    private void Start()
    {
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        targetYaw = yaw;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();
        HandleRotation();
        UpdateCameraPosition();
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetYaw += mouseX * rotationSpeed * 100f * Time.deltaTime;
        }

        float snappedYaw = Mathf.Round(targetYaw / snapAngle) * snapAngle;
        yaw = Mathf.LerpAngle(yaw, snappedYaw, Time.deltaTime * 10f);
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

    private void UpdateCameraPosition()
    {
        Vector3 focusPoint = target.position + targetOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 direction = rotation * Vector3.forward;

        Vector3 desiredPosition = focusPoint - direction * currentZoom;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(focusPoint);
    }
}