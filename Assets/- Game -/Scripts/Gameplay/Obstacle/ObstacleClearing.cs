using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class ObstacleClearing : IDisposable
    {
        public event Action OnFinished;

        private readonly ObstacleRegistry _obstacleRegistry;
        private readonly ShotConfig _shotConfig;

        private List<ObstacleObject> _clearingObstacles;
        private int _remainingClearCount;
        
        public bool IsBusy { get; private set; }

        public ObstacleClearing(ObstacleRegistry obstacleRegistry, ShotConfig shotConfig)
        {
            _obstacleRegistry = obstacleRegistry;
            _shotConfig = shotConfig;
        }

        void IDisposable.Dispose()
        {
            ReleaseClearing();
        }

        public void Impact(Vector3 point, float chargeAmount, ObstacleObject obstacleObject)
        {
            if (obstacleObject == null)
            {
                OnFinished?.Invoke();
                return;
            }

            var radius = chargeAmount * _shotConfig.RadiusPerSize;
            var collected = _obstacleRegistry.CollectInRadius(point, radius, obstacleObject);
            BeginClearing(collected);
        }

        private void BeginClearing(List<ObstacleObject> collected)
        {
            _clearingObstacles = collected;
            _remainingClearCount = collected.Count;
            IsBusy = _remainingClearCount > 0;

            foreach (var obstacleObject in collected)
            {
                obstacleObject.OnCleared += NodeCleared;
                obstacleObject.Consume();
            }

            if (_remainingClearCount > 0)
            {
                return;
            }

            ReleaseClearing();
            OnFinished?.Invoke();
        }

        private void NodeCleared()
        {
            _remainingClearCount--;

            if (_remainingClearCount > 0)
            {
                return;
            }

            ReleaseClearing();
            OnFinished?.Invoke();
        }

        private void ReleaseClearing()
        {
            IsBusy = false;

            if (_clearingObstacles == null)
            {
                return;
            }

            foreach (var obstacleObject in _clearingObstacles)
            {
                obstacleObject.OnCleared -= NodeCleared;
            }

            _clearingObstacles = null;
            _remainingClearCount = 0;
        }
    }
}
