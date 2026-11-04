using System;
using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "PlayerConfig_Gameplay", menuName = ProjectSettings.TITLE + "/Configs/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] private float _startSize;
        [SerializeField] private float _approachGap = 0.75f;
        [SerializeField] private JumpSettings _jump = new();

        public float StartSize => _startSize;
        public float ApproachGap => _approachGap;
        public JumpSettings Jump => _jump;
    }

    [Serializable]
    public class JumpSettings
    {
        [SerializeField] private float _duration = 0.4f;
        [SerializeField] private float _height = 1.2f;
        [SerializeField] private Vector3 _punch = new(0.15f, -0.1f, 0.15f);
        [SerializeField] private float _punchDuration = 0.2f;

        public float Duration => _duration;
        public float Height => _height;
        public Vector3 Punch => _punch;
        public float PunchDuration => _punchDuration;
    }
}
