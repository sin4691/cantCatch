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

        if (stickController == null)
        {
            Debug.LogError("막대기 컨트롤러를 찾을 수 없습니다.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (!other.gameObject.CompareTag("IceCreamTub"))
            return;

        stickController.TryAttachIceCream();
    }
}
