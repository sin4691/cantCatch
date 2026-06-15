using UnityEngine;

public class CustomerSlowController : MonoBehaviour
{
    public float SpeedMultiplier { get; private set; } = 1f;
    public float SlowTimeRemaining { get; private set; }
    public bool IsSlowed => SlowTimeRemaining > 0f;

    private void Update()
    {
        if (!IsSlowed)
            return;

        SlowTimeRemaining = Mathf.Max(0f, SlowTimeRemaining - Time.deltaTime);

        if (SlowTimeRemaining <= 0f)
            SpeedMultiplier = 1f;
    }

    public bool ApplySlow(float speedMultiplier, float duration)
    {
        if (speedMultiplier <= 0f || speedMultiplier >= 1f || duration <= 0f)
            return false;

        SpeedMultiplier = speedMultiplier;
        SlowTimeRemaining = duration;
        return true;
    }

    private void OnDisable()
    {
        SpeedMultiplier = 1f;
        SlowTimeRemaining = 0f;
    }
}
