using UnityEngine;

public class CustomerAIInput : MonoBehaviour
{
    [SerializeField] private CustomerHandSensor handSensor;
    [SerializeField] private MashInput mashInput;

    [Header("Timing")]
    [SerializeField, Min(0.01f)] private float startAttemptInterval = 0.3f;
    [SerializeField, Min(0.01f)] private float mashInterval = 0.3f;

    private float startTimer;
    private float mashTimer;

    private void Awake()
    {
        if (handSensor == null)
            handSensor = GetComponentInParent<CustomerHandSensor>();

        if (mashInput == null)
            mashInput = GetComponent<MashInput>();
    }

    private void Update()
    {
        var mgr = MinigameManager.Instance;
        if (mgr == null) return;

        if (!mgr.IsActive)
        {
            startTimer += Time.deltaTime;
            if (startTimer >= startAttemptInterval)
            {
                startTimer = 0f;
                handSensor?.TryStartMinigame();
            }
        }
        else
        {
            mashTimer += Time.deltaTime;
            if (mashTimer >= mashInterval)
            {
                mashTimer = 0f;
                mashInput?.TriggerMash();
            }
        }
    }
}
