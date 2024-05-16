using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private Transform _scoreboardContent;

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
                ScoreBoardLine tempLine = Instantiate(_scoreBoardLinePrefab, _scoreboardContent);
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
                ScoreBoardLine tempLine = Instantiate(_scoreBoardLinePrefab, _scoreboardContent);
                _playerLines.Add(pair.Key, tempLine);
                tempLine.InstantiateRoundScores(numberOfRounds, _forceSameWidth);
                tempLine.Color = pair.Value.Color;
                tempLine.PlayerName = "P" + pair.Value.PlayerID;
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
            
            //Move lines depending on player standing
            List<KeyValuePair<int, Player>> sortedPairs = _gameManager.Players.OrderBy(pair => pair.Value.ShotsTakenTotal).ToList();
            foreach (KeyValuePair<int, Player> sortedPair in sortedPairs)
            {
                _playerLines[sortedPair.Key].transform.SetAsLastSibling();
            }
            
            _content.gameObject.SetActive(true);
        }

        public void StartNextRound()
        {
            if (_gameManager.GamePhase != Enums.GamePhase.RoundEnd) return;

            _content.gameObject.SetActive(false);
            _gameManager.StartNextRound();
        }

        private void OnEnable()
        {
            GameManager.OnRoundEnd += OnRoundEnd;
            Player.OnUiSelect += StartNextRound;
        }

        private void OnDisable()
        {
            GameManager.OnRoundEnd -= OnRoundEnd;
            Player.OnUiSelect -= StartNextRound;
        }
    }
}
