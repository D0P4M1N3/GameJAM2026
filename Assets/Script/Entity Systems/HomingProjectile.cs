using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingProjectile : MonoBehaviour
{
    private Rigidbody rb;

    private Transform target;
    private float speed;
    private float turnSpeed;

    public void Initialize(Transform target, float speed, float turnSpeed)
    {
        this.target = target;
        this.speed = speed;
        this.turnSpeed = turnSpeed;

        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Direction to target
        Vector3 direction = (target.position - rb.position).normalized;

        // Desired rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smooth rotate using physics-safe method
        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);

        // Move forward
        rb.linearVelocity = rb.transform.forward * speed;
    }
}