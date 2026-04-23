using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Library")]
    public Sound[] sounds;

    [Header("SFX Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Global Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Dictionary<string, Sound> soundDict;

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
    }

    public void Play(string name)
    {
        if (!soundDict.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }

        sfxSource.pitch = s.pitch;
        sfxSource.PlayOneShot(s.clip, s.volume * sfxVolume);
    }

    public void StopAllSFX()
    {
        sfxSource.Stop();
    }
}