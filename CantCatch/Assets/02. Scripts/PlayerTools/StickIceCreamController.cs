using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Action = System.Action;

public class StickIceCreamController : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Transform skillVisualRoot;
    [SerializeField] private Vector3 rotationSkillAxis = Vector3.up;

    [Header("Ice Cream")]
    [SerializeField] private GameObject iceCreamPrefab;
    [SerializeField] private Transform iceCreamAttachPoint;

    [Header("Cone")]
    [SerializeField] private GameObject conePrefab;
    [SerializeField] private Transform coneAttachPoint;

    private GameObject currentIceCream;
    private AttachedIceCream attachedIceCream;
    private GameObject currentCone;
    private Coroutine rotationSkillRoutine;
    private Quaternion visualOriginalRotation;
    private Vector3 iceCreamAttachOriginalPosition;
    private Quaternion iceCreamAttachOriginalRotation;
    private Vector3 coneAttachOriginalPosition;
    private Quaternion coneAttachOriginalRotation;
    private bool isConfigured;

    public bool HasIceCream => currentIceCream != null;
    public bool HasCone => currentCone != null;
    public bool HasCompleteServing => HasIceCream && HasCone;
    public bool IsHeldWithTwoHands =>
        grabInteractable != null &&
        grabInteractable.interactorsSelecting.Count >= 2;
    public bool IsRotationSkillActive => rotationSkillRoutine != null;

    public event Action ServingStateChanged;

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        isConfigured = ValidateReferences();
        enabled = isConfigured;

        if (isConfigured)
        {
            visualOriginalRotation = skillVisualRoot.localRotation;
            iceCreamAttachOriginalPosition = iceCreamAttachPoint.localPosition;
            iceCreamAttachOriginalRotation = iceCreamAttachPoint.localRotation;
            coneAttachOriginalPosition = coneAttachPoint.localPosition;
            coneAttachOriginalRotation = coneAttachPoint.localRotation;
        }
    }

    public bool TryAttachIceCream()
    {
        if (!isConfigured || !IsHeldWithTwoHands || HasIceCream)
            return false;

        currentIceCream = InstantiateAttached(iceCreamPrefab, iceCreamAttachPoint);
        attachedIceCream = currentIceCream.GetComponent<AttachedIceCream>();

        if (attachedIceCream == null)
            attachedIceCream = currentIceCream.AddComponent<AttachedIceCream>();

        attachedIceCream.Initialize(this);
        NotifyServingStateChanged();

        Debug.Log("막대기에 아이스크림 생성");
        return true;
    }

    public bool TryAttachCone()
    {
        if (!isConfigured || !IsHeldWithTwoHands || !HasIceCream || HasCone)
            return false;

        currentCone = InstantiateAttached(conePrefab, coneAttachPoint);
        AttachedCone attachedCone = currentCone.GetComponent<AttachedCone>();
        attachedCone.Initialize(this);
        NotifyServingStateChanged();
        GameFlowManager.Instance?.CompletePreparation();

        Debug.Log("아이스크림에 콘 생성");
        return true;
    }

    public bool TryLeaveCone(Transform receivePoint)
    {
        if (!isConfigured || !HasCone || receivePoint == null)
            return false;

        AttachedCone attachedCone = currentCone.GetComponent<AttachedCone>();
        attachedCone.Detach();
        SetCollidersEnabled(currentCone, false);
        currentCone.transform.SetParent(receivePoint, false);
        currentCone.transform.localPosition = Vector3.zero;
        currentCone.transform.localRotation = Quaternion.identity;
        currentCone = null;
        NotifyServingStateChanged();

        Debug.Log("손님 손 위치에 콘만 남김");
        return true;
    }

    public bool TryDestroyCone()
    {
        if (!isConfigured || !HasCone)
            return false;

        Destroy(currentCone);
        currentCone = null;
        NotifyServingStateChanged();
        return true;
    }

    public bool TryRotateServing(float holdDuration)
    {
        if (!isConfigured || IsRotationSkillActive || holdDuration <= 0f)
            return false;

        rotationSkillRoutine = StartCoroutine(RotateServingRoutine(holdDuration));
        return true;
    }

    public bool TryGetFaceSkillTarget(out CustomerSlowController target)
    {
        if (attachedIceCream != null && attachedIceCream.TryGetFaceTarget(out target))
            return true;

        target = null;
        return false;
    }

    public void Clear()
    {
        bool hadServing = HasIceCream || HasCone;

        if (currentIceCream != null)
        {
            Destroy(currentIceCream);
            currentIceCream = null;
            attachedIceCream = null;
        }

        if (currentCone != null)
        {
            Destroy(currentCone);
            currentCone = null;
        }

        if (hadServing)
            NotifyServingStateChanged();
    }



    private void NotifyServingStateChanged()
    {
        ServingStateChanged?.Invoke();
    }

    private GameObject InstantiateAttached(GameObject prefab, Transform attachPoint)
    {
        GameObject instance = Instantiate(prefab, attachPoint);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        return instance;
    }

    private float currentRotationAngle;

    private IEnumerator RotateServingRoutine(float duration)
    {
        float startAngle = currentRotationAngle;
        float targetAngle = currentRotationAngle + 180f;
        float elapsed = 0f;
        Vector3 axis = rotationSkillAxis.normalized;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            float angle = Mathf.LerpAngle(startAngle, targetAngle, t);
            ApplyServingRotation(Quaternion.AngleAxis(angle, axis));
            yield return null;
        }

        currentRotationAngle = targetAngle % 360f;
        ApplyServingRotation(Quaternion.AngleAxis(currentRotationAngle, axis));
        rotationSkillRoutine = null;
    }

    private void ApplyServingRotation(Quaternion rotation)
    {
        skillVisualRoot.localRotation = rotation * visualOriginalRotation;
        iceCreamAttachPoint.localPosition = rotation * iceCreamAttachOriginalPosition;
        iceCreamAttachPoint.localRotation = rotation * iceCreamAttachOriginalRotation;
        coneAttachPoint.localPosition = rotation * coneAttachOriginalPosition;
        coneAttachPoint.localRotation = rotation * coneAttachOriginalRotation;
    }

    private void RestoreServingRotation()
    {
        skillVisualRoot.localRotation = visualOriginalRotation;
        iceCreamAttachPoint.localPosition = iceCreamAttachOriginalPosition;
        iceCreamAttachPoint.localRotation = iceCreamAttachOriginalRotation;
        coneAttachPoint.localPosition = coneAttachOriginalPosition;
        coneAttachPoint.localRotation = coneAttachOriginalRotation;
    }

    private void SetCollidersEnabled(GameObject target, bool isEnabled)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        foreach (Collider targetCollider in colliders)
            targetCollider.enabled = isEnabled;
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        isValid &= ValidateReference(grabInteractable, nameof(grabInteractable));
        isValid &= ValidateReference(skillVisualRoot, nameof(skillVisualRoot));
        isValid &= ValidateReference(iceCreamPrefab, nameof(iceCreamPrefab));
        isValid &= ValidateReference(iceCreamAttachPoint, nameof(iceCreamAttachPoint));
        isValid &= ValidateReference(conePrefab, nameof(conePrefab));
        isValid &= ValidateReference(coneAttachPoint, nameof(coneAttachPoint));

        if (iceCreamPrefab != null && conePrefab != null && iceCreamPrefab == conePrefab)
        {
            Debug.LogError("아이스크림 프리팹과 콘 프리팹에 같은 오브젝트가 연결되어 있습니다.", this);
            isValid = false;
        }

        if (iceCreamAttachPoint != null && coneAttachPoint != null && iceCreamAttachPoint == coneAttachPoint)
        {
            Debug.LogError("아이스크림과 콘 부착 지점에 같은 Transform이 연결되어 있습니다.", this);
            isValid = false;
        }

        if (iceCreamPrefab != null && iceCreamPrefab.GetComponentInChildren<Collider>(true) == null)
        {
            Debug.LogError("아이스크림 프리팹에 Collider가 없습니다.", this);
            isValid = false;
        }

        if (conePrefab != null && conePrefab.GetComponentInChildren<Collider>(true) == null)
        {
            Debug.LogError("콘 프리팹에 Collider가 없습니다.", this);
            isValid = false;
        }

        if (conePrefab != null && conePrefab.GetComponent<AttachedCone>() == null)
        {
            Debug.LogError("콘 프리팹에 AttachedCone 컴포넌트가 없습니다.", this);
            isValid = false;
        }

        if (iceCreamAttachPoint != null && !iceCreamAttachPoint.IsChildOf(transform))
        {
            Debug.LogError("아이스크림 부착 지점은 막대기의 자식이어야 합니다.", this);
            isValid = false;
        }

        if (coneAttachPoint != null && !coneAttachPoint.IsChildOf(transform))
        {
            Debug.LogError("콘 부착 지점은 막대기의 자식이어야 합니다.", this);
            isValid = false;
        }

        if (rotationSkillAxis.sqrMagnitude <= Mathf.Epsilon)
        {
            Debug.LogError("180도 회전 스킬의 회전축이 0입니다.", this);
            isValid = false;
        }

        return isValid;
    }

    private void OnDisable()
    {
        if (rotationSkillRoutine == null)
            return;

        StopCoroutine(rotationSkillRoutine);
        rotationSkillRoutine = null;
        currentRotationAngle = 0f;
        RestoreServingRotation();
    }

    private bool ValidateReference(Object reference, string fieldName)
    {
        if (reference != null)
            return true;

        Debug.LogError($"{nameof(StickIceCreamController)}의 {fieldName} 참조가 비어 있습니다.", this);
        return false;
    }
}
