using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset skillInputActions;
    [SerializeField] private string skillActionMapName = "Player";
    [SerializeField] private string coneSkillActionName = "Attack";
    [SerializeField] private string rotationSkillActionName = "RotateSkill";

    [Header("Skill Target")]
    [SerializeField] private CustomerHandSensor handSensor;
    [SerializeField] private StickIceCreamController stickController;

    [Header("180 Rotation Skill")]
    [SerializeField, Min(0.01f)] private float rotationHoldDuration = 1f;
    [SerializeField, Min(0f)] private float rotationSkillCooldown = 1f;

    private InputAction coneSkillAction;
    private InputAction rotationSkillAction;
    private float rotationCooldownRemaining;
    private bool enabledConeSkillAction;
    private bool enabledRotationSkillAction;

    public float RotationCooldownRemaining => rotationCooldownRemaining;

    private void Awake()
    {
        bool isValid = true;

        if (stickController == null)
        {
            stickController = GetComponent<StickIceCreamController>();
        }

        if (stickController == null)
        {
            Debug.LogError("180도 회전 스킬을 사용할 막대기 컨트롤러가 없습니다.", this);
            isValid = false;
        }

        if (skillInputActions == null)
        {
            Debug.LogError("플레이어 스킬 입력 액션 에셋이 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        coneSkillAction = FindSkillAction(coneSkillActionName, ref isValid);
        rotationSkillAction = FindSkillAction(rotationSkillActionName, ref isValid);
        enabled = isValid;
    }

    private void Update()
    {
        rotationCooldownRemaining = Mathf.Max(
            0f,
            rotationCooldownRemaining - Time.deltaTime);
    }

    private InputAction FindSkillAction(string actionName, ref bool isValid)
    {
        InputAction action = skillInputActions.FindAction(
            $"{skillActionMapName}/{actionName}",
            false);

        if (action == null)
        {
            Debug.LogError(
                $"플레이어 스킬 입력 액션을 찾을 수 없습니다: {skillActionMapName}/{actionName}",
                this);
            isValid = false;
        }

        return action;
    }

    private void OnEnable()
    {
        EnableAction(coneSkillAction, OnConeSkillPerformed, ref enabledConeSkillAction);
        EnableAction(rotationSkillAction, OnRotationSkillPerformed, ref enabledRotationSkillAction);
    }

    private void EnableAction(
        InputAction action,
        System.Action<InputAction.CallbackContext> callback,
        ref bool enabledByController)
    {
        if (action == null)
            return;

        action.performed += callback;

        if (!action.enabled)
        {
            action.Enable();
            enabledByController = true;
        }
    }

    private void OnConeSkillPerformed(InputAction.CallbackContext context)
    {
        if (TryGetHandSensor(out CustomerHandSensor targetSensor))
        {
            targetSensor.TryUseConeSkill();
        }
    }

    private void OnRotationSkillPerformed(InputAction.CallbackContext context)
    {
        if (rotationCooldownRemaining > 0f || stickController == null)
            return;

        if (!stickController.TryRotateServing(rotationHoldDuration))
            return;

        rotationCooldownRemaining = rotationSkillCooldown;
        Debug.Log(
            $"180도 회전 스킬 사용: {rotationHoldDuration:0.##}초 유지, " +
            $"쿨타임 {rotationSkillCooldown:0.##}초",
            this);
    }

    private bool TryGetHandSensor(out CustomerHandSensor targetSensor)
    {
        if (handSensor == null)
        {
            handSensor = FindAnyObjectByType<CustomerHandSensor>();
        }

        targetSensor = handSensor;
        return targetSensor != null && targetSensor.isActiveAndEnabled;
    }

    private void DisableAction(
        InputAction action,
        System.Action<InputAction.CallbackContext> callback,
        ref bool enabledByController)
    {
        if (action == null)
            return;

        action.performed -= callback;

        if (enabledByController)
        {
            action.Disable();
            enabledByController = false;
        }
    }

    private void OnDisable()
    {
        DisableAction(coneSkillAction, OnConeSkillPerformed, ref enabledConeSkillAction);
        DisableAction(rotationSkillAction, OnRotationSkillPerformed, ref enabledRotationSkillAction);
        rotationCooldownRemaining = 0f;
    }
}
