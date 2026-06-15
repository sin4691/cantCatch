using UnityEngine;
using UnityEngine.Events;

public class TimingGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private Transform handTransform;
    [SerializeField] private CustomerArmIK armIK;
    [SerializeField] private Animator animator;
    [SerializeField] private string winTriggerName = "Win";

    [Header("Settings")]
    [SerializeField, Min(1)] private int requiredMashCount = 5;

    [Header("Events")]
    public UnityEvent onCustomerWin;
    public UnityEvent onSellerWin;

    public bool IsActive { get; private set; }

    private ICustomerInput customerInput;
    private int currentMashCount;
    private AttachedCone pendingCone;

    private void Awake()
    {
        if (gameFlowManager == null)
            gameFlowManager = GameFlowManager.Instance;

        customerInput = GetComponent<ICustomerInput>();
    }

    public void StartTimingGame(AttachedCone cone = null)
    {
        if (IsActive)
            return;

        IsActive = true;
        currentMashCount = 0;
        pendingCone = cone;

        float timeRatio = gameFlowManager != null
            ? Mathf.Clamp01(gameFlowManager.ElapsedTime / gameFlowManager.GameDuration)
            : 0f;

        if (customerInput is TimedCustomerInput timed)
            timed.StartMashing(timeRatio);

        Debug.Log("타이밍 게임 시작", this);
    }

    public void StopTimingGame()
    {
        IsActive = false;
    }

    private void Update()
    {
        if (!IsActive || gameFlowManager.IsGameOver)
            return;

        if (customerInput != null && customerInput.TryMash())
        {
            currentMashCount++;
            Debug.Log($"연타: {currentMashCount}/{requiredMashCount}", this);

            if (currentMashCount >= requiredMashCount)
            {
                IsActive = false;
                GrabCone();
                animator?.SetTrigger(winTriggerName);
                onCustomerWin.Invoke();
                gameFlowManager.GameOver("손님이 콘을 가져갔습니다.");
                Debug.Log("손님 승리: 콘 가져감", this);
            }
        }
    }

    private void GrabCone()
    {
        if (pendingCone == null || handTransform == null)
            return;

        if (armIK != null)
            armIK.SetIKActive(false);

        pendingCone.Stick.TryLeaveCone(handTransform);
        pendingCone = null;
    }

    public void OnSellerSuccess()
    {
        if (!IsActive)
            return;

        IsActive = false;
        onSellerWin.Invoke();
        Debug.Log("판매자 승리: 콘 빼앗김", this);
    }
}
