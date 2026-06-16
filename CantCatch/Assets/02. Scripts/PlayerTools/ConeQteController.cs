using UnityEngine;
using UnityEngine.Events;

public class ConeQteController : MonoBehaviour
{
    private enum EQtePhase
    {
        Inactive,
        Running,
        Resolved
    }

    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private GameObject qteRoot;
    [SerializeField] private RectTransform needle;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float qteDuration = 5f;
    [SerializeField, Min(1f)] private float startNeedleSpeed = 180f;
    [SerializeField, Min(1f)] private float endNeedleSpeed = 360f;

    [Header("Judgment Zones")]
    [SerializeField, Range(0f, 1f)] private float successZoneCenter;
    [SerializeField, Range(0.01f, 1f)] private float successZoneSize = 0.25f;

    [Header("Temporary Customer Click Mission")]
    [SerializeField, Min(1)] private int customerRequiredClicks = 15;
    [SerializeField, Min(1)] private int customerTotalWinThreshold = 80;

    [Header("Events")]
    public UnityEvent onQteStarted = new();
    public UnityEvent onQteSucceeded = new();
    public UnityEvent onQteFailed = new();
    public UnityEvent onQteCancelled = new();

    public bool IsActive => phase != EQtePhase.Inactive;
    public bool IsAcceptingInput => phase == EQtePhase.Running && qteTimeRemaining > 0f;
    public float QteDuration => qteDuration;
    public float QteTimeRemaining => qteTimeRemaining;
    public float QteProgress => qteDuration > 0f
        ? 1f - Mathf.Clamp01(qteTimeRemaining / qteDuration)
        : 1f;
    public int CustomerRequiredClicks => customerRequiredClicks;
    public int CustomerCurrentClickCount => customerCurrentClickCount;
    public int CustomerTotalClickCount => customerTotalClickCount;
    public int CustomerTotalWinThreshold => customerTotalWinThreshold;

    private EQtePhase phase;
    private AttachedCone targetCone;
    private Transform receivePoint;
    private float qteTimeRemaining;
    private float needleProgress;
    private int customerCurrentClickCount;
    private int customerTotalClickCount;
    private bool playerMissionCleared;
    private EGameState previousGameState = EGameState.Idle;

    private void Awake()
    {
        if (gameFlowManager == null)
        {
            gameFlowManager = GetComponent<GameFlowManager>();

        }

        if (gameFlowManager == null)
        {
            gameFlowManager = GameFlowManager.Instance;

        }

        if (gameFlowManager != null)
        {
            previousGameState = gameFlowManager.State;

        }

        SetUiActive(false);
    }

    private void Update()
    {
        UpdateGameStateTracking();

        if (!IsActive)
            return;

        if (gameFlowManager == null || gameFlowManager.State != EGameState.Qte)
        {
            ResetQte();
            return;
        }

        if (targetCone == null || targetCone.Stick == null)
        {
            CancelQte();
            return;
        }

        if (phase != EQtePhase.Running)
            return;

        UpdateQteSession();
        UpdatePlayerTimingMission();

        ResolveQteOutcome();
    }

    private void UpdateGameStateTracking()
    {
        if (gameFlowManager == null || gameFlowManager.State == previousGameState)
            return;

        previousGameState = gameFlowManager.State;

        if (previousGameState == EGameState.Preparation)
        {
            ResetCustomerTotalClicks();

        }
    }

    private void ResolveQteOutcome()
    {
        if (HasCustomerReachedTotalLimit())
        {
            ResolveCustomerWin("Customer total QTE clicks reached the lose threshold.");
            return;
        }

        if (HasCustomerWonCurrentQte())
        {
            ResolveCustomerWin("Customer completed the QTE click mission first.");
            return;
        }

        if (IsPlayerTimingMissionCleared())
        {
            ResolvePlayerWin();
            return;
        }

        if (qteTimeRemaining <= 0f)
            ResolveTimeout();
    }

    public bool TryStart(AttachedCone cone, Transform targetReceivePoint)
    {
        if (IsActive || cone == null || cone.Stick == null || targetReceivePoint == null)
            return false;

        if (gameFlowManager == null || !gameFlowManager.BeginQte())
            return false;

        targetCone = cone;
        receivePoint = targetReceivePoint;

        StartQteSession();
        StartPlayerTimingMission();
        StartCustomerClickMission();
        SetUiActive(true);
        onQteStarted.Invoke();

        return true;
    }

    public void SubmitPlayerQteInput()
    {
        if (!IsAcceptingInput)
            return;

        SubmitPlayerTimingInput();
    }

