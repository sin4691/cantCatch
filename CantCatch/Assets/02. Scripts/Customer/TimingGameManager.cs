using UnityEngine;
using UnityEngine.Events;

public class TimingGameManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onTimingGameStarted;
    public UnityEvent onCustomerWin;
    public UnityEvent onSellerWin;

    public static TimingGameManager Instance { get; private set; }
    public bool IsActive { get; private set; }

    public AttachedCone PendingCone { get; private set; }
    public Transform ReceivePoint { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

#if UNITY_EDITOR
    private void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;

        if (kb.tKey.wasPressedThisFrame)
        {
            AttachedCone cone = FindAnyObjectByType<AttachedCone>();
            CustomerHandZone receiveZone = FindAnyObjectByType<CustomerHandZone>();
            StartTimingGame(cone, receiveZone?.transform);
            OnSellerWin();
        }

        if (kb.yKey.wasPressedThisFrame)
            StartTimingGame(FindAnyObjectByType<AttachedCone>(), null);

        if (kb.gKey.wasPressedThisFrame)
        {
            GameFlowManager.Instance?.StartSinglePlayerImmediate();
            GameFlowManager.Instance?.CompletePreparation();
        }
    }
#endif

    public void StartTimingGame(AttachedCone cone, Transform coneReceivePoint)
    {
        if (IsActive)
            return;

        IsActive = true;
        PendingCone = cone;
        ReceivePoint = coneReceivePoint;

        onTimingGameStarted.Invoke();
        Debug.Log("타이밍 게임 시작", this);
    }

    public void StopTimingGame()
    {
        IsActive = false;
        PendingCone = null;
        ReceivePoint = null;
    }

    public void OnCustomerWin()
    {
        if (!IsActive)
            return;

        IsActive = false;
        onCustomerWin.Invoke();
        GameFlowManager.Instance.GameOver("손님이 콘을 가져갔습니다.");
        Debug.Log("손님 승리", this);
    }

    public void OnSellerWin()
    {
        if (!IsActive)
            return;

        IsActive = false;
        onSellerWin.Invoke();
        Debug.Log("판매자 승리", this);
    }
}
