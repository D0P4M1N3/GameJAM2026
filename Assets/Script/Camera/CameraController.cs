using UnityEngine;

public enum CameraMode
{
    Target,
    TransformTo,
}

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform transformTarget;
    [SerializeField] private CameraMode mode = CameraMode.Target;
    public bool isFocusTarget = true;

    private Transform defaultTarget;
    private CameraMode defaultMode = CameraMode.Target;

    [Header("Offset")]
    [SerializeField] private Vector3 targetOffset = new(0f, 1.5f, 0f);

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;
    private float currentZoom = 10f;

    [Header("Rotation")]
    private float yaw;
    [SerializeField] private float pitch = 45f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float snapAngle = 45f;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 10f;

    private float targetYaw;

    private void Start()
    {
        if (target == null)
        {
            TopDownController playerController = FindAnyObjectByType<TopDownController>();
            if (playerController != null)
            {
                target = playerController.transform;
            }
        }

        defaultTarget = target;
        defaultMode = CameraMode.Target;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        targetYaw = yaw;
    }

    public void SetTargetMode()
    {
        if (defaultTarget == null)
        {
            TopDownController playerController = FindAnyObjectByType<TopDownController>();
            if (playerController != null)
            {
                defaultTarget = playerController.transform;
            }
        }

        target = defaultTarget;
        mode = defaultMode;
        isFocusTarget = target != null;
    }

    public void SetTransformToMode()
    {
        if (transformTarget == null)
        {
            return;
        }

        mode = CameraMode.TransformTo;
        isFocusTarget = true;
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
        isFocusTarget = target != null;
    }

    private void LateUpdate()
    {
        if (!isFocusTarget)
        {
            transform.position = Vector3.zero;
            return;
        }

        if (mode == CameraMode.Target)
        {
            if (target == null)
            {
                return;
            }

            HandleZoom();
            HandleRotation();
        }
        else if (transformTarget == null)
        {
            return;
        }

        UpdateCameraPosition();
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetYaw += mouseX * rotationSpeed * 100f * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            targetYaw -= snapAngle;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            targetYaw += snapAngle;
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
        if (mode == CameraMode.TransformTo)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                transformTarget.position,
                followSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                transformTarget.rotation,
                followSpeed * Time.deltaTime
            );
            return;
        }

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
