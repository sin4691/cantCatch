using UnityEngine;

public class AttachedCone : MonoBehaviour
{
    public StickIceCreamController Stick { get; private set; }

    public void Initialize(StickIceCreamController stick)
    {
        Stick = stick;
    }

    public void Detach()
    {
        Stick = null;
    }
}
