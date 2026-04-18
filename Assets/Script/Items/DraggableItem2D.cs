using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableItem2D : MonoBehaviour
{
    [SerializeField] private Camera dragCamera;
    [SerializeField] private float dragDepth = 0f;
    [SerializeField] private float dragFrequency = 12f;
    [SerializeField] private float dragDampingRatio = 1f;
    [SerializeField] private float maxDragForce = 250f;

    private Rigidbody2D cachedRigidbody;
    private Camera cachedCamera;
    private TargetJoint2D dragJoint;
    private ItemWorldObject itemWorldObject;
    private Vector3 startPosition;
    private bool wasKinematic;
    private bool isDragging;

    public bool IsDragging => isDragging;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody2D>();
        itemWorldObject = GetComponent<ItemWorldObject>();
        EnsureJointReference();
        dragJoint.enabled = false;
    }

    private void OnMouseDown()
    {
        Camera activeCamera = ResolveCamera();
        if (activeCamera == null)
        {
            return;
        }

        startPosition = transform.position;
        wasKinematic = cachedRigidbody.bodyType == RigidbodyType2D.Kinematic;
        cachedRigidbody.linearVelocity = Vector2.zero;
        cachedRigidbody.angularVelocity = 0f;

        Vector3 pointerWorldPosition = GetPointerWorldPosition(activeCamera);
        cachedRigidbody.bodyType = RigidbodyType2D.Dynamic;

        if (itemWorldObject != null)
        {
            itemWorldObject.SuppressCollisionSound();
            itemWorldObject.PlayPickupSound();
        }

        EnsureJointReference();
        dragJoint.autoConfigureTarget = false;
        dragJoint.anchor = transform.InverseTransformPoint(pointerWorldPosition);
        dragJoint.target = pointerWorldPosition;
        dragJoint.frequency = dragFrequency;
        dragJoint.dampingRatio = dragDampingRatio;
        dragJoint.maxForce = maxDragForce;
        dragJoint.enabled = true;

        isDragging = true;
    }

    private void Update()
    {
        if (!isDragging)
        {
            return;
        }

        Camera activeCamera = ResolveCamera();
        if (activeCamera == null)
        {
            return;
        }

        dragJoint.target = GetPointerWorldPosition(activeCamera);
    }

    private void OnMouseUp()
    {
        StopDragging();
    }

    private void OnDisable()
    {
        StopDragging();
    }

    public void ReturnToStartPosition()
    {
        transform.position = startPosition;
    }

    private void StopDragging()
    {
        if (!isDragging || cachedRigidbody == null)
        {
            return;
        }

        if (dragJoint != null)
        {
            dragJoint.enabled = false;
        }

        cachedRigidbody.bodyType = wasKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        isDragging = false;
    }

    private void EnsureJointReference()
    {
        if (dragJoint == null)
        {
            dragJoint = GetComponent<TargetJoint2D>();
        }

        if (dragJoint == null)
        {
            dragJoint = gameObject.AddComponent<TargetJoint2D>();
        }
    }

    private Camera ResolveCamera()
    {
        if (dragCamera != null)
        {
            return dragCamera;
        }

        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        return cachedCamera;
    }

    private Vector3 GetPointerWorldPosition(Camera activeCamera)
    {
        Vector3 screenPoint = Input.mousePosition;
        float depth = Mathf.Abs(activeCamera.transform.position.z - dragDepth);
        screenPoint.z = depth;

        Vector3 worldPoint = activeCamera.ScreenToWorldPoint(screenPoint);
        worldPoint.z = dragDepth;
        return worldPoint;
    }
}
