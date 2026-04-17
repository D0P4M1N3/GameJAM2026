using UnityEngine;

public class TopDownController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private void Update()
    {
        Vector2 input = inputReader.MoveInput;

        Vector3 move = new Vector3(input.x, 0f, input.y);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}