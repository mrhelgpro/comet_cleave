using System;

namespace Gameplay
{
    public class ChargeTransfer
    {
        public event Action<float, float> OnChargeChanged;
        public event Action<float> OnChargeCommitted;
        public event Action OnChargeCanceled;
        public event Action OnSourceDepleted;

        private readonly ChargeConfig _chargeConfig;

        public float SourceRemaining { get; private set; }
        public float ChargeAmount { get; private set; }

        public ChargeTransfer(ChargeConfig chargeConfig)
        {
            _chargeConfig = chargeConfig;
        }

        public void Reset(float source)
        {
            SourceRemaining = source;
            ChargeAmount = 0f;
            OnChargeChanged?.Invoke(SourceRemaining, ChargeAmount);
        }

        public void Begin()
        {
            ChargeAmount = 0f;
        }

        public void Advance(float deltaTime)
        {
            if (SourceRemaining <= _chargeConfig.CriticalRemaining)
            {
                return;
            }

            var transfer = _chargeConfig.TransferPerSecond * deltaTime;
            var available = SourceRemaining - _chargeConfig.CriticalRemaining;

            if (transfer >= available)
            {
                ChargeAmount += available;
                SourceRemaining = _chargeConfig.CriticalRemaining;
                OnChargeChanged?.Invoke(SourceRemaining, ChargeAmount);
                OnSourceDepleted?.Invoke();
                return;
            }

            SourceRemaining -= transfer;
            ChargeAmount += transfer;
            OnChargeChanged?.Invoke(SourceRemaining, ChargeAmount);
        }

        public bool TryCommit()
        {
            if (ChargeAmount < _chargeConfig.MinimumCommit)
            {
                Refund();
                return false;
            }

            OnChargeCommitted?.Invoke(ChargeAmount);
            return true;
        }

        public void Refund()
        {
            SourceRemaining += ChargeAmount;
            ChargeAmount = 0f;
            OnChargeChanged?.Invoke(SourceRemaining, ChargeAmount);
            OnChargeCanceled?.Invoke();
        }
    }
}
