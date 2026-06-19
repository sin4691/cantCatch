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

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float qteDuration = 5f;

    [Header("Mash Scoring")]
    [SerializeField, Min(0)] private int merchantWinScore = 10;
    [SerializeField, Min(0)] private int customerWinScorePenalty = 10;
    [SerializeField, Min(0)] private int customerStartAdvantagePerGrab = 1;

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
    public int MerchantMashCount => merchantMashCount;
    public int CustomerMashCount => customerMashCount;
    public int CustomerStartAdvantage => customerStartAdvantage;
    public int MerchantFinalValue => merchantMashCount;
    public int CustomerFinalValue => customerMashCount + customerStartAdvantage;
    public int CustomerConeGrabCount => customerConeGrabCount;

    private EQtePhase phase;
    private AttachedCone targetCone;
    private Transform receivePoint;
    private float qteTimeRemaining;
    private int merchantMashCount;
    private int customerMashCount;
    private int customerStartAdvantage;
    private int customerConeGrabCount;
    private EGameState previousGameState = EGameState.Idle;

    private void Awake()
    {
        if (gameFlowManager == null)
            gameFlowManager = GetComponent<GameFlowManager>();

        if (gameFlowManager == null)
            gameFlowManager = GameFlowManager.Instance;

        if (gameFlowManager != null)
            previousGameState = gameFlowManager.State;

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

        if (qteTimeRemaining <= 0f)
            ResolveQteOutcome();
    }

    private void UpdateGameStateTracking()
    {
        if (gameFlowManager == null || gameFlowManager.State == previousGameState)
            return;

        previousGameState = gameFlowManager.State;

        if (previousGameState == EGameState.Preparation)
            ResetCustomerQteStartCount();
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
        SetUiActive(true);
        onQteStarted.Invoke();

        return true;
    }

    public void SubmitPlayerQteInput()
    {
        if (!IsAcceptingInput)
            return;

        merchantMashCount++;
    }

    public void NotifyPlayerTimingMissionCleared()
    {
        SubmitPlayerQteInput();
    }

    public void SubmitCustomerQteInput()
    {
        if (!IsAcceptingInput)
            return;

        customerMashCount++;
    }

    public void ResetCustomerTotalClicks()
    {
        ResetCustomerQteStartCount();
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
        customerStartAdvantage = customerConeGrabCount * customerStartAdvantagePerGrab;
        customerConeGrabCount++;
        merchantMashCount = 0;
        customerMashCount = 0;
        qteTimeRemaining = qteDuration;
        phase = EQtePhase.Running;
        GameSound.PlaySfx(ESfxSoundId.MiniGameStart);
    }

    private void UpdateQteSession()
    {
        qteTimeRemaining = Mathf.Max(0f, qteTimeRemaining - Time.deltaTime);
    }

    private void ResolveQteOutcome()
    {
        phase = EQtePhase.Resolved;

        if (MerchantFinalValue > CustomerFinalValue)
        {
            ResolveMerchantWin();
            return;
        }

        if (CustomerFinalValue > MerchantFinalValue)
        {
            ResolveCustomerWin();
            return;
        }

        ResolveDraw();
    }

    private void ResolveMerchantWin()
    {
        StickIceCreamController stick = targetCone.Stick;
        Transform targetReceivePoint = receivePoint;

        if (stick != null && stick.TryLeaveCone(targetReceivePoint))
            gameFlowManager?.AddScore(merchantWinScore);

        ResetQte();
        gameFlowManager?.EndQte();
        onQteSucceeded.Invoke();
    }

    private void ResolveCustomerWin()
    {
        StickIceCreamController stick = targetCone != null ? targetCone.Stick : null;

        if (stick != null && stick.TryDestroyCone())
            gameFlowManager?.AddScore(-customerWinScorePenalty);

        ResetQte();
        gameFlowManager?.EndQte();
        onQteFailed.Invoke();
    }

    private void ResolveDraw()
    {
        ResetQte();
        gameFlowManager?.EndQte();
    }

    private void ResetQte()
    {
        phase = EQtePhase.Inactive;
        qteTimeRemaining = 0f;
        merchantMashCount = 0;
        customerMashCount = 0;
        customerStartAdvantage = 0;
        targetCone = null;
        receivePoint = null;
        SetUiActive(false);
    }

    private void ResetCustomerQteStartCount()
    {
        customerConeGrabCount = 0;
    }

    private void SetUiActive(bool active)
    {
        if (qteRoot != null)
            qteRoot.SetActive(active);
    }

    private void OnDisable()
    {
        ResetQte();
    }
}
