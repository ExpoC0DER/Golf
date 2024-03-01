using _game.Scripts.Building;
using UnityEngine;

namespace _game.Scripts.ObstacleScripts
{
    public class StaticObstacle : ObstacleBase
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private Collider _object;
        public override void Place()
        {
            _collider.enabled = false;
            _object.enabled = true;
            PlacementCheck.enabled = false;
        }
    }
}
