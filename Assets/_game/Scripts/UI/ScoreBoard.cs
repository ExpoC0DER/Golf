using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace _game.Scripts.UI
{
    public class ScoreBoard : MonoBehaviour
    {
        [SerializeField] private ScoreBoardLine _scoreBoardLinePrefab;
        [SerializeField] private ScoreBoardLine _headerLine;

        [Header("Settings"), SerializeField, Min(0), OnValueChanged("UpdateScoreBoard")]
        private int _numberOfRounds;
        [SerializeField, Min(0), OnValueChanged("UpdateScoreBoard")]
        private int _numberOfPlayers;
        [SerializeField, OnValueChanged("UpdateScoreBoard")]
        private bool _forceSameWidth = true;

        [SerializeField, ReadOnly]
        private List<ScoreBoardLine> _players = new List<ScoreBoardLine>();

        //* Update scoreboard when changed from editor
        private void UpdateScoreBoard() { InstantiateScoreboard(_numberOfPlayers, _numberOfRounds); }

        private void InstantiateScoreboard(int numberOfPlayers, int numberOfRounds)
        {
            DeletePlayers();

            for(int i = 0; i < numberOfPlayers; i++)
            {
                ScoreBoardLine tempLine = Instantiate(_scoreBoardLinePrefab, transform);
                _players.Add(tempLine);
                tempLine.InstantiateRoundScores(numberOfRounds, _forceSameWidth);
            }

            SetHeaderLine(numberOfRounds);

            //*Update settings if changed from script
            _numberOfPlayers = numberOfPlayers;
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
            foreach (ScoreBoardLine roundScore in _players)
            {
                DestroyImmediate(roundScore.gameObject);
            }
            _players.Clear();
        }
    }
}
