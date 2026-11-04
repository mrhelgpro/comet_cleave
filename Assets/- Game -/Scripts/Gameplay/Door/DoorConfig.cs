using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "DoorConfig_Gameplay", menuName = ProjectSettings.TITLE + "/Configs/Door Config")]
    public class DoorConfig : ScriptableObject
    {
        [SerializeField] private float _openDistance = 5f;
        [SerializeField] private float _slideDuration = 0.4f;

        public float OpenDistance => _openDistance;
        public float SlideDuration => _slideDuration;
    }
}
