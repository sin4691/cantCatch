using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Scene References")]
    [SerializeField] private Transform xrOriginRoot;
    [SerializeField] private Transform xrCameraTransform;
    [SerializeField] private Transform sellerBodyRoot;
    [SerializeField] private Transform titlePosition;
    [SerializeField] private Transform sellerStartPosition;
    [SerializeField] private GameObject menuBoardRoot;
    [SerializeField] private GameObject customerRoot;

    [Header("Time")]
    [SerializeField, Min(1f)] private float gameDuration = 60f;
    [SerializeField, Min(1f)] private float preparationDuration = 15f;

    [Header("Fade")]
    [SerializeField] private FadeCanvas fadeCanvas;
    [SerializeField, Min(0f)] private float startFadeDuration = 0.5f;

    [Header("Start Sequence")]
    [SerializeField] private bool snapToTitlePositionOnAwake = true;

    private int lastNotifiedGameTimerSeconds = -1;
    private int lastNotifiedPreparationTimerSeconds = -1;
    private bool lastNotifiedPreparationVisibility;
    private bool isStartSequenceRunning;

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

        if (snapToTitlePositionOnAwake)
            InitializeTitle();

        lastNotifiedGameTimerSeconds = GameTimeRemainingSeconds;
        lastNotifiedPreparationTimerSeconds = PreparationTimeRemainingSeconds;
        lastNotifiedPreparationVisibility = ShouldShowPreparationTimer;
    }

    private void Update()
    {
        if (!IsGameRunning || State == EGameState.Qte)
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

    public void StartSinglePlayer()
    {
        StartSinglePlayerAsync().Forget();
    }

    public void StartSinglePlayerImmediate()
    {
        if (!ValidateSingleGameStartReferences(requireFadeCanvas: false))
            return;

        PrepareSingleGameStart();
        BeginSinglePlayer();
    }

    public void InitializeTitle()
    {
        if (!ValidateTitleReferences())
            return;

        ResetGameDataForNewSession();
        ResetSessionStateToIdle();
        MoveXrOriginTo(titlePosition);
        SetGameObjectActive(menuBoardRoot, true);
        SetGameObjectActive(customerRoot, false);
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
        if (amount == 0 ||
            (State != EGameState.Playing && State != EGameState.Qte))
        {
            return;
        }

        Score += amount;
        ScoreChanged?.Invoke(Score);
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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BeginSinglePlayer()
    {
        Score = 0;
        ElapsedTime = 0f;
        PreparationTimeRemaining = preparationDuration;
        EndReason = string.Empty;

        ChangeState(EGameState.Preparation);
    }

    private async UniTaskVoid StartSinglePlayerAsync()
    {
        if (State != EGameState.Idle || isStartSequenceRunning)
            return;

        if (!ValidateSingleGameStartReferences(requireFadeCanvas: true))
            return;

        isStartSequenceRunning = true;

        try
        {
            await fadeCanvas.FadeOutAsync(startFadeDuration);

            PrepareSingleGameStart();
            BeginSinglePlayer();

            await fadeCanvas.FadeInAsync(startFadeDuration);
        }
        finally
        {
            isStartSequenceRunning = false;
        }
    }

    private void ChangeState(EGameState nextState)
    {
        if (State == nextState)
            return;

        State = nextState;

        NotifyTimerEvents(force: true);
        Debug.Log($"게임 상태: {State}");
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

    private void PrepareSingleGameStart()
    {
        ResetGameDataForNewSession();
        MoveXrOriginTo(sellerStartPosition);
        SetGameObjectActive(menuBoardRoot, false);
        SetGameObjectActive(customerRoot, true);
    }

    private void ResetGameDataForNewSession()
    {
        ConeQteController coneQteController = GetComponent<ConeQteController>();
        if (coneQteController != null)
        {
            coneQteController.CancelQte();
            coneQteController.ResetCustomerTotalClicks();
        }

        // TODO: Player 상위 스크립트가 생기면 그곳에서 플레이어 관련 초기화를 묶어서 처리한다.
    }

    private void ResetSessionStateToIdle()
    {
        Score = 0;
        ElapsedTime = 0f;
        PreparationTimeRemaining = 0f;
        EndReason = string.Empty;
        ChangeState(EGameState.Idle);
    }

    private void MoveXrOriginTo(Transform targetPoint)
    {
        Transform referenceTransform = xrCameraTransform != null ? xrCameraTransform : xrOriginRoot;
        float yawDelta = targetPoint.eulerAngles.y - referenceTransform.eulerAngles.y;
        xrOriginRoot.Rotate(Vector3.up, yawDelta, Space.World);

        if (sellerBodyRoot != null)
            sellerBodyRoot.Rotate(Vector3.up, yawDelta, Space.World);

        if (xrCameraTransform != null)
        {
            Vector3 rootToCameraOffset = xrOriginRoot.position - xrCameraTransform.position;
            xrOriginRoot.position = targetPoint.position + rootToCameraOffset;
        }
        else
        {
            xrOriginRoot.position = targetPoint.position;
        }
    }

    private bool ValidateTitleReferences()
    {
        bool isValid = true;
        isValid &= ValidateRequiredReference(xrOriginRoot, nameof(xrOriginRoot));
        isValid &= ValidateRequiredReference(xrCameraTransform, nameof(xrCameraTransform));
        isValid &= ValidateRequiredReference(titlePosition, nameof(titlePosition));
        isValid &= ValidateRequiredReference(menuBoardRoot, nameof(menuBoardRoot));
        isValid &= ValidateRequiredReference(customerRoot, nameof(customerRoot));
        return isValid;
    }

    private bool ValidateSingleGameStartReferences(bool requireFadeCanvas)
    {
        bool isValid = true;
        isValid &= ValidateRequiredReference(xrOriginRoot, nameof(xrOriginRoot));
        isValid &= ValidateRequiredReference(xrCameraTransform, nameof(xrCameraTransform));
        isValid &= ValidateRequiredReference(sellerStartPosition, nameof(sellerStartPosition));
        isValid &= ValidateRequiredReference(menuBoardRoot, nameof(menuBoardRoot));
        isValid &= ValidateRequiredReference(customerRoot, nameof(customerRoot));

        if (requireFadeCanvas)
            isValid &= ValidateRequiredReference(fadeCanvas, nameof(fadeCanvas));

        return isValid;
    }

    private bool ValidateRequiredReference(UnityEngine.Object target, string fieldName)
    {
        if (target != null)
            return true;

        Debug.LogError($"GameFlowManager의 `{fieldName}`가 인스펙터에 연결되어 있지 않습니다.", this);
        return false;
    }

    private void SetGameObjectActive(GameObject targetObject, bool isActive)
    {
        if (targetObject != null)
            targetObject.SetActive(isActive);
    }
}
