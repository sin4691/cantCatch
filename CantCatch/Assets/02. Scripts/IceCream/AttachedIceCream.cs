using System.Collections.Generic;
using UnityEngine;

public class AttachedIceCream : MonoBehaviour
{
    private readonly HashSet<CustomerSlowController> faceTargets = new();

    public StickIceCreamController Stick { get; private set; }

    public void Initialize(StickIceCreamController stick)
    {
        Stick = stick;
    }

    public void EnterFaceTarget(CustomerSlowController target)
    {
        if (target != null)
            faceTargets.Add(target);
    }

    public void ExitFaceTarget(CustomerSlowController target)
    {
        if (target != null)
            faceTargets.Remove(target);
    }

    public bool TryGetFaceTarget(out CustomerSlowController target)
    {
        faceTargets.RemoveWhere(candidate => candidate == null);

        foreach (CustomerSlowController candidate in faceTargets)
        {
            target = candidate;
            return true;
        }

        target = null;
        return false;
    }
}
