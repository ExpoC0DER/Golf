using System.Collections.Generic;
using UnityEngine;

namespace _game.Scripts.Building
{
    public abstract class ObstacleBase : MonoBehaviour
    {
        public BuildController BuildController { get; set; }
        public List<Rigidbody> PlayersInRange { get; set; } = new List<Rigidbody>();

        public abstract void Place();
    }
}
