using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DefaultExecutionOrder(9999)]
public class GrabLock : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private bool isLocked;
    private Transform trackedInteractor;
    private Vector3 localOffset;
    private Quaternion localRotOffset;

    private bool isFrozen;
    private Vector3 frozenPosition;
    private Quaternion frozenRotation;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        grabInteractable.throwOnDetach = false;
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (isLocked) return;
        isLocked = true;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!isLocked) return;

        // 어느 손이 놓든 항상 마지막 손 기준 offset 갱신
        Transform interactor = (args.interactorObject as MonoBehaviour)?.transform;
        if (interactor == null) return;

        trackedInteractor = interactor;
        localOffset = interactor.InverseTransformPoint(transform.position);
        localRotOffset = Quaternion.Inverse(interactor.rotation) * transform.rotation;
    }

    private void FixedUpdate()
    {
        if (!isFrozen || rb == null) return;
        rb.MovePosition(frozenPosition);
        rb.MoveRotation(frozenRotation);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (isFrozen)
        {
            if (rb != null && !rb.isKinematic) rb.isKinematic = true;
            transform.SetPositionAndRotation(frozenPosition, frozenRotation);
            return;
        }

        if (!isLocked) return;

        // XRI가 잡고 있으면 XRI한테 맡김
        if (grabInteractable.interactorsSelecting.Count > 0) return;

        // 아무도 안 잡고 있으면 마지막 손 위치 기준으로 유지
        if (trackedInteractor == null) return;

        if (rb != null && !rb.isKinematic) rb.isKinematic = true;

        transform.SetPositionAndRotation(
            trackedInteractor.TransformPoint(localOffset),
            trackedInteractor.rotation * localRotOffset
        );
    }

    public void Freeze()
    {
        frozenPosition = transform.position;
        frozenRotation = transform.rotation;
        isFrozen = true;
        Application.onBeforeRender += EnforceFreeze;
    }

    public void Unfreeze()
    {
        isFrozen = false;
        Application.onBeforeRender -= EnforceFreeze;
    }

    [BeforeRenderOrder(int.MaxValue)]
    private void EnforceFreeze()
    {
        transform.SetPositionAndRotation(frozenPosition, frozenRotation);
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void Unlock()
    {
        isLocked = false;
        isFrozen = false;
        trackedInteractor = null;

        if (rb != null)
            rb.isKinematic = false;
    }
}
