using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay
{
    public class HoldInputController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ITickable, IInitializable, IDisposable
    {
        public event Action OnHoldBegan;
        public event Action<float> OnHoldTick;
        public event Action<float> OnHoldReleased;
        public event Action OnHoldCanceled;

        private bool _isGestureActive;
        private bool _isGestureCanceled;
        private float _gestureStartTime;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (_isGestureActive)
            {
                return;
            }

            _isGestureActive = true;
            _isGestureCanceled = false;
            _gestureStartTime = Time.time;
            OnHoldBegan?.Invoke();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!_isGestureActive)
            {
                return;
            }

            if (_isGestureCanceled)
            {
                _isGestureActive = false;
                return;
            }

            var duration = Time.time - _gestureStartTime;
            _isGestureActive = false;
            OnHoldReleased?.Invoke(duration);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!_isGestureActive || _isGestureCanceled)
            {
                return;
            }

            _isGestureCanceled = true;
            OnHoldCanceled?.Invoke();
        }

        void ITickable.Tick()
        {
            if (!_isGestureActive || _isGestureCanceled)
            {
                return;
            }

            OnHoldTick?.Invoke(Time.time - _gestureStartTime);
        }

        void IInitializable.Initialize()
        {
            ResetGesture();
        }

        void IDisposable.Dispose()
        {
            ResetGesture();
        }

        private void ResetGesture()
        {
            _isGestureActive = false;
            _isGestureCanceled = false;
            _gestureStartTime = 0f;
        }
    }
}
