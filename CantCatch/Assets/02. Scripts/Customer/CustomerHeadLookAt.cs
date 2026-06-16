using RootMotion.FinalIK;
using UnityEngine;

public class CustomerHeadLookAt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private LookAtIK lookAtIK;

    private AttachedCone trackedCone;
    private bool trackingEnabled = true;

    private void Awake()
    {
        if (lookAtIK == null)
            lookAtIK = GetComponentInParent<LookAtIK>();
    }

    public void SetTrackingEnabled(bool enabled)
    {
        trackingEnabled = enabled;
        if (lookAtIK != null) lookAtIK.enabled = enabled;
        if (!enabled) trackedCone = null;
    }

    private void OnEnable()
    {
        trackedCone = null;
    }

    private void Update()
    {
        if (!trackingEnabled)
            return;

        if (trackedCone == null)
        {
            trackedCone = FindAnyObjectByType<AttachedCone>();

            if (trackedCone == null)
                return;
        }

        if (lookAtTarget != null)
            lookAtTarget.position = trackedCone.transform.position;
    }
}
