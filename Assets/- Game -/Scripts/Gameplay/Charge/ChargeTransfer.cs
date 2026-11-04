using System;

namespace Gameplay
{
    public class ChargeTransfer
    {
        public event Action<float, float> OnChargeChanged;
        public event Action<float> OnChargeCommitted;
        public event Action OnChargeCanceled;
        public event Action OnSourceDepleted;

        private readonly ChargeSettings _chargeSettings;

        public float SourceRemaining { get; private set; }
        public float ChargeAmount { get; private set; }

        public ChargeTransfer(ChargeSettings chargeSettings)
        {
            _chargeSettings = chargeSettings;
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
            if (SourceRemaining <= _chargeSettings.CriticalRemaining)
            {
                return;
            }

            var transfer = _chargeSettings.TransferPerSecond * deltaTime;
            var available = SourceRemaining - _chargeSettings.CriticalRemaining;

            if (transfer >= available)
            {
                ChargeAmount += available;
                SourceRemaining = _chargeSettings.CriticalRemaining;
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
            if (ChargeAmount < _chargeSettings.MinimumCommit)
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
