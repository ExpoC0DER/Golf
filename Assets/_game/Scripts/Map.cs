using System.Collections.Generic;
using UnityEngine;

namespace _game.Scripts
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private List<Transform> _roundStartLocations = new();
        [SerializeField] private List<Renderer> _roundBases = new();
        private readonly int _materialColorReference = Shader.PropertyToID("_BaseColor");
        
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

        public void SetGrassColor(Color color)
        {
            foreach (Renderer r in _roundBases)
            {
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetColor(_materialColorReference, color);
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
