using UnityEngine;

public class UI_PlaySound : MonoBehaviour
{
    public void PlaySound(string sfx)
    {
        AudioManager.Instance.Play(sfx);
    }

    public void PlaySoundLoop(string sfx)
    {
        AudioManager.Instance?.PlayLoop(sfx);
    }

    public void StopSoundLoop()
    {
        AudioManager.Instance?.StopLoop();
    }

    public void StopSoundLoopWithFade(float fadeOutDuration)
    {
        AudioManager.Instance?.StopLoopWithFade(fadeOutDuration);
    }
}
