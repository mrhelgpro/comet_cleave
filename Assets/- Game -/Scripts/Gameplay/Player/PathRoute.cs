using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class PathRoute : MonoBehaviour
    {
        [SerializeField] private List<Transform> _points = new();
        [SerializeField] private List<PathWidthBinding> _tiles = new();

        public int PointCount => _points.Count;
        public int TileCount => _tiles.Count;

        public Transform GetPoint(int index)
        {
            return _points[index];
        }

        public PathWidthBinding GetTile(int index)
        {
            return _tiles[index];
        }
    }
}
