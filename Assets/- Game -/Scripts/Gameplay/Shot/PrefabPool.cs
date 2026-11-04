using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class PrefabPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly List<GameObject> _instances = new();

        public PrefabPool(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public void Warm(int count)
        {
            for (var index = 0; index < count; index++)
            {
                var created = Object.Instantiate(_prefab, _parent);
                created.SetActive(false);
                _instances.Add(created);
            }
        }

        public GameObject Get()
        {
            for (var index = 0; index < _instances.Count; index++)
            {
                var instance = _instances[index];

                if (instance.activeSelf)
                {
                    continue;
                }

                instance.SetActive(true);
                return instance;
            }

            var created = Object.Instantiate(_prefab, _parent);
            created.SetActive(true);
            _instances.Add(created);
            return created;
        }

        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.transform.DOKill();
            instance.SetActive(false);
            instance.transform.SetParent(_parent, false);
        }

        public void Dispose()
        {
            for (var index = 0; index < _instances.Count; index++)
            {
                var instance = _instances[index];

                if (instance == null)
                {
                    continue;
                }

                instance.transform.DOKill();
                Object.Destroy(instance);
            }

            _instances.Clear();
        }
    }
}
