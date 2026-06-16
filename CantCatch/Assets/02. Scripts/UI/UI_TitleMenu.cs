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
        // TODO GameFlowManager 시작 버튼 연결
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
