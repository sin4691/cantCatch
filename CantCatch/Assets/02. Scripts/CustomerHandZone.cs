using UnityEngine;

public enum CustomerHandZoneType
{
    Near,
    Receive
}

[RequireComponent(typeof(Collider))]
public class CustomerHandZone : MonoBehaviour
{
    [SerializeField] private CustomerHandZoneType zoneType;
    [SerializeField] private CustomerHandSensor handSensor;

    private void Reset()
    {
        handSensor = GetComponentInParent<CustomerHandSensor>();

        Collider zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void Awake()
    {
        if (handSensor == null)
        {
            handSensor = GetComponentInParent<CustomerHandSensor>();
        }

        if (handSensor == null)
        {
            Debug.LogError("손님 손 센서를 찾을 수 없습니다.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        AttachedCone cone = other.GetComponentInParent<AttachedCone>();
        if (!IsCompleteServing(cone))
            return;

        handSensor.Enter(zoneType, cone);
    }

    private void OnTriggerExit(Collider other)
    {
        AttachedCone cone = other.GetComponentInParent<AttachedCone>();
        if (cone == null)
            return;

        handSensor.Exit(zoneType, cone);
    }

    private bool IsCompleteServing(AttachedCone cone)
    {
        return cone != null &&
               cone.Stick != null &&
               cone.Stick.HasCompleteServing;
    }
}
