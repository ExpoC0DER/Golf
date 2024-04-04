using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _game.Scripts
{
    [Serializable]
    public class Hole : MonoBehaviour
    {
        [field: SerializeField] public Transform StartLocation { get; private set; }
        [field: SerializeField] public int PlayerId { get; set; }
        [SerializeField] private Renderer _renderer;
        [SerializeField] private MeshRenderer[] _renderers;
        private readonly int _materialColorReference = Shader.PropertyToID("_Color");

        public void SetGrassColor(Color color)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor(_materialColorReference, color);
            //_renderer.SetPropertyBlock(mpb);
            _renderers = GetComponentsInChildren<MeshRenderer>(); 
            foreach (MeshRenderer r in _renderers)
            {
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
