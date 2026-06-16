using UnityEngine;

public class CustomerMashGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CustomerArmIK armIK;
    [SerializeField] private CustomerHeadLookAt headLookAt;

    [Header("Mash Settings")]
    [SerializeField, Min(1)] private int requiredMashCount = 5;
    [SerializeField] private string winTriggerName = "Win";
    [SerializeField] private string loseTriggerName = "Throw";

    [Header("Throw Settings")]
    [SerializeField] private Vector3 throwDirection = new Vector3(0f, 1f, 1f);
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float coneDestroyDelay = 2f;

    [Header("Mash Speed")]
    [SerializeField, Min(0.01f)] private float startInterval = 1f;
    [SerializeField, Min(0.01f)] private float endInterval = 0.2f;

    private ICustomerInput customerInput;
    private int currentMashCount;
    private bool isActive;
    private float mashTimer;
    private float currentInterval;

    private void Awake()
    {
        customerInput = GetComponent<ICustomerInput>();

        if (armIK == null) armIK = GetComponentInParent<CustomerArmIK>();
        if (headLookAt == null) headLookAt = GetComponentInParent<CustomerHeadLookAt>();
    }

    public void StartMashing()
    {
        isActive = true;
        currentMashCount = 0;
        mashTimer = 0f;

        float timeRatio = GameFlowManager.Instance != null
            ? Mathf.Clamp01(GameFlowManager.Instance.ElapsedTime / GameFlowManager.Instance.GameDuration)
            : 0f;

        currentInterval = Mathf.Lerp(startInterval, endInterval, timeRatio);

        Debug.Log($"손님 연타 시작: 간격 {currentInterval:0.##}초", this);
    }

    public void StopMashing()
    {
        isActive = false;
    }

    public void OnCustomerWin()
    {
        StopMashing();
        animator?.SetTrigger(winTriggerName);
        GrabCone();
        DisableTracking();
    }

    public void OnSellerWin()
    {
        StopMashing();
        animator?.SetTrigger(loseTriggerName);
        ThrowCone();
        DisableTracking();
    }

    private void GrabCone()
    {
        var mgr = TimingGameManager.Instance;
        if (mgr?.PendingCone == null) return;
        mgr.PendingCone.Stick.TryLeaveCone(mgr.ReceivePoint);
    }

    [SerializeField, Min(0f)] private float throwReleaseDelay = 0.5f;

    private void ThrowCone()
    {
        var mgr = TimingGameManager.Instance;
        if (mgr?.PendingCone == null) return;

        mgr.PendingCone.Stick?.TryLeaveCone(mgr.ReceivePoint);
        StartCoroutine(ApplyThrowForce(mgr.PendingCone));
    }

    private System.Collections.IEnumerator ApplyThrowForce(AttachedCone cone)
    {
        yield return new WaitForSeconds(throwReleaseDelay);
        if (cone == null) yield break;

        cone.transform.SetParent(null, true);

        Rigidbody rb = cone.GetComponent<Rigidbody>();
        if (rb == null)
            rb = cone.gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.AddForce(transform.TransformDirection(throwDirection.normalized) * throwForce, ForceMode.Impulse);
        Destroy(cone.gameObject, coneDestroyDelay);
    }

    [SerializeField, Min(0f)] private float trackingDisableDuration = 2f;

    private void DisableTracking()
    {
        armIK?.SetTrackingEnabled(false);
        if (headLookAt != null) headLookAt.SetTrackingEnabled(false);
        StartCoroutine(ReenableTrackingAfterDelay());
    }

    private System.Collections.IEnumerator ReenableTrackingAfterDelay()
    {
        yield return new WaitForSeconds(trackingDisableDuration);
        armIK?.SetTrackingEnabled(true);
        if (headLookAt != null) headLookAt.SetTrackingEnabled(true);
    }

    private void Update()
    {
        if (!isActive)
            return;

        mashTimer += Time.deltaTime;

        if (mashTimer >= currentInterval)
        {
            mashTimer = 0f;
            currentMashCount++;
            Debug.Log($"연타: {currentMashCount}/{requiredMashCount}", this);

            if (currentMashCount >= requiredMashCount)
            {
                isActive = false;
                animator?.SetTrigger(winTriggerName);
                TimingGameManager.Instance?.OnCustomerWin();
            }
        }
    }
}
