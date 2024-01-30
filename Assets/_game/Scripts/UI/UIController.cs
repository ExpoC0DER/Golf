using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private Image _playerNameGraphic;
        [SerializeField] private GameObject _playerJoinScreen;


        private void OnActivePlayerChanged(int playerId)
        {
            _playerName.text = _gameManager.GetPlayer(playerId).PlayerName;
            Color playerColor = _gameManager.GetPlayer(playerId).Color;
            _playerName.color = playerColor;
            _playerNameGraphic.color = playerColor;
        }

        private void OnGameStart(int round)
        {
            if(round != 0) return;
            
            _playerJoinScreen.SetActive(false);
            _playerNameGraphic.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            GameManager.OnActivePlayerChanged += OnActivePlayerChanged;
            GameManager.OnRoundStart += OnGameStart;
        }

        private void OnDisable()
        {
            GameManager.OnActivePlayerChanged -= OnActivePlayerChanged; 
            GameManager.OnRoundStart -= OnGameStart;
        }
    }
}
