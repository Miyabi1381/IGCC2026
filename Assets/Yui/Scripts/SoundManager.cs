
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup seGroup;
    [SerializeField] private string exposedBgmVolumeParam = "BGMVolume";
    [SerializeField] private string exposedSeVolumeParam = "SEVolume";

    [Header("Sound Library (Optional)")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("BGM")]
    [SerializeField] private float defaultBgmFadeSeconds = 1.0f;

    [Header("SE Pool")]
    [SerializeField] private int sePoolSize = 12;
    [SerializeField] private bool allowPoolGrowth = true;

    private const string BGM_VOLUME_PREF_KEY = "SoundManager_BGMVolume";
    private const string SE_VOLUME_PREF_KEY = "SoundManager_SEVolume";
    private const float MIN_DECIBEL = -80f;
    private const float DEFAULT_VOLUME = 0.75f;

    private AudioSource bgmSourceA;
    private AudioSource bgmSourceB;
    private AudioSource activeBgmSource;
    private Coroutine bgmFadeRoutine;

    private readonly List<AudioSource> sePool = new List<AudioSource>();

    private float currentBgmVolume = DEFAULT_VOLUME;
    private float currentSeVolume = DEFAULT_VOLUME;
    private float bgmVolumeBeforeMute = DEFAULT_VOLUME;
    private float seVolumeBeforeMute = DEFAULT_VOLUME;
    private bool isBgmMuted = false;
    private bool isSeMuted = false;

    public bool IsBgmMuted => isBgmMuted;
    public bool IsSeMuted => isSeMuted;
    public bool IsBgmPlaying => activeBgmSource != null && activeBgmSource.isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupBgmSources();
        SetupSePool();
        LoadVolumeSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void SetupBgmSources()
    {
        bgmSourceA = CreateAudioSource("BGM_Source_A", bgmGroup, loop: true);
        bgmSourceB = CreateAudioSource("BGM_Source_B", bgmGroup, loop: true);
        activeBgmSource = bgmSourceA;
    }

    private void SetupSePool()
    {
        for (int i = 0; i < sePoolSize; i++)
        {
            AudioSource source = CreateAudioSource("SE_Source_" + i, seGroup, loop: false);
            sePool.Add(source);
        }
    }

    private AudioSource CreateAudioSource(string objectName, AudioMixerGroup group, bool loop)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        if (group != null)
        {
            source.outputAudioMixerGroup = group;
        }
        return source;
    }

    private void LoadVolumeSettings()
    {
        currentBgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_PREF_KEY, DEFAULT_VOLUME);
        currentSeVolume = PlayerPrefs.GetFloat(SE_VOLUME_PREF_KEY, DEFAULT_VOLUME);
        ApplyBgmVolumeToMixer(currentBgmVolume);
        ApplySeVolumeToMixer(currentSeVolume);
    }

    // ---------------------------------------------------------------
    // BGM
    // ---------------------------------------------------------------
    public void PlayBGM(AudioClip clip, bool loop = true, float fadeSeconds = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("SoundManager.PlayBGM: clip is null.");
            return;
        }

        if (activeBgmSource.clip == clip && activeBgmSource.isPlaying)
        {
            return;
        }

        float fade = fadeSeconds >= 0f ? fadeSeconds : defaultBgmFadeSeconds;

        if (bgmFadeRoutine != null)
        {
            StopCoroutine(bgmFadeRoutine);
        }
        bgmFadeRoutine = StartCoroutine(CrossfadeBgmRoutine(clip, loop, fade));
    }

    public void PlayBGM(string soundId, bool loop = true, float fadeSeconds = -1f)
    {
        if (soundLibrary == null)
        {
            Debug.LogWarning("SoundManager.PlayBGM: SoundLibrary is not assigned.");
            return;
        }

        if (soundLibrary.TryGet(soundId, out AudioClip clip, out _))
        {
            PlayBGM(clip, loop, fadeSeconds);
        }
        else
        {
            Debug.LogWarning("SoundManager.PlayBGM: id not found: " + soundId);
        }
    }

    public void StopBGM(float fadeSeconds = -1f)
    {
        float fade = fadeSeconds >= 0f ? fadeSeconds : defaultBgmFadeSeconds;

        if (bgmFadeRoutine != null)
        {
            StopCoroutine(bgmFadeRoutine);
        }
        bgmFadeRoutine = StartCoroutine(FadeOutAndStopRoutine(activeBgmSource, fade));
    }

    public void PauseBGM()
    {
        if (activeBgmSource != null && activeBgmSource.isPlaying)
        {
            activeBgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (activeBgmSource != null && activeBgmSource.clip != null && !activeBgmSource.isPlaying)
        {
            activeBgmSource.UnPause();
        }
    }

    private AudioSource GetInactiveBgmSource()
    {
        return activeBgmSource == bgmSourceA ? bgmSourceB : bgmSourceA;
    }

    private IEnumerator CrossfadeBgmRoutine(AudioClip newClip, bool loop, float fadeSeconds)
    {
        AudioSource fadeOutSource = activeBgmSource;
        AudioSource fadeInSource = GetInactiveBgmSource();

        bool wasFadeOutPlaying = fadeOutSource.isPlaying;
        float startVolumeOut = fadeOutSource.volume;

        fadeInSource.clip = newClip;
        fadeInSource.loop = loop;
        fadeInSource.volume = 0f;
        fadeInSource.Play();

        if (fadeSeconds <= 0f)
        {
            fadeInSource.volume = 1f;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeSeconds);
                fadeInSource.volume = t;
                if (wasFadeOutPlaying)
                {
                    fadeOutSource.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                }
                yield return null;
            }
            fadeInSource.volume = 1f;
        }

        if (wasFadeOutPlaying)
        {
            fadeOutSource.Stop();
            fadeOutSource.volume = 1f;
        }

        activeBgmSource = fadeInSource;
        bgmFadeRoutine = null;
    }

    private IEnumerator FadeOutAndStopRoutine(AudioSource source, float fadeSeconds)
    {
        if (source == null)
        {
            bgmFadeRoutine = null;
            yield break;
        }

        if (fadeSeconds <= 0f)
        {
            source.Stop();
            source.volume = 1f;
            bgmFadeRoutine = null;
            yield break;
        }

        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeSeconds);
            source.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        source.Stop();
        source.volume = 1f;
        bgmFadeRoutine = null;
    }

    // ---------------------------------------------------------------
    // SE
    // ---------------------------------------------------------------

    public void PlaySE(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("SoundManager.PlaySE: clip is null.");
            return;
        }

        AudioSource source = GetAvailableSeSource();
        if (source == null)
        {
            Debug.LogWarning("SoundManager.PlaySE: no available source in the SE pool.");
            return;
        }

        source.pitch = pitch;
        source.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void PlaySE(string soundId, float pitch = 1f)
    {
        if (soundLibrary == null)
        {
            Debug.LogWarning("SoundManager.PlaySE: SoundLibrary is not assigned.");
            return;
        }

        if (soundLibrary.TryGet(soundId, out AudioClip clip, out float volumeScale))
        {
            Debug.Log("SoundManager.PlaySE: id=" + soundId + " volumeScale=" + volumeScale);
            PlaySE(clip, volumeScale, pitch);
        }
        else
        {
            Debug.LogWarning("SoundManager.PlaySE: id not found: " + soundId);
        }
    }

  

    public void StopAllSE()
    {
        foreach (AudioSource source in sePool)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    private AudioSource GetAvailableSeSource()
    {
        foreach (AudioSource source in sePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        if (allowPoolGrowth)
        {
            AudioSource newSource = CreateAudioSource("SE_Source_" + sePool.Count, seGroup, loop: false);
            sePool.Add(newSource);
            return newSource;
        }

        return null;
    }

    // ---------------------------------------------------------------
    // Volume
    // ---------------------------------------------------------------

    public void SetBgmVolume(float linearVolume)
    {
        currentBgmVolume = Mathf.Clamp01(linearVolume);
        isBgmMuted = false;
        ApplyBgmVolumeToMixer(currentBgmVolume);
        PlayerPrefs.SetFloat(BGM_VOLUME_PREF_KEY, currentBgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSeVolume(float linearVolume)
    {
        currentSeVolume = Mathf.Clamp01(linearVolume);
        isSeMuted = false;
        ApplySeVolumeToMixer(currentSeVolume);
        PlayerPrefs.SetFloat(SE_VOLUME_PREF_KEY, currentSeVolume);
        PlayerPrefs.Save();
    }

    public float GetBgmVolume()
    {
        return currentBgmVolume;
    }

    public float GetSeVolume()
    {
        return currentSeVolume;
    }

    public void ToggleMuteBgm()
    {
        if (isBgmMuted)
        {
            isBgmMuted = false;
            currentBgmVolume = bgmVolumeBeforeMute;
            ApplyBgmVolumeToMixer(currentBgmVolume);
        }
        else
        {
            bgmVolumeBeforeMute = currentBgmVolume;
            isBgmMuted = true;
            ApplyBgmVolumeToMixer(0f);
        }
    }

    public void ToggleMuteSe()
    {
        if (isSeMuted)
        {
            isSeMuted = false;
            currentSeVolume = seVolumeBeforeMute;
            ApplySeVolumeToMixer(currentSeVolume);
        }
        else
        {
            seVolumeBeforeMute = currentSeVolume;
            isSeMuted = true;
            ApplySeVolumeToMixer(0f);
        }
    }

    private void ApplyBgmVolumeToMixer(float linearVolume)
    {
        if (audioMixer == null)
        {
            return;
        }

        float dB = LinearToDecibel(linearVolume);
        bool success = audioMixer.SetFloat(exposedBgmVolumeParam, dB);
        if (!success)
        {
            Debug.LogWarning("SoundManager: failed to set BGM volume. Check the exposed parameter name: " + exposedBgmVolumeParam);
        }
    }

    private void ApplySeVolumeToMixer(float linearVolume)
    {
        if (audioMixer == null)
        {
            return;
        }

        float dB = LinearToDecibel(linearVolume);
        bool success = audioMixer.SetFloat(exposedSeVolumeParam, dB);
        if (!success)
        {
            Debug.LogWarning("SoundManager: failed to set SE volume. Check the exposed parameter name: " + exposedSeVolumeParam);
        }
    }

    private static float LinearToDecibel(float linearVolume)
    {
        if (linearVolume <= 0.0001f)
        {
            return MIN_DECIBEL;
        }
        return Mathf.Log10(linearVolume) * 20f;
    }
}