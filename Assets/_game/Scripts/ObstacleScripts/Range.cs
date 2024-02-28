using System;
using _game.Scripts.Building;
using UnityEngine;

namespace _game.Scripts.ObstacleScripts
{
    public class Range : MonoBehaviour
    {
        [SerializeField] private ObstacleBase _obstacleBase;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                _obstacleBase.PlayersInRange.Add(other.gameObject.GetComponent<Rigidbody>());
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _obstacleBase.PlayersInRange.Remove(other.gameObject.GetComponent<Rigidbody>());
        }
    }
}
