using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private bool _isInteractable = true;
    [SerializeField] private bool _isActionable = true;
    
    [Header("SoundFX")]
    [SerializeField] private SoundAsset _actionSoundAsset;
    [SerializeField] private SoundAsset _messageSoundAsset;
    
    [Header("Option")]
    [SerializeField] private UIButtonMessage _buttonMessage;
    [SerializeField] private RectTransform _animationTarget;
        
    public event Action OnClick;
        
    private RectTransform _rectTransform;
    private Vector3 _defaultScale;
    private Tween _shakeTween;
        
    public bool IsInteractable
    {
        get => _isInteractable;
        set => _isInteractable = value;
    }

    public bool IsActionable
    {
        get => _isActionable;
        set => _isActionable = value;
    }
        
    private void OnEnable()
    {
        if (_rectTransform == null)
        {
            _rectTransform = _animationTarget != null ? _animationTarget : GetComponent<RectTransform>();
        }

        _defaultScale = _rectTransform.localScale;
    }

    private void OnDestroy()
    {
        _rectTransform?.DOKill(); 
        _shakeTween?.Kill();
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable)
        {
            DownScale();
        }
    }
    
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (IsInteractable)
        {
            DefaultScale();
        }
    }
        
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (IsInteractable)
        {
            DefaultScale();
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (IsInteractable)
        {
            OnButtonClicked();
        }
    }

    private void ShowMessage()
    {
        AudioService.PlaySfx(_messageSoundAsset);
            
        _shakeTween?.Kill();

        if (_buttonMessage != null)
        {
            _buttonMessage.ShowMessage();
        }
            
        _shakeTween = transform.DOShakeRotation(
                duration: 0.5f, 
                strength: new Vector3(0, 0, 5), 
                vibrato: 20, 
                randomness: 25)
            .SetUpdate(true)
            .OnComplete(() => transform.rotation = Quaternion.identity); 
    }

    private void OnButtonClicked()
    {
        DefaultScale();
            
        if (IsActionable)
        {
            AudioService.PlaySfx(_actionSoundAsset);
            OnClick?.Invoke();
                
            return;
        }
            
        ShowMessage();
    }

    private void DownScale()
    {
        _rectTransform.DOScale(_defaultScale * 0.95f, 0.1f)
            .SetUpdate(true);
    }

    private void DefaultScale() 
    {
        _rectTransform.DOScale(_defaultScale, 0.1f)
            .SetUpdate(true);
    }
}