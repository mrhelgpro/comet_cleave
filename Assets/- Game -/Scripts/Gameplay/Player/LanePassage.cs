using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class LanePassage
    {
        private readonly PlayerAdvance _playerAdvance;
        private readonly ObstacleRegistry _obstacleRegistry;
        private readonly PlayerConfig _playerConfig;
        private readonly PathRoute _route;
        private readonly List<ObstacleObject> _blockingObstacles = new();

        public LanePassage(
            PlayerAdvance playerAdvance,
            ObstacleRegistry obstacleRegistry,
            PlayerConfig playerConfig,
            PathRoute route)
        {
            _playerAdvance = playerAdvance;
            _obstacleRegistry = obstacleRegistry;
            _playerConfig = playerConfig;
            _route = route;
        }

        public bool TryNextDestination(out Vector3 destination)
        {
            var origin = _playerAdvance.transform.position;
            destination = origin;
            var route = _route;

            if (route == null)
            {
                return false;
            }

            var playerRadius = _playerAdvance.transform.lossyScale.x * 0.5f;
            _obstacleRegistry.CollectActive(_blockingObstacles);
            var cursor = origin;
            var traveled = 0f;
            var blockedTravel = float.PositiveInfinity;

            for (var pointIndex = _playerAdvance.CurrentIndex + 1; pointIndex < route.PointCount; pointIndex++)
            {
                var point = route.GetPoint(pointIndex).position;
                var segment = point - cursor;
                segment.y = 0f;
                var length = segment.magnitude;

                if (length <= 0.0001f)
                {
                    cursor = point;
                    continue;
                }

                var direction = segment / length;
                var entry = NearestEntry(cursor, direction, length, playerRadius);

                if (entry < length)
                {
                    blockedTravel = traveled + entry;
                    break;
                }

                traveled += length;
                cursor = point;
            }

            var travel = float.IsPositiveInfinity(blockedTravel) ? traveled : blockedTravel - _playerConfig.ApproachGap;

            if (!float.IsPositiveInfinity(blockedTravel) && travel < blockedTravel && travel < 0.02f && blockedTravel > 0.02f)
            {
                travel = blockedTravel;
            }

            if (travel < 0f)
            {
                travel = 0f;
            }

            if (travel <= 0.02f)
            {
                return false;
            }

            destination = PositionAlong(origin, travel);
            return true;
        }

        private float NearestEntry(Vector3 origin, Vector3 direction, float length, float playerRadius)
        {
            var nearest = length;

            foreach (var obstacleObject in _blockingObstacles)
            {
                if (!obstacleObject.gameObject.activeInHierarchy || obstacleObject.IsCleared)
                {
                    continue;
                }

                var required = playerRadius + obstacleObject.BodyRadius;
                var offsetX = obstacleObject.transform.position.x - origin.x;
                var offsetZ = obstacleObject.transform.position.z - origin.z;
                var along = offsetX * direction.x + offsetZ * direction.z;
                var distanceSquared = offsetX * offsetX + offsetZ * offsetZ;
                var lateralSquared = distanceSquared - along * along;

                if (lateralSquared < 0f)
                {
                    lateralSquared = 0f;
                }

                var requiredSquared = required * required;

                if (lateralSquared >= requiredSquared)
                {
                    continue;
                }

                var entry = along - Mathf.Sqrt(requiredSquared - lateralSquared);

                if (entry > length)
                {
                    continue;
                }

                if (entry < 0f)
                {
                    if (distanceSquared < requiredSquared && along >= 0f)
                    {
                        return 0f;
                    }

                    continue;
                }

                if (entry < nearest)
                {
                    nearest = entry;
                }
            }

            return nearest;
        }

        private Vector3 PositionAlong(Vector3 origin, float distance)
        {
            var route = _route;
            var cursor = origin;
            var remaining = distance;

            for (var pointIndex = _playerAdvance.CurrentIndex + 1; pointIndex < route.PointCount; pointIndex++)
            {
                var point = route.GetPoint(pointIndex).position;
                var segment = point - cursor;
                segment.y = 0f;
                var length = segment.magnitude;

                if (length <= 0.0001f)
                {
                    cursor = point;
                    continue;
                }

                if (remaining <= length)
                {
                    var destination = cursor + segment / length * remaining;
                    destination.y = origin.y;
                    return destination;
                }

                remaining -= length;
                cursor = point;
            }

            cursor.y = origin.y;
            return cursor;
        }
    }
}
