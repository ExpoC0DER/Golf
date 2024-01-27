using System.Collections.Generic;
using _game.Scripts.Controls;
using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _game.Scripts.UI
{
    public class ScoreBoard : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private ScoreBoardLine _scoreBoardLinePrefab;
        [SerializeField] private ScoreBoardLine _headerLine;
        [SerializeField] private Transform _content;

        [Header("Settings"), SerializeField, Min(0), OnValueChanged("UpdateScoreBoard")]
        private int _numberOfRounds;
        [SerializeField, Min(0), OnValueChanged("UpdateScoreBoard")]
        private int _numberOfPlayers;
        [SerializeField, OnValueChanged("UpdateScoreBoard")]
        private bool _forceSameWidth = true;

        [SerializeField, SerializedDictionary("PlayerId", "Line"), ReadOnly]
        private SerializedDictionary<int, ScoreBoardLine> _playerLines = new();


        //* Update scoreboard when changed from editor
        private void UpdateScoreBoard() { InstantiateScoreboard(_numberOfPlayers, _numberOfRounds); }

        private void InstantiateScoreboard(int numberOfPlayers, int numberOfRounds)
        {
            DeletePlayers();

            for(int i = 0; i < numberOfPlayers; i++)
            {
                ScoreBoardLine tempLine = Instantiate(_scoreBoardLinePrefab, _content);
                _playerLines.Add(i, tempLine);
                tempLine.InstantiateRoundScores(numberOfRounds, _forceSameWidth);
            }

            SetHeaderLine(numberOfRounds);

            //*Update settings if changed from script
            _numberOfPlayers = numberOfPlayers;
            _numberOfRounds = numberOfRounds;
        }

        public void InstantiateScoreboard(SerializedDictionary<int, Player> players, int numberOfRounds)
        {
            DeletePlayers();

            foreach (KeyValuePair<int, Player> pair in players)
            {
                ScoreBoardLine tempLine = Instantiate(_scoreBoardLinePrefab, _content);
                _playerLines.Add(pair.Key, tempLine);
                tempLine.InstantiateRoundScores(numberOfRounds, _forceSameWidth);
            }

            SetHeaderLine(numberOfRounds);

            //*Update settings if changed from script
            _numberOfPlayers = players.Count;
            _numberOfRounds = numberOfRounds;
        }

        private void SetHeaderLine(int numberOfRounds)
        {
            _headerLine.InstantiateRoundScores(numberOfRounds, _forceSameWidth);
            for(int i = 0; i < numberOfRounds; i++)
            {
                _headerLine.SetScore(i, i + 1);
            }
        }

        private void DeletePlayers()
        {
            foreach (KeyValuePair<int, ScoreBoardLine> pair in _playerLines)
            {
                DestroyImmediate(pair.Value.gameObject);
            }
            _playerLines.Clear();
        }

        private void OnRoundEnd(int round)
        {
            foreach (KeyValuePair<int, Player> pair in _gameManager.Players)
            {
                _playerLines[pair.Key].SetScore(round, pair.Value.ShotsTaken);
                _playerLines[pair.Key].SetTotal(pair.Value.ShotsTakenTotal);
            }
            _content.gameObject.SetActive(true);
        }

        private void OnEnable() { GameManager.OnRoundEnd += OnRoundEnd; }

        private void OnDisable() { GameManager.OnRoundEnd -= OnRoundEnd; }
    }
}
