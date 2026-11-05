using System;

namespace Gameplay
{
    public class LaneAdvance
    {
        public event Action OnBlocked;

        private readonly LanePassage _lanePassage;
        private readonly PlayerAdvance _playerAdvance;

        public LaneAdvance(LanePassage lanePassage, PlayerAdvance playerAdvance)
        {
            _lanePassage = lanePassage;
            _playerAdvance = playerAdvance;
        }

        public void Resolve()
        {
            if (_lanePassage.TryNextDestination(out var destination))
            {
                _playerAdvance.AdvanceToward(destination);

                if (_playerAdvance.IsMoving)
                {
                    return;
                }
            }

            OnBlocked?.Invoke();
        }
    }
}
