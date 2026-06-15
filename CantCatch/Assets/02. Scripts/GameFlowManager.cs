using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [SerializeField, Min(1f)] private float gameDuration = 60f;

    public bool IsGameOver { get; private set; }
    public int Score { get; private set; }
    public float ElapsedTime { get; private set; }
    public float GameDuration => gameDuration;

    private void Update()
    {
        if (!IsGameOver)
            ElapsedTime += Time.deltaTime;
    }

    public void AddScore(int amount)
    {
        if (IsGameOver || amount <= 0)
            return;

        Score += amount;
        Debug.Log($"점수: {Score}");
    }

    public void GameOver(string reason)
    {
        if (IsGameOver)
            return;

        IsGameOver = true;

        Debug.LogError($"게임오버: {reason}");
    }
}
