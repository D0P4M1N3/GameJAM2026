using TMPro;
using UnityEngine;

public class FloatingValueText : MonoBehaviour
{
    [SerializeField] private Vector3 moveOffset = new(0f, 1.25f, 0f);
    [SerializeField] [Min(0.01f)] private float duration = 0.75f;
    [SerializeField] private bool faceMainCamera = true;

    private TMP_Text targetText;
    private Vector3 startPosition;
    private Color startColor;
    private float elapsed;
    private bool isPlaying;

    public void Play()
    {
        targetText = GetComponentInChildren<TMP_Text>(true);
        if (targetText == null)
        {
            Destroy(gameObject);
            return;
        }

        startPosition = transform.position;
        startColor = targetText.color;
        elapsed = 0f;
        isPlaying = true;
    }

    private void Awake()
    {
        targetText = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnEnable()
    {
        if (isPlaying)
        {
            return;
        }

        Play();
    }

    private void Update()
    {
        if (!isPlaying || targetText == null)
        {
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        transform.position = Vector3.Lerp(startPosition, startPosition + moveOffset, t);

        Color fadedColor = startColor;
        fadedColor.a = Mathf.Lerp(startColor.a, 0f, t);
        targetText.color = fadedColor;

        if (faceMainCamera && Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
