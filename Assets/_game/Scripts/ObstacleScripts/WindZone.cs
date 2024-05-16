using System;
using _game.Scripts.Building;
using UnityEngine;

namespace _game.Scripts.ObstacleScripts
{
    [RequireComponent(typeof(ObstacleBase))]
    public class WindZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _strength;
        [Space]
        [SerializeField] private Transform _range;
        
        private ObstacleBase _obstacleBase;

        private void Awake()
        {
            _obstacleBase = GetComponent<ObstacleBase>();
        }
        
        private void FixedUpdate()
        {
            if(_obstacleBase.BuildController.Player.GameManager.GamePhase != Enums.GamePhase.Play) return;
            
            foreach (Rigidbody player in _obstacleBase.PlayersInRange)
            {
                player.AddForce(_range.forward * (_strength * Time.deltaTime), ForceMode.Acceleration);
            }
        }

        private void Start()
        {
            _obstacleBase.BuildController.CanPlace = true;
        }
    }
}
