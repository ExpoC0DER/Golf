using System;
using UnityEngine;

namespace _game.Scripts
{
    [Serializable]
    public class Hole : MonoBehaviour
    {
        [field: SerializeField] public Transform StartLocation { get; private set; }
        [field: SerializeField] public int PlayerId { get; set; }
        [SerializeField] private Renderer _renderer;
        private readonly int _materialColorReference = Shader.PropertyToID("_BaseColor");

        public void SetGrassColor(Color color)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor(_materialColorReference, color);
            _renderer.SetPropertyBlock(mpb);
        }
    }
}
