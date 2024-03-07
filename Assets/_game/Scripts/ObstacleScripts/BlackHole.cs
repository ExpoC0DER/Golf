using System;
using _game.Scripts.Building;
using UnityEngine;

namespace _game.Scripts.ObstacleScripts
{
    [RequireComponent(typeof(ObstacleBase))]
    public class BlackHole : MonoBehaviour
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
            
            Vector3 position = transform.position;
            foreach (Rigidbody player in _obstacleBase.PlayersInRange)
            {
                Vector3 playerPosition = player.position;
                float distanceFromCenter = Vector3.Distance(position, playerPosition);
                float scaledStrength = distanceFromCenter.RemapClamped(0, _range.lossyScale.x, _strength, 0);
                player.AddForce((position - playerPosition).normalized * (scaledStrength * Time.deltaTime), ForceMode.Acceleration);
            }
        }
    }
}
