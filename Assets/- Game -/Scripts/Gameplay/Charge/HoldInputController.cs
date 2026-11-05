using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay
{
    public class HoldInputController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ITickable, IInitializable, IDisposable
    {
        public event Action OnHoldBegan;
        public event Action<float> OnHoldTick;
        public event Action<float> OnHoldReleased;

        private bool _isGestureActive;
        private float _gestureStartTime;
        
        void IInitializable.Initialize()
        {
            ResetGesture();
        }

        void IDisposable.Dispose()
        {
            ResetGesture();
        }
        
        void ITickable.Tick()
        {
            if (!_isGestureActive)
            {
                return;
            }

            OnHoldTick?.Invoke(Time.time - _gestureStartTime);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (_isGestureActive)
            {
                return;
            }

            _isGestureActive = true;
            _gestureStartTime = Time.time;
            OnHoldBegan?.Invoke();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!_isGestureActive)
            {
                return;
            }

            var duration = Time.time - _gestureStartTime;
            _isGestureActive = false;
            OnHoldReleased?.Invoke(duration);
        }

        private void ResetGesture()
        {
            _isGestureActive = false;
            _gestureStartTime = 0f;
        }
    }
}
