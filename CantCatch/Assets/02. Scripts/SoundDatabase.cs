using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BgmSoundEntry
{
    public EBgmSoundId soundId;
    public AudioClip clip;
    [Range(0f, 1f)] public float volumeScale = 1f;
}

[System.Serializable]
public class SfxSoundEntry
{
    public ESfxSoundId soundId;
    public AudioClip clip;
    [Range(0f, 1f)] public float volumeScale = 1f;
}

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/Sound Database")]
public class SoundDatabase : ScriptableObject
{
    [SerializeField] private List<BgmSoundEntry> bgmEntries = new();
    [SerializeField] private List<SfxSoundEntry> sfxEntries = new();

    public IReadOnlyList<BgmSoundEntry> BgmEntries => bgmEntries;
    public IReadOnlyList<SfxSoundEntry> SfxEntries => sfxEntries;
}
