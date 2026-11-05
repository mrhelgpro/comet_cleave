using System;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class UIGameplayStartPopup : BaseBehaviour
    {
        [SerializeField] private UIButtonClick _uiButtonClick;

        private Action _onCompleted;
        
        protected override void Enabled()
        {
            _uiButtonClick.OnClick += ButtonClicked;
        }

        protected override void Disabled()
        {
            _uiButtonClick.OnClick -= ButtonClicked;
            _uiButtonClick.transform.DOKill();
            _onCompleted = null;
        }
        
        public void Open(Action onCompleted)
        {
            _onCompleted = onCompleted;
            gameObject.SetActive(true);
        }

        private void ButtonClicked()
        {
            var onCompleted = _onCompleted;
            gameObject.SetActive(false);
            onCompleted?.Invoke();
        }
    }
}
