using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace _game.Scripts.Building
{
    public class ObstacleBase : MonoBehaviour
    {
        public BuildController BuildController { get; set; }
        public List<Rigidbody> PlayersInRange { get; set; } = new List<Rigidbody>();
        public PlacementCheck PlacementCheck { get; set; }

        [SerializeField] private UnityEvent _onPlace;

        private float _fullTurnsUntilDespawn = 3;
        private float _fullTurnsCounter = -0.5f;

        public void Place()
        {
            if (PlacementCheck)
                PlacementCheck.enabled = false;
            _onPlace.Invoke();
        }

        private void OnGamePhaseChanged(Enums.GamePhase gamePhase)
        {
            _fullTurnsCounter += 0.5f;
            if (_fullTurnsCounter > _fullTurnsUntilDespawn)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable() { GameManager.OnGamePhaseChanged += OnGamePhaseChanged; }

        private void OnDisable() { GameManager.OnGamePhaseChanged -= OnGamePhaseChanged; }
    }
}
