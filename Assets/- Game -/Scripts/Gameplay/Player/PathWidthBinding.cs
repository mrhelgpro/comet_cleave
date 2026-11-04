using UnityEngine;

namespace Gameplay
{
    public class PathWidthBinding : MonoBehaviour
    {
        [SerializeField] private Vector3 _axis = new(1f, 0f, 0f);

        private Vector3 _baseScale;
        private bool _hasBaseScale;

        public void SetWidth(float width)
        {
            CaptureBaseScale();
            transform.localScale = _baseScale + _axis * width;
        }

        private void CaptureBaseScale()
        {
            if (_hasBaseScale)
            {
                return;
            }

            _baseScale = transform.localScale - Vector3.Scale(transform.localScale, _axis);
            _hasBaseScale = true;
        }
    }
}
