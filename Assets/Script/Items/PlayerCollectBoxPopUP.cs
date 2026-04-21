using UnityEngine;

public class PlayerCollectBoxPopUP : MonoBehaviour
{
    [SerializeField] private GameObject collectBoxPopUp;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            OpenPopUP();
            Debug.Log("Player entered the collect box area.");
        }
    }

    public void OpenPopUP()
    {
        collectBoxPopUp.SetActive(true);
        Time.timeScale = 0f;
    }


    public void ClosePopUp()
    {
        collectBoxPopUp.SetActive(false);
        Time.timeScale = 1f;
    }
}
