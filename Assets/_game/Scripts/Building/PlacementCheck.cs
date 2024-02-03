using System;
using UnityEngine;
using static _game.Scripts.Enums;

namespace _game.Scripts.Building
{
    public class PlacementCheck : MonoBehaviour
    {
        public BuildController BuildController { get; set; }
        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public void Place() { _collider.isTrigger = false; }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(Tags.Obstacle.ToString()) || other.CompareTag(Tags.Wall.ToString()))
            {
                BuildController.CanPlace = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(Tags.Obstacle.ToString()) || other.CompareTag(Tags.Wall.ToString()))
            {
                BuildController.CanPlace = true;
            }
        }
    }
}
