using TMPro;
using UnityEngine;

public class GameTimerTextUI : MonoBehaviour
{
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private TMP_Text gameTimerText;
    [SerializeField] private TMP_Text preparationTimerText;
    [SerializeField] private GameObject preparationTimerRoot;

    private void Awake()
    {
        ResolveGameFlowManager();
    }

    private void OnEnable()
    {
        ResolveGameFlowManager();

        if (gameFlowManager != null)
        {
            gameFlowManager.GameTimerSecondsChanged += HandleGameTimerSecondsChanged;
            gameFlowManager.PreparationTimerSecondsChanged += HandlePreparationTimerSecondsChanged;
            gameFlowManager.PreparationTimerVisibilityChanged += HandlePreparationTimerVisibilityChanged;
        }

        RefreshTimerTexts();
    }

    private void OnDisable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.GameTimerSecondsChanged -= HandleGameTimerSecondsChanged;
            gameFlowManager.PreparationTimerSecondsChanged -= HandlePreparationTimerSecondsChanged;
            gameFlowManager.PreparationTimerVisibilityChanged -= HandlePreparationTimerVisibilityChanged;
        }
    }

    private void HandleGameTimerSecondsChanged(int remainingSeconds)
    {
        SetTimerText(gameTimerText, remainingSeconds);
    }

    private void HandlePreparationTimerSecondsChanged(int remainingSeconds)
    {
        SetTimerText(preparationTimerText, remainingSeconds);
    }

    private void HandlePreparationTimerVisibilityChanged(bool isVisible)
    {
        if (preparationTimerRoot != null)
            preparationTimerRoot.SetActive(isVisible);
        else if (preparationTimerText != null)
            preparationTimerText.gameObject.SetActive(isVisible);
    }

    private void RefreshTimerTexts()
    {
        int gameSeconds = gameFlowManager != null
            ? gameFlowManager.GameTimeRemainingSeconds
            : 0;
        int preparationSeconds = gameFlowManager != null
            ? gameFlowManager.PreparationTimeRemainingSeconds
            : 0;
        bool showPreparationTimer = gameFlowManager != null && gameFlowManager.ShouldShowPreparationTimer;

        SetTimerText(gameTimerText, gameSeconds);
        SetTimerText(preparationTimerText, preparationSeconds);
        HandlePreparationTimerVisibilityChanged(showPreparationTimer);
    }

    private void SetTimerText(TMP_Text targetText, int remainingSeconds)
    {
        if (targetText == null)
            return;

        int clampedSeconds = Mathf.Max(0, remainingSeconds);
        int minutes = clampedSeconds / 60;
        int seconds = clampedSeconds % 60;
        targetText.text = $"{minutes:00}:{seconds:00}";
    }

    private void ResolveGameFlowManager()
    {
        if (gameFlowManager != null)
            return;

        gameFlowManager = GameFlowManager.Instance;

        if (gameFlowManager == null)
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();
    }
}
