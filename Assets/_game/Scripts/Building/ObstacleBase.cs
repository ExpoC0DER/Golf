using System.Collections.Generic;
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

        public void Place()
        {
            if (PlacementCheck)
                PlacementCheck.enabled = false;
            _onPlace.Invoke();
        }
    }
}
