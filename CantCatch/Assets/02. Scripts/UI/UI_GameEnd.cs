using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_GameEnd : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private string titleSceneName = "TitleScene";

    public void OnClickReStart()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("재시작할 게임 씬 이름이 비어 있습니다.", this);
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickTitle()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);

        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError("이동할 타이틀 씬 이름이 비어 있습니다.", this);
            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }
}
