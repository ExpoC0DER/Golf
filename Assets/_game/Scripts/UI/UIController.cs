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
        [SerializeField] private Image _obstacleIcon;


        private void OnActivePlayerChanged(int playerId)
        {
            _playerName.text = _gameManager.GetPlayer(playerId).PlayerName;
            Color playerColor = _gameManager.GetPlayer(playerId).Color;
            _playerName.color = playerColor;
            _playerNameGraphic.color = playerColor;

            _spectatingPlayerText.gameObject.SetActive(_gameManager.GamePhase == GamePhase.Build);
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

        private IEnumerator AnimateRandomObstacle(int index, float time)
        {
            int iterator = (int)(time * 10);
            for(int i = 0; i < iterator; i++)
            {
                _obstacleIcon.sprite = _obstacles.EnabledObstacles[Random.Range(0, _obstacles.EnabledObstacles.Count)].Sprite;
                yield return new WaitForSeconds(time / iterator);
            }
            _obstacleIcon.sprite = _obstacles.EnabledObstacles[index].Sprite;
        }

        private void OnStartBuild(int index, float time)
        {
            if (index < 0 || index >= _obstacles.EnabledObstacles.Count)
            {
                Debug.LogError($"Cannot find enabled obstacle on index {index}!");
                return;
            }
            StartCoroutine(AnimateRandomObstacle(index, time));
        }

        private void OnGamePhaseChanged(GamePhase gamePhase) { _obstacleIcon.gameObject.SetActive(gamePhase == GamePhase.Build); }

        private void OnEnable()
        {
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
            GameManager.OnActivePlayerChanged += OnActivePlayerChanged;
            GameManager.OnRoundStart += OnGameStart;
            BuildController.OnSpectatingPlayerChanged += SetSpectatingPlayerText;
            BuildController.OnStartBuild += OnStartBuild;
        }

        private void OnDisable()
        {
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged;
            GameManager.OnActivePlayerChanged -= OnActivePlayerChanged;
            GameManager.OnRoundStart -= OnGameStart;
            BuildController.OnSpectatingPlayerChanged -= SetSpectatingPlayerText;
            BuildController.OnStartBuild -= OnStartBuild;
        }
    }
}
