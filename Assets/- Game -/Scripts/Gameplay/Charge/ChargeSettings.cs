using System;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class ChargeSettings
    {
        [SerializeField] private float _transferPerSecond;
        [SerializeField] private float _minimumCommit;
        [SerializeField] private float _minimumCharge;
        [SerializeField] private float _lossSize;

        public float TransferPerSecond => _transferPerSecond;
        public float MinimumCommit => _minimumCommit;
        public float MinimumCharge => _minimumCharge;
        public float LossSize => _lossSize;
    }
}
