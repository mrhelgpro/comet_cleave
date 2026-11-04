using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class UIButtonDelayAppear : MonoBehaviour
{
    [SerializeField] private UIButtonClick _buttonClick;
    [SerializeField] private RectTransform _animationTarget;
    [SerializeField] private float _delayDuration;
    [SerializeField] private float _delayedAppearDuration;
        
    private TweenerCore<Vector3, Vector3, VectorOptions> _tweener;
    private RectTransform _rectTransform;

    private void OnEnable()
    {
        if (_rectTransform == null)
        {
            _rectTransform = _animationTarget != null 
                ? _animationTarget 
                : GetComponent<RectTransform>();
        }
            
        if (_delayDuration > 0f && _delayedAppearDuration > 0f)
        {
            _tweener?.Kill();
            _tweener = null;
            _rectTransform.DOKill();
            _rectTransform.localScale = Vector3.zero;
            _buttonClick.IsInteractable = false;

            _tweener = _rectTransform.DOScale(Vector3.one, _delayedAppearDuration)
                .SetDelay(_delayDuration)
                .From(Vector3.zero)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _buttonClick.IsInteractable = true;
                });
        }
    }

    private void OnDisable()
    {
        _tweener?.Rewind();
        _buttonClick.IsInteractable = true;
    }

    private void OnDestroy()
    {
        _rectTransform?.DOKill();
    }
}