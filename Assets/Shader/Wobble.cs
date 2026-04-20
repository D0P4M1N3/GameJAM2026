using UnityEngine;

public class Wobble : MonoBehaviour
{
    private Renderer rend;
    private Material mat;

    private Vector3 lastPos;
    private Vector3 velocity;

    private Quaternion lastRot;
    private Vector3 angularVelocity;

    [Header("Wobble Settings")]
    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    private float wobbleAmountX;
    private float wobbleAmountZ;
    private float wobbleToAddX;
    private float wobbleToAddZ;

    private float pulse;
    private float time = 0f;

    [Header("Speed Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float wobbleInfluence = 0.5f;

    private float smoothSpeed;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material; // ✅ cache once

        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        time += deltaTime;

        // ===== VELOCITY =====
        velocity = (transform.position - lastPos) / deltaTime;

        // ===== ANGULAR VELOCITY (SAFE) =====
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(lastRot);
        deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
        angularVelocity = axis * angle * Mathf.Deg2Rad / deltaTime;

        // ===== DECAY =====
        wobbleToAddX = Mathf.Lerp(wobbleToAddX, 0, deltaTime * Recovery);
        wobbleToAddZ = Mathf.Lerp(wobbleToAddZ, 0, deltaTime * Recovery);

        // ===== ADD MOVEMENT INFLUENCE =====
        wobbleToAddX += Mathf.Clamp(
            (velocity.x + angularVelocity.z * 0.2f) * MaxWobble,
            -MaxWobble,
            MaxWobble
        );

        wobbleToAddZ += Mathf.Clamp(
            (velocity.z + angularVelocity.x * 0.2f) * MaxWobble,
            -MaxWobble,
            MaxWobble
        );

        // ===== SINE WOBBLE =====
        pulse = 2 * Mathf.PI * WobbleSpeed;

        wobbleAmountX = wobbleToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleToAddZ * Mathf.Sin(pulse * time);

        // ===== SPEED (movement + wobble combined) =====
        float moveSpeed = velocity.magnitude;
        float normalizedMove = Mathf.Clamp01(moveSpeed / maxSpeed);

        float wobbleStrength = Mathf.Abs(wobbleAmountX) + Mathf.Abs(wobbleAmountZ);
        float normalizedWobble = Mathf.Clamp01(wobbleStrength / (MaxWobble * 2f));

        float targetSpeed = Mathf.Clamp01(
            normalizedMove + normalizedWobble * wobbleInfluence
        );

        // smooth it (important)
        smoothSpeed = Mathf.Lerp(smoothSpeed, targetSpeed, deltaTime * 8f);

        // ===== SEND TO SHADER =====
        mat.SetFloat("_WobbleX", wobbleAmountX);
        mat.SetFloat("_WobbleZ", wobbleAmountZ);
        mat.SetFloat("_Speed", smoothSpeed);

        // ===== STORE LAST =====
        lastPos = transform.position;
        lastRot = transform.rotation;
    }
}