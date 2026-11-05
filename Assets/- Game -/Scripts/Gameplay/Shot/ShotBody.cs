using UnityEngine;

namespace Gameplay
{
    public class ShotBody : MonoBehaviour
    {
        [SerializeField] private ScaleBinding _scaleBinding;
        [SerializeField] private ShotMotion _motion;

        public ShotMotion Motion => _motion;

        public void ShowCharge(Transform player, PathRoute route, float chargeAmount)
        {
            _scaleBinding.SetUniform(chargeAmount);
            var playerRadius = player.lossyScale.x * 0.5f;
            var shotRadius = transform.lossyScale.x * 0.5f;
            var position = player.position + AimDirection(player, route) * (playerRadius + shotRadius);
            transform.position = position;
        }

        public Vector3 AimDirection(Transform player, PathRoute route)
        {
            var direction = Vector3.zero;

            if (route != null && route.PointCount > 0)
            {
                direction = route.GetPoint(route.PointCount - 1).position - player.position;
                direction.y = 0f;
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }

            return direction;
        }
    }
}
