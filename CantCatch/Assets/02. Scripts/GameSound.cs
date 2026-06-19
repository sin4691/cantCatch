public static class GameSound
{
    public static bool HasManager => SoundManager.Instance != null;

    public static void PlayBgm(EBgmSoundId soundId, bool loop = true)
    {
        if (SoundManager.Instance == null)
            return;

        SoundManager.Instance.PlayBgm(soundId, loop);
    }

    public static void StopBgm()
    {
        if (SoundManager.Instance == null)
            return;

        SoundManager.Instance.StopBgm();
    }

    public static void PlaySfx(ESfxSoundId soundId)
    {
        if (SoundManager.Instance == null)
            return;

        SoundManager.Instance.PlaySfx(soundId);
    }

    public static void SetBgmVolume(float volume)
    {
        if (SoundManager.Instance == null)
            return;

        SoundManager.Instance.SetBgmVolume(volume);
    }

    public static void SetSfxVolume(float volume)
    {
        if (SoundManager.Instance == null)
            return;

        SoundManager.Instance.SetSfxVolume(volume);
    }

    public static bool TryGetBgmVolume(out float volume)
    {
        if (SoundManager.Instance != null)
        {
            volume = SoundManager.Instance.BgmVolume;
            return true;
        }

        volume = 0f;
        return false;
    }

    public static bool TryGetSfxVolume(out float volume)
    {
        if (SoundManager.Instance != null)
        {
            volume = SoundManager.Instance.SfxVolume;
            return true;
        }

        volume = 0f;
        return false;
    }
}
