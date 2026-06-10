using UnityEngine;

public class IceCreamAttachTrigger : MonoBehaviour
{
    [SerializeField] private StickIceCreamController stickController;

    private void Reset()
    {
        stickController = GetComponentInParent<StickIceCreamController>();
    }

    private void Awake()
    {
        if (stickController == null)
        {
            stickController = GetComponentInParent<StickIceCreamController>();

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IceCreamTub tub = other.GetComponentInParent<IceCreamTub>();

        if (tub == null)
            return;

        stickController.AttachIceCream();
    }
}