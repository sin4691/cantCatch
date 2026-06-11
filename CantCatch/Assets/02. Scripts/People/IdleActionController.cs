using UnityEngine;

public class IdleActionController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "DoAction";
    [SerializeField] private int minIdleLoops = 2;
    [SerializeField] private int maxIdleLoops = 4;

    private int idleLoopCount;
    private int targetLoopCount;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ResetTargetLoopCount();
    }

    public void OnIdleLoopEnd()
    {
        idleLoopCount++;

        if (idleLoopCount >= targetLoopCount)
        {
            animator.SetTrigger(triggerName);
            idleLoopCount = 0;
            ResetTargetLoopCount();
        }
    }

    private void ResetTargetLoopCount()
    {
        targetLoopCount = Random.Range(minIdleLoops, maxIdleLoops + 1);
    }
}