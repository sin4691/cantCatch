using UnityEngine;
using UnityEngine.Events;

public class SingleConeQteController : MonoBehaviour
{
    private enum EQtePhase
    {
        Inactive,
        Preparing,
        Judging,
        BetweenRounds
    }

    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private GameObject qteRoot;
    [SerializeField] private RectTransform needle;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float preparationDuration = 3f;
    [SerializeField, Min(0.1f)] private float judgmentDuration = 3f;
    [SerializeField, Min(0f)] private float intervalBetweenJudgments = 0.5f;
    [SerializeField, Min(1f)] private float startNeedleSpeed = 180f;
    [SerializeField, Min(1f)] private float endNeedleSpeed = 360f;

    [Header("Judgment Zones")]
    [SerializeField, Range(0f, 1f)] private float successZoneCenter;
    [SerializeField, Range(0.01f, 1f)] private float successZoneSize = 0.25f;
    [SerializeField, Range(0.01f, 1f)] private float greatZoneSize = 0.0625f;

    [Header("Rules")]
    [SerializeField, Min(1)] private int maxJudgmentCount = 3;
    [SerializeField, Min(1)] private int requiredSuccessCount = 2;
    [SerializeField, Min(1)] private int successScore = 10;
    [SerializeField, Min(1)] private int greatScore = 20;

    [Header("Events")]
    public UnityEvent onQteStarted = new();
    public UnityEvent onQteSucceeded = new();
    public UnityEvent onQteFailed = new();
    public UnityEvent onQteCancelled = new();

    public bool IsActive => phase != EQtePhase.Inactive;
    public bool IsAcceptingInput => phase == EQtePhase.Judging;
    public int JudgmentCount { get; private set; }
    public int SuccessCount { get; private set; }
    public int FailCount { get; private set; }
    public ETimingResult LastResult { get; private set; }

    private EQtePhase phase;
    private AttachedCone targetCone;
    private Transform receivePoint;
    private float phaseTimeRemaining;
    private float needleProgress;

    private void Awake()
    {
        if (gameFlowManager == null)
            gameFlowManager = GetComponent<GameFlowManager>();

        if (gameFlowManager == null)
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();

        SetUiActive(false);
    }

    private void Update()
    {
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

        switch (phase)
        {
            case EQtePhase.Preparing:
            case EQtePhase.BetweenRounds:
                phaseTimeRemaining -= Time.deltaTime;
                if (phaseTimeRemaining <= 0f)
                    BeginJudgment();
                break;

            case EQtePhase.Judging:
                UpdateNeedle();
                phaseTimeRemaining -= Time.deltaTime;
                if (phaseTimeRemaining <= 0f)
                    ResolveJudgment(ETimingResult.Fail);
                break;
        }
    }

    public bool TryStart(AttachedCone cone, Transform targetReceivePoint)
    {
        if (IsActive || cone == null || cone.Stick == null || targetReceivePoint == null)
            return false;

        if (gameFlowManager == null || !gameFlowManager.BeginQte())
            return false;

        targetCone = cone;
        receivePoint = targetReceivePoint;
        JudgmentCount = 0;
        SuccessCount = 0;
        FailCount = 0;
        needleProgress = 0f;
        phase = EQtePhase.Preparing;
        phaseTimeRemaining = preparationDuration;

        SetNeedleRotation();
        SetUiActive(true);
        onQteStarted.Invoke();

        if (phaseTimeRemaining <= 0f)
            BeginJudgment();

        return true;
    }

    public void SubmitInput()
    {
        if (!IsAcceptingInput)
            return;

        ResolveJudgment(JudgeNeedle());
    }

    public void CancelQte()
    {
        if (!IsActive)
            return;

        ResetQte();
        gameFlowManager?.EndQte();
        onQteCancelled.Invoke();
    }

    private void BeginJudgment()
    {
        phase = EQtePhase.Judging;
        phaseTimeRemaining = judgmentDuration;
        needleProgress = Random.value;
        SetNeedleRotation();
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

    private ETimingResult JudgeNeedle()
    {
        float distance = Mathf.Abs(Mathf.DeltaAngle(
            needleProgress * 360f,
            successZoneCenter * 360f)) / 360f;

        if (distance <= greatZoneSize * 0.5f)
            return ETimingResult.Great;

        if (distance <= successZoneSize * 0.5f)
            return ETimingResult.Success;

        return ETimingResult.Fail;
    }

    private void ResolveJudgment(ETimingResult result)
    {
        LastResult = result;
        JudgmentCount++;

        switch (result)
        {
            case ETimingResult.Great:
                SuccessCount++;
                gameFlowManager.AddScore(greatScore);
                break;

            case ETimingResult.Success:
                SuccessCount++;
                gameFlowManager.AddScore(successScore);
                break;

            case ETimingResult.Fail:
                FailCount++;
                break;
        }

        if (FailCount >= 2)
        {
            FailQte();
            return;
        }

        if (JudgmentCount >= maxJudgmentCount)
        {
            if (SuccessCount >= requiredSuccessCount)
                CompleteQte();
            else
                FailQte();

            return;
        }

        phase = EQtePhase.BetweenRounds;
        phaseTimeRemaining = intervalBetweenJudgments;

        if (phaseTimeRemaining <= 0f)
            BeginJudgment();
    }

    private void CompleteQte()
    {
        StickIceCreamController stick = targetCone.Stick;
        Transform targetReceivePoint = receivePoint;

        ResetQte();

        if (stick == null || !stick.TryLeaveCone(targetReceivePoint))
        {
            gameFlowManager.GameOver("QTE 성공 후 콘을 전달하지 못했습니다.");
            return;
        }

        gameFlowManager.EndQte();
        onQteSucceeded.Invoke();
    }

    private void FailQte()
    {
        StickIceCreamController stick = targetCone != null ? targetCone.Stick : null;

        ResetQte();
        stick?.Clear();
        onQteFailed.Invoke();
        gameFlowManager.GameOver("콘 주기 QTE에서 두 번 실패했습니다.");
    }

    private void ResetQte()
    {
        phase = EQtePhase.Inactive;
        phaseTimeRemaining = 0f;
        needleProgress = 0f;
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
            qteRoot.SetActive(active);
    }

    private void OnDisable()
    {
        ResetQte();
    }
}
