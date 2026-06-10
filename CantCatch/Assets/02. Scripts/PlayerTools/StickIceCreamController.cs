using UnityEngine;

public class StickIceCreamController : MonoBehaviour
{
    [Header("Ice Cream")]
    [SerializeField] private GameObject iceCreamPrefab;
    [SerializeField] private Transform iceCreamAttachPoint;

    [Header("Cone")]
    [SerializeField] private GameObject conePrefab;
    [SerializeField] private Transform coneAttachPoint;

    private GameObject currentIceCream;
    private GameObject currentCone;

    public bool HasIceCream => currentIceCream != null;
    public bool HasCone => currentCone != null;

    public void AttachIceCream()
    {
        if (HasIceCream)
            return;

        currentIceCream = Instantiate(iceCreamPrefab);
        currentIceCream.transform.SetParent(iceCreamAttachPoint, false);
        currentIceCream.transform.localPosition = Vector3.zero;

        Rigidbody rb = currentIceCream.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("막대기에 아이스크림 생성");
    }

    public void AttachCone()
    {
        if (!HasIceCream)
            return;

        if (HasCone)
            return;

        currentCone = Instantiate(conePrefab);
        currentCone.transform.SetParent(coneAttachPoint, false);
        currentCone.transform.localPosition = Vector3.zero;

        Rigidbody rb = currentCone.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("아이스크림에 콘 생성");
    }

    public void RemoveIceCreamAndCone()
    {
        if (currentIceCream != null)
        {
            Destroy(currentIceCream);
            currentIceCream = null;
        }

        if (currentCone != null)
        {
            Destroy(currentCone);
            currentCone = null;
        }
    }
}