using UnityEngine;

public class TimedCustomerInput : MonoBehaviour, ICustomerInput
{
    [Header("Mash Interval")]
    [SerializeField, Min(0.01f)] private float startInterval = 1f;
    [SerializeField, Min(0.01f)] private float endInterval = 0.2f;

    private float timer;
    private float currentInterval;

    public void StartMashing(float timeRatio)
    {
        currentInterval = Mathf.Lerp(startInterval, endInterval, timeRatio);
        timer = 0f;
    }

    public bool TryMash()
    {
        timer += Time.deltaTime;

        if (timer >= currentInterval)
        {
            timer = 0f;
            return true;
        }

        return false;
    }
}
