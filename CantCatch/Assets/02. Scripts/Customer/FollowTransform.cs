using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform defaultTarget;
    [SerializeField] private Transform overrideTarget;

    private Transform activeTarget;

    private void Awake()
    {
        activeTarget = defaultTarget;
    }

    public void SetOverrideTarget(Transform target)
    {
        overrideTarget = target;
        activeTarget = overrideTarget != null ? overrideTarget : defaultTarget;
    }

    public void ResetToDefault()
    {
        activeTarget = defaultTarget;
    }

    private void LateUpdate()
    {
        if (activeTarget == null)
            return;

        transform.position = activeTarget.position;
    }
}
