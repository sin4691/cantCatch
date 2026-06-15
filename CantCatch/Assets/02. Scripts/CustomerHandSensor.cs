using System.Collections.Generic;
using UnityEngine;

public class CustomerHandSensor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private SingleConeQteController singleConeQteController;
    [SerializeField] private Transform coneReceivePoint;

    [Header("Near Score")]
    [SerializeField, Min(0.01f)] private float nearScoreInterval = 1f;
    [SerializeField, Min(1)] private int nearScorePerInterval = 1;

    private readonly Dictionary<AttachedCone, int> nearContacts = new();
    private readonly Dictionary<AttachedCone, int> receiveContacts = new();
    private readonly List<AttachedCone> invalidCones = new();

    private float nearScoreTimer;

    public bool HasConeInReceiveZone => receiveContacts.Count > 0;

    private void Awake()
    {
        bool isValid = true;

        if (gameFlowManager == null || !gameFlowManager.gameObject.scene.IsValid())
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();

        if (singleConeQteController == null)
            singleConeQteController = FindAnyObjectByType<SingleConeQteController>();

        if (gameFlowManager == null)
        {
            Debug.LogError("씬에서 GameFlowManager를 찾을 수 없습니다.", this);
            isValid = false;
        }

        if (singleConeQteController == null)
        {
            Debug.LogError("씬에서 SingleConeQteController를 찾을 수 없습니다.", this);
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
            singleConeQteController.TryStart(cone, coneReceivePoint);
    }

    public void Exit(CustomerHandZoneType zoneType, AttachedCone cone)
    {
        if (cone == null)
            return;

        Dictionary<AttachedCone, int> contacts = GetContacts(zoneType);
        if (!contacts.TryGetValue(cone, out int contactCount))
            return;

        if (contactCount <= 1)
            contacts.Remove(cone);
        else
            contacts[cone] = contactCount - 1;

        if (zoneType == CustomerHandZoneType.Near && nearContacts.Count == 0)
            nearScoreTimer = 0f;
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
        return zoneType == CustomerHandZoneType.Near
            ? nearContacts
            : receiveContacts;
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
            contacts.Remove(cone);
    }

    private void OnDisable()
    {
        nearContacts.Clear();
        receiveContacts.Clear();
        nearScoreTimer = 0f;
    }
}
