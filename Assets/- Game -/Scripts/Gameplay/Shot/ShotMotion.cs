using System;
using UnityEngine;

namespace Gameplay
{
    public class ShotMotion : MonoBehaviour
    {
        public event Action<RaycastHit> OnHit;
        public event Action OnExpired;

        private bool _isFlying;
        private Vector3 _direction;
        private float _speed;
        private float _bodyRadius;
        private float _maxDistance;
        private float _traveled;
        private LayerMask _hitMask;

        public void Launch(Vector3 direction, float speed, float bodyRadius, float maxDistance, LayerMask hitMask)
        {
            _direction = direction;
            _speed = speed;
            _bodyRadius = bodyRadius;
            _maxDistance = maxDistance;
            _hitMask = hitMask;
            _traveled = 0f;
            _isFlying = true;
        }

        public void Step()
        {
            if (!_isFlying)
            {
                return;
            }

            var stepDistance = _speed * Time.deltaTime;
            var remaining = _maxDistance - _traveled;

            if (stepDistance > remaining)
            {
                stepDistance = remaining;
            }

            var upperPoint = transform.position;
            var lowerPoint = upperPoint;
            lowerPoint.y = 0f;

            if (Physics.CapsuleCast(upperPoint, lowerPoint, _bodyRadius, _direction, out RaycastHit hit, stepDistance, _hitMask))
            {
                transform.position += _direction * hit.distance;
                _isFlying = false;
                OnHit?.Invoke(hit);
                return;
            }

            transform.position += _direction * stepDistance;
            _traveled += stepDistance;

            if (_traveled >= _maxDistance)
            {
                _isFlying = false;
                OnExpired?.Invoke();
            }
        }
    }
}
