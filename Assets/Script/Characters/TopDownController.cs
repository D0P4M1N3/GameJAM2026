using UnityEngine;

public class TopDownController : MonoBehaviour
{
    [Header("References")]
    public BB_Player_Master BB_Player_Master;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed => BB_Player_Master.CharacterStats.finalSpeed;

    private void Update()
    {
        Vector2 input = inputReader.MoveInput;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * input.y + camRight * input.x;

        move = Vector3.ClampMagnitude(move, 1f);

        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}