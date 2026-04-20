using UnityEngine;

public class WorldPickupModeProxy : MonoBehaviour
{
    [SerializeField] private GameplayItemPickup gameplayItemPickup;

    private void OnTriggerEnter(Collider other)
    {
        gameplayItemPickup?.HandleTriggerEnter(other);
    }
}
