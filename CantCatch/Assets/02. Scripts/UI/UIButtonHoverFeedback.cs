using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private GameObject outlineObject;
    [SerializeField] private Transform scaleTarget;

    [Header("Scale")]
    [SerializeField, Min(0.01f)] private float hoverScaleMultiplier = 1.06f;
    [SerializeField, Min(0f)] private float scaleDuration = 0.12f;
    [SerializeField] private Ease hoverEase = Ease.OutQuad;
    [SerializeField] private Ease exitEase = Ease.OutQuad;

    private Vector3 initialScale = Vector3.one;
    private Tween scaleTween;

    private void Reset()
    {
        scaleTarget = transform;
    }

    private void Awake()
    {
        if (scaleTarget == null)
            scaleTarget = transform;

        initialScale = scaleTarget.localScale;
    }

    private void OnEnable()
    {
        SetOutline(false);
        ResetScaleInstant();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetOutline(true);
        PlayScaleTween(initialScale * hoverScaleMultiplier, hoverEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetOutline(false);
        PlayScaleTween(initialScale, exitEase);
    }

    private void OnDisable()
    {
        SetOutline(false);
        ResetScaleInstant();
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }

    private void SetOutline(bool isActive)
    {
        if (outlineObject != null)
            outlineObject.SetActive(isActive);
    }

    private void PlayScaleTween(Vector3 targetScale, Ease ease)
    {
        if (scaleTarget == null)
            return;

        scaleTween?.Kill();
        scaleTween = scaleTarget
            .DOScale(targetScale, scaleDuration)
            .SetEase(ease)
            .SetUpdate(true);
    }

    private void ResetScaleInstant()
    {
        if (scaleTarget == null)
            return;

        scaleTween?.Kill();
        scaleTarget.localScale = initialScale;
    }
}
