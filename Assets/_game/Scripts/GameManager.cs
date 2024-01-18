using System;
using System.Linq;
using _game.Scripts.Controls;
using _game.Scripts.UI;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;

namespace _game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _sceneCamera;
        [SerializeField] private ScoreBoard _scoreBoard;
        [SerializeField, SerializedDictionary("PlayerId", "Player"), ReadOnly]
        private SerializedDictionary<int, PlayerController> _players = new();
        private int _nextAvailableId = 1;
        private int _activePlayerId = -1;
        private int _currentRound = 0;
        private int _numberOfFinishedPlayers = 0;

        public static event Action<int> OnActivePlayerChanged;
        public static event Action<int, SerializedDictionary<int, PlayerController>> OnRoundEnd;

        private void Update()
        {
            // Check for debug input
            if (Input.GetKeyDown(KeyCode.Period))
            {
                SwitchPlayer(1);
            }
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                SwitchPlayer(-1);
            }
        }


        private void ChangeActivePlayer(int playerId)
        {
            _activePlayerId = playerId;
            OnActivePlayerChanged?.Invoke(_activePlayerId);
        }

        private void AddPlayer(PlayerController player)
        {
            if (player.PlayerID == -1)
            {
                player.PlayerID = GetNextAvailableId();
                if (_activePlayerId == -1)
                    _activePlayerId = player.PlayerID;
            }
            player.GameManager = this;
            player.PlayerName = "Player " + player.PlayerID;
            _players.Add(player.PlayerID, player);
        }

        public void StartGame()
        {
            _sceneCamera.m_Priority = 0;
            ChangeActivePlayer(_activePlayerId);
            _scoreBoard.InstantiateScoreboard(_players, 10);
        }

        public PlayerController GetPlayer(int playerId) { return _players.TryGetValue(playerId, out PlayerController player) ? player : null; }

        private void RemovePlayer(int playerId)
        {
            if (_players.ContainsKey(playerId))
            {
                //TODO: Disable/Delete Player

                _players.Remove(playerId);
            }
        }

        private int GetNextAvailableId()
        {
            while (_players.ContainsKey(_nextAvailableId))
            {
                _nextAvailableId++;
            }
            return _nextAvailableId++;
        }

        private void OnPlayerFinished(int playerId)
        {
            _numberOfFinishedPlayers++;
            if (_numberOfFinishedPlayers >= _players.Count)
            {
                OnRoundEnd?.Invoke(_currentRound, _players);
            }
        }

        private void OnEnable()
        {
            PlayerController.OnPlayerJoined += AddPlayer;
            PlayerController.OnPlayerLeft += RemovePlayer;
            PlayerController.OnPlayerFinished += OnPlayerFinished;
        }

        private void OnDestroy()
        {
            PlayerController.OnPlayerJoined -= AddPlayer;
            PlayerController.OnPlayerLeft -= RemovePlayer;
            PlayerController.OnPlayerFinished -= OnPlayerFinished;
        }

        public void SwitchPlayer(int value)
        {
            if (_numberOfFinishedPlayers >= _players.Count) return;

            int[] keys = _players.Keys.ToArray();
            Array.Sort(keys);
            int firstPlayerId = keys.First();
            int lastPlayerId = keys.Last();
            while (true)
            {
                while (true)
                {
                    _activePlayerId += value;

                    if (_activePlayerId > lastPlayerId)
                        _activePlayerId = firstPlayerId;
                    if (_activePlayerId < firstPlayerId)
                        _activePlayerId = lastPlayerId;

                    if (!_players.ContainsKey(_activePlayerId))
                        continue;
                    break;
                }
                if (_players[_activePlayerId].Finished)
                {
                    continue;
                }
                ChangeActivePlayer(_activePlayerId);
                break;
            }
        }
    }
}
