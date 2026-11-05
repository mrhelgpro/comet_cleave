using System;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class PlayerAdvance : MonoBehaviour, IInitializable, IDisposable
    {
        public event Action OnMoved;
        public event Action OnAdvanceCompleted;

        private PlayerConfig _playerConfig;
        private ChargeTransfer _chargeTransfer;
        private PathRoute _route;
        private ScaleBinding _scaleBinding;
        private Tween _jump;

        public int CurrentIndex { get; private set; }
        public bool IsMoving { get; private set; }

        public bool IsComplete =>
            _route != null && _route.PointCount > 0 && CurrentIndex == _route.PointCount - 1 && !IsMoving;

        [Inject]
        public void Inject(PlayerConfig playerConfig, PathRoute route, ChargeTransfer chargeTransfer)
        {
            _playerConfig = playerConfig;
            _chargeTransfer = chargeTransfer;
            
            _route = route;
        }

        void IInitializable.Initialize()
        {
            _chargeTransfer.OnChargeChanged += ChargeChanged;
        }

        void IDisposable.Dispose()
        {
            _chargeTransfer.OnChargeChanged -= ChargeChanged;
            _jump?.Kill();
            _jump = null;

            if (this == null)
            {
                return;
            }

            transform.DOKill();
        }
        
        public void AdvanceToward(Vector3 destination)
        {
            if (IsMoving || _route == null)
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
            IsMoving = true;
            
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
                IsMoving = false;
                Reach(destination);
                OnAdvanceCompleted?.Invoke();
            });
            
            _jump = jump;
        }
        
        private void ChargeChanged(float sourceRemaining, float chargeAmount)
        {
            SetSize(sourceRemaining);
        }
        
        private void SetSize(float size)
        {
            if (_scaleBinding == null)
            {
                _scaleBinding = GetComponent<ScaleBinding>();
            }

            _scaleBinding.SetUniform(size);
            var route = _route;

            if (route == null)
            {
                return;
            }

            for (var index = 0; index < route.TileCount; index++)
            {
                route.GetTile(index).SetWidth(size);
            }

            RestOnLane();
        }

        private void Reach(Vector3 position)
        {
            while (CurrentIndex + 1 < _route.PointCount)
            {
                var previous = _route.GetPoint(CurrentIndex).position;
                var next = _route.GetPoint(CurrentIndex + 1).position;
                var segment = next - previous;
                segment.y = 0f;
                var length = segment.magnitude;

                if (length <= 0.0001f)
                {
                    CurrentIndex++;
                    continue;
                }

                var direction = segment / length;
                var along = (position.x - previous.x) * direction.x + (position.z - previous.z) * direction.z;

                if (along < length - 0.05f)
                {
                    break;
                }

                CurrentIndex++;
            }
        }

        private void RestOnLane()
        {
            var route = _route;

            if (route == null || route.TileCount == 0)
            {
                return;
            }

            var radius = transform.lossyScale.y * 0.5f;
            var position = transform.position;
            position.y = route.GetTile(0).SurfaceHeight + radius;
            transform.position = position;
        }

        private void RaiseMoved()
        {
            OnMoved?.Invoke();
        }
    }
}
