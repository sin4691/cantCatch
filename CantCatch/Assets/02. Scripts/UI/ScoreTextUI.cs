using TMPro;
using UnityEngine;

public class ScoreTextUI : MonoBehaviour
{
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        ResolveGameFlowManager();
    }

    private void OnEnable()
    {
        ResolveGameFlowManager();

        if (gameFlowManager != null)
            gameFlowManager.ScoreChanged += HandleScoreChanged;

        RefreshScore();
    }

    private void OnDisable()
    {
        if (gameFlowManager != null)
            gameFlowManager.ScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int score)
    {
        SetScoreText(score);
    }

    private void RefreshScore()
    {
        SetScoreText(gameFlowManager != null ? gameFlowManager.Score : 0);
    }

    private void SetScoreText(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
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
