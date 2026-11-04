using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay
{
    public class PlayerAdvance : MonoBehaviour
    {
        public event Action OnMoved;
        public event Action OnAdvanceCompleted;

        [SerializeField] private PathRoute _route;

        private PlayerConfig _playerConfig;
        private Tween _jump;
        private int _currentIndex;
        private bool _isMoving;

        public PathRoute Route => _route;
        public int CurrentIndex => _currentIndex;
        public bool IsMoving => _isMoving;

        public bool IsComplete =>
            _route != null && _route.PointCount > 0 && _currentIndex == _route.PointCount - 1 && !_isMoving;

        [Inject]
        public void Inject(PlayerConfig playerConfig)
        {
            _playerConfig = playerConfig;
        }

        public void AdvanceTo(int pointIndex, float pointInset)
        {
            if (_isMoving || _route == null || pointIndex <= _currentIndex || pointIndex >= _route.PointCount)
            {
                return;
            }

            transform.DOKill();
            _isMoving = true;
            JumpTo(_currentIndex + 1, pointIndex, pointInset, true);
        }

        public void AdvanceToward(Vector3 destination)
        {
            if (_isMoving || _route == null)
            {
                return;
            }

            var restingHeight = transform.position.y;
            destination.y = restingHeight;
            var offset = destination - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude <= 0.0004f)
            {
                return;
            }

            transform.DOKill();
            _isMoving = true;
            var jumpSettings = _playerConfig.Jump;
            transform.DOPunchScale(jumpSettings.Punch, jumpSettings.PunchDuration).SetLink(gameObject);
            var jump = transform.DOJump(destination, jumpSettings.Height, 1, jumpSettings.Duration).SetLink(gameObject);
            jump.OnUpdate(RaiseMoved);
            jump.OnComplete(() =>
            {
                if (_jump != jump)
                {
                    return;
                }

                _jump = null;
                _isMoving = false;
                Reach(destination);
                OnAdvanceCompleted?.Invoke();
            });
            _jump = jump;
        }

        private void Reach(Vector3 position)
        {
            while (_currentIndex + 1 < _route.PointCount)
            {
                var previous = _route.GetPoint(_currentIndex).position;
                var next = _route.GetPoint(_currentIndex + 1).position;
                var segment = next - previous;
                segment.y = 0f;
                var length = segment.magnitude;

                if (length <= 0.0001f)
                {
                    _currentIndex++;
                    continue;
                }

                var direction = segment / length;
                var along = (position.x - previous.x) * direction.x + (position.z - previous.z) * direction.z;

                if (along < length - 0.05f)
                {
                    break;
                }

                _currentIndex++;
            }
        }

        private void JumpTo(int index, int pointIndex, float pointInset, bool withPunch)
        {
            var restingHeight = transform.position.y;
            var destination = _route.GetPoint(index).position;
            destination.y = restingHeight;

            if (index == pointIndex && pointInset > 0f)
            {
                destination = PullBack(destination, _route.GetPoint(index - 1).position, pointInset, restingHeight);
            }

            var jumpSettings = _playerConfig.Jump;

            if (withPunch)
            {
                transform.DOPunchScale(jumpSettings.Punch, jumpSettings.PunchDuration).SetLink(gameObject);
            }

            var jump = transform.DOJump(destination, jumpSettings.Height, 1, jumpSettings.Duration).SetLink(gameObject);
            jump.OnUpdate(RaiseMoved);
            jump.OnComplete(() =>
            {
                if (_jump != jump)
                {
                    return;
                }

                if (index < pointIndex)
                {
                    JumpTo(index + 1, pointIndex, pointInset, false);
                    return;
                }

                _jump = null;
                _currentIndex = pointIndex;
                _isMoving = false;
                OnAdvanceCompleted?.Invoke();
            });
            _jump = jump;
        }

        private static Vector3 PullBack(Vector3 destination, Vector3 previous, float pointInset, float restingHeight)
        {
            var along = destination - previous;
            along.y = 0f;
            var length = along.magnitude;

            if (length <= 0.0001f)
            {
                return destination;
            }

            var inset = pointInset;

            if (inset > length)
            {
                inset = length;
            }

            destination -= along / length * inset;
            destination.y = restingHeight;
            return destination;
        }

        private void RaiseMoved()
        {
            OnMoved?.Invoke();
        }

        private void OnDestroy()
        {
            _jump?.Kill();
            transform.DOKill();
        }

#if UNITY_EDITOR
        [Button]
        private void AdvanceAcrossOne()
        {
            AdvanceTo(_currentIndex + 2, 0f);
        }
#endif
    }
}
