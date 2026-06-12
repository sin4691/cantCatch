using UnityEngine;
using UnityEngine.Events;

public class CustomerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CustomerHandSensor handSensor;

    [Header("Waiting")]
    [SerializeField, Min(0f)] private float waitTimeout = 10f;

    [Header("Events")]
    public UnityEvent onStartWaiting;
    public UnityEvent onExiting;

    public CustomerState State { get; private set; }
    public float WaitTimeRemaining { get; private set; }

    private void Awake()
    {
        if (handSensor == null)
            handSensor = GetComponentInChildren<CustomerHandSensor>();

        if (handSensor == null)
        {
            Debug.LogError("CustomerHandSensor를 찾을 수 없습니다.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        StartWaiting();
    }

    private void Update()
    {
        if (State != CustomerState.Waiting)
            return;

        if (waitTimeout > 0f)
        {
            WaitTimeRemaining -= Time.deltaTime;

            if (WaitTimeRemaining <= 0f)
            {
                Exit("대기 시간 초과");
            }
        }
    }

    public void StartWaiting()
    {
        State = CustomerState.Waiting;
        WaitTimeRemaining = waitTimeout;
        handSensor.enabled = true;
        onStartWaiting.Invoke();

        Debug.Log("손님 대기 시작", this);
    }

    public void Exit(string reason = "")
    {
        if (State == CustomerState.Exiting)
            return;

        State = CustomerState.Exiting;
        handSensor.enabled = false;
        onExiting.Invoke();

        Debug.Log($"손님 퇴장: {reason}", this);
    }
}
