using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Gameplay
{
    public class ObstacleZone : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private GameObject _obstaclePrefab;
        [SerializeField] private float _outerRadius = 11f;
        [SerializeField] private float _emptyRadius = 3.8f;
        [SerializeField] private int _capsuleCount = 320;
        [SerializeField] private float _size = 0.9f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _positionChaos = 0.35f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _sizeChaos = 0.15f;

        [SerializeField] private int _seed = 1;

        [Button]
        private void Generate()
        {
            if (Application.isPlaying || _obstaclePrefab == null)
            {
                return;
            }

            if (_capsuleCount < 1 || _emptyRadius < 0f || _outerRadius <= _emptyRadius || _size <= 0f)
            {
                return;
            }

            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Generate Obstacles");

            for (var childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
            {
                Undo.DestroyObjectImmediate(transform.GetChild(childIndex).gameObject);
            }

            var random = new System.Random(_seed);
            var innerSquared = _emptyRadius * _emptyRadius;
            var outerSquared = _outerRadius * _outerRadius;
            var annulusArea = Mathf.PI * (outerSquared - innerSquared);
            var spacing = Mathf.Sqrt(annulusArea / _capsuleCount);
            var goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));

            for (var capsuleIndex = 0; capsuleIndex < _capsuleCount; capsuleIndex++)
            {
                var offsetX = ((float)random.NextDouble() - 0.5f) * spacing * _positionChaos;
                var offsetZ = ((float)random.NextDouble() - 0.5f) * spacing * _positionChaos;
                var sizeJitter = ((float)random.NextDouble() * 2f - 1f) * _sizeChaos;
                var scale = _size * (1f + sizeJitter);
                var minimumScale = _size * 0.5f;

                if (scale < minimumScale)
                {
                    scale = minimumScale;
                }

                var areaT = (capsuleIndex + 0.5f) / _capsuleCount;
                var radius = Mathf.Sqrt(innerSquared + areaT * (outerSquared - innerSquared));
                var angle = capsuleIndex * goldenAngle;
                var positionX = Mathf.Cos(angle) * radius + offsetX;
                var positionZ = Mathf.Sin(angle) * radius + offsetZ;
                var padding = scale * 0.5f;
                var minimumRadius = _emptyRadius + padding;
                var maximumRadius = _outerRadius - padding;

                if (maximumRadius <= minimumRadius)
                {
                    continue;
                }

                var placedRadius = Mathf.Sqrt(positionX * positionX + positionZ * positionZ);

                if (placedRadius <= 0.0001f)
                {
                    positionX = minimumRadius;
                    positionZ = 0f;
                }
                else
                {
                    var clampedRadius = Mathf.Clamp(placedRadius, minimumRadius, maximumRadius);
                    var radiusScale = clampedRadius / placedRadius;
                    positionX *= radiusScale;
                    positionZ *= radiusScale;
                }

                var instance = PrefabUtility.InstantiatePrefab(_obstaclePrefab, transform) as GameObject;

                if (instance == null)
                {
                    continue;
                }

                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = new Vector3(scale, scale, scale);
                instance.transform.localPosition = new Vector3(positionX, scale, positionZ);
                Undo.RegisterCreatedObjectUndo(instance, "Generate Obstacles");
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}
