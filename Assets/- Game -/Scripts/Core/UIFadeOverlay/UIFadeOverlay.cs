using System;
using DG.Tweening;
using UnityEngine;

public class UIFadeOverlay : BaseBehaviour, IInitializable, IDisposable
{
    [SerializeField] private CanvasGroup _canvasGroup;
    
    private UIQueuePopup _uiQueuePopup;

    [Inject]
    public void Inject(UIQueuePopup uiQueuePopup)
    {
        _uiQueuePopup = uiQueuePopup;
    }

    public void Initialize()
    {
        SceneLoadService.OnLoading += ShowOverlay;
        SceneLoadService.OnLoaded += HideOverlay;
    }

    public void Dispose()
    {
        SceneLoadService.OnLoading -= ShowOverlay;
        SceneLoadService.OnLoaded -= HideOverlay;
    }
    
    protected override void Enabled()
    {
        _uiQueuePopup.SetActivePopup(this);
    }

    protected override void Disabled()
    {
    }
    
    private void ShowOverlay()
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1;
    }
    
    private void HideOverlay()
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1;
        _canvasGroup.DOFade(0f, 0.25f)
            .SetUpdate(true)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
