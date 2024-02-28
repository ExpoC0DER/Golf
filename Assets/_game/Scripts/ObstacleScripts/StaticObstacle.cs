using _game.Scripts.Building;
using UnityEngine;

namespace _game.Scripts.ObstacleScripts
{
    public class StaticObstacle : ObstacleBase
    {
        [SerializeField] private Collider _collider;
        public override void Place() { _collider.isTrigger = false; }
    }
}
