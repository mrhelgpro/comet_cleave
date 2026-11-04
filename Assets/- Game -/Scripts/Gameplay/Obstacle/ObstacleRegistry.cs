using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class ObstacleRegistry
    {
        private readonly List<ObstacleObject> _obstacleObjects = new();

        public void Register(ObstacleObject obstacleObject)
        {
            _obstacleObjects.Add(obstacleObject);
        }

        public void Unregister(ObstacleObject obstacleObject)
        {
            _obstacleObjects.Remove(obstacleObject);
        }

        public void CollectActive(List<ObstacleObject> result)
        {
            result.Clear();

            foreach (var obstacleObject in _obstacleObjects)
            {
                if (!obstacleObject.gameObject.activeInHierarchy || obstacleObject.IsCleared)
                {
                    continue;
                }

                result.Add(obstacleObject);
            }
        }

        public List<ObstacleObject> CollectInRadius(Vector3 origin, float radius, ObstacleObject struck)
        {
            var result = new List<ObstacleObject>();
            var radiusSquared = radius * radius;

            if (struck != null && !struck.IsInfected && !struck.IsCleared)
            {
                result.Add(struck);
            }

            foreach (var candidate in _obstacleObjects)
            {
                if (candidate == struck)
                {
                    continue;
                }

                if (candidate.IsInfected || candidate.IsCleared)
                {
                    continue;
                }

                if (DistanceSquaredXZ(origin, candidate.transform.position) > radiusSquared)
                {
                    continue;
                }

                result.Add(candidate);
            }

            return result;
        }

        private static float DistanceSquaredXZ(Vector3 current, Vector3 candidate)
        {
            var offsetX = current.x - candidate.x;
            var offsetZ = current.z - candidate.z;
            return offsetX * offsetX + offsetZ * offsetZ;
        }
    }
}