    public void NotifyPlayerTimingMissionCleared()
    {
        if (!IsAcceptingInput)
            return;

        playerMissionCleared = true;
    }

    public void SubmitCustomerQteInput()
    {
        if (!IsAcceptingInput)
            return;

        SubmitCustomerClickInput();
    }

    public void ResetCustomerTotalClicks()
    {
        customerTotalClickCount = 0;
    }

    public void CancelQte()
    {
        if (!IsActive)
            return;

        ResetQte();
        gameFlowManager?.EndQte();
        onQteCancelled.Invoke();
    }

    private void StartQteSession()
    {
        qteTimeRemaining = qteDuration;
        phase = EQtePhase.Running;
    }

    private void UpdateQteSession()
    {
        qteTimeRemaining = Mathf.Max(0f, qteTimeRemaining - Time.deltaTime);
    }

    // Temporary player timing mission. Replace these methods with TimingManager integration later.
    private void StartPlayerTimingMission()
    {
        needleProgress = Random.value;
        playerMissionCleared = false;
        SetNeedleRotation();
    }

    private void UpdatePlayerTimingMission()
    {
        UpdateNeedle();
    }

    private void SubmitPlayerTimingInput()
    {
        if (IsNeedleInSuccessZone())
            playerMissionCleared = true;
    }

    private bool IsPlayerTimingMissionCleared()
    {
        return playerMissionCleared;
    }

    private void StopPlayerTimingMission()
    {
        needleProgress = 0f;
        playerMissionCleared = false;
    }

    // Temporary customer click mission. Replace these methods with QuickClick integration later.
    private void StartCustomerClickMission()
    {
        customerCurrentClickCount = 0;
    }

    private void SubmitCustomerClickInput()
    {
        customerCurrentClickCount++;
        customerTotalClickCount++;
    }

    private bool HasCustomerWonCurrentQte()
    {
        return customerCurrentClickCount >= customerRequiredClicks;
    }

    private bool HasCustomerReachedTotalLimit()
    {
        return customerTotalClickCount >= customerTotalWinThreshold;
    }

    private void StopCustomerClickMission()
    {
        customerCurrentClickCount = 0;
    }

    private void UpdateNeedle()
    {
        float timeRatio = gameFlowManager.GameDuration > 0f
            ? Mathf.Clamp01(gameFlowManager.ElapsedTime / gameFlowManager.GameDuration)
            : 0f;
        float needleSpeed = Mathf.Lerp(startNeedleSpeed, endNeedleSpeed, timeRatio);

        needleProgress = Mathf.Repeat(
            needleProgress + needleSpeed * Time.deltaTime / 360f,
            1f);
        SetNeedleRotation();
    }

    private bool IsNeedleInSuccessZone()
    {
        float distance = Mathf.Abs(Mathf.DeltaAngle(
            needleProgress * 360f,
            successZoneCenter * 360f)) / 360f;

        return distance <= successZoneSize * 0.5f;
    }

    private void ResolvePlayerWin()
    {
        phase = EQtePhase.Resolved;

        StickIceCreamController stick = targetCone.Stick;
        Transform targetReceivePoint = receivePoint;

        ResetQte();

        if (stick == null || !stick.TryLeaveCone(targetReceivePoint))
        {
            gameFlowManager.GameOver("QTE success could not deliver the cone.");
            return;
        }

        gameFlowManager.EndQte();
        onQteSucceeded.Invoke();
    }

    private void ResolveCustomerWin(string reason)
    {
        phase = EQtePhase.Resolved;

        StickIceCreamController stick = targetCone != null ? targetCone.Stick : null;

        ResetQte();
        stick?.Clear();
        onQteFailed.Invoke();
        gameFlowManager.GameOver(reason);
    }

    private void ResolveTimeout()
    {
        phase = EQtePhase.Resolved;
        ResetQte();
        gameFlowManager?.EndQte();
    }

    private void ResetQte()
    {
        phase = EQtePhase.Inactive;
        qteTimeRemaining = 0f;
        StopPlayerTimingMission();
        StopCustomerClickMission();
        targetCone = null;
        receivePoint = null;
        SetUiActive(false);
    }

    private void SetNeedleRotation()
    {
        if (needle != null)
            needle.localRotation = Quaternion.Euler(0f, 0f, -needleProgress * 360f);
    }

    private void SetUiActive(bool active)
    {
        if (qteRoot != null)
        {
            qteRoot.SetActive(active);

        }
    }

    private void OnDisable()
    {
        ResetQte();
    }
}
