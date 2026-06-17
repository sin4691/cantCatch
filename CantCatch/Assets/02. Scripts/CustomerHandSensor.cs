using System.Collections.Generic;
using UnityEngine;

public class CustomerHandSensor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private Transform coneReceivePoint;

    [Header("Grab Cooldown")]
    [SerializeField, Min(0f)] private float grabCooldown = 0.5f;

    [Header("Near Score")]
    [SerializeField, Min(0.01f)] private float nearScoreInterval = 1f;
    [SerializeField, Min(1)] private int nearScorePerInterval = 1;

    private readonly Dictionary<AttachedCone, int> nearContacts = new();
    private readonly Dictionary<AttachedCone, int> receiveContacts = new();
    private readonly List<AttachedCone> invalidCones = new();
    private readonly HashSet<AttachedCone> rotationEvadeAwardedCones = new();

    private float nearScoreTimer;
    private float grabCooldownRemaining;

    public bool HasConeInReceiveZone => receiveContacts.Count > 0;

    private void Awake()
    {
        if (gameFlowManager == null || !gameFlowManager.gameObject.scene.IsValid())
            gameFlowManager = GameFlowManager.Instance;

        if (gameFlowManager == null)
            Debug.LogError("씬에서 GameFlowManager를 찾을 수 없습니다.", this);

        if (coneReceivePoint == null)
            Debug.LogError("콘을 남겨둘 ConeReceivePoint가 연결되지 않았습니다.", this);
    }

    private void Update()
    {
        grabCooldownRemaining = Mathf.Max(0f, grabCooldownRemaining - Time.deltaTime);
        RemoveDestroyedContacts(nearContacts);
        RemoveDestroyedContacts(receiveContacts);
        UpdateNearScore();
    }

    public void Enter(CustomerHandZoneType zoneType, AttachedCone cone)
    {
        if (cone == null || gameFlowManager.State != EGameState.Playing)
            return;

        Dictionary<AttachedCone, int> contacts = GetContacts(zoneType);
        bool wasAlreadyContacting = contacts.TryGetValue(cone, out int contactCount);
        contacts[cone] = contactCount + 1;

        if (zoneType == CustomerHandZoneType.Receive && !wasAlreadyContacting)
            rotationEvadeAwardedCones.Remove(cone);
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
            if (zoneType == CustomerHandZoneType.Receive)
                rotationEvadeAwardedCones.Remove(cone);
        }
        else
        {
            contacts[cone] = contactCount - 1;
        }

        if (zoneType == CustomerHandZoneType.Near && nearContacts.Count == 0)
            nearScoreTimer = 0f;
    }

    public bool TryStartMinigame()
    {
        if (gameFlowManager == null ||
            gameFlowManager.State != EGameState.Playing ||
            grabCooldownRemaining > 0f)
            return false;

        AttachedCone cone = GetFirstValidReceiveCone();
        if (cone == null)
            return false;

        grabCooldownRemaining = grabCooldown;
        return MinigameManager.Instance != null && MinigameManager.Instance.StartMinigame(cone, coneReceivePoint);
    }

    public bool TryConsumeRotationEvade(StickIceCreamController stickController)
    {
        if (stickController == null ||
            gameFlowManager == null ||
            gameFlowManager.State != EGameState.Playing)
            return false;

        foreach (AttachedCone cone in receiveContacts.Keys)
        {
            if (cone == null ||
                cone.Stick != stickController ||
                rotationEvadeAwardedCones.Contains(cone))
                continue;

            rotationEvadeAwardedCones.Add(cone);
            return true;
        }

        return false;
    }

    private void UpdateNearScore()
    {
        if (gameFlowManager.State != EGameState.Playing || nearContacts.Count == 0)
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

    private Dictionary<AttachedCone, int> GetContacts(CustomerHandZoneType zoneType)
    {
        return zoneType == CustomerHandZoneType.Near ? nearContacts : receiveContacts;
    }

    private AttachedCone GetFirstValidReceiveCone()
    {
        foreach (AttachedCone cone in receiveContacts.Keys)
        {
            if (cone != null && cone.Stick != null)
                return cone;
        }
        return null;
    }

    private void RemoveDestroyedContacts(Dictionary<AttachedCone, int> contacts)
    {
        invalidCones.Clear();

        foreach (AttachedCone cone in contacts.Keys)
        {
            if (cone == null || cone.Stick == null)
                invalidCones.Add(cone);
        }

        foreach (AttachedCone cone in invalidCones)
        {
            contacts.Remove(cone);
            rotationEvadeAwardedCones.Remove(cone);
        }
    }

    private void OnDisable()
    {
        nearContacts.Clear();
        receiveContacts.Clear();
        rotationEvadeAwardedCones.Clear();
        nearScoreTimer = 0f;
        grabCooldownRemaining = 0f;
    }
}
