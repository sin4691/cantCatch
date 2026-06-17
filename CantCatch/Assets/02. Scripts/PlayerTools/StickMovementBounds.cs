using UnityEngine;

public class StickMovementBounds : MonoBehaviour
{
    [SerializeField] private Vector3 halfExtents = new Vector3(0.5f, 0.4f, 0.5f);

    private void Start()
    {
        CreateWall(new Vector3(halfExtents.x, 0, 0), new Vector3(0.01f, halfExtents.y * 2, halfExtents.z * 2));
        CreateWall(new Vector3(-halfExtents.x, 0, 0), new Vector3(0.01f, halfExtents.y * 2, halfExtents.z * 2));
        CreateWall(new Vector3(0, halfExtents.y, 0), new Vector3(halfExtents.x * 2, 0.01f, halfExtents.z * 2));
        CreateWall(new Vector3(0, -halfExtents.y, 0), new Vector3(halfExtents.x * 2, 0.01f, halfExtents.z * 2));
        CreateWall(new Vector3(0, 0, halfExtents.z), new Vector3(halfExtents.x * 2, halfExtents.y * 2, 0.01f));
        CreateWall(new Vector3(0, 0, -halfExtents.z), new Vector3(halfExtents.x * 2, halfExtents.y * 2, 0.01f));
    }

    private void CreateWall(Vector3 localPos, Vector3 size)
    {
        var wall = new GameObject("BoundsWall");
        wall.transform.SetParent(transform, false);
        wall.transform.localPosition = localPos;
        wall.layer = gameObject.layer;

        var col = wall.AddComponent<BoxCollider>();
        col.size = size;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, halfExtents * 2f);
        Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
    }
}
