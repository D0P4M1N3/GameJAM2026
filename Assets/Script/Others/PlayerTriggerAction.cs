using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggerAction : MonoBehaviour
{
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private UnityEvent onTriggered;

    private bool hasTriggered;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (debugLogs)
        {
            Debug.Log($"[PlayerTriggerAction] Trigger entered by '{other?.name ?? "null"}' on '{name}'.", this);
        }

        if (hasTriggered && triggerOnce)
        {
            if (debugLogs)
            {
                Debug.Log("[PlayerTriggerAction] Ignored because it already triggered once.", this);
            }
            return;
        }

        if (other == null)
        {
            if (debugLogs)
            {
                Debug.Log("[PlayerTriggerAction] Ignored because collider was null.", this);
            }
            return;
        }

        GameObject hitObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if (!hitObject.CompareTag("Player") && !other.CompareTag("Player"))
        {
            if (debugLogs)
            {
                Debug.Log($"[PlayerTriggerAction] Ignored because '{hitObject.name}' is not tagged Player.", this);
            }
            return;
        }

        TriggerAction();
    }

    public void TriggerAction()
    {
        if (hasTriggered && triggerOnce)
        {
            return;
        }

        hasTriggered = true;
        if (debugLogs)
        {
            Debug.Log($"[PlayerTriggerAction] Action invoked on '{name}'.", this);
        }
        onTriggered?.Invoke();
    }

    public void ResetTriggerState()
    {
        hasTriggered = false;
    }
}
