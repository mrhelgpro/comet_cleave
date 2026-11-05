using System;
using UnityEngine;

namespace Gameplay
{
    public class ShotSequence : IInitializable, ITickable, IDisposable
    {
        public event Action OnExpired;

        private readonly ShotConfig _shotConfig;
        private readonly ChargeTransfer _chargeTransfer;
        private readonly HoldInputController _holdInputController;
        private readonly PlayerAdvance _playerAdvance;
        private readonly PathRoute _route;
        private readonly ShotSpawner _shotSpawner;
        private readonly ObstacleClearing _obstacleClearing;
        private readonly int _obstacleMask;

        private GameObject _shotObject;
        private ShotBody _shotBody;
        private float _launchedCharge;
        private bool _isCharging;
        private bool _isRoundActive;

        public ShotSequence(
            ShotConfig shotConfig,
            ChargeTransfer chargeTransfer,
            HoldInputController holdInputController,
            PlayerAdvance playerAdvance,
            PathRoute route,
            ShotSpawner shotSpawner,
            ObstacleClearing obstacleClearing)
        {
            _shotConfig = shotConfig;
            _chargeTransfer = chargeTransfer;
            _holdInputController = holdInputController;
            _playerAdvance = playerAdvance;
            _route = route;
            _shotSpawner = shotSpawner;
            _obstacleClearing = obstacleClearing;
            _obstacleMask = LayerMask.GetMask("Obstacle");
        }

        public void SetRoundActive(bool isActive)
        {
            _isRoundActive = isActive;
        }

        void IInitializable.Initialize()
        {
            _holdInputController.OnHoldBegan += HoldBegan;
            _holdInputController.OnHoldTick += HoldTick;
            _holdInputController.OnHoldReleased += HoldReleased;
            _chargeTransfer.OnChargeChanged += ChargeChanged;
            _chargeTransfer.OnSourceDepleted += SourceDepleted;
        }

        void ITickable.Tick()
        {
            if (_shotBody == null)
            {
                return;
            }

            _shotBody.Motion.Step();
        }

        void IDisposable.Dispose()
        {
            _holdInputController.OnHoldBegan -= HoldBegan;
            _holdInputController.OnHoldTick -= HoldTick;
            _holdInputController.OnHoldReleased -= HoldReleased;
            _chargeTransfer.OnChargeChanged -= ChargeChanged;
            _chargeTransfer.OnSourceDepleted -= SourceDepleted;
            ReleaseShot();
        }

        private bool CanAcceptHold()
        {
            return _isRoundActive
                && _shotObject == null
                && !_playerAdvance.IsMoving
                && !_obstacleClearing.IsBusy;
        }

        private void HoldBegan()
        {
            if (!CanAcceptHold())
            {
                return;
            }

            _isCharging = true;
            _chargeTransfer.Begin();
            _shotObject = _shotSpawner.Get();
            _shotBody = _shotObject.GetComponent<ShotBody>();
            _shotBody.ShowCharge(_playerAdvance.transform, _route, 0f);
            _shotBody.Motion.OnHit += ProjectileHit;
            _shotBody.Motion.OnExpired += ProjectileExpired;
        }

        private void HoldTick(float duration)
        {
            if (!_isCharging)
            {
                return;
            }

            _chargeTransfer.Advance(Time.deltaTime);
        }

        private void HoldReleased(float duration)
        {
            if (!_isCharging)
            {
                return;
            }

            _isCharging = false;
            _chargeTransfer.Commit();
            LaunchShot();
        }

        private void ChargeChanged(float sourceRemaining, float chargeAmount)
        {
            if (_shotBody == null)
            {
                return;
            }

            _shotBody.ShowCharge(_playerAdvance.transform, _route, chargeAmount);
        }

        private void SourceDepleted()
        {
            if (!_isCharging)
            {
                return;
            }

            _isCharging = false;
            ReleaseShot();
        }

        private void LaunchShot()
        {
            AudioService.PlaySfx(_shotConfig.ShotSound);
            var direction = _shotBody.AimDirection(_playerAdvance.transform, _route);
            var bodyRadius = _shotObject.transform.lossyScale.x * 0.5f;
            
            _launchedCharge = _chargeTransfer.ChargeAmount;
            _shotBody.Motion.Launch(
                direction,
                _shotConfig.ShotSpeed,
                bodyRadius,
                _shotConfig.ShotMaxDistance,
                _obstacleMask);
        }

        private void ProjectileHit(RaycastHit hit)
        {
            var chargeAmount = _launchedCharge;
            var obstacleObject = hit.collider != null ? hit.collider.GetComponent<ObstacleObject>() : null;
            ReleaseShot();
            _obstacleClearing.Impact(hit.point, chargeAmount, obstacleObject);
        }

        private void ProjectileExpired()
        {
            ReleaseShot();
            OnExpired?.Invoke();
        }

        private void ReleaseShot()
        {
            if (_shotBody != null)
            {
                _shotBody.Motion.OnHit -= ProjectileHit;
                _shotBody.Motion.OnExpired -= ProjectileExpired;
                _shotBody = null;
            }

            if (_shotObject == null)
            {
                return;
            }

            _shotSpawner.Release(_shotObject);
            _shotObject = null;
        }
    }
}
