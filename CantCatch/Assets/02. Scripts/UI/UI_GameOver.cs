using UnityEngine;

public class UI_GameOver : MonoBehaviour
{
    public void OnClickReStart()
    {
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
        GameFlowManager gameFlowManager = GameFlowManager.Instance;
        if (gameFlowManager == null)
        {
            Debug.LogError("GameFlowManager.Instance를 찾을 수 없습니다.", this);
            return;
        }

        gameFlowManager.ReturnToTitle();
    }
}
