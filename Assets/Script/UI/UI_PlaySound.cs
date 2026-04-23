using UnityEngine;

public class UI_PlaySound : MonoBehaviour
{


    public void PlaySound(string sfx)
    {
        AudioManager.Instance.Play(sfx);
    }
}
