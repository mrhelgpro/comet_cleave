using System;

namespace Gameplay
{
    public class ChargeTransfer
    {
        public event Action<float, float> OnChargeChanged;
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
            if (SourceRemaining <= _chargeSettings.LossSize)
            {
                return;
            }

            var transfer = _chargeSettings.TransferPerSecond * deltaTime;
            var available = SourceRemaining - _chargeSettings.LossSize;

            if (transfer >= available)
            {
                ChargeAmount += available;
                SourceRemaining = _chargeSettings.LossSize;
                OnChargeChanged?.Invoke(SourceRemaining, ChargeAmount);
                OnSourceDepleted?.Invoke();
                return;
            }

            SourceRemaining -= transfer;
            ChargeAmount += transfer;
            OnChargeChanged?.Invoke(SourceRemaining, ChargeAmount);
        }

        public void Commit()
        {
            if (ChargeAmount >= _chargeSettings.MinimumCharge)
            {
                return;
            }

            var deficit = _chargeSettings.MinimumCharge - ChargeAmount;
            var available = SourceRemaining - _chargeSettings.LossSize;

            if (available >= deficit)
            {
                SourceRemaining -= deficit;
                ChargeAmount = _chargeSettings.MinimumCharge;
                OnChargeChanged?.Invoke(SourceRemaining, ChargeAmount);
                return;
            }

            if (available > 0f)
            {
                SourceRemaining = _chargeSettings.LossSize;
            }

            ChargeAmount = _chargeSettings.MinimumCharge;
            OnChargeChanged?.Invoke(SourceRemaining, ChargeAmount);
            OnSourceDepleted?.Invoke();
        }
    }
}
