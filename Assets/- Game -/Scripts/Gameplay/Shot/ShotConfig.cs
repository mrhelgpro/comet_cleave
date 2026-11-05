using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "ShotConfig_Gameplay", menuName = ProjectSettings.TITLE + "/Configs/Shot Config")]
    public class ShotConfig : ScriptableObject
    {
        [SerializeField] private float _shotSpeed;
        [SerializeField] private float _shotMaxDistance;
        [SerializeField] private float _radiusPerSize;
        [SerializeField] private ChargeSettings _charge = new();
        [SerializeField] private GameObject _shotPrefab;
        [SerializeField] private SoundAsset _shotSound;

        public float ShotSpeed => _shotSpeed;
        public float ShotMaxDistance => _shotMaxDistance;
        public float RadiusPerSize => _radiusPerSize;
        public ChargeSettings Charge => _charge;
        public GameObject ShotPrefab => _shotPrefab;
        public SoundAsset ShotSound => _shotSound;
    }
}
