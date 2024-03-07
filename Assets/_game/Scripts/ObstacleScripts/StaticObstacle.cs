using System;
using _game.Scripts.Building;
using UnityEngine;

namespace _game.Scripts.ObstacleScripts
{
    [RequireComponent(typeof(ObstacleBase))]
    public class StaticObstacle : MonoBehaviour
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private Collider _object;

        public void OnPlace()
        {
            _collider.enabled = false;
            _object.enabled = true;
        }
    }
}
