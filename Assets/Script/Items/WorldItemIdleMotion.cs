using UnityEngine;

public class WorldItemIdleMotion : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobFrequency = 1.5f;
    [SerializeField] private float bobPhaseOffset;

    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;

    private void Awake()
    {
        CacheRestTransform();
    }

    private void OnEnable()
    {
        CacheRestTransform();
    }

    private void Update()
    {
        Vector3 axis = rotationAxis.sqrMagnitude > 0f ? rotationAxis.normalized : Vector3.up;
        transform.localRotation = initialLocalRotation * Quaternion.AngleAxis(rotationSpeed * Time.time, axis);

        Vector3 localPosition = initialLocalPosition;
        float bob = (Mathf.Sin((Time.time + bobPhaseOffset) * bobFrequency) + 1f) * 0.5f;
        localPosition.y += bob * bobAmplitude;
        transform.localPosition = localPosition;
    }

    private void CacheRestTransform()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }
}
