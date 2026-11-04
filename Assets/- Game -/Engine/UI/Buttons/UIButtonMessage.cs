using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIButtonMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageValue;
    [SerializeField] private CanvasGroup _canvasGroup;

    private Tween _scaleTween;
    private Tween _fadeTween;

    public void ShowMessage()
    {
        if (_messageValue == null)
        {
            return;
        }

        ResetMessage();
        StartMessage();
        FadeMessage();
    }

    private void ResetMessage()
    {
        _scaleTween?.Kill();
        _fadeTween?.Kill();
            
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        _canvasGroup.alpha = 1;
    }

    private void StartMessage()
    {
        _scaleTween = transform
            .DOScale(Vector3.one, duration: 0.25f)
            .SetEase(Ease.OutBack);
    }

    private void FadeMessage()
    {
        _fadeTween = _canvasGroup
            .DOFade(endValue: 0, duration: 1.0f)
            .SetDelay(0.75f)
            .OnComplete(HideMessage);
    }

    private void HideMessage()
    {
        gameObject.SetActive(false);
    }
}
