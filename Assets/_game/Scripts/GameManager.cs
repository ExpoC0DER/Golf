using System;
using System.Collections.Generic;
using System.Linq;
using _game.Scripts.Controls;
using _game.Scripts.UI;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using static _game.Scripts.Enums;

namespace _game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Map _gameMap;
        [SerializeField] private CinemachineVirtualCamera _sceneCamera;
        [SerializeField] private ScoreBoard _scoreBoard;
        
        [SerializeField,ReadOnly] private GamePhase _gamePhase = GamePhase.Play;
        [field: SerializeField, SerializedDictionary("PlayerId", "Player"), ReadOnly]
        public SerializedDictionary<int, Player> Players { get; private set; } = new();
        
        private int _nextAvailableId = 1;
        private int _activePlayerId = -1;
        private int _currentRound = 0;
        private int _numberOfFinishedPlayers = 0;

        public static event Action<int> OnActivePlayerChanged;
        public static event Action<int> OnRoundEnd;
        public static event Action<int> OnRoundStart;
        public static event Action<GamePhase> OnGamePhaseChanged;

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

        public void PrintTest(string message) { Debug.Log(message); }


        private void ChangeActivePlayer(int playerId)
        {
            _activePlayerId = playerId;
            OnActivePlayerChanged?.Invoke(_activePlayerId);
        }

        private void AddPlayer(Player player)
        {
            if (player.PlayerID == -1)
            {
                player.PlayerID = GetNextAvailableId();
                if (_activePlayerId == -1)
                    _activePlayerId = player.PlayerID;
            }
            player.GameManager = this;
            player.PlayerName = "Player " + player.PlayerID;
            Players.Add(player.PlayerID, player);
        }

        public void StartGame()
        {
            foreach (KeyValuePair<int, Player> pair in Players)
            {
                Map newMap = Instantiate(_gameMap);
                int playerId = pair.Value.PlayerID;
                newMap.transform.position = Vector3.right * playerId * 100;
                pair.Value.Map = newMap;
            }

            _sceneCamera.m_Priority = 0;
            ChangeActivePlayer(_activePlayerId);
            _scoreBoard.InstantiateScoreboard(Players, _gameMap.RoundsCount);

            OnRoundStart?.Invoke(_currentRound);
        }

        public Player GetPlayer(int playerId) { return Players.TryGetValue(playerId, out Player player) ? player : null; }

        private void RemovePlayer(int playerId)
        {
            if (Players.ContainsKey(playerId))
            {
                //TODO: Disable/Delete Player

                Players.Remove(playerId);
            }
        }

        private int GetNextAvailableId()
        {
            while (Players.ContainsKey(_nextAvailableId))
            {
                _nextAvailableId++;
            }
            return _nextAvailableId++;
        }

        private void OnPlayerFinished(int playerId)
        {
            _numberOfFinishedPlayers++;
            if (_numberOfFinishedPlayers >= Players.Count)
            {
                EndRound();
            }
        }

        public void StartNextRound()
        {
            _numberOfFinishedPlayers = 0;
            OnRoundStart?.Invoke(_currentRound);
            ChangeActivePlayer(_activePlayerId);
        }

        private void EndRound()
        {
            OnRoundEnd?.Invoke(_currentRound);
            _currentRound++;
        }

        private void OnEnable()
        {
            Player.OnPlayerJoined += AddPlayer;
            Player.OnPlayerLeft += RemovePlayer;
            PlayController.OnPlayerFinished += OnPlayerFinished;
        }

        private void OnDestroy()
        {
            Player.OnPlayerJoined -= AddPlayer;
            Player.OnPlayerLeft -= RemovePlayer;
            PlayController.OnPlayerFinished -= OnPlayerFinished;
        }

        public void SwitchPlayer(int value)
        {
            //Do not switch player if round finishes
            if (_numberOfFinishedPlayers >= Players.Count) return;

            int[] keys = Players.Keys.ToArray();
            Array.Sort(keys);
            int firstPlayerId = keys.First();
            int lastPlayerId = keys.Last();
            while (true)
            {
                while (true)
                {
                    _activePlayerId += value;

                    if (_activePlayerId > lastPlayerId)
                    {
                        _activePlayerId = firstPlayerId;
                        _gamePhase = _gamePhase == GamePhase.Play ? GamePhase.Build : GamePhase.Play;
                        OnGamePhaseChanged?.Invoke(_gamePhase);
                    }
                    if (_activePlayerId < firstPlayerId)
                        _activePlayerId = lastPlayerId;

                    if (!Players.ContainsKey(_activePlayerId))
                        continue;
                    break;
                }
                if (Players[_activePlayerId].Finished)
                {
                    continue;
                }
                ChangeActivePlayer(_activePlayerId);
                break;
            }
        }
    }
}