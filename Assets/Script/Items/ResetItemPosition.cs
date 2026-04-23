using UnityEngine;

public class ResetItemPosition : MonoBehaviour
{
    [SerializeField] private StashSpawner spawner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            //Debug.Log("Resetting item position");
            spawner.ResetStash();
        }
    }
}
