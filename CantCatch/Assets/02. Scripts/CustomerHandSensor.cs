using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomerHandSensor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private Transform coneReceivePoint;
    [SerializeField] private InputActionAsset skillInputActions;
    [SerializeField] private string skillActionMapName = "Player";
    [SerializeField] private string coneSkillActionName = "Attack";
    [SerializeField] private string rotationSkillActionName = "RotateSkill";

    [Header("Near Score")]
    [SerializeField, Min(0.01f)] private float nearScoreInterval = 1f;
    [SerializeField, Min(1)] private int nearScorePerInterval = 1;

    [Header("Skill Window")]
    [SerializeField, Min(0.01f)] private float skillInputWindow = 1f;

    [Header("Cone Leave Skill")]
    [SerializeField, Min(0f)] private float coneSkillCooldown = 5f;
    [SerializeField, Min(1)] private int coneSkillSuccessScore = 10;

    [Header("180 Rotation Skill")]
    [SerializeField, Min(0.01f)] private float rotationHoldDuration = 1f;
    [SerializeField, Min(0f)] private float rotationSkillCooldown = 3f;

    private readonly Dictionary<AttachedCone, int> nearContacts = new();
    private readonly Dictionary<AttachedCone, int> receiveContacts = new();
    private readonly List<AttachedCone> invalidCones = new();

    private InputAction coneSkillAction;
    private InputAction rotationSkillAction;
    private AttachedCone pendingCone;
    private StickIceCreamController waitingStick;
    private float nearScoreTimer;
    private float skillTimeRemaining;
    private float coneCooldownRemaining;
    private float rotationCooldownRemaining;
    private bool isSkillWindowOpen;
    private bool enabledConeSkillAction;
    private bool enabledRotationSkillAction;

    public bool HasConeInReceiveZone => receiveContacts.Count > 0;
    public bool IsSkillWindowOpen => isSkillWindowOpen;
    public bool IsWaitingForCone => waitingStick != null && !waitingStick.HasCone;
    public float SkillTimeRemaining => skillTimeRemaining;
    public float ConeCooldownRemaining => coneCooldownRemaining;
    public float RotationCooldownRemaining => rotationCooldownRemaining;

    private void Awake()
    {
        bool isValid = true;

        if (gameFlowManager == null || !gameFlowManager.gameObject.scene.IsValid())
        {
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();
        }

        if (gameFlowManager == null)
        {
            Debug.LogError("씬에서 GameFlowManager를 찾을 수 없습니다.", this);
            isValid = false;
        }

        if (coneReceivePoint == null)
        {
            Debug.LogError("콘을 남겨둘 ConeReceivePoint가 연결되지 않았습니다.", this);
            isValid = false;
        }

        if (skillInputActions == null)
        {
            Debug.LogError("스킬 입력 액션 에셋이 연결되지 않았습니다.", this);
            isValid = false;
        }
        else
        {
            coneSkillAction = FindSkillAction(coneSkillActionName, ref isValid);
            rotationSkillAction = FindSkillAction(rotationSkillActionName, ref isValid);
        }

        enabled = isValid;
    }

    private InputAction FindSkillAction(string actionName, ref bool isValid)
    {
        InputAction action = skillInputActions.FindAction(
            $"{skillActionMapName}/{actionName}",
            false);

        if (action == null)
        {
            Debug.LogError(
                $"스킬 입력 액션을 찾을 수 없습니다: {skillActionMapName}/{actionName}",
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
        ref bool enabledBySensor)
    {
        if (action == null)
            return;

        action.performed += callback;

        if (!action.enabled)
        {
            action.Enable();
            enabledBySensor = true;
        }
    }

    private void Update()
    {
        RemoveDestroyedContacts(nearContacts);
        RemoveDestroyedContacts(receiveContacts);

        if (gameFlowManager.IsGameOver)
        {
            nearScoreTimer = 0f;
            return;
        }

        UpdateWaitingState();
        UpdateCooldowns();
        UpdateSkillWindow();
        UpdateNearScore();
    }

    public void Enter(CustomerHandZoneType zoneType, AttachedCone cone)
    {
        if (cone == null || gameFlowManager.IsGameOver)
            return;

        Dictionary<AttachedCone, int> contacts = GetContacts(zoneType);
        bool wasAlreadyContacting = contacts.TryGetValue(cone, out int contactCount);
        contacts[cone] = contactCount + 1;

        if (zoneType == CustomerHandZoneType.Receive && !wasAlreadyContacting)
        {
            BeginSkillWindow(cone);
        }
    }

    public void Exit(CustomerHandZoneType zoneType, AttachedCone cone)
    {
        if (cone == null)
            return;

        Dictionary<AttachedCone, int> contacts = GetContacts(zoneType);
        if (!contacts.TryGetValue(cone, out int contactCount))
            return;

        if (contactCount <= 1)
        {
            contacts.Remove(cone);
        }
        else
        {
            contacts[cone] = contactCount - 1;
        }

        if (zoneType == CustomerHandZoneType.Near && nearContacts.Count == 0)
        {
            nearScoreTimer = 0f;
        }
    }

    private void BeginSkillWindow(AttachedCone cone)
    {
        if (coneCooldownRemaining > 0f && rotationCooldownRemaining > 0f)
        {
            gameFlowManager.GameOver("모든 스킬이 쿨타임인 상태에서 콘이 수령 범위에 닿았습니다.");
            return;
        }

        if (isSkillWindowOpen)
            return;

        pendingCone = cone;
        skillTimeRemaining = skillInputWindow;
        isSkillWindowOpen = true;

        Debug.Log($"스킬 입력 시작: {skillInputWindow:0.##}초", this);
    }

    private void OnConeSkillPerformed(InputAction.CallbackContext context)
    {
        if (!CanUsePendingSkill() || coneCooldownRemaining > 0f)
            return;

        AttachedCone completedCone = pendingCone;
        StickIceCreamController stick = completedCone.Stick;

        if (!stick.TryLeaveCone(coneReceivePoint))
        {
            FailSkill("콘을 손님 손 위치에 남기지 못했습니다.");
            return;
        }

        CompleteSkill(completedCone);
        waitingStick = stick;
        coneCooldownRemaining = coneSkillCooldown;
        gameFlowManager.AddScore(coneSkillSuccessScore);

        Debug.Log($"콘만 남기기 성공: +{coneSkillSuccessScore}점", this);
    }

    private void OnRotationSkillPerformed(InputAction.CallbackContext context)
    {
        if (!CanUsePendingSkill() || rotationCooldownRemaining > 0f)
            return;

        AttachedCone completedCone = pendingCone;

        if (!completedCone.Stick.TryRotateServing(rotationHoldDuration))
        {
            FailSkill("막대기를 180도 회전하지 못했습니다.");
            return;
        }

        CompleteSkill(completedCone);
        rotationCooldownRemaining = rotationSkillCooldown;

        Debug.Log($"180도 회전 성공: {rotationHoldDuration:0.##}초 뒤 복원", this);
    }

    private bool CanUsePendingSkill()
    {
        if (!isSkillWindowOpen || gameFlowManager.IsGameOver)
            return false;

        if (pendingCone != null && pendingCone.Stick != null)
            return true;

        FailSkill("스킬 대상 콘 또는 막대기를 찾을 수 없습니다.");
        return false;
    }

    private void CompleteSkill(AttachedCone completedCone)
    {
        ResetSkillWindow();
        nearContacts.Remove(completedCone);
        receiveContacts.Remove(completedCone);
        nearScoreTimer = 0f;
    }

    private void UpdateWaitingState()
    {
        if (waitingStick == null)
            return;

        if (waitingStick.HasCone)
        {
            waitingStick = null;
            Debug.Log("콘 리필 완료: 손님이 다시 받을 준비를 합니다.", this);
        }
    }

    private void UpdateCooldowns()
    {
        coneCooldownRemaining = Mathf.Max(0f, coneCooldownRemaining - Time.deltaTime);
        rotationCooldownRemaining = Mathf.Max(0f, rotationCooldownRemaining - Time.deltaTime);
    }

    private void UpdateSkillWindow()
    {
        if (!isSkillWindowOpen)
            return;

        if (pendingCone == null)
        {
            FailSkill("입력 대기 중인 콘이 사라졌습니다.");
            return;
        }

        skillTimeRemaining -= Time.deltaTime;

        if (skillTimeRemaining <= 0f)
        {
            FailSkill("1초 안에 스킬을 사용하지 못했습니다.");
        }
    }

    private void UpdateNearScore()
    {
        if (nearContacts.Count == 0)
        {
            nearScoreTimer = 0f;
            return;
        }

        nearScoreTimer += Time.deltaTime;

        while (nearScoreTimer >= nearScoreInterval)
        {
            nearScoreTimer -= nearScoreInterval;
            gameFlowManager.AddScore(nearScorePerInterval);
        }
    }

    private void FailSkill(string reason)
    {
        ResetSkillWindow();
        gameFlowManager.GameOver(reason);
    }

    private void ResetSkillWindow()
    {
        pendingCone = null;
        skillTimeRemaining = 0f;
        isSkillWindowOpen = false;
    }

    private Dictionary<AttachedCone, int> GetContacts(CustomerHandZoneType zoneType)
    {
        return zoneType == CustomerHandZoneType.Near
            ? nearContacts
            : receiveContacts;
    }

    private void RemoveDestroyedContacts(Dictionary<AttachedCone, int> contacts)
    {
        invalidCones.Clear();

        foreach (AttachedCone cone in contacts.Keys)
        {
            if (cone == null)
            {
                invalidCones.Add(cone);
            }
        }

        foreach (AttachedCone cone in invalidCones)
        {
            contacts.Remove(cone);
        }
    }

    private void DisableAction(
        InputAction action,
        System.Action<InputAction.CallbackContext> callback,
        ref bool enabledBySensor)
    {
        if (action == null)
            return;

        action.performed -= callback;

        if (enabledBySensor)
        {
            action.Disable();
            enabledBySensor = false;
        }
    }

    private void OnDisable()
    {
        DisableAction(coneSkillAction, OnConeSkillPerformed, ref enabledConeSkillAction);
        DisableAction(rotationSkillAction, OnRotationSkillPerformed, ref enabledRotationSkillAction);

        nearContacts.Clear();
        receiveContacts.Clear();
        waitingStick = null;
        nearScoreTimer = 0f;
        coneCooldownRemaining = 0f;
        rotationCooldownRemaining = 0f;
        ResetSkillWindow();
    }
}
