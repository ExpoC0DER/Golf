using System.Collections;
using _game.Scripts.Building;
using static _game.Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private ObstaclesDatabaseSO _obstacles;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private Image _playerNameGraphic;
        [SerializeField] private GameObject _playerJoinScreen;
        [SerializeField] private TMP_Text _spectatingPlayerText;


        private void OnActivePlayerChanged(int playerId)
        {
            _playerName.text = _gameManager.GetPlayer(playerId).PlayerName;
            Color playerColor = _gameManager.GetPlayer(playerId).Color;
            _playerNameGraphic.color = playerColor;
        }

        private void OnGameStart(int round)
        {
            if (round != 0) return;

            _playerJoinScreen.SetActive(false);
            _playerNameGraphic.gameObject.SetActive(true);
        }

        private void SetSpectatingPlayerText(string playerName, Color color)
        {
            _spectatingPlayerText.text = "Spectating : " + playerName;
            _spectatingPlayerText.color = color;
        }

        private void OnGamePhaseChanged(GamePhase gamePhase)
        {
            _spectatingPlayerText.gameObject.SetActive(gamePhase is GamePhase.Build);
            _playerNameGraphic.gameObject.SetActive(gamePhase is GamePhase.Build or GamePhase.Play);
        }

        private void OnEnable()
        {
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
            GameManager.OnActivePlayerChanged += OnActivePlayerChanged;
            GameManager.OnRoundStart += OnGameStart;
            BuildController.OnSpectatingPlayerChanged += SetSpectatingPlayerText;
        }

        private void OnDisable()
        {
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged;
            GameManager.OnActivePlayerChanged -= OnActivePlayerChanged;
            GameManager.OnRoundStart -= OnGameStart;
            BuildController.OnSpectatingPlayerChanged -= SetSpectatingPlayerText;
        }
    }
}
