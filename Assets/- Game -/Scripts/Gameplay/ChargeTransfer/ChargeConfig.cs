using System;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class ChargeConfig
    {
        [SerializeField] private float _transferPerSecond;
        [SerializeField] private float _minimumCommit;
        [SerializeField] private float _criticalRemaining;

        public float TransferPerSecond => _transferPerSecond;
        public float MinimumCommit => _minimumCommit;
        public float CriticalRemaining => _criticalRemaining;
    }
}
