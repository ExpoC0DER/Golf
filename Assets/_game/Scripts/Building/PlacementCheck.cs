using System;
using UnityEngine;

namespace _game.Scripts.Building
{
    public class PlacementCheck : MonoBehaviour
    {
        public BuildManager BuildManager { get; set; }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Obstacle"))
            {
                BuildManager.CanPlace = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Obstacle"))
            {
                BuildManager.CanPlace = true;
            }
        }
    }
}
