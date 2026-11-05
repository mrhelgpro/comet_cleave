using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class UIGameplayVictoryPopup : BaseBehaviour
    {
        [SerializeField] private UIButtonClick _uiButtonClick;
        [SerializeField] private CanvasGroup _backgroundCanvasGroup;
        [SerializeField] private TMP_Text _remainingText;
        [SerializeField] private Transform _content;

        private Action _onCompleted;

        public void Open(int remainingPercent, Action onCompleted)
        {
            _onCompleted = onCompleted;
            _remainingText.text = $"{remainingPercent}% remaining";
            gameObject.SetActive(true);
            _backgroundCanvasGroup.alpha = 0f;
            _backgroundCanvasGroup.DOFade(1f, 0.35f).SetUpdate(true).SetLink(gameObject);
            _content.DOKill();
            _content.localScale = Vector3.one * 0.4f;
            _content.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
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
            _content.DOKill();
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
