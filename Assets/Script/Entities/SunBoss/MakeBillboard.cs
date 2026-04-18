using UnityEngine;

public class MakeBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        transform.forward = Camera.main.transform.forward;
    }
}