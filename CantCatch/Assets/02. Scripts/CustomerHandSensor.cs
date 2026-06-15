using System.Collections.Generic;
using UnityEngine;

public class CustomerHandSensor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private Transform coneReceivePoint;

    [Header("Near Score")]
    [SerializeField, Min(0.01f)] private float nearScoreInterval = 1f;
    [SerializeField, Min(1)] private int nearScorePerInterval = 1;

    [Header("Skill Window")]
    [SerializeField, Min(0.01f)] private float skillInputWindow = 1f;

    [Header("Cone Leave Skill")]
    [SerializeField, Min(0f)] private float coneSkillCooldown = 5f;
    [SerializeField, Min(1)] private int coneSkillSuccessScore = 10;

    private readonly Dictionary<AttachedCone, int> nearContacts = new();
    private readonly Dictionary<AttachedCone, int> receiveContacts = new();
    private readonly List<AttachedCone> invalidCones = new();

    private AttachedCone pendingCone;
    private StickIceCreamController waitingStick;
    private float nearScoreTimer;
    private float skillTimeRemaining;
    private float coneCooldownRemaining;
    private bool isSkillWindowOpen;

    public bool HasConeInReceiveZone => receiveContacts.Count > 0;
    public bool IsSkillWindowOpen => isSkillWindowOpen;
    public bool IsWaitingForCone => waitingStick != null && !waitingStick.HasCone;
    public float SkillTimeRemaining => skillTimeRemaining;
    public float ConeCooldownRemaining => coneCooldownRemaining;

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

        enabled = isValid;
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
        if (isSkillWindowOpen)
            return;

        pendingCone = cone;
        skillTimeRemaining = skillInputWindow;
        isSkillWindowOpen = true;

        Debug.Log($"스킬 입력 시작: {skillInputWindow:0.##}초", this);
    }

    public void TryUseConeSkill()
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
        SetWaitingStick(stick);
        coneCooldownRemaining = coneSkillCooldown;
        gameFlowManager.AddScore(coneSkillSuccessScore);

        Debug.Log($"콘만 남기기 성공: +{coneSkillSuccessScore}점", this);
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

    private void SetWaitingStick(StickIceCreamController stick)
    {
        if (waitingStick != null)
        {
            waitingStick.ServingStateChanged -= HandleWaitingStickStateChanged;
        }

        waitingStick = stick;

        if (waitingStick != null)
        {
            waitingStick.ServingStateChanged += HandleWaitingStickStateChanged;
            HandleWaitingStickStateChanged();
        }
    }

    private void HandleWaitingStickStateChanged()
    {
        if (waitingStick == null || !waitingStick.HasCone)
            return;

        Debug.Log("콘 리필 완료: 손님이 다시 받을 준비를 합니다.", this);
        SetWaitingStick(null);
    }

    private void UpdateCooldowns()
    {
        coneCooldownRemaining = Mathf.Max(0f, coneCooldownRemaining - Time.deltaTime);
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

    private void OnDisable()
    {
        nearContacts.Clear();
        receiveContacts.Clear();
        SetWaitingStick(null);
        nearScoreTimer = 0f;
        coneCooldownRemaining = 0f;
        ResetSkillWindow();
    }
}
