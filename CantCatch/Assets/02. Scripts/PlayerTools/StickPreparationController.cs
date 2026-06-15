using UnityEngine;

public class StickPreparationController : MonoBehaviour
{
    [SerializeField] private StickIceCreamController stickController;
    [SerializeField] private GameFlowManager gameFlowManager;

    private void Awake()
    {
        if (stickController == null)
        {
            stickController = GetComponent<StickIceCreamController>();
        }

        if (gameFlowManager == null)
        {
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();
        }

        if (stickController == null)
        {
            Debug.LogError("준비 상태를 확인할 막대기 컨트롤러가 없습니다.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (stickController != null)
        {
            stickController.ServingStateChanged += HandleServingStateChanged;
        }
    }

    private void Start()
    {
        HandleServingStateChanged();
    }

    private void HandleServingStateChanged()
    {
        if (stickController == null || !stickController.HasCompleteServing)
            return;

        if (gameFlowManager == null)
        {
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();
        }

        if (gameFlowManager != null && gameFlowManager.State == EGameState.Preparation)
        {
            gameFlowManager.CompletePreparation();
        }
    }

    private void OnDisable()
    {
        if (stickController != null)
        {
            stickController.ServingStateChanged -= HandleServingStateChanged;
        }
    }
}
