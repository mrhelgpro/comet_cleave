using System;
using UnityEngine;

public class UIConfirmationPopup : MonoBehaviour
{
    [SerializeField] private UIButtonClick _confirmButton;
    [SerializeField] private UIButtonClick _cancelButton;

    private Action _onConfirm;
        
    public void ShowConfirmation(Action onConfirm)
    {
        _onConfirm = onConfirm;
            
        _confirmButton.OnClick += ConfirmButtonClicked;
        _cancelButton.OnClick += CancelButtonClicked;
            
        gameObject.SetActive(true);
    }
        
    private void OnDisable()
    {
        _confirmButton.OnClick -= ConfirmButtonClicked;
        _cancelButton.OnClick -= CancelButtonClicked;
    }

    private void ConfirmButtonClicked()
    {
        _onConfirm?.Invoke();
    }

    private void CancelButtonClicked()
    {
        gameObject.SetActive(false);
    }
}
