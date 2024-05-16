using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace _game.Scripts.Building
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Obstacle Database")]
    // ReSharper disable once InconsistentNaming
    public class ObstaclesDatabaseSO : ScriptableObject
    {
        [field: SerializeField] public List<ObstacleData> Obstacles { get; private set; }
        public List<ObstacleData> EnabledObstacles
        {
            get
            {
                // Use LINQ to filter the list based on the Enabled property
                return Obstacles.Where(obstacle => obstacle.Enabled).ToList();
            }
        }
    }

    [Serializable]
    public class ObstacleData
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public ObstacleBase Prefab { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public bool Enabled { get; private set; }
    }
}
