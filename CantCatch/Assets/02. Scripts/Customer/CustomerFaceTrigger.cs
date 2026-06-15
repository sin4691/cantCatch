using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CustomerFaceTrigger : MonoBehaviour
{
    [SerializeField] private CustomerSlowController slowController;

    private void Reset()
    {
        slowController = GetComponentInParent<CustomerSlowController>();

        Collider faceCollider = GetComponent<Collider>();
        faceCollider.isTrigger = true;
    }

    private void Awake()
    {
        if (slowController == null)
            slowController = GetComponentInParent<CustomerSlowController>();

        if (slowController == null)
        {
            Debug.LogError("얼굴 스킬을 적용할 손님 감속 컨트롤러가 없습니다.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        AttachedIceCream iceCream = other.GetComponentInParent<AttachedIceCream>();
        iceCream?.EnterFaceTarget(slowController);
    }

    private void OnTriggerExit(Collider other)
    {
        AttachedIceCream iceCream = other.GetComponentInParent<AttachedIceCream>();
        iceCream?.ExitFaceTarget(slowController);
    }
}
