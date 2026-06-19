using UnityEngine;

public class CustomerBodyTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform body;
    [SerializeField] private CustomerArmIK armIK;

    [Header("Tracking")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
    [SerializeField, Min(0f)] private float maxDistance = 0.5f;
    [SerializeField, Min(0f)] private float reachThreshold = 0.8f;

    private Vector3 originPosition;
    private AttachedCone trackedCone;
    private GameFlowManager gameFlowManager;

    private void Awake()
    {
        if (body == null)
            body = transform;

        gameFlowManager = GameFlowManager.Instance;
        originPosition = body.position;
    }

    private void Update()
    {
        if (!CanTrackCone())
        {
            trackedCone = null;
            return;
        }

        if (trackedCone == null)
        {
            trackedCone = FindAnyObjectByType<AttachedCone>();
        }

        if (armIK == null || armIK.IsAtMaxReach)
        {
            Vector3 targetPosition = GetTargetPosition();
            body.position = Vector3.MoveTowards(body.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    private bool CanTrackCone()
    {
        if (gameFlowManager == null)
            gameFlowManager = GameFlowManager.Instance;

        return gameFlowManager != null && gameFlowManager.State == EGameState.Playing;
    }

    private Vector3 GetTargetPosition()
    {
        if (trackedCone == null)
            return originPosition;

        float coneLocalX = transform.InverseTransformPoint(trackedCone.transform.position).x;
        float distance = Vector3.Distance(body.position, trackedCone.transform.position);
        float moveAmount = Mathf.Min(Mathf.Max(distance - reachThreshold, 0f), maxDistance);
        float direction = Mathf.Sign(coneLocalX);

        return body.position + transform.right * direction * moveAmount;
    }
}
