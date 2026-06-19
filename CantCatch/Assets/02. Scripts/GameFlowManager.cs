using System;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private UI_GameEnd uiGameEnd;

    [Header("Time")]
    [SerializeField, Min(1f)] private float gameDuration = 60f;
    [SerializeField, Min(1f)] private float preparationDuration = 15f;

    private int lastNotifiedGameTimerSeconds = -1;
    private int lastNotifiedPreparationTimerSeconds = -1;
    private bool lastNotifiedPreparationVisibility;

    public event Action<int> ScoreChanged;
    public event Action<int> GameTimerSecondsChanged;
    public event Action<int> PreparationTimerSecondsChanged;
    public event Action<bool> PreparationTimerVisibilityChanged;

    public EGameState State { get; private set; } = EGameState.Idle;
    public int Score { get; private set; }
    public float ElapsedTime { get; private set; }
    public float GameDuration => gameDuration;
    public float GameTimeRemaining => Mathf.Max(0f, gameDuration - ElapsedTime);
    public int GameTimeRemainingSeconds => Mathf.CeilToInt(GameTimeRemaining);
    public float PreparationTimeRemaining { get; private set; }
    public int PreparationTimeRemainingSeconds => Mathf.CeilToInt(Mathf.Max(0f, PreparationTimeRemaining));
    public bool ShouldShowPreparationTimer => State == EGameState.Preparation;
    public string EndReason { get; private set; } = string.Empty;

    public bool IsGameOver => State == EGameState.GameOver;
    public bool IsGameRunning =>
        State == EGameState.Preparation ||
        State == EGameState.Playing ||
        State == EGameState.Qte;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("GameFlowManager가 씬에 두 개 이상 존재합니다.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetGameEndUiVisible(false);

        lastNotifiedGameTimerSeconds = GameTimeRemainingSeconds;
        lastNotifiedPreparationTimerSeconds = PreparationTimeRemainingSeconds;
        lastNotifiedPreparationVisibility = ShouldShowPreparationTimer;
    }

    private void Start()
    {
        StartGameSession();
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
        if (amount == 0 || State != EGameState.Playing)
            return;

        Score += amount;
        ScoreChanged?.Invoke(Score);

        if (amount > 0)
            GameSound.PlaySfx(ESfxSoundId.Score);

        Debug.Log($"점수: {Score}");
    }

    public void GameOver(string reason)
    {
        if (!IsGameRunning)
            return;

        EndReason = reason;
        ChangeState(EGameState.GameOver);
        SetGameEndUiVisible(true);
        GameSound.PlaySfx(ESfxSoundId.Lose);

        //Debug.LogError($"게임오버: {reason}");
        Debug.Log($"게임오버: {reason}");
    }

    private void CompleteGame()
    {
        EndReason = "60초 생존 성공";
        ChangeState(EGameState.Cleared);
        SetGameEndUiVisible(true);
        GameSound.PlaySfx(ESfxSoundId.Win);

        Debug.Log($"게임 클리어: {Score}점");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsGameRunning)
            return;

        ElapsedTime = Mathf.Min(gameDuration, ElapsedTime + Time.deltaTime);
        NotifyTimerEvents();

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
        NotifyTimerEvents();

        if (PreparationTimeRemaining <= 0f)
        {
            GameOver("15초 안에 아이스크림과 콘을 준비하지 못했습니다.");
        }
    }

    private void ChangeState(EGameState nextState)
    {
        if (State == nextState)
            return;

        State = nextState;
        Debug.Log($"게임 상태: {State}");
    }

    private void NotifyHudEvents(bool forceTimer = false)
    {
        ScoreChanged?.Invoke(Score);
        NotifyTimerEvents(force: forceTimer);
    }

    private void NotifyTimerEvents(bool force = false)
    {
        int gameSeconds = GameTimeRemainingSeconds;
        if (force || gameSeconds != lastNotifiedGameTimerSeconds)
        {
            lastNotifiedGameTimerSeconds = gameSeconds;
            GameTimerSecondsChanged?.Invoke(gameSeconds);
        }

        int preparationSeconds = PreparationTimeRemainingSeconds;
        if (force || preparationSeconds != lastNotifiedPreparationTimerSeconds)
        {
            lastNotifiedPreparationTimerSeconds = preparationSeconds;
            PreparationTimerSecondsChanged?.Invoke(preparationSeconds);
        }

        bool shouldShowPreparation = ShouldShowPreparationTimer;
        if (force || shouldShowPreparation != lastNotifiedPreparationVisibility)
        {
            lastNotifiedPreparationVisibility = shouldShowPreparation;
            PreparationTimerVisibilityChanged?.Invoke(shouldShowPreparation);
        }
    }

    private void StartGameSession()
    {
        Score = 0;
        ElapsedTime = 0f;
        PreparationTimeRemaining = preparationDuration;
        EndReason = string.Empty;

        ChangeState(EGameState.Preparation);
        SetGameEndUiVisible(false);
        NotifyHudEvents(forceTimer: true);
        GameSound.PlayBgm(EBgmSoundId.Game);
        Debug.Log("PlayerBGM EBgmSoundId.Game");
    }

    private void SetGameEndUiVisible(bool isVisible)
    {
        if (uiGameEnd == null)
            return;

        GameObject uiGameEndObject = uiGameEnd.gameObject;
        if (uiGameEndObject.activeSelf == isVisible)
            return;

        uiGameEndObject.SetActive(isVisible);
    }

}
