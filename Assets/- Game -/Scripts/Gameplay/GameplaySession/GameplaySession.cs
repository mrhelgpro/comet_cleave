using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class GameplaySession : IInitializable, ITickable, IDisposable
    {
        public enum State
        {
            WaitingToStart,
            Ready,
            Charging,
            ShotInFlight,
            Clearing,
            Advancing,
            Won,
            Lost
        }

        public event Action<State> OnStateChanged;

        private readonly ShotConfig _shotConfig;
        private readonly PlayerConfig _playerConfig;
        private readonly ChargeTransfer _chargeTransfer;
        private readonly ObstacleRegistry _obstacleRegistry;
        private readonly HoldInputController _holdInputController;
        private readonly PlayerAdvance _playerAdvance;
        private readonly DoorObject _doorObject;
        private readonly ScaleBinding _playerScale;
        private readonly ShotSpawner _shotSpawner;
        private readonly int _obstacleMask;

        private GameObject _shotObject;
        private ShotMotion _projectileMotion;
        private ScaleBinding _shotScale;
        private readonly List<ObstacleObject> _blockingObstacles = new();
        private List<ObstacleObject> _clearingObstacles;
        private int _remainingClearCount;
        private float _launchedCharge;
        private bool _isDoorOpened;

        public GameplaySession(
            ShotConfig shotConfig,
            PlayerConfig playerConfig,
            ChargeTransfer chargeTransfer,
            ObstacleRegistry obstacleRegistry,
            HoldInputController holdInputController,
            PlayerAdvance playerAdvance,
            DoorObject doorObject,
            ScaleBinding playerScale,
            ShotSpawner shotSpawner)
        {
            _shotConfig = shotConfig;
            _playerConfig = playerConfig;
            _chargeTransfer = chargeTransfer;
            _obstacleRegistry = obstacleRegistry;
            _holdInputController = holdInputController;
            _playerAdvance = playerAdvance;
            _doorObject = doorObject;
            _playerScale = playerScale;
            _shotSpawner = shotSpawner;
            _obstacleMask = LayerMask.GetMask("Obstacle");
        }

        public State CurrentState { get; private set; }

        void IInitializable.Initialize()
        {
            _chargeTransfer.Reset(_playerConfig.StartSize);
            ApplyPlayerAndLane(_playerConfig.StartSize);
            
            _holdInputController.OnHoldBegan += HoldBegan;
            _holdInputController.OnHoldTick += HoldTick;
            _holdInputController.OnHoldReleased += HoldReleased;
            _holdInputController.OnHoldCanceled += HoldCanceled;
            _chargeTransfer.OnChargeChanged += ChargeChanged;
            _chargeTransfer.OnSourceDepleted += SourceDepleted;
            _playerAdvance.OnAdvanceCompleted += AdvanceCompleted;
            _doorObject.OnOpened += DoorOpened;
            
            SetState(State.WaitingToStart);
        }

        void ITickable.Tick()
        {
            if (CurrentState != State.ShotInFlight || _projectileMotion == null)
            {
                return;
            }

            _projectileMotion.Step();
        }

        void IDisposable.Dispose()
        {
            _holdInputController.OnHoldBegan -= HoldBegan;
            _holdInputController.OnHoldTick -= HoldTick;
            _holdInputController.OnHoldReleased -= HoldReleased;
            _holdInputController.OnHoldCanceled -= HoldCanceled;
            _chargeTransfer.OnChargeChanged -= ChargeChanged;
            _chargeTransfer.OnSourceDepleted -= SourceDepleted;
            _playerAdvance.OnAdvanceCompleted -= AdvanceCompleted;
            _doorObject.OnOpened -= DoorOpened;
            
            ReleaseClearing();
            ReleaseShot();
        }

        public void StartRound()
        {
            if (CurrentState != State.WaitingToStart)
            {
                return;
            }

            SetState(State.Ready);
            ResolveLane();
        }

        private void HoldBegan()
        {
            if (CurrentState != State.Ready)
            {
                return;
            }

            _chargeTransfer.Begin();
            SetState(State.Charging);
            _shotObject = _shotSpawner.Get();
            _shotObject.transform.position = _playerAdvance.transform.position;
            _projectileMotion = _shotObject.GetComponent<ShotMotion>();
            _shotScale = _shotObject.GetComponent<ScaleBinding>();
            _shotScale.SetUniform(0f);
            _projectileMotion.OnHit += ProjectileHit;
            _projectileMotion.OnExpired += ProjectileExpired;
        }

        private void HoldTick(float duration)
        {
            if (CurrentState != State.Charging)
            {
                return;
            }

            _chargeTransfer.Advance(Time.deltaTime);
        }

        private void HoldReleased(float duration)
        {
            if (CurrentState != State.Charging)
            {
                return;
            }

            if (!_chargeTransfer.TryCommit())
            {
                ReleaseShot();
                SetState(State.Ready);
                return;
            }

            LaunchShot();
            SetState(State.ShotInFlight);
        }

        private void HoldCanceled()
        {
            if (CurrentState != State.Charging)
            {
                return;
            }

            _chargeTransfer.Refund();
            ReleaseShot();
            SetState(State.Ready);
        }

        private void ChargeChanged(float sourceRemaining, float chargeAmount)
        {
            ApplyPlayerAndLane(sourceRemaining);

            if (_shotScale == null)
            {
                return;
            }

            _shotScale.SetUniform(chargeAmount);
            RestOnLane(_shotScale.transform);
        }

        private void SourceDepleted()
        {
            if (CurrentState != State.Charging)
            {
                return;
            }

            ReleaseShot();
            SetState(State.Lost);
        }

        private void ProjectileHit(RaycastHit hit)
        {
            var chargeAmount = _launchedCharge;
            var obstacleObject = hit.collider != null ? hit.collider.GetComponent<ObstacleObject>() : null;
            ReleaseShot();

            if (obstacleObject == null)
            {
                ResolveLane();
                return;
            }

            var radius = chargeAmount * _shotConfig.RadiusPerSize;
            var collected = _obstacleRegistry.CollectInRadius(hit.point, radius, obstacleObject);
            BeginClearing(collected);
        }

        private void ProjectileExpired()
        {
            ReleaseShot();
            ResolveLane();
        }

        private void NodeCleared()
        {
            _remainingClearCount--;

            if (_remainingClearCount > 0)
            {
                return;
            }

            ReleaseClearing();
            ResolveLane();
        }

        private void AdvanceCompleted()
        {
            if (CurrentState is State.WaitingToStart or State.Won or State.Lost)
            {
                return;
            }

            if (_playerAdvance.IsComplete && _isDoorOpened)
            {
                SetState(State.Won);
                return;
            }

            SetState(State.Ready);
        }

        private void DoorOpened()
        {
            _isDoorOpened = true;

            if (_playerAdvance.IsComplete)
            {
                SetState(State.Won);
            }
        }

        private void LaunchShot()
        {
            var route = _playerAdvance.Route;
            var destination = route.GetPoint(route.PointCount - 1).position;
            var direction = destination - _playerAdvance.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }

            _launchedCharge = _chargeTransfer.ChargeAmount;
            var bodyRadius = _shotObject.transform.lossyScale.x * 0.5f;
            _projectileMotion.Launch(
                direction,
                _shotConfig.ShotSpeed,
                bodyRadius,
                _shotConfig.ShotMaxDistance,
                _obstacleMask);
        }

        private void BeginClearing(List<ObstacleObject> collected)
        {
            _clearingObstacles = collected;
            _remainingClearCount = collected.Count;
            SetState(State.Clearing);

            foreach (var obstacleObject in collected)
            {
                obstacleObject.OnCleared += NodeCleared;
                obstacleObject.Consume();
            }

            if (_remainingClearCount > 0)
            {
                return;
            }

            ReleaseClearing();
            ResolveLane();
        }

        private void ResolveLane()
        {
            if (CurrentState is State.Won or State.Lost)
            {
                return;
            }

            if (TryMoveAlongPath(out var destination))
            {
                SetState(State.Advancing);
                _playerAdvance.AdvanceToward(destination);

                if (_playerAdvance.IsMoving)
                {
                    return;
                }
            }

            var chargeSettings = _shotConfig.Charge;
            var requiredShot = chargeSettings.MinimumCharge;

            if (chargeSettings.MinimumCommit > requiredShot)
            {
                requiredShot = chargeSettings.MinimumCommit;
            }

            var minimumToContinue = chargeSettings.LossSize + requiredShot;

            if (_chargeTransfer.SourceRemaining < minimumToContinue)
            {
                SetState(State.Lost);
                return;
            }

            SetState(State.Ready);
        }

        private bool TryMoveAlongPath(out Vector3 destination)
        {
            var origin = _playerAdvance.transform.position;
            destination = origin;
            var route = _playerAdvance.Route;

            if (route == null)
            {
                return false;
            }

            var playerRadius = _playerAdvance.transform.lossyScale.x * 0.5f;
            _obstacleRegistry.CollectActive(_blockingObstacles);
            var cursor = origin;
            var traveled = 0f;
            var blockedTravel = float.PositiveInfinity;

            for (var pointIndex = _playerAdvance.CurrentIndex + 1; pointIndex < route.PointCount; pointIndex++)
            {
                var point = route.GetPoint(pointIndex).position;
                var segment = point - cursor;
                segment.y = 0f;
                var length = segment.magnitude;

                if (length <= 0.0001f)
                {
                    cursor = point;
                    continue;
                }

                var direction = segment / length;
                var entry = NearestEntry(cursor, direction, length, playerRadius);

                if (entry < length)
                {
                    blockedTravel = traveled + entry;
                    break;
                }

                traveled += length;
                cursor = point;
            }

            var travel = float.IsPositiveInfinity(blockedTravel) ? traveled : blockedTravel - _playerConfig.ApproachGap;

            if (!float.IsPositiveInfinity(blockedTravel) && travel < blockedTravel && travel < 0.02f && blockedTravel > 0.02f)
            {
                travel = blockedTravel;
            }

            if (travel < 0f)
            {
                travel = 0f;
            }

            if (travel <= 0.02f)
            {
                return false;
            }

            destination = PositionAlong(origin, travel);
            return true;
        }

        private float NearestEntry(Vector3 origin, Vector3 direction, float length, float playerRadius)
        {
            var nearest = length;

            foreach (var obstacleObject in _blockingObstacles)
            {
                if (!obstacleObject.gameObject.activeInHierarchy || obstacleObject.IsCleared)
                {
                    continue;
                }

                var required = playerRadius + obstacleObject.BodyRadius;
                var offsetX = obstacleObject.transform.position.x - origin.x;
                var offsetZ = obstacleObject.transform.position.z - origin.z;
                var along = offsetX * direction.x + offsetZ * direction.z;
                var distanceSquared = offsetX * offsetX + offsetZ * offsetZ;
                var lateralSquared = distanceSquared - along * along;

                if (lateralSquared < 0f)
                {
                    lateralSquared = 0f;
                }

                var requiredSquared = required * required;

                if (lateralSquared >= requiredSquared)
                {
                    continue;
                }

                var entry = along - Mathf.Sqrt(requiredSquared - lateralSquared);

                if (entry > length)
                {
                    continue;
                }

                if (entry < 0f)
                {
                    if (distanceSquared < requiredSquared && along >= 0f)
                    {
                        return 0f;
                    }

                    continue;
                }

                if (entry < nearest)
                {
                    nearest = entry;
                }
            }

            return nearest;
        }

        private Vector3 PositionAlong(Vector3 origin, float distance)
        {
            var route = _playerAdvance.Route;
            var cursor = origin;
            var remaining = distance;

            for (var pointIndex = _playerAdvance.CurrentIndex + 1; pointIndex < route.PointCount; pointIndex++)
            {
                var point = route.GetPoint(pointIndex).position;
                var segment = point - cursor;
                segment.y = 0f;
                var length = segment.magnitude;

                if (length <= 0.0001f)
                {
                    cursor = point;
                    continue;
                }

                if (remaining <= length)
                {
                    var destination = cursor + segment / length * remaining;
                    destination.y = origin.y;
                    return destination;
                }

                remaining -= length;
                cursor = point;
            }

            cursor.y = origin.y;
            return cursor;
        }

        private void ApplyPlayerAndLane(float size)
        {
            _playerScale.SetUniform(size);
            var route = _playerAdvance.Route;

            if (route == null)
            {
                return;
            }

            for (var index = 0; index < route.TileCount; index++)
            {
                route.GetTile(index).SetWidth(size);
            }

            RestOnLane(_playerAdvance.transform);
        }

        private void RestOnLane(Transform body)
        {
            var route = _playerAdvance.Route;

            if (route == null || route.TileCount == 0)
            {
                return;
            }

            var radius = body.lossyScale.y * 0.5f;
            var position = body.position;
            position.y = route.GetTile(0).SurfaceHeight + radius;
            body.position = position;
        }

        private void ReleaseShot()
        {
            if (_projectileMotion != null)
            {
                _projectileMotion.OnHit -= ProjectileHit;
                _projectileMotion.OnExpired -= ProjectileExpired;
                _projectileMotion = null;
            }

            _shotScale = null;

            if (_shotObject == null)
            {
                return;
            }

            _shotSpawner.Release(_shotObject);
            _shotObject = null;
        }

        private void ReleaseClearing()
        {
            if (_clearingObstacles == null)
            {
                return;
            }

            foreach (var obstacleObject in _clearingObstacles)
            {
                obstacleObject.OnCleared -= NodeCleared;
            }

            _clearingObstacles = null;
            _remainingClearCount = 0;
        }

        private void SetState(State state)
        {
            CurrentState = state;
            OnStateChanged?.Invoke(state);
        }
    }
}
