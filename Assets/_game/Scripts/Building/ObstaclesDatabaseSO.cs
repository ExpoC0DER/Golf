using System;
using System.Collections.Generic;
using UnityEngine;

namespace _game.Scripts.Building
{
    [CreateAssetMenu(menuName = "Scriptable Objects")]
    // ReSharper disable once InconsistentNaming
    public class ObstaclesDatabaseSO : ScriptableObject
    {
        // ReSharper disable once InconsistentNaming
        public List<ObstacleData> ObstaclesData;
    }

    [Serializable]
    public class ObstacleData
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public PlacementCheck Prefab { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
    }
}
