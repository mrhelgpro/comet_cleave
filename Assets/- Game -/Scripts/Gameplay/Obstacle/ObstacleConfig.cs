using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "ObstacleConfig_Gameplay", menuName = ProjectSettings.TITLE + "/Configs/Obstacle Config")]
    public class ObstacleConfig : ScriptableObject
    {
        [SerializeField] private float _shakeDuration = 0.25f;
        [SerializeField] private float _shakeStrength = 0.12f;
        [SerializeField] private int _shakeVibrato = 12;
        [SerializeField] private float _shakeRandomness = 90f;
        [SerializeField] private Material _infectedMaterial;
        [SerializeField] private float _infectionDuration = 0.45f;
        [SerializeField] private float _explosionDuration = 0.2f;
        [SerializeField] private float _explosionTargetScale;

        public float ShakeDuration => _shakeDuration;
        public float ShakeStrength => _shakeStrength;
        public int ShakeVibrato => _shakeVibrato;
        public float ShakeRandomness => _shakeRandomness;
        public Material InfectedMaterial => _infectedMaterial;
        public float InfectionDuration => _infectionDuration;
        public float ExplosionDuration => _explosionDuration;
        public float ExplosionTargetScale => _explosionTargetScale;
    }
}
