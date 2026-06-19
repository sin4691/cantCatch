using UnityEngine;
using UnityEngine.UI;

public class UI_TitleMenu : MonoBehaviour
{
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        ResetPanel();
        InitializeVolumeSliders();
    }

    private void OnDestroy()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnChangedBgmVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnChangedSfxVolume);
    }

    private void ResetPanel()
    {
        titlePanel.SetActive(true);
        optionPanel.SetActive(false);
    }

    private void InitializeVolumeSliders()
    {
        if (!GameSound.TryGetBgmVolume(out float bgmVolume) ||
            !GameSound.TryGetSfxVolume(out float sfxVolume))
        {
            Debug.LogWarning("SoundManager.Instance를 찾을 수 없어 볼륨 슬라이더를 초기화하지 못했습니다.", this);
            return;
        }

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(bgmVolume);
            bgmSlider.onValueChanged.RemoveListener(OnChangedBgmVolume);
            bgmSlider.onValueChanged.AddListener(OnChangedBgmVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfxVolume);
            sfxSlider.onValueChanged.RemoveListener(OnChangedSfxVolume);
            sfxSlider.onValueChanged.AddListener(OnChangedSfxVolume);
        }
    }

    public void OnChangedBgmVolume(float value)
    {
        GameSound.SetBgmVolume(value);
    }

    public void OnChangedSfxVolume(float value)
    {
        GameSound.SetSfxVolume(value);
    }

    public void OnClickSingleStart()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);

        GameFlowManager gameFlowManager = GameFlowManager.Instance;
        if (gameFlowManager == null)
        {
            Debug.LogError("GameFlowManager.Instance를 찾을 수 없습니다.", this);
            return;
        }

        gameFlowManager.StartSinglePlayer();
    }

    public void OnClickMultiStart()
    {
        // TODO 이후 멀티 플레이 연결되면 작업 진행
    }

    public void OnClickOption()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);
        titlePanel.SetActive(false);
        optionPanel.SetActive(true);
    }

    public void OnClickBack()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);
        ResetPanel();
    }

    public void OnClickExit()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);
        // TODO 게임 종료
    }
}
