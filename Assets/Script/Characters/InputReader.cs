using UnityEngine;

public class InputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    void Update()
    {
        MoveInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
    }
}