using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Library")]
    public Sound[] sounds;

    [Header("SFX Source")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;

    [Header("Global Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Dictionary<string, Sound> soundDict;
    private Coroutine loopFadeCoroutine;
    private float currentLoopBaseVolume = 1f;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup dictionary
        soundDict = new Dictionary<string, Sound>();

        foreach (Sound s in sounds)
        {
            if (!soundDict.ContainsKey(s.name))
            {
                soundDict.Add(s.name, s);
            }
            else
            {
                Debug.LogWarning("Duplicate sound name: " + s.name);
            }
        }

        // Ensure AudioSource exists
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        if (loopSfxSource == null)
            loopSfxSource = gameObject.AddComponent<AudioSource>();

        loopSfxSource.loop = true;
        loopSfxSource.playOnAwake = false;
    }

    public void Play(string name)
    {
        if (!soundDict.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }

        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(s.clip, s.volume * sfxVolume);
    }


    public void StopAllSFX()
    {
        sfxSource.Stop();
        loopSfxSource.Stop();
    }

    public void PlayLoop(string name)
    {
        if (!soundDict.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }

        if (s.clip == null)
        {
            return;
        }

        if (loopFadeCoroutine != null)
        {
            StopCoroutine(loopFadeCoroutine);
            loopFadeCoroutine = null;
        }

        currentLoopBaseVolume = s.volume * sfxVolume;
        loopSfxSource.clip = s.clip;
        loopSfxSource.volume = currentLoopBaseVolume;
        loopSfxSource.pitch = s.pitch;
        loopSfxSource.loop = true;
        loopSfxSource.Play();
    }

    public void StopLoop()
    {
        if (loopSfxSource == null)
        {
            return;
        }

        if (loopFadeCoroutine != null)
        {
            StopCoroutine(loopFadeCoroutine);
            loopFadeCoroutine = null;
        }

        loopSfxSource.Stop();
        loopSfxSource.clip = null;
        loopSfxSource.volume = currentLoopBaseVolume;
    }

    public void StopLoopWithFade(float fadeOutDuration)
    {
        if (loopSfxSource == null)
        {
            return;
        }

        if (!loopSfxSource.isPlaying || fadeOutDuration <= 0f)
        {
            StopLoop();
            return;
        }

        if (loopFadeCoroutine != null)
        {
            StopCoroutine(loopFadeCoroutine);
        }

        loopFadeCoroutine = StartCoroutine(FadeOutLoopRoutine(fadeOutDuration));
    }

    private IEnumerator FadeOutLoopRoutine(float fadeOutDuration)
    {
        float startVolume = loopSfxSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration && loopSfxSource != null && loopSfxSource.isPlaying)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeOutDuration);
            loopSfxSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        StopLoop();
        loopFadeCoroutine = null;
    }
}
