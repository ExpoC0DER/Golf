using System;
using System.Linq;
using _game.Scripts.Controls;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField, SerializedDictionary("PlayerId", "Player")]
        private SerializedDictionary<int, PlayerController> _players = new();
        private int _nextAvailableId = 1;
        private int _activePlayerId = 1;

        public static event Action<int> OnActivePlayerChanged;

        void Update()
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
                player.PlayerID = GetNextAvailableId();
            player.GameManager = this;
            _players.Add(player.PlayerID, player);
            ChangeActivePlayer(_activePlayerId);
        }

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

        private void OnEnable()
        {
            PlayerController.OnPlayerJoined += AddPlayer;
            PlayerController.OnPlayerLeft += RemovePlayer;
        }

        private void OnDestroy()
        {
            PlayerController.OnPlayerJoined -= AddPlayer;
            PlayerController.OnPlayerLeft -= RemovePlayer;
        }

        public void SwitchPlayer(int value)
        {
            int[] keys = _players.Keys.ToArray();
            Array.Sort(keys);
            int firstPlayerId = keys.First();
            int lastPlayerId = keys.Last();
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
            ChangeActivePlayer(_activePlayerId);
        }
    }
}
