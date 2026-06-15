using UnityEngine;

public class CustomerHeadLookAt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform lookAtTarget;

    private AttachedCone trackedCone;

    private void Update()
    {
        if (trackedCone == null)
        {
            trackedCone = FindAnyObjectByType<AttachedCone>();

            if (trackedCone == null)
            {
                return;
            }
        }

        if (lookAtTarget != null)
            lookAtTarget.position = trackedCone.transform.position;
    }
}
