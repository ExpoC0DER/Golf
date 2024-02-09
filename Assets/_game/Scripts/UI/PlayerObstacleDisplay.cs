using System.Collections;
using System.Collections.Generic;
using _game.Scripts.Building;
using _game.Scripts.Controls;
using static _game.Scripts.Enums;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _game.Scripts.UI
{
    public class PlayerObstacleDisplay : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Transform _content;
        [SerializeField] private ObstaclesDatabaseSO _obstacles;
        [SerializeField] private PlayerObstacleDisplayItem _prefab;
        [SerializeField] private SerializedDictionary<int, PlayerObstacleDisplayItem> _obstacleDisplayItems;

        private void OnGameStart(int round)
        {
            if (round != 0) return;

            foreach (KeyValuePair<int, Player> pair in _gameManager.Players)
            {
                PlayerObstacleDisplayItem newObstacleDisplayItem = Instantiate(_prefab, _content);
                newObstacleDisplayItem.SetPlayerNameAndColor(pair.Value.PlayerName, pair.Value.Color);
                _obstacleDisplayItems.Add(pair.Key, newObstacleDisplayItem);
            }
        }

        private IEnumerator AnimateRandomObstacle(int playerId, int index, float time)
        {
            int iterator = (int)(time * 10);
            for(int i = 0; i < iterator; i++)
            {
                _obstacleDisplayItems[playerId].SetObstacleIcon(_obstacles.EnabledObstacles[Random.Range(0, _obstacles.EnabledObstacles.Count)].Sprite);
                yield return new WaitForSeconds(time / iterator);
            }
            _obstacleDisplayItems[playerId].SetObstacleIcon(_obstacles.EnabledObstacles[index].Sprite);
        }

        private void OnStartBuild(int playerId, int index, float time)
        {
            if (index < 0 || index >= _obstacles.EnabledObstacles.Count)
            {
                Debug.LogError($"Cannot find enabled obstacle on index {index}!");
                return;
            }
            StartCoroutine(AnimateRandomObstacle(playerId, index, time));
        }
        
        private void OnGamePhaseChanged(GamePhase gamePhase) { _content.gameObject.SetActive(gamePhase is GamePhase.Intermission or GamePhase.Build); }

        private void OnEnable()
        {
            GameManager.OnRoundStart += OnGameStart;
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
            Player.OnRandomObstacleGenerated += OnStartBuild;
        }

        private void OnDisable()
        {
            GameManager.OnRoundStart -= OnGameStart;
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged;
            Player.OnRandomObstacleGenerated -= OnStartBuild;
        }
    }
}
