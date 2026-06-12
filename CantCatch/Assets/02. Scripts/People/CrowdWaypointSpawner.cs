using UnityEngine;

namespace PolyPerfect
{
    public class CrowdWaypointSpawner : MonoBehaviour
    {
        [SerializeField] private CrowdWaypointGroup waypointGroup;
        [SerializeField] private GameObject[] crowdPrefabs;
        [SerializeField] private int spawnCount = 4;
        [SerializeField] private bool spawnAlongPath = true;
        [SerializeField, Range(0f, 0.45f)] private float pathSpawnPadding = 0.1f;
        [SerializeField] private bool randomizeSpawnRotation = true;
        [SerializeField] private float verticalOffset = 0f;

        private bool hasSpawned;

        private void Start()
        {
            if (hasSpawned)
                return;

            SpawnCrowd();
        }

        [ContextMenu("Spawn Crowd")]
        public void SpawnCrowd()
        {
            if (hasSpawned || waypointGroup == null || crowdPrefabs == null || crowdPrefabs.Length == 0)
                return;

            var waypointCount = waypointGroup.WaypointCount;
            if (waypointCount == 0)
                return;

            var initialIndex = waypointGroup.GetStartingIndex(true);
            var segmentCount = waypointGroup.PathSegmentCount;
            var initialSegmentIndex = segmentCount > 0 ? Random.Range(0, segmentCount) : -1;
            var crowdToSpawn = Mathf.Max(1, spawnCount);

            for (var i = 0; i < crowdToSpawn; i++)
            {
                var spawnIndex = (initialIndex + i) % waypointCount;
                var patrolStartIndex = spawnIndex;
                Vector3 spawnPosition;

                if (spawnAlongPath && segmentCount > 0)
                {
                    var segmentIndex = (initialSegmentIndex + i) % segmentCount;
                    if (!waypointGroup.TryGetPathSegment(segmentIndex, out patrolStartIndex, out _, out var segmentStart, out var segmentEnd))
                        continue;

                    var padding = Mathf.Clamp(pathSpawnPadding, 0f, 0.45f);
                    var t = Random.Range(padding, 1f - padding);
                    spawnPosition = Vector3.Lerp(segmentStart, segmentEnd, t);
                }
                else if (!waypointGroup.TryGetWaypointPosition(spawnIndex, out spawnPosition))
                {
                    continue;
                }

                var prefab = crowdPrefabs[Random.Range(0, crowdPrefabs.Length)];
                if (prefab == null)
                    continue;

                var rotation = randomizeSpawnRotation
                    ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                    : transform.rotation;

                var instance = Instantiate(prefab, spawnPosition + Vector3.up * verticalOffset, rotation, transform);

                var patrolAgent = instance.GetComponent<CrowdPatrolAgent>();
                if (patrolAgent == null)
                    patrolAgent = instance.AddComponent<CrowdPatrolAgent>();

                patrolAgent.SetSpawnContext(waypointGroup, patrolStartIndex, true);
            }

            hasSpawned = true;
        }
    }
}
