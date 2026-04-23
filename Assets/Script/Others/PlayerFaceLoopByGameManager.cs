using System.Collections;
using UnityEngine;

public class PlayerFaceLoopByGameManager : MonoBehaviour
{
    [SerializeField] [Min(0.05f)] private float intervalSeconds = 1f;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool resetToDefaultOnDisable = true;

    private static readonly PlayerFaceVariant[] FaceLoop =
    {
        PlayerFaceVariant.A,
        PlayerFaceVariant.B,
        PlayerFaceVariant.D,
        PlayerFaceVariant.G,
    };

    private Coroutine loopRoutine;

    private void OnEnable()
    {
        if (!playOnEnable)
        {
            return;
        }

        StartLoop();
    }

    private void OnDisable()
    {
        StopLoop();

        if (resetToDefaultOnDisable)
        {
            GameManager.Instance?.ResetPlayerFace();
        }
    }

    public void StartLoop()
    {
        if (loopRoutine != null)
        {
            return;
        }

        loopRoutine = StartCoroutine(LoopFaces());
    }

    public void StopLoop()
    {
        if (loopRoutine == null)
        {
            return;
        }

        StopCoroutine(loopRoutine);
        loopRoutine = null;
    }

    private IEnumerator LoopFaces()
    {
        int faceIndex = 0;

        while (true)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPlayerFace(FaceLoop[faceIndex]);
                faceIndex = (faceIndex + 1) % FaceLoop.Length;
            }

            yield return new WaitForSeconds(intervalSeconds);
        }
    }
}
