using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset skillInputActions;
    [SerializeField] private string skillActionMapName = "Player";
    [SerializeField] private string coneSkillActionName = "Attack";
    [SerializeField] private string rotationSkillActionName = "RotateSkill";
    [SerializeField] private string faceSkillActionName = "FaceSkill";

    [Header("Skill Target")]
    [SerializeField] private SingleConeQteController singleConeQteController;
    [SerializeField] private StickIceCreamController stickController;
    [SerializeField] private GameFlowManager gameFlowManager;

    [Header("180 Rotation Skill")]
    [SerializeField, Min(0.01f)] private float rotationHoldDuration = 1f;
    [SerializeField, Min(0f)] private float rotationSkillCooldown = 1f;

    [Header("Face Slow Skill")]
    [SerializeField, Min(1)] private int maxFaceSkillUses = 2;
    [SerializeField, Range(0.01f, 0.99f)] private float faceSlowMultiplier = 0.5f;
    [SerializeField, Min(0.01f)] private float faceSlowDuration = 3f;
    [SerializeField, Min(0f)] private float faceSkillCooldown = 20f;

    private InputAction coneSkillAction;
    private InputAction rotationSkillAction;
    private InputAction faceSkillAction;
    private float rotationCooldownRemaining;
    private float faceCooldownRemaining;
    private bool enabledConeSkillAction;
    private bool enabledRotationSkillAction;
    private bool enabledFaceSkillAction;
    private EGameState previousGameState = EGameState.Idle;

    public float RotationCooldownRemaining => rotationCooldownRemaining;
    public float FaceCooldownRemaining => faceCooldownRemaining;
    public int FaceSkillUsesRemaining { get; private set; }

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

        if (singleConeQteController == null)
            singleConeQteController = FindAnyObjectByType<SingleConeQteController>();

        if (gameFlowManager == null)
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();

        if (skillInputActions == null)
        {
            Debug.LogError("플레이어 스킬 입력 액션 에셋이 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        coneSkillAction = FindSkillAction(coneSkillActionName, ref isValid);
        rotationSkillAction = FindSkillAction(rotationSkillActionName, ref isValid);
        faceSkillAction = FindSkillAction(faceSkillActionName, ref isValid);
        FaceSkillUsesRemaining = maxFaceSkillUses;
        enabled = isValid;
    }

    private void Update()
    {
        rotationCooldownRemaining = Mathf.Max(
            0f,
            rotationCooldownRemaining - Time.deltaTime);
        faceCooldownRemaining = Mathf.Max(
            0f,
            faceCooldownRemaining - Time.deltaTime);

        if (gameFlowManager != null && gameFlowManager.State != previousGameState)
        {
            previousGameState = gameFlowManager.State;

            if (previousGameState == EGameState.Preparation)
                ResetFaceSkillUses();
        }
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
        EnableAction(faceSkillAction, OnFaceSkillPerformed, ref enabledFaceSkillAction);
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
        singleConeQteController?.SubmitInput();
    }

    private void OnRotationSkillPerformed(InputAction.CallbackContext context)
    {
        if (rotationCooldownRemaining > 0f ||
            stickController == null ||
            gameFlowManager == null ||
            gameFlowManager.State != EGameState.Playing)
        {
            return;
        }

        if (!stickController.TryRotateServing(rotationHoldDuration))
            return;

        rotationCooldownRemaining = rotationSkillCooldown;
        Debug.Log(
            $"180도 회전 스킬 사용: {rotationHoldDuration:0.##}초 유지, " +
            $"쿨타임 {rotationSkillCooldown:0.##}초",
            this);
    }

    private void OnFaceSkillPerformed(InputAction.CallbackContext context)
    {
        if (FaceSkillUsesRemaining <= 0 ||
            faceCooldownRemaining > 0f ||
            stickController == null ||
            gameFlowManager == null ||
            gameFlowManager.State != EGameState.Playing ||
            !stickController.TryGetFaceSkillTarget(out CustomerSlowController target) ||
            !target.ApplySlow(faceSlowMultiplier, faceSlowDuration))
        {
            return;
        }

        FaceSkillUsesRemaining--;
        faceCooldownRemaining = faceSkillCooldown;
        Debug.Log(
            $"얼굴 아이스크림 스킬 사용: {faceSlowDuration:0.##}초 동안 " +
            $"속도 {faceSlowMultiplier:P0}, 쿨타임 {faceSkillCooldown:0.##}초, " +
            $"남은 횟수 {FaceSkillUsesRemaining}",
            this);
    }

    public void ResetFaceSkillUses()
    {
        FaceSkillUsesRemaining = maxFaceSkillUses;
        faceCooldownRemaining = 0f;
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
        DisableAction(faceSkillAction, OnFaceSkillPerformed, ref enabledFaceSkillAction);
        rotationCooldownRemaining = 0f;
        faceCooldownRemaining = 0f;
    }
}
