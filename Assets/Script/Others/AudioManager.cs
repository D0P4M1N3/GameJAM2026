using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Library")]
    public Sound[] sounds;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSourceA;
    [SerializeField] private AudioSource bgmSourceB;
    [SerializeField] private float crossfadeTime = 1.5f;

    private AudioSource activeBGM;
    private AudioSource idleBGM;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 10;
    private AudioSource[] sfxPool;
    private int sfxIndex = 0;

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

        // Dictionary setup
        soundDict = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            if (!soundDict.ContainsKey(s.name))
                soundDict.Add(s.name, s);
            else
                Debug.LogWarning("Duplicate sound: " + s.name);
        }

        SetupBGMSources();

        SetupSFXPool();
    }

    private void SetupBGMSources()
    {
        if (bgmSourceA == null)
            bgmSourceA = gameObject.AddComponent<AudioSource>();

        if (bgmSourceB == null)
            bgmSourceB = gameObject.AddComponent<AudioSource>();

        bgmSourceA.outputAudioMixerGroup = bgmGroup;
        bgmSourceB.outputAudioMixerGroup = bgmGroup;

        bgmSourceA.playOnAwake = false;
        bgmSourceB.playOnAwake = false;

        activeBGM = bgmSourceA;
        idleBGM = bgmSourceB;
    }

    private void SetupSFXPool()
    {
        sfxPool = new AudioSource[sfxPoolSize];

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            src.playOnAwake = false;

            sfxPool[i] = src;
        }
    }

    public void Play(string name)
    {
        if (!soundDict.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }

        if (s.type == SoundType.BGM)
        {
            PlayBGM(s);
        }
        else
        {
            PlaySFX(s);
        }
    }

    private void PlaySFX(Sound s)
    {
        AudioSource src = sfxPool[sfxIndex];

        src.pitch = s.pitch;
        src.PlayOneShot(s.clip, s.volume);

        sfxIndex = (sfxIndex + 1) % sfxPoolSize;
    }

    private void PlayBGM(Sound s)
    {
        if (activeBGM.clip == s.clip && activeBGM.isPlaying)
            return;

        idleBGM.clip = s.clip;
        idleBGM.volume = 0f;
        idleBGM.pitch = s.pitch;
        idleBGM.loop = s.loop;

        idleBGM.Play();

        StopAllCoroutines();
        StartCoroutine(CrossfadeBGM(s.volume));
    }

    private IEnumerator CrossfadeBGM(float targetVolume)
    {
        float time = 0f;

        while (time < crossfadeTime)
        {
            time += Time.deltaTime;
            float t = time / crossfadeTime;

            activeBGM.volume = Mathf.Lerp(targetVolume, 0f, t);
            idleBGM.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        activeBGM.Stop();

        AudioSource temp = activeBGM;
        activeBGM = idleBGM;
        idleBGM = temp;
    }

    public void SetBGMVolume(float value)
    {
        mixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
    }
}