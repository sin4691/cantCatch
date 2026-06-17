using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class FadeCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField, Min(0f)] private float defaultFadeDuration = 0.5f;
    [SerializeField] private bool startFilled;
    private Tween fadeTween;

    private void Awake()
    {
        ResolveCanvasGroup();
        ApplyImmediateState(startFilled ? 1f : 0f);
    }

    private void OnDestroy()
    {
        KillTween();
    }

    public UniTask FadeOutAsync(float duration = -1f)
    {
        return FadeToAsync(1f, ResolveDuration(duration));
    }

    public UniTask FadeInAsync(float duration = -1f)
    {
        return FadeToAsync(0f, ResolveDuration(duration));
    }

    private async UniTask FadeToAsync(float targetAlpha, float duration)
    {
        ResolveCanvasGroup();
        if (canvasGroup == null)
        {
            Debug.LogError("FadeCanvas에 CanvasGroup이 연결되어 있지 않습니다.", this);
            return;
        }

        KillTween();
        SetInputBlocking(targetAlpha > 0f);

        if (duration <= 0f)
        {
            ApplyImmediateState(targetAlpha);
            return;
        }

        fadeTween = canvasGroup
            .DOFade(targetAlpha, duration)
            .SetEase(Ease.Linear)
            .SetUpdate(true);

        await fadeTween.AsyncWaitForCompletion();
        fadeTween = null;

        SetInputBlocking(targetAlpha > 0f);
    }

    private float ResolveDuration(float duration)
    {
        return duration >= 0f ? duration : defaultFadeDuration;
    }

    private void ApplyImmediateState(float alpha)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = alpha;
        SetInputBlocking(alpha > 0f);
    }

    private void SetInputBlocking(bool isBlocking)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.blocksRaycasts = isBlocking;
        canvasGroup.interactable = isBlocking;
    }

    private void ResolveCanvasGroup()
    {
        if (canvasGroup != null)
            return;

        canvasGroup = GetComponentInChildren<CanvasGroup>(true);
    }

    private void KillTween()
    {
        if (fadeTween == null)
            return;

        if (fadeTween.IsActive())
            fadeTween.Kill();

        fadeTween = null;
    }
}
