using System;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class DoorObject : MonoBehaviour
    {
        public event Action OnOpened;

        [SerializeField] private PlayerAdvance _playerAdvance;
        [SerializeField] private Vector3 _openLocalPosition;

        private DoorConfig _doorConfig;
        private bool _isOpened;

        [Inject]
        public void Inject(DoorConfig doorConfig)
        {
            _doorConfig = doorConfig;
        }

        private void OnEnable()
        {
            if (_playerAdvance == null)
            {
                return;
            }

            _playerAdvance.OnMoved += PlayerAdvanceMoved;
        }

        private void OnDisable()
        {
            if (_playerAdvance == null)
            {
                return;
            }

            _playerAdvance.OnMoved -= PlayerAdvanceMoved;
        }

        private void PlayerAdvanceMoved()
        {
            if (_isOpened || _playerAdvance == null)
            {
                return;
            }

            var offset = _playerAdvance.transform.position - transform.position;
            offset.y = 0f;

            var distance = _doorConfig.OpenDistance;

            if (offset.sqrMagnitude > distance * distance)
            {
                return;
            }

            Open();
        }

        private void Open()
        {
            _isOpened = true;

            if (_playerAdvance != null)
            {
                _playerAdvance.OnMoved -= PlayerAdvanceMoved;
            }

            transform.DOLocalMove(_openLocalPosition, _doorConfig.SlideDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
            OnOpened?.Invoke();
        }
    }
}
