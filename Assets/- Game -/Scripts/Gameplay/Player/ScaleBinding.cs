using UnityEngine;

namespace Gameplay
{
    public class ScaleBinding : MonoBehaviour
    {
        [SerializeField] private float _multiplier = 1f;

        public void SetUniform(float value)
        {
            var uniformScale = value * _multiplier;
            transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
        }
    }
}
