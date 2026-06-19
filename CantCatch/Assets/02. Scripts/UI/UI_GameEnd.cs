using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_GameEnd : MonoBehaviour
{
    [SerializeField] private FadeCanvas fadeCanvas;
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private string titleSceneName = "TitleScene";

    public async void OnClickReStart()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("재시작할 게임 씬 이름이 비어 있습니다.", this);
            return;
        }

        if (fadeCanvas != null)
            await fadeCanvas.FadeOutAsync();

        SceneManager.LoadScene(gameSceneName);
    }

    public async void OnClickTitle()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);

        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError("이동할 타이틀 씬 이름이 비어 있습니다.", this);
            return;
        }

        if (fadeCanvas != null)
            await fadeCanvas.FadeOutAsync();

        SceneManager.LoadScene(titleSceneName);
    }
}
