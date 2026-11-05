using System;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class DoorObject : MonoBehaviour, IInitializable, ITickable, IDisposable
    {
        public event Action OnOpened;

        [SerializeField] private Vector3 _openLocalPosition;
        [SerializeField] private DoorConfig _doorConfig;
        [SerializeField] private PlayerAdvance _playerAdvance;

        private bool _isOpened;

        void IInitializable.Initialize()
        {
            _isOpened = false;
        }
        
        void IDisposable.Dispose()
        {
            if (this == null)
            {
                return;
            }

            transform.DOKill();
        }
        
        void ITickable.Tick()
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

            transform.DOLocalMove(_openLocalPosition, _doorConfig.SlideDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);

            OnOpened?.Invoke();
        }
    }
}
