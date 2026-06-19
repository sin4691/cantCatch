using System.Collections;
using UnityEngine;

public class CustomerMashGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CustomerArmIK armIK;
    [SerializeField] private CustomerHeadLookAt headLookAt;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Transform ikTarget;
    [SerializeField] private FollowTransform nearZone;
    [SerializeField] private FollowTransform receiveZone;
    [SerializeField] private Transform pinkyBone;

    [Header("Animation")]
    [SerializeField] private string winTriggerName = "Win";
    [SerializeField] private string loseTriggerName = "Throw";

    [Header("Throw Settings")]
    [SerializeField] private Vector3 throwDirection = new Vector3(0f, 1f, 1f);
    [SerializeField] private float throwForce = 5f;
    [SerializeField, Min(0f)] private float throwReleaseDelay = 0.5f;
    [SerializeField] private float coneDestroyDelay = 2f;

    [Header("Tracking")]
    [SerializeField, Min(0f)] private float trackingDisableDuration = 2f;

    private void Awake()
    {
        if (armIK == null) armIK = GetComponentInParent<CustomerArmIK>();
        if (headLookAt == null) headLookAt = GetComponentInParent<CustomerHeadLookAt>();
    }

    // MinigameManager.onMinigameStarted 에 연결
    public void OnMinigameStart()
    {
        if (grabPoint != null && ikTarget != null)
            grabPoint.position = ikTarget.position;

        if (headLookAt != null) headLookAt.SetTrackingEnabled(false);
    }

    // MinigameManager.onCustomerWin 에 연결
    public void OnCustomerWin()
    {
        animator?.SetTrigger(winTriggerName);
        SwitchZoneToPinky();
        GrabCone();
        DisableTracking();
    }

    // MinigameManager.onSellerWin 에 연결
    public void OnSellerWin()
    {
        animator?.SetTrigger(loseTriggerName);
        SwitchZoneToPinky();
        ThrowCone();
        DisableTracking();
    }

    private void GrabCone()
    {
        var mgr = MinigameManager.Instance;
        if (mgr?.PendingCone == null) return;
        mgr.PendingCone.Stick?.TryLeaveCone(grabPoint != null ? grabPoint : mgr.ReceivePoint);
    }

    private void ThrowCone()
    {
        var mgr = MinigameManager.Instance;
        if (mgr?.PendingCone == null) return;
        mgr.PendingCone.Stick?.TryLeaveCone(mgr.ReceivePoint);
        StartCoroutine(ApplyThrowForce(mgr.PendingCone));
    }

    private IEnumerator ApplyThrowForce(AttachedCone cone)
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

    private void SwitchZoneToPinky()
    {
        nearZone?.SetOverrideTarget(pinkyBone);
        receiveZone?.SetOverrideTarget(pinkyBone);
    }

    private void DisableTracking()
    {
        armIK?.SetTrackingEnabled(false);
        if (headLookAt != null) headLookAt.SetTrackingEnabled(false);
        StartCoroutine(ReenableTrackingAfterDelay());
    }

    private IEnumerator ReenableTrackingAfterDelay()
    {
        yield return new WaitForSeconds(trackingDisableDuration);
        armIK?.Unfreeze();
        nearZone?.ResetToDefault();
        receiveZone?.ResetToDefault();
        if (headLookAt != null) headLookAt.SetTrackingEnabled(true);
    }
}
