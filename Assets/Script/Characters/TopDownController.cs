using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class TopDownController : MonoBehaviour
{
    [Header("References")]
    public BB_Player_Master BB_Player_Master;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if (!IsValid()) return;

        Vector2 input = inputReader.MoveInput;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();
        Vector3 move = camForward * input.y + camRight * input.x;
        move = Vector3.ClampMagnitude(move, 1f);

        float moveSpeed = BB_Player_Master.CharacterStats.finalSpeed;
        Vector3 delta = move * moveSpeed * Time.fixedDeltaTime;

        MoveWithCollision(delta);

        RotateTowards(move);
    }

    private void MoveWithCollision(Vector3 delta)
    {
        if (delta.sqrMagnitude == 0f) return;

        float distance = delta.magnitude;
        Vector3 direction = delta.normalized;
        float height = capsule.height * 0.5f;
        float radius = capsule.radius * 0.95f;

        Vector3 point1 = transform.position + Vector3.up * radius;
        Vector3 point2 = transform.position + Vector3.up * (height * 2f - radius);

        if (Physics.CapsuleCast(point1, point2, radius, direction, out RaycastHit hit, distance, collisionMask))
        {
            float safeDistance = hit.distance - 0.01f;
            if (safeDistance > 0f)
            {
                rb.MovePosition(rb.position + direction * safeDistance);
            }
        }
        else
        {
            rb.MovePosition(rb.position + delta);
        }
    }
    private void RotateTowards(Vector3 move)
    {
        if (move.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(move);
        rb.rotation = Quaternion.Lerp(rb.rotation, targetRot, 10f * Time.fixedDeltaTime);
    }
    private bool IsValid()
    {
        return inputReader != null &&
               cameraTransform != null &&
               BB_Player_Master != null &&
               BB_Player_Master.CharacterStats != null;
    }
}