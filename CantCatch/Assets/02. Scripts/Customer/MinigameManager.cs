using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MinigameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinigameSliderUI sliderUI;

    [Header("Duration")]
    [SerializeField, Min(1f)] private float gameDuration = 10f;

    [Header("Cooldown")]
    [SerializeField, Min(0f)] private float cooldownAfterGame = 3f;

    [Header("Events")]
    public UnityEvent onMinigameStarted;
    public UnityEvent onCustomerWin;
    public UnityEvent onSellerWin;

    public static MinigameManager Instance { get; private set; }
    public bool IsActive { get; private set; }
    public AttachedCone PendingCone { get; private set; }
    public Transform ReceivePoint { get; private set; }

    private float cooldownRemaining;

    private void Awake()
    {
        Instance = this;
        if (sliderUI != null)
            sliderUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.deltaTime;

#if UNITY_EDITOR
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;

        if (kb.gKey.wasPressedThisFrame)
        {
            GameFlowManager.Instance?.StartSinglePlayer();
            GameFlowManager.Instance?.CompletePreparation();
        }

        if (kb.tKey.wasPressedThisFrame)
        {
            AttachedCone cone = FindAnyObjectByType<AttachedCone>();
            CustomerHandZone receiveZone = FindAnyObjectByType<CustomerHandZone>();
            StartMinigame(cone, receiveZone?.transform);
        }
#endif
    }

    public bool StartMinigame(AttachedCone cone, Transform receivePoint)
    {
        if (IsActive || cooldownRemaining > 0f || cone == null)
            return false;

        IsActive = true;
        PendingCone = cone;
        ReceivePoint = receivePoint;

        if (sliderUI != null)
        {
            sliderUI.gameObject.SetActive(true);
            sliderUI.SetDuration(gameDuration);
            sliderUI.ResetView();
            sliderUI.StartTimer();
        }

        StartCoroutine(TimerCoroutine());
        onMinigameStarted.Invoke();
        Debug.Log("미니게임 시작", this);
        return true;
    }

    public void StopMinigame()
    {
        IsActive = false;
        ClearState();
    }

    public void CustomerMash()
    {
        if (!IsActive) return;
        sliderUI?.AddProgress(0.1f);
        Debug.Log($"손님 연타 | 현재 값: {sliderUI?.TargetProgress:0.00}", this);
    }

    public void SellerMash()
    {
        if (!IsActive) return;
        sliderUI?.AddProgress(-0.1f);
        Debug.Log($"판매자 연타 | 현재 값: {sliderUI?.TargetProgress:0.00}", this);
    }

    public void OnCustomerWin()
    {
        if (!IsActive) return;
        IsActive = false;
        onCustomerWin.Invoke();
        GameFlowManager.Instance?.GameOver("손님이 콘을 가져갔습니다.");
        Debug.Log("손님 승리", this);
        ClearState();
    }

    public void OnSellerWin()
    {
        if (!IsActive) return;
        IsActive = false;
        onSellerWin.Invoke();
        Debug.Log("판매자 승리", this);
        ClearState();
    }

    private void ClearState()
    {
        StopAllCoroutines();
        PendingCone = null;
        ReceivePoint = null;
        cooldownRemaining = cooldownAfterGame;

        if (sliderUI != null)
            sliderUI.gameObject.SetActive(false);
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(gameDuration);
        if (!IsActive) yield break;

        float progress = sliderUI != null ? sliderUI.TargetProgress : 0f;

        if (progress > 0f)
            OnCustomerWin();
        else if (progress < 0f)
            OnSellerWin();
        else
            StopMinigame();
    }
}
