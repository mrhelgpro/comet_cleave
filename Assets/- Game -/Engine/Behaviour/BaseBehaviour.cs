using System;
using UnityEngine;

public abstract class BaseBehaviour : MonoBehaviour
{
    public event Action OnEnabled;
    public event Action OnDisabled;

    private bool _isEnabled;
    
    protected virtual void OnEnable()
    {
        if (CoreContext.IsReady)
        {
            _isEnabled  = true;
            Enabled();
            OnEnabled?.Invoke();
        }
    }
        
    protected virtual void OnDisable()
    {
        if (CoreContext.IsReady)
        {
            Disabled();
            ScheduleService.RunNextFrame(InvokeOnPopupDisabled);
        }
    }

    private void OnDestroy()
    {
        if (CoreContext.IsReady)
        {
            Disabled();
            ScheduleService.RunNextFrame(InvokeOnPopupDisabled);
        }
    }

    private void InvokeOnPopupDisabled()
    {
        if (_isEnabled)
        {
            _isEnabled  = false;
            OnDisabled?.Invoke();
        }
    }

    protected abstract void Enabled();
    protected abstract void Disabled();
}
