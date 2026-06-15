using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("Time")]
    [SerializeField, Min(1f)] private float gameDuration = 60f;
    [SerializeField, Min(1f)] private float preparationDuration = 15f;

    public EGameState State { get; private set; } = EGameState.Idle;
    public int Score { get; private set; }
    public float ElapsedTime { get; private set; }
    public float GameDuration => gameDuration;
    public float GameTimeRemaining => Mathf.Max(0f, gameDuration - ElapsedTime);
    public float PreparationTimeRemaining { get; private set; }
    public string EndReason { get; private set; } = string.Empty;

    public bool IsGameOver => State == EGameState.GameOver;
    public bool IsGameRunning =>
        State == EGameState.Preparation ||
        State == EGameState.Playing ||
        State == EGameState.Qte;

    private void Update()
    {
        if (!IsGameRunning || State == EGameState.Qte)
            return;

        ElapsedTime = Mathf.Min(gameDuration, ElapsedTime + Time.deltaTime);

        if (ElapsedTime >= gameDuration)
        {
            CompleteGame();
            return;
        }

        if (State != EGameState.Preparation)
            return;

        PreparationTimeRemaining = Mathf.Max(
            0f,
            PreparationTimeRemaining - Time.deltaTime);

        if (PreparationTimeRemaining <= 0f)
        {
            GameOver("15초 안에 아이스크림과 콘을 준비하지 못했습니다.");
        }
    }

    public void StartSinglePlayer()
    {
        Score = 0;
        ElapsedTime = 0f;
        PreparationTimeRemaining = preparationDuration;
        EndReason = string.Empty;

        ChangeState(EGameState.Preparation);
    }

    public bool CompletePreparation()
    {
        if (State != EGameState.Preparation)
            return false;

        PreparationTimeRemaining = 0f;
        ChangeState(EGameState.Playing);
        return true;
    }

    public bool BeginQte()
    {
        if (State != EGameState.Playing)
            return false;

        ChangeState(EGameState.Qte);
        return true;
    }

    public void EndQte()
    {
        if (State == EGameState.Qte)
        {
            ChangeState(EGameState.Playing);
        }
    }

    public void AddScore(int amount)
    {
        if (amount <= 0 ||
            (State != EGameState.Playing && State != EGameState.Qte))
        {
            return;
        }

        Score += amount;
        Debug.Log($"점수: {Score}");
    }

    public void GameOver(string reason)
    {
        if (!IsGameRunning)
            return;

        EndReason = reason;
        ChangeState(EGameState.GameOver);

        Debug.LogError($"게임오버: {reason}");
    }

    private void CompleteGame()
    {
        EndReason = "60초 생존 성공";
        ChangeState(EGameState.Cleared);

        Debug.Log($"게임 클리어: {Score}점");
    }

    private void ChangeState(EGameState nextState)
    {
        if (State == nextState)
            return;

        State = nextState;
        Debug.Log($"게임 상태: {State}");
    }
}
