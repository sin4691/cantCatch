using UnityEngine;

public class ConeAttachTrigger : MonoBehaviour
{
    [SerializeField] private StickIceCreamController stickController;

    private void Reset()
    {
        stickController = GetComponentInParent<StickIceCreamController>();
    }

    private void Awake()
    {
        if (stickController == null)
            stickController = GetComponentInParent<StickIceCreamController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        ConeBox coneBox = other.GetComponentInParent<ConeBox>();

        if (coneBox == null)
            return;

        stickController.AttachCone();
    }
}