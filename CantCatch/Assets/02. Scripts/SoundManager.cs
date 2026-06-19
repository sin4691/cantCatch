using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-900)]
public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    private class BgmSoundEntry
    {
        public EBgmSoundId soundId;
        public AudioClip clip;
    }

    [System.Serializable]
    private class SfxSoundEntry
    {
        public ESfxSoundId soundId;
        public AudioClip clip;
    }

    public static SoundManager Instance { get; private set; }

    [Header("Hierarchy")]
    [SerializeField] private Transform bgmRoot;
    [SerializeField] private Transform sfxRoot;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField, Min(1)] private int initialSfxSourceCount = 5;

    [Header("Sound Clips")]
    [SerializeField] private List<BgmSoundEntry> bgmClips = new();
    [SerializeField] private List<SfxSoundEntry> sfxClips = new();

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private bool keepAliveBetweenScenes = true;

    private readonly List<AudioSource> sfxSources = new();
    private readonly Dictionary<EBgmSoundId, AudioClip> bgmClipMap = new();
    private readonly Dictionary<ESfxSoundId, AudioClip> sfxClipMap = new();
    private bool isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (keepAliveBetweenScenes)
            DontDestroyOnLoad(gameObject);

        BuildClipMaps();
        isInitialized = InitializeSources();

        if (isInitialized)
            ApplyVolumes();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayBgm(EBgmSoundId soundId, bool loop = true)
    {
        if (!isInitialized)
            return;

        if (!TryGetClip(bgmClipMap, soundId, out AudioClip clip, "BGM"))
        {
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (!isInitialized)
            return;

        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlaySfx(ESfxSoundId soundId)
    {
        if (!isInitialized)
            return;

        if (!TryGetClip(sfxClipMap, soundId, out AudioClip clip, "SFX"))
        {
            return;
        }

        AudioSource availableSfxSource = GetAvailableSfxSource();
        availableSfxSource.PlayOneShot(clip);
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (isInitialized && bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        for (int i = 0; i < sfxSources.Count; i++)
        {
            if (sfxSources[i] != null)
                sfxSources[i].volume = sfxVolume;
        }
    }

    private bool InitializeSources()
    {
        sfxSources.Clear();
        ResolveHierarchyRoots();

        if (bgmRoot == null || sfxRoot == null)
        {
            Debug.LogError("SoundManager 하위에 BGM, SFX 오브젝트를 배치해주세요.", this);
            return false;
        }

        bgmSource = GetOrAddAudioSource(bgmRoot.gameObject);

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;

        CollectExistingSfxSources();

        for (int i = sfxSources.Count; i < initialSfxSourceCount; i++)
        {
            sfxSources.Add(CreateSfxSource($"SFX Source {i + 1}"));
        }

        return true;
    }

    private void ApplyVolumes()
    {
        bgmSource.volume = bgmVolume;
        SetSfxVolume(sfxVolume);
    }

    private void BuildClipMaps()
    {
        bgmClipMap.Clear();
        sfxClipMap.Clear();

        FillBgmClipMap();
        FillSfxClipMap();
    }

    private AudioSource GetAvailableSfxSource()
    {
        for (int i = 0; i < sfxSources.Count; i++)
        {
            AudioSource currentSource = sfxSources[i];
            if (currentSource != null && !currentSource.isPlaying)
                return currentSource;
        }

        AudioSource newSource = CreateSfxSource($"SFX Source {sfxSources.Count + 1}");
        sfxSources.Add(newSource);
        return newSource;
    }

    private AudioSource CreateSfxSource(string objectName)
    {
        GameObject sourceObject = new GameObject(objectName);
        sourceObject.transform.SetParent(sfxRoot, false);

        AudioSource newSource = sourceObject.AddComponent<AudioSource>();
        ConfigureSfxSource(newSource);
        return newSource;
    }

    private void ResolveHierarchyRoots()
    {
        if (bgmRoot == null)
            bgmRoot = transform.Find("BGM");

        if (sfxRoot == null)
            sfxRoot = transform.Find("SFX");
    }

    private void CollectExistingSfxSources()
    {
        AudioSource[] existingSources = sfxRoot.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < existingSources.Length; i++)
        {
            AudioSource currentSource = existingSources[i];
            ConfigureSfxSource(currentSource);
            sfxSources.Add(currentSource);
        }
    }

    private AudioSource GetOrAddAudioSource(GameObject targetObject)
    {
        AudioSource audioSource = targetObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = targetObject.AddComponent<AudioSource>();

        return audioSource;
    }

    private void FillBgmClipMap()
    {
        for (int i = 0; i < bgmClips.Count; i++)
        {
            BgmSoundEntry entry = bgmClips[i];
            if (entry == null || entry.soundId == EBgmSoundId.None || entry.clip == null)
                continue;

            if (bgmClipMap.ContainsKey(entry.soundId))
            {
                Debug.LogWarning($"BGM에 `{entry.soundId}`가 중복 등록되어 있습니다.", this);
                continue;
            }

            bgmClipMap.Add(entry.soundId, entry.clip);
        }
    }

    private void FillSfxClipMap()
    {
        for (int i = 0; i < sfxClips.Count; i++)
        {
            SfxSoundEntry entry = sfxClips[i];
            if (entry == null || entry.soundId == ESfxSoundId.None || entry.clip == null)
                continue;

            if (sfxClipMap.ContainsKey(entry.soundId))
            {
                Debug.LogWarning($"SFX에 `{entry.soundId}`가 중복 등록되어 있습니다.", this);
                continue;
            }

            sfxClipMap.Add(entry.soundId, entry.clip);
        }
    }

    private bool TryGetClip(
        Dictionary<EBgmSoundId, AudioClip> clipMap,
        EBgmSoundId soundId,
        out AudioClip clip,
        string categoryName)
    {
        clip = null;

        if (soundId == EBgmSoundId.None)
        {
            Debug.LogWarning($"{categoryName} 사운드 ID가 None입니다.", this);
            return false;
        }

        if (clipMap.TryGetValue(soundId, out clip))
            return true;

        Debug.LogWarning($"{categoryName}에 `{soundId}`가 등록되어 있지 않습니다.", this);
        return false;
    }

    private bool TryGetClip(
        Dictionary<ESfxSoundId, AudioClip> clipMap,
        ESfxSoundId soundId,
        out AudioClip clip,
        string categoryName)
    {
        clip = null;

        if (soundId == ESfxSoundId.None)
        {
            Debug.LogWarning($"{categoryName} 사운드 ID가 None입니다.", this);
            return false;
        }

        if (clipMap.TryGetValue(soundId, out clip))
            return true;

        Debug.LogWarning($"{categoryName}에 `{soundId}`가 등록되어 있지 않습니다.", this);
        return false;
    }

    private void ConfigureSfxSource(AudioSource targetSource)
    {
        if (targetSource == null)
            return;

        targetSource.playOnAwake = false;
        targetSource.loop = false;
        targetSource.spatialBlend = 0f;
        targetSource.volume = sfxVolume;
    }
}
