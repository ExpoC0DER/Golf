using System.Collections.Generic;
using UnityEngine;

namespace _game.Scripts
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private List<Transform> _roundStartLocations = new();
        public int RoundsCount { get => _roundStartLocations.Count; }

        public bool GetRoundStartLocation(int round, out Transform startTransform)
        {
            if (round <= _roundStartLocations.Count)
            {
                startTransform = _roundStartLocations[round];
                return true;
            }
            startTransform = null;
            return false;
        }
    }
}
