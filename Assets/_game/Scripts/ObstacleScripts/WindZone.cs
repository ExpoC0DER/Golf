using System;
using _game.Scripts.Building;
using UnityEngine;

namespace _game.Scripts.ObstacleScripts
{
    public class WindZone : ObstacleBase
    {
        [Header("Settings")]
        [SerializeField] private float _strength;
        [Space]
        [SerializeField] private Transform _range;
        private void FixedUpdate()
        {
            if(BuildController.Player.GameManager.GamePhase != Enums.GamePhase.Play) return;
            
            foreach (Rigidbody player in PlayersInRange)
            {
                player.AddForce(_range.forward * (_strength * Time.deltaTime), ForceMode.Acceleration);
            }
        }

        private void Start()
        {
            BuildController.CanPlace = true;
        }
        public override void Place()
        {

        }
    }
}
