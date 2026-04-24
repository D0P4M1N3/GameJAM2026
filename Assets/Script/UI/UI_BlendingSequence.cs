using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UI_BlendingSequence : MonoBehaviour
{
    [SerializeField] private GameManagerActions gameManagerActions;
    [SerializeField] private Animator blenderBladeAnimator;
    [SerializeField] private Animator JarManAnimator;
    [SerializeField] [Min(0.1f)] private float blendingTime = 2f;
    [SerializeField] [Min(0f)] private float loopFadeOutTime = 0.35f;
    [SerializeField] private AudioClip blenderLoopClip;
    [SerializeField] [Range(0f, 1f)] private float loopVolume = 1f;
    [SerializeField] [Range(0.1f, 3f)] private float loopPitch = 1f;
    [SerializeField] private string loadSceneName = "Level";
    [SerializeField] private string spinBoolName = "Spin";

    private AudioSource loopAudioSource;
    private Coroutine blendingRoutine;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnValidate()
    {
        EnsureReferences();
    }

    public void PlayBlendingSequence()
    {
        EnsureReferences();

        if (blendingRoutine != null)
        {
            return;
        }

        blendingRoutine = StartCoroutine(PlayBlendingSequenceRoutine());
    }

    private IEnumerator PlayBlendingSequenceRoutine()
    {
        JarManAnimator.CrossFadeInFixedTime("Blend", 0.32f);

        CharacterStats playerStats = DATA_Player.Instance != null ? DATA_Player.Instance.CharacterStats : null;

        SetBlenderSpin(true);
        PlayLoopSound();

        playerStats = DATA_Player.Instance != null ? DATA_Player.Instance.CharacterStats : null;
        float startHp = playerStats != null ? playerStats.HP : 0f;
        float targetHp = playerStats != null ? playerStats.finalMaxHP : 0f;
        float elapsed = 0f;

        while (elapsed < blendingTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = blendingTime <= 0f ? 1f : Mathf.Clamp01(elapsed / blendingTime);

            if (playerStats != null)
            {
                playerStats.HP = Mathf.Lerp(startHp, targetHp, progress);
            }

            yield return null;
        }

        if (playerStats != null)
        {
            playerStats.HP = targetHp;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyInventoryStatsAndDeleteInventoryItemsWithoutHealing();
        }
        else
        {
            gameManagerActions?.ApplyInventoryStatsAndDeleteInventoryItems();
        }

        StopLoopSound(loopFadeOutTime);
        if (loopFadeOutTime > 0f)
        {
            yield return new WaitForSecondsRealtime(loopFadeOutTime);
        }

        SetBlenderSpin(false);
        GameManager.Instance?.LoadScene(loadSceneName);

        blendingRoutine = null;
    }

    private void EnsureReferences()
    {
        if (loopAudioSource == null)
        {
            loopAudioSource = GetComponent<AudioSource>();
            loopAudioSource.playOnAwake = false;
            loopAudioSource.loop = true;
        }

        if (gameManagerActions == null)
        {
            gameManagerActions = GetComponent<GameManagerActions>();
        }

        if (blenderBladeAnimator == null)
        {
            blenderBladeAnimator = GetComponent<Animator>();
        }
    }

    private void PlayLoopSound()
    {
        if (loopAudioSource == null || blenderLoopClip == null)
        {
            return;
        }

        loopAudioSource.clip = blenderLoopClip;
        loopAudioSource.volume = loopVolume;
        loopAudioSource.pitch = loopPitch;
        loopAudioSource.loop = true;
        loopAudioSource.Play();
    }

    private void StopLoopSound(float fadeOutDuration)
    {
        if (loopAudioSource == null)
        {
            return;
        }

        if (fadeOutDuration <= 0f || !loopAudioSource.isPlaying)
        {
            loopAudioSource.Stop();
            loopAudioSource.clip = null;
            loopAudioSource.volume = loopVolume;
            return;
        }

        StartCoroutine(FadeOutLoopRoutine(fadeOutDuration));
    }

    private IEnumerator FadeOutLoopRoutine(float fadeOutDuration)
    {
        float startVolume = loopAudioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration && loopAudioSource != null && loopAudioSource.isPlaying)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeOutDuration);
            loopAudioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        if (loopAudioSource != null)
        {
            loopAudioSource.Stop();
            loopAudioSource.clip = null;
            loopAudioSource.volume = loopVolume;
        }
    }

    private void SetBlenderSpin(bool isSpinning)
    {
        if (blenderBladeAnimator == null || string.IsNullOrWhiteSpace(spinBoolName))
        {
            return;
        }

        blenderBladeAnimator.SetBool(spinBoolName, isSpinning);
    }
}
