using System;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class UIGameplayDefeatPopup : BaseBehaviour
    {
        [SerializeField] private UIButtonClick _uiButtonClick;
        [SerializeField] private CanvasGroup _backgroundCanvasGroup;

        private Action _onCompleted;

        public void Open(Action onCompleted)
        {
            _onCompleted = onCompleted;
            gameObject.SetActive(true);
            _backgroundCanvasGroup.alpha = 0f;
            _backgroundCanvasGroup.DOFade(1f, 0.35f).SetUpdate(true).SetLink(gameObject);
        }

        protected override void Enabled()
        {
            _uiButtonClick.OnClick += ButtonClicked;
        }

        protected override void Disabled()
        {
            _uiButtonClick.OnClick -= ButtonClicked;
            _uiButtonClick.transform.DOKill();
            _backgroundCanvasGroup.DOKill();
            _onCompleted = null;
        }

        private void ButtonClicked()
        {
            var onCompleted = _onCompleted;
            gameObject.SetActive(false);
            onCompleted?.Invoke();
        }
    }
}
