using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum MinigameSliderResult
{
    None,
    CustomerWin,
    SellerWin
}

public class MinigameSliderUI : MonoBehaviour
{
    [Header("Bar Visuals")]
    [SerializeField] private Image customerFillImage;
    [SerializeField] private Image sellerFillImage;

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float progressTweenSpeed = 8f;
    [SerializeField] private bool resetOnEnable = true;

    [Header("Debug")]
    [SerializeField] private bool enableKeyboardDebugInput;
    [SerializeField, Min(0f)] private float keyboardDebugSpeed = 1.5f;

    [Header("Timer")]
    [SerializeField, Min(0f)] private float defaultDurationSeconds = 10f;
    [SerializeField] private bool autoStartTimerOnEnable = true;
    [SerializeField] private Ease timerEase = Ease.Linear;

    private float targetProgress;
    private float displayedProgress;
    private float timeRemainingSeconds;
    private MinigameSliderResult result = MinigameSliderResult.None;
    private Tween progressTween;
    private Tween timerTween;
    private CancellationTokenSource timerCts;
    private bool timerCompletedNaturally;

    public float TargetProgress => targetProgress;
    public float DisplayedProgress => displayedProgress;
    public float TimeRemainingSeconds => timeRemainingSeconds;
    public MinigameSliderResult Result => result;
    public bool IsTimerRunning => timerTween != null && timerTween.IsActive() && timerTween.IsPlaying();

    private void OnEnable()
    {
        if (resetOnEnable)
            ResetView();
        else
            RefreshVisuals();

        if (autoStartTimerOnEnable)
            BeginSession();
    }

    private void Update()
    {
        if (enableKeyboardDebugInput)
            UpdateKeyboardDebugInput();
    }

    private void OnDisable()
    {
        StopTimer();
        KillProgressTween();
    }

    private void UpdateKeyboardDebugInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        float direction = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            direction -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            direction += 1f;

        if (!Mathf.Approximately(direction, 0f))
            AddProgress(direction * keyboardDebugSpeed * Time.unscaledDeltaTime);
    }

    public void ResetView()
    {
        StopTimer();
        KillProgressTween();

        targetProgress = 0f;
        displayedProgress = 0f;
        timeRemainingSeconds = defaultDurationSeconds;
        result = MinigameSliderResult.None;

        RefreshVisuals();
    }

    public void SetProgress(float normalizedProgress)
    {
        float nextProgress = Mathf.Clamp(normalizedProgress, -1f, 1f);
        targetProgress = nextProgress;
        result = MinigameSliderResult.None;

        KillProgressTween();

        float duration = progressTweenSpeed <= 0f
            ? 0f
            : Mathf.Abs(displayedProgress - targetProgress) / progressTweenSpeed;

        if (duration <= 0f)
        {
            displayedProgress = targetProgress;
            RefreshBarVisuals();
            return;
        }

        progressTween = DOTween.To(
                () => displayedProgress,
                value =>
                {
                    displayedProgress = value;
                    RefreshBarVisuals();
                },
                targetProgress,
                duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    public void AddProgress(float delta)
    {
        SetProgress(targetProgress + delta);
    }

    public void SetTimeRemaining(float remainingSeconds)
    {
        timeRemainingSeconds = Mathf.Max(0f, remainingSeconds);
        RefreshTimerVisuals();
    }

    public void SetDuration(float durationSeconds, bool resetRemainingTime = true)
    {
        defaultDurationSeconds = Mathf.Max(0f, durationSeconds);

        if (resetRemainingTime)
            timeRemainingSeconds = defaultDurationSeconds;

        RefreshTimerVisuals();
    }

    public void SetResult(MinigameSliderResult nextResult)
    {
        result = nextResult;
    }

    public void BeginSession(float durationSeconds = -1f)
    {
        ResetView();
        StartTimer(durationSeconds);
    }

    public void StartTimer(float durationSeconds = -1f)
    {
        if (IsTimerRunning)
            return;

        StartTimerAsync(durationSeconds).Forget();
    }

    public void StopTimer()
    {
        timerCompletedNaturally = false;

        if (timerCts != null)
        {
            timerCts.Cancel();
            timerCts.Dispose();
            timerCts = null;
        }

        if (timerTween != null)
        {
            timerTween.Kill();
            timerTween = null;
        }
    }

    public void LogCurrentWinner()
    {
        if (targetProgress > 0f)
        {
            result = MinigameSliderResult.SellerWin;
            Debug.Log("타이머 종료: 사장님 승리", this);
        }
        else if (targetProgress < 0f)
        {
            result = MinigameSliderResult.CustomerWin;
            Debug.Log("타이머 종료: 손님 승리", this);
        }
        else
        {
            result = MinigameSliderResult.None;
            Debug.Log("타이머 종료: 무승부", this);
        }
    }

    private async UniTaskVoid StartTimerAsync(float durationSeconds)
    {
        StopTimer();

        timerCompletedNaturally = false;
        timerCts = new CancellationTokenSource();
        CancellationToken token = timerCts.Token;

        timeRemainingSeconds = ResolveDuration(durationSeconds);

        RefreshTimerVisuals();

        timerTween = DOTween.To(
                () => timeRemainingSeconds,
                value =>
                {
                    timeRemainingSeconds = value;
                    RefreshTimerVisuals();
                },
                0f,
                timeRemainingSeconds)
            .SetEase(timerEase)
            .SetUpdate(true)
            .OnComplete(() => timerCompletedNaturally = true);

        try
        {
            await UniTask.WaitUntil(
                () => timerTween == null || !timerTween.IsActive(),
                cancellationToken: token);
        }
        catch (System.OperationCanceledException)
        {
            return;
        }
        finally
        {
            timerTween = null;

            timerCts?.Dispose();
            timerCts = null;
        }

        if (timerCompletedNaturally)
            LogCurrentWinner();
    }

    private void RefreshVisuals()
    {
        RefreshBarVisuals();
        RefreshTimerVisuals();
    }

    private void RefreshBarVisuals()
    {
        float sellerRatio = Mathf.InverseLerp(-1f, 1f, displayedProgress);
        float customerRatio = 1f - sellerRatio;

        if (customerFillImage != null)
            customerFillImage.fillAmount = customerRatio;

        if (sellerFillImage != null)
            sellerFillImage.fillAmount = sellerRatio;
    }

    private void RefreshTimerVisuals()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, timeRemainingSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void KillProgressTween()
    {
        if (progressTween != null)
        {
            progressTween.Kill();
            progressTween = null;
        }
    }

    private float ResolveDuration(float durationSeconds)
    {
        return durationSeconds > 0f ? durationSeconds : defaultDurationSeconds;
    }
}
