using System;
using UnityEngine;
using static _game.Scripts.Enums;

namespace _game.Scripts.Building
{
    public class PlacementCheck : MonoBehaviour
    {
        [SerializeField] private ObstacleBase _obstacleObject;

        private void Start() { _obstacleObject.PlacementCheck = this; }

        private void OnTriggerStay(Collider other)
        {
            if (enabled)
                if (other.CompareTag(Tags.Obstacle.ToString()) || other.CompareTag(Tags.Wall.ToString()))
                {
                    _obstacleObject.BuildController.CanPlace = false;
                }
        }

        private void OnTriggerExit(Collider other)
        {
            if (enabled)
                if (other.CompareTag(Tags.Obstacle.ToString()) || other.CompareTag(Tags.Wall.ToString()))
                {
                    _obstacleObject.BuildController.CanPlace = true;
                }
        }
    }
}
