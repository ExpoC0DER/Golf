using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _game.Scripts.Controls;
using _game.Scripts.UI;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using static _game.Scripts.Enums;

namespace _game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Map _gameMap;
        [SerializeField] private CinemachineVirtualCamera _sceneCamera;
        [SerializeField] private ScoreBoard _scoreBoard;

        [field: SerializeField, ReadOnly] public GamePhase GamePhase { get; private set; } = GamePhase.Menu;
        [field: SerializeField, SerializedDictionary("PlayerId", "Player"), ReadOnly]
        public SerializedDictionary<int, Player> Players { get; private set; } = new();

        private int _nextAvailableId = 1;
        private int _activePlayerId = -1;
        private int _currentRound;
        private int _numberOfFinishedPlayers;

        public static event Action<Player> OnPlayerAdded;
        public static event Action<int> OnActivePlayerChanged;
        public static event Action<int> OnRoundEnd;
        public static event Action<int> OnRoundStart;
        public static event Action<GamePhase> OnGamePhaseChanged;
        public static event Action<SerializedDictionary<int, Player>> OnGameEnd;

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                Cursor.visible = false;
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
            OnPlayerAdded?.Invoke(player);
        }

        public void StartGame()
        {
            Map newMap = Instantiate(_gameMap);
            newMap.GenerateMap(Players);
            foreach (KeyValuePair<int, Player> pair in Players)
            {
                // int playerId = pair.Value.PlayerID;
                // newMap.transform.position = Vector3.right * playerId * 100;
                pair.Value.Map = newMap;
            }

            _sceneCamera.m_Priority = 0;
            ChangeActivePlayer(_activePlayerId);
            _scoreBoard.InstantiateScoreboard(Players, _gameMap.RoundsCount);

            OnRoundStart?.Invoke(_currentRound);
            GamePhase = GamePhase.Play;
            OnGamePhaseChanged?.Invoke(GamePhase);
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

        private void OnPlayerTookTurn(int playerId, bool finished)
        {
            if (finished)
            {
                _numberOfFinishedPlayers++;
                if (_numberOfFinishedPlayers >= Players.Count)
                {
                    if (_currentRound < _gameMap.RoundsCount - 1)
                        EndRound();
                    else
                        EndGame();
                    return; //Do not switch player if round finishes
                }
            }
            SwitchPlayer(Iterate.Next);
        }

        public void StartNextRound()
        {
            _numberOfFinishedPlayers = 0;
            OnRoundStart?.Invoke(_currentRound);
            GamePhase = GamePhase.Play;
            OnGamePhaseChanged?.Invoke(GamePhase);
            _activePlayerId = Players[FirstPlayerId].PlayerID;
            ChangeActivePlayer(_activePlayerId);
        }

        private void SwitchPlayer(Iterate iterate)
        {
            int nextPlayerId = GetIteratedPlayerId(iterate);
            if (nextPlayerId <= _activePlayerId)
            {
                GamePhase = GamePhase == GamePhase.Play ? GamePhase.ObstacleSelection : GamePhase.Play;
                OnGamePhaseChanged?.Invoke(GamePhase);
            }
            _activePlayerId = nextPlayerId;
            ChangeActivePlayer(_activePlayerId);
        }

        private int FirstPlayerId
        {
            get
            {
                int[] keys = Players.Keys.ToArray();
                Array.Sort(keys);
                return keys.First();
            }
        }

        private int LastPlayerId
        {
            get
            {
                int[] keys = Players.Keys.ToArray();
                Array.Sort(keys);
                return keys.Last();
            }
        }

        private int GetIteratedPlayerId(Iterate iterate, int startPlayerId = -1)
        {
            int nextPlayerId = startPlayerId == -1 ? _activePlayerId : startPlayerId;
            int iterateBy = iterate == Iterate.Next ? 1 : -1;
            int firstPlayerId = FirstPlayerId;
            int lastPlayerId = LastPlayerId;
            while (true)
            {
                while (true)
                {
                    nextPlayerId += iterateBy;

                    if (nextPlayerId > lastPlayerId)
                        nextPlayerId = firstPlayerId;
                    if (nextPlayerId < firstPlayerId)
                        nextPlayerId = lastPlayerId;

                    if (!Players.ContainsKey(nextPlayerId))
                        continue;
                    break;
                }
                if (Players[nextPlayerId].Finished)
                {
                    continue;
                }
                break;
            }
            return nextPlayerId;
        }

        public void ChangeGamePhase(GamePhase newGamePhase)
        {
            GamePhase = GamePhase;
            OnGamePhaseChanged?.Invoke(newGamePhase);
        }

        private void EndRound()
        {
            OnRoundEnd?.Invoke(_currentRound);
            _currentRound++;
            GamePhase = GamePhase.RoundEnd;
            OnGamePhaseChanged?.Invoke(GamePhase);
        }

        private void EndGame()
        {
            OnGameEnd?.Invoke(Players);
            GamePhase = GamePhase.GameEnd;
            OnGamePhaseChanged?.Invoke(GamePhase);
        }

        public void RestartGame()
        {
            List<Player> p = Players.Values.ToList();
            foreach (Player player in p)
            {
                Destroy(player.gameObject);
            }
            OnDisable();
        }

        private void OnEnable()
        {
            Player.OnPlayerJoined += AddPlayer;
            Player.OnPlayerLeft += RemovePlayer;
            Player.OnPlayerTookTurn += OnPlayerTookTurn;
        }

        private void OnDisable()
        {
            Player.OnPlayerJoined -= AddPlayer;
            Player.OnPlayerLeft -= RemovePlayer;
            Player.OnPlayerTookTurn -= OnPlayerTookTurn;
        }
    }
}
