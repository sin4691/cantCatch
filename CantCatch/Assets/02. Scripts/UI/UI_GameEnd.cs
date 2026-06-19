using UnityEngine;

public class UI_GameEnd : MonoBehaviour
{
    public void OnClickReStart()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);

        GameFlowManager gameFlowManager = GameFlowManager.Instance;
        if (gameFlowManager == null)
        {
            Debug.LogError("GameFlowManager.Instance를 찾을 수 없습니다.", this);
            return;
        }

        gameFlowManager.RestartSinglePlayer();
    }

    public void OnClickTitle()
    {
        GameSound.PlaySfx(ESfxSoundId.Click);

        GameFlowManager gameFlowManager = GameFlowManager.Instance;
        if (gameFlowManager == null)
        {
            Debug.LogError("GameFlowManager.Instance를 찾을 수 없습니다.", this);
            return;
        }

        gameFlowManager.ReturnToTitle();
    }
}
