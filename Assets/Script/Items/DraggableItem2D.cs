using UnityEngine;

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
    private Collider2D[] itemColliders;
    private Vector3 startPosition;
    private bool wasKinematic;
    private bool isDragging;
    private bool wasPointerOverItem;

    public bool IsDragging => isDragging;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody2D>();
        itemWorldObject = GetComponent<ItemWorldObject>();
        RefreshColliderCache();
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }
        EnsureJointReference();
        dragJoint.enabled = false;
    }

    private void OnValidate()
    {
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }
    }

    private void Reset()
    {
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }
    }

    public void HandlePointerDown()
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
            Debug.Log($"Grabbed item: {itemWorldObject.ItemData?.DisplayName ?? gameObject.name}", this);
            itemWorldObject.SuppressCollisionSound();
            itemWorldObject.PlayPickupSound();
            itemWorldObject.BeginDragHover();
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
        Camera activeCamera = ResolveCamera();
        bool isPointerOverItem = IsPointerOverItem(activeCamera);

        if (!isDragging && isPointerOverItem && !wasPointerOverItem)
        {
            itemWorldObject?.HandlePointerEnter();
        }
        else if (!isDragging && !isPointerOverItem && wasPointerOverItem)
        {
            itemWorldObject?.HandlePointerExit();
        }

        if (!isDragging && Input.GetMouseButtonDown(0) && isPointerOverItem)
        {
            HandlePointerDown();
            wasPointerOverItem = true;
            return;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            HandlePointerUp();
            return;
        }

        if (!isDragging)
        {
            wasPointerOverItem = isPointerOverItem;
            return;
        }

        if (activeCamera == null)
        {
            return;
        }

        dragJoint.target = GetPointerWorldPosition(activeCamera);
        wasPointerOverItem = isPointerOverItem;
    }

    public void HandlePointerUp()
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
        wasPointerOverItem = false;

        if (itemWorldObject != null)
        {
            itemWorldObject.EndDragHover();
            itemWorldObject.HandlePointerExit();
        }
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

    private bool IsPointerOverItem(Camera activeCamera)
    {
        if (activeCamera == null)
        {
            return false;
        }

        if (itemColliders == null || itemColliders.Length == 0)
        {
            RefreshColliderCache();
        }

        Vector3 worldPoint = activeCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point2D = new(worldPoint.x, worldPoint.y);

        for (int i = 0; i < itemColliders.Length; i++)
        {
            Collider2D itemCollider = itemColliders[i];
            if (itemCollider == null || !itemCollider.enabled || !itemCollider.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (itemCollider.OverlapPoint(point2D))
            {
                return true;
            }
        }

        return false;
    }

    private void RefreshColliderCache()
    {
        itemColliders = GetComponentsInChildren<Collider2D>(true);
    }
}
