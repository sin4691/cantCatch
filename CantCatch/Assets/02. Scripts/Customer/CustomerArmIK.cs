using RootMotion.FinalIK;
using UnityEngine;

public class CustomerArmIK : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LimbIK limbIK;
    [SerializeField] private Transform ikTarget;
    [SerializeField] private GameFlowManager gameFlowManager;

    [Header("Tracking Speed")]
    [SerializeField, Min(0.1f)] private float speed = 5f;
    [SerializeField, Min(0.1f)] private float speedAtEnd = 15f;

    [Header("Reach Limit")]
    [SerializeField, Min(0f)] private float maxReach = 1f;
    [SerializeField, Min(0f)] private float forwardOffset = 0.3f;


    public bool IsAtMaxReach { get; private set; }

    private AttachedCone trackedCone;

    private void Awake()
    {
        if (limbIK == null)
            limbIK = GetComponentInChildren<LimbIK>();

        if (gameFlowManager == null)
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();

        SetIKActive(false);
    }

    private void Update()
    {
        if (trackedCone == null)
        {
            trackedCone = FindAnyObjectByType<AttachedCone>();

            if (trackedCone == null)
            {
                SetIKActive(false);
                return;
            }

            SetIKActive(true);
        }

        Vector3 clampedTarget = ClampTarget(trackedCone.transform.position);
        float distance = Vector3.Distance(ikTarget.position, clampedTarget);
        float timeRatio = gameFlowManager != null ? Mathf.Clamp01(gameFlowManager.ElapsedTime / gameFlowManager.GameDuration) : 0f;
        float currentSpeed = Mathf.Lerp(speed, speedAtEnd, timeRatio);

        ikTarget.position = Vector3.MoveTowards(ikTarget.position, clampedTarget, currentSpeed * Time.deltaTime);

    }

    private Vector3 ClampTarget(Vector3 worldTarget)
    {
        Vector3 localTarget = transform.InverseTransformPoint(worldTarget);

        // 뒤로 가지 못하게 앞쪽으로 고정
        localTarget.z = Mathf.Max(localTarget.z, forwardOffset);

        // 최대 도달 거리 제한
        if (localTarget.magnitude > maxReach)
        {
            localTarget = localTarget.normalized * maxReach;
            IsAtMaxReach = true;
        }
        else
        {
            IsAtMaxReach = false;
        }

        return transform.TransformPoint(localTarget);
    }

    public void SetIKActive(bool active)
    {
        if (limbIK != null)
            limbIK.enabled = active;
    }
}
