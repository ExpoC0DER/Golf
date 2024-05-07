using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace _game.Scripts.UI
{
    public class ScoreBoardLine : MonoBehaviour
    {
        [SerializeField] private RectTransform _roundsHolder;
        [SerializeField] private HorizontalLayoutGroup _roundsHolderLayout;
        [SerializeField] private TMP_Text _roundScorePrefab;
        [SerializeField] private TMP_Text _total;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private Image _bg;

        public Color Color { set { _bg.color = value; } }
        public string PlayerName { set { _playerName.text = value; } }

        [Header("Settings"), SerializeField, Min(0), OnValueChanged("UpdateLine")]
        private int _numberOfScores;
        [SerializeField] private bool _forceSameWidth = true;

        [SerializeField, ReadOnly] private List<TMP_Text> _roundScores = new List<TMP_Text>();

        //* Update line when changed from editor
        private void UpdateLine() { InstantiateRoundScores(_numberOfScores, _forceSameWidth); }

        public void InstantiateRoundScores(int numberOfScores, bool forceSameWidth)
        {
            DeleteRoundScores();

            for(int i = 0; i < numberOfScores; i++)
            {
                _roundScores.Add(Instantiate(_roundScorePrefab, _roundsHolder));
            }


            //* Consistent width
            _roundsHolderLayout.childControlWidth = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_roundsHolder);
            _roundsHolderLayout.childControlWidth = forceSameWidth;

            //*Update settings if changed from script
            _forceSameWidth = forceSameWidth;
            _numberOfScores = numberOfScores;
        }

        /// <summary>
        /// Set the play score on round if it exists else does nothing
        /// </summary>
        /// <param name="round">Round index (0 indexed)</param>
        /// <param name="score">Player score</param>
        public void SetScore(int round, int score)
        {
            if (round < _roundScores.Count)
                _roundScores[round].text = score.ToString();
        }

        public void SetTotal(int totalScore) { _total.text = totalScore.ToString(); }

        private void DeleteRoundScores()
        {
            foreach (TMP_Text roundScore in _roundScores)
            {
                DestroyImmediate(roundScore.gameObject);
            }
            _roundScores.Clear();
        }
    }
}
