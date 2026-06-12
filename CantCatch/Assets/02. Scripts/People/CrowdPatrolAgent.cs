using UnityEngine;
using UnityEngine.AI;

namespace PolyPerfect
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class CrowdPatrolAgent : MonoBehaviour
    {
        private enum IdleAction
        {
            None,
            Texting,
            Calling
        }

        [SerializeField] private CrowdWaypointGroup waypointGroup;
        [SerializeField] private bool randomizeStartingWaypoint = true;
        [SerializeField] private string walkAnimationBool = "isWalking";
        [SerializeField] private string textingAnimationBool = "isTexting";
        [SerializeField] private string callingAnimationBool = "isCalling";
        [SerializeField, Min(0.1f)] private float arrivalDistance = 0.35f;
        [SerializeField, Min(0f)] private float idleDurationMin = 1.5f;
        [SerializeField, Min(0f)] private float idleDurationMax = 3f;
        [SerializeField, Min(0f)] private float idleIntervalMin = 8f;
        [SerializeField, Min(0f)] private float idleIntervalMax = 14f;
        [SerializeField, Min(0f)] private float idleWeight = 1f;
        [SerializeField, Min(0f)] private float textingWeight = 1f;
        [SerializeField, Min(0f)] private float callingWeight = 1f;

        private Animator animator;
        private NavMeshAgent navMeshAgent;
        private int currentWaypointIndex = -1;
        private int destinationWaypointIndex = -1;
        private bool movingForward = true;
        private bool isIdling;
        private float idleUntilTime;
        private float nextIdleTime;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            if (waypointGroup == null || !waypointGroup.HasWaypoints)
                return;

            if (currentWaypointIndex < 0)
                currentWaypointIndex = waypointGroup.GetStartingIndex(randomizeStartingWaypoint);

            ScheduleNextIdle();
            ContinueWalking();
        }

        private void Update()
        {
            if (waypointGroup == null || !waypointGroup.HasWaypoints)
                return;

            if (isIdling)
            {
                if (Time.time >= idleUntilTime)
                    ContinueWalking();
                return;
            }

            if (Time.time >= nextIdleTime)
            {
                BeginIdle();
                return;
            }

            if (HasReachedDestination())
            {
                currentWaypointIndex = destinationWaypointIndex;
                destinationWaypointIndex = -1;
                MoveToNextWaypoint();
            }
            else if (destinationWaypointIndex < 0)
            {
                MoveToNextWaypoint();
            }
        }

        public void SetSpawnContext(CrowdWaypointGroup group, int startingWaypointIndex, bool randomizeStart)
        {
            waypointGroup = group;
            currentWaypointIndex = startingWaypointIndex;
            randomizeStartingWaypoint = randomizeStart;
        }

        private void BeginIdle()
        {
            isIdling = true;
            idleUntilTime = Time.time + Random.Range(idleDurationMin, Mathf.Max(idleDurationMin, idleDurationMax));
            navMeshAgent.isStopped = true;
            SetWalking(false);
            SetIdleAction(ChooseIdleAction());
        }

        private void ContinueWalking()
        {
            isIdling = false;
            navMeshAgent.isStopped = false;
            SetIdleAction(IdleAction.None);
            SetWalking(true);
            ScheduleNextIdle();

            if (destinationWaypointIndex < 0)
                MoveToNextWaypoint();
        }

        private void MoveToNextWaypoint()
        {
            if (!waypointGroup.TryGetNextWaypointIndex(currentWaypointIndex, movingForward, out destinationWaypointIndex, out movingForward))
                return;

            if (!waypointGroup.TryGetWaypointPosition(destinationWaypointIndex, out var destinationPosition))
                return;

            navMeshAgent.destination = destinationPosition;
        }

        private bool HasReachedDestination()
        {
            if (destinationWaypointIndex < 0 || navMeshAgent.pathPending)
                return false;

            var threshold = Mathf.Max(arrivalDistance, navMeshAgent.stoppingDistance);
            return navMeshAgent.remainingDistance <= threshold;
        }

        private void ScheduleNextIdle()
        {
            var minInterval = Mathf.Max(0f, idleIntervalMin);
            var maxInterval = Mathf.Max(minInterval, idleIntervalMax);
            nextIdleTime = Time.time + Random.Range(minInterval, maxInterval);
        }

        private IdleAction ChooseIdleAction()
        {
            var clampedIdleWeight = Mathf.Max(0f, idleWeight);
            var clampedTextingWeight = Mathf.Max(0f, textingWeight);
            var clampedCallingWeight = Mathf.Max(0f, callingWeight);
            var totalWeight = clampedIdleWeight + clampedTextingWeight + clampedCallingWeight;

            if (totalWeight <= 0f)
                return IdleAction.None;

            var roll = Random.Range(0f, totalWeight);
            if (roll < clampedIdleWeight)
                return IdleAction.None;

            roll -= clampedIdleWeight;
            if (roll < clampedTextingWeight)
                return IdleAction.Texting;

            return IdleAction.Calling;
        }

        private void SetWalking(bool isWalking)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(walkAnimationBool))
                animator.SetBool(walkAnimationBool, isWalking);
        }

        private void SetIdleAction(IdleAction idleAction)
        {
            if (animator == null)
                return;

            SetAnimationBool(textingAnimationBool, idleAction == IdleAction.Texting);
            SetAnimationBool(callingAnimationBool, idleAction == IdleAction.Calling);
        }

        private void SetAnimationBool(string parameterName, bool value)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
                animator.SetBool(parameterName, value);
        }
    }
}
