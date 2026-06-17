using UnityEngine;

public class UI_TitleMenu : MonoBehaviour
{
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject optionPanel;

    private void Start()
    {
        ResetPanel();
    }

    private void ResetPanel()
    {
        titlePanel.SetActive(true);
        optionPanel.SetActive(false);
    }

    public void OnClickSingleStart()
    {
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
        titlePanel.SetActive(false);
        optionPanel.SetActive(true);
    }

    public void OnClickBack()
    {
        ResetPanel();
    }

    public void OnClickExit()
    {
        // TODO 게임 종료
    }
}
