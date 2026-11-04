using System;
using UnityEngine;

namespace Gameplay
{
    public class ShotSpawner : IDisposable
    {
        private const int WARM_COUNT = 4;

        private readonly PrefabPool _pool;

        public ShotSpawner(ShotConfig shotConfig, Transform shotParent)
        {
            _pool = new PrefabPool(shotConfig.ShotPrefab, shotParent);
            _pool.Warm(WARM_COUNT);
        }

        public GameObject Get()
        {
            return _pool.Get();
        }

        public void Release(GameObject instance)
        {
            _pool.Release(instance);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
