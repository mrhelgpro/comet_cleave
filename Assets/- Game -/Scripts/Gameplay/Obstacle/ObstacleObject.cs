using System;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class ObstacleObject : MonoBehaviour, IInitializable, IDisposable
    {
        public event Action OnCleared;

        [SerializeField] private Collider _collider;
        [SerializeField] private Renderer _renderer;

        private ObstacleRegistry _obstacleRegistry;
        private ObstacleConfig _obstacleConfig;
        private Tween _shake;
        private Tween _explosionDelay;

        public bool IsInfected { get; private set; }
        public bool IsCleared { get; private set; }

        public float BodyRadius
        {
            get
            {
                var scale = transform.lossyScale.x;

                return _collider switch
                {
                    CapsuleCollider capsule => capsule.radius * scale,
                    SphereCollider sphere => sphere.radius * scale,
                    _ => scale * 0.5f
                };
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
            _obstacleRegistry.Register(this);
        }

        void IDisposable.Dispose()
        {
            KillPresentation();

            _obstacleRegistry?.Unregister(this);
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
