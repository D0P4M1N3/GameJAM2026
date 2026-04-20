using UnityEngine;

public class UiItemModeProxy : MonoBehaviour
{
    [SerializeField] private ItemWorldObject itemWorldObject;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        itemWorldObject?.HandleCollisionEnter2D(collision);
    }
}
