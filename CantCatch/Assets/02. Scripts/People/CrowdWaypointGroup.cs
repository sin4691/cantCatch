using System.Collections.Generic;
using UnityEngine;

namespace PolyPerfect
{
    public class CrowdWaypointGroup : MonoBehaviour
    {
        public enum TraversalMode
        {
            Sequential,
            Random
        }

        [SerializeField] private TraversalMode traversalMode = TraversalMode.Sequential;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool includeChildWaypoints = true;
        [SerializeField] private Transform[] additionalWaypoints;
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private float gizmoRadius = 0.35f;

        private readonly List<Transform> waypointBuffer = new List<Transform>();

        public TraversalMode Mode => traversalMode;

        public int WaypointCount
        {
            get
            {
                CollectWaypoints();
                return waypointBuffer.Count;
            }
        }

        public int PathSegmentCount
        {
            get
            {
                CollectWaypoints();
                return Mathf.Max(0, waypointBuffer.Count - 1);
            }
        }

        public bool HasWaypoints => WaypointCount > 0;

        public int GetStartingIndex(bool randomizeStart)
        {
            if (!HasWaypoints)
                return -1;

            if (randomizeStart)
                return Random.Range(0, waypointBuffer.Count);

            return 0;
        }

        public bool TryGetNextWaypointIndex(int currentIndex, bool movingForward, out int nextIndex, out bool nextMovingForward)
        {
            CollectWaypoints();
            if (waypointBuffer.Count == 0)
            {
                nextIndex = -1;
                nextMovingForward = movingForward;
                return false;
            }

            if (currentIndex < 0)
            {
                nextIndex = 0;
                nextMovingForward = true;
                return true;
            }

            if (traversalMode == TraversalMode.Random)
            {
                nextMovingForward = movingForward;
                if (waypointBuffer.Count == 1)
                {
                    nextIndex = 0;
                    return true;
                }

                do
                {
                    nextIndex = Random.Range(0, waypointBuffer.Count);
                } while (nextIndex == currentIndex);

                return true;
            }

            if (waypointBuffer.Count == 1)
            {
                nextIndex = 0;
                nextMovingForward = movingForward;
                return true;
            }

            nextMovingForward = movingForward;
            var candidateIndex = currentIndex + (movingForward ? 1 : -1);
            if (candidateIndex >= 0 && candidateIndex < waypointBuffer.Count)
            {
                nextIndex = candidateIndex;
                return true;
            }

            if (loop)
            {
                nextMovingForward = !movingForward;
                nextIndex = currentIndex + (nextMovingForward ? 1 : -1);
                return true;
            }

            nextIndex = movingForward ? waypointBuffer.Count - 1 : 0;
            return false;
        }

        public bool TryGetWaypointPosition(int index, out Vector3 position)
        {
            CollectWaypoints();
            if (index < 0 || index >= waypointBuffer.Count)
            {
                position = Vector3.zero;
                return false;
            }

            position = waypointBuffer[index].position;
            return true;
        }

        public bool TryGetPathSegment(int segmentIndex, out int startWaypointIndex, out int endWaypointIndex, out Vector3 startPosition, out Vector3 endPosition)
        {
            CollectWaypoints();

            if (waypointBuffer.Count < 2 || segmentIndex < 0 || segmentIndex >= waypointBuffer.Count - 1)
            {
                startWaypointIndex = -1;
                endWaypointIndex = -1;
                startPosition = Vector3.zero;
                endPosition = Vector3.zero;
                return false;
            }

            startWaypointIndex = segmentIndex;
            endWaypointIndex = segmentIndex + 1;
            startPosition = waypointBuffer[startWaypointIndex].position;
            endPosition = waypointBuffer[endWaypointIndex].position;
            return true;
        }

        private void CollectWaypoints()
        {
            waypointBuffer.Clear();

            if (includeChildWaypoints)
            {
                for (var i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    if (child != null)
                        waypointBuffer.Add(child);
                }
            }

            if (additionalWaypoints == null)
                return;

            for (var i = 0; i < additionalWaypoints.Length; i++)
            {
                var waypoint = additionalWaypoints[i];
                if (waypoint != null && !waypointBuffer.Contains(waypoint))
                    waypointBuffer.Add(waypoint);
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            CollectWaypoints();
            if (waypointBuffer.Count == 0)
                return;

            Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.85f);

            for (var i = 0; i < waypointBuffer.Count; i++)
            {
                var current = waypointBuffer[i];
                if (!current)
                    continue;

                var currentPosition = current.position;
                Gizmos.DrawSphere(currentPosition, gizmoRadius);

                if (waypointBuffer.Count == 1)
                    continue;

                var nextIndex = i + 1;
                if (nextIndex >= waypointBuffer.Count)
                    continue;

                var next = waypointBuffer[nextIndex];
                if (next)
                    Gizmos.DrawLine(currentPosition, next.position);
            }
        }
    }
}
