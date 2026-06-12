using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public bool IsGameOver { get; private set; }
    public int Score { get; private set; }

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
