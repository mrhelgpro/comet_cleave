using System;

namespace Gameplay
{
    public class GameplaySession : IInitializable, IDisposable
    {
        public enum State
        {
            WaitingToStart,
            Playing,
            Won,
            Lost
        }

        public event Action<State> OnStateChanged;

        private readonly PlayerConfig _playerConfig;
        private readonly ChargeTransfer _chargeTransfer;
        private readonly ChargeSettings _chargeSettings;
        private readonly PlayerAdvance _playerAdvance;
        private readonly DoorObject _doorObject;
        private readonly LaneAdvance _laneAdvance;
        private readonly ObstacleClearing _obstacleClearing;
        private readonly ShotSequence _shotSequence;

        private bool _isDoorOpened;
        
        public State CurrentState { get; private set; }

        public GameplaySession(
            PlayerConfig playerConfig,
            ChargeTransfer chargeTransfer,
            ChargeSettings chargeSettings,
            PlayerAdvance playerAdvance,
            DoorObject doorObject,
            LaneAdvance laneAdvance,
            ObstacleClearing obstacleClearing,
            ShotSequence shotSequence)
        {
            _playerConfig = playerConfig;
            _chargeTransfer = chargeTransfer;
            _chargeSettings = chargeSettings;
            _playerAdvance = playerAdvance;
            _doorObject = doorObject;
            _laneAdvance = laneAdvance;
            _obstacleClearing = obstacleClearing;
            _shotSequence = shotSequence;
        }
        
        void IInitializable.Initialize()
        {
            _chargeTransfer.Reset(_playerConfig.StartSize);
            _chargeTransfer.OnSourceDepleted += SourceDepleted;
            _laneAdvance.OnBlocked += PassageBlocked;
            _playerAdvance.OnAdvanceCompleted += AdvanceCompleted;
            _doorObject.OnOpened += DoorOpened;
            _obstacleClearing.OnFinished += ContinuePassage;
            _shotSequence.OnExpired += ContinuePassage;
            SetState(State.WaitingToStart);
        }

        void IDisposable.Dispose()
        {
            _chargeTransfer.OnSourceDepleted -= SourceDepleted;
            _laneAdvance.OnBlocked -= PassageBlocked;
            _playerAdvance.OnAdvanceCompleted -= AdvanceCompleted;
            _doorObject.OnOpened -= DoorOpened;
            _obstacleClearing.OnFinished -= ContinuePassage;
            _shotSequence.OnExpired -= ContinuePassage;
        }

        public void StartRound()
        {
            if (CurrentState != State.WaitingToStart)
            {
                return;
            }

            SetState(State.Playing);
            _laneAdvance.Resolve();
        }

        private void SourceDepleted()
        {
            if (CurrentState != State.Playing)
            {
                return;
            }

            SetState(State.Lost);
        }

        private void PassageBlocked()
        {
            if (CurrentState is State.Won or State.Lost)
            {
                return;
            }

            var minimumToContinue = _chargeSettings.LossSize + _chargeSettings.MinimumCharge;

            if (_chargeTransfer.SourceRemaining < minimumToContinue)
            {
                SetState(State.Lost);
            }
        }

        private void AdvanceCompleted()
        {
            if (CurrentState is State.WaitingToStart or State.Won or State.Lost)
            {
                return;
            }

            if (_playerAdvance.IsComplete && _isDoorOpened)
            {
                SetState(State.Won);
            }
        }

        private void DoorOpened()
        {
            _isDoorOpened = true;

            if (CurrentState is State.Won or State.Lost)
            {
                return;
            }

            if (_playerAdvance.IsComplete)
            {
                SetState(State.Won);
            }
        }

        private void ContinuePassage()
        {
            if (CurrentState is State.Won or State.Lost)
            {
                return;
            }

            _laneAdvance.Resolve();
        }

        private void SetState(State state)
        {
            CurrentState = state;
            _shotSequence.SetRoundActive(state == State.Playing);
            OnStateChanged?.Invoke(state);
        }
    }
}
