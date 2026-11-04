using System;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class ObstacleObject : MonoBehaviour, IInitializable, IDisposable
    {
        public event Action OnCleared;

        private ObstacleRegistry _obstacleRegistry;
        private ObstacleConfig _obstacleConfig;
        private Collider _collider;
        private Renderer _renderer;
        private Tween _shake;
        private Tween _explosionDelay;

        public bool IsInfected { get; private set; }
        public bool IsCleared { get; private set; }

        public float BodyRadius
        {
            get
            {
                var collider = _collider != null ? _collider : GetComponent<Collider>();
                var scale = transform.lossyScale.x;

                if (collider is CapsuleCollider capsule)
                {
                    return capsule.radius * scale;
                }

                if (collider is SphereCollider sphere)
                {
                    return sphere.radius * scale;
                }

                return scale * 0.5f;
            }
        }

        [Inject]
        public void Inject(ObstacleRegistry obstacleRegistry, ObstacleConfig obstacleConfig)
        {
            _obstacleRegistry = obstacleRegistry;
            _obstacleConfig = obstacleConfig;
        }

        void IInitializable.Initialize()
        {
            _collider = GetComponent<Collider>();
            _renderer = GetComponent<Renderer>();
            _obstacleRegistry.Register(this);
        }

        void IDisposable.Dispose()
        {
            KillPresentation();

            if (_obstacleRegistry == null)
            {
                return;
            }

            _obstacleRegistry.Unregister(this);
        }

        public void Consume()
        {
            _collider.enabled = false;
            IsInfected = true;
            _renderer.sharedMaterial = _obstacleConfig.InfectedMaterial;
            KillPresentation();

            _shake = transform
                .DOShakePosition(_obstacleConfig.ShakeDuration, _obstacleConfig.ShakeStrength, _obstacleConfig.ShakeVibrato, _obstacleConfig.ShakeRandomness)
                .SetLink(gameObject);

            _explosionDelay = DOVirtual.DelayedCall(_obstacleConfig.InfectionDuration, StartExplosion).SetLink(gameObject);
        }

        private void StartExplosion()
        {
            transform.DOKill();
            _shake = null;

            transform
                .DOScale(_obstacleConfig.ExplosionTargetScale, _obstacleConfig.ExplosionDuration)
                .SetLink(gameObject)
                .OnComplete(Finish);
        }

        private void Finish()
        {
            IsCleared = true;
            OnCleared?.Invoke();
            gameObject.SetActive(false);
        }

        private void KillPresentation()
        {
            _explosionDelay?.Kill();
            _explosionDelay = null;

            if (this == null)
            {
                _shake = null;
                return;
            }

            transform.DOKill();
            _shake = null;
        }
    }
}
