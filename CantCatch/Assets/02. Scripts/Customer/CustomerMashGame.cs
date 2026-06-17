using System.Collections;
using UnityEngine;

public class CustomerMashGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CustomerArmIK armIK;
    [SerializeField] private CustomerHeadLookAt headLookAt;

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

    // MinigameManager.onCustomerWin 에 연결
    public void OnCustomerWin()
    {
        animator?.SetTrigger(winTriggerName);
        GrabCone();
        DisableTracking();
    }

    // MinigameManager.onSellerWin 에 연결
    public void OnSellerWin()
    {
        animator?.SetTrigger(loseTriggerName);
        ThrowCone();
        DisableTracking();
    }

    private void GrabCone()
    {
        var mgr = MinigameManager.Instance;
        if (mgr?.PendingCone == null) return;
        mgr.PendingCone.Stick?.TryLeaveCone(mgr.ReceivePoint);
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

    private void DisableTracking()
    {
        armIK?.SetTrackingEnabled(false);
        if (headLookAt != null) headLookAt.SetTrackingEnabled(false);
        StartCoroutine(ReenableTrackingAfterDelay());
    }

    private IEnumerator ReenableTrackingAfterDelay()
    {
        yield return new WaitForSeconds(trackingDisableDuration);
        armIK?.SetTrackingEnabled(true);
        if (headLookAt != null) headLookAt.SetTrackingEnabled(true);
    }
}
