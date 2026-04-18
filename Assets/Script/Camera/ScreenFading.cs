using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFading : MonoBehaviour
{
    public static ScreenFading Instance;

    [Header("UI")]
    [SerializeField] private Image blackScreen;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Shader Settings")]
    [SerializeField] private string valueProperty = "_Value";
    [SerializeField] private float blackValue = 20f;
    [SerializeField] private float clearValue = 0f;

    private Material runtimeMaterial;
    private Coroutine currentFade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (blackScreen != null)
        {
            runtimeMaterial = Instantiate(blackScreen.material);
            blackScreen.material = runtimeMaterial;
            runtimeMaterial.SetFloat(valueProperty, blackValue);
        }
    }

    private void Start()
    {
        StartCoroutine(FadeFromBlack());
    }

    public IEnumerator FadeToBlack()
    {
        yield return StartCoroutine(FadeRoutine(blackValue));
    }

    public IEnumerator FadeFromBlack()
    {
        yield return StartCoroutine(FadeRoutine(clearValue));
    }

    private IEnumerator FadeRoutine(float targetValue)
    {
        if (runtimeMaterial == null) yield break;

        float startValue = runtimeMaterial.GetFloat(valueProperty);
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float blend = t / fadeDuration;

            float value = Mathf.Lerp(startValue, targetValue, blend);
            runtimeMaterial.SetFloat(valueProperty, value);

            yield return null;
        }

        runtimeMaterial.SetFloat(valueProperty, targetValue);
        currentFade = null;
    }
}