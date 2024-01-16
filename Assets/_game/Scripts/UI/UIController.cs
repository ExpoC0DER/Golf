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


        private void OnActivePlayerChanged(int playerId)
        {
            _playerName.text = "Player " + playerId;
            Color playerColor = _gameManager.GetPlayer(playerId).Color;
            _playerName.color = playerColor;
            _playerNameGraphic.color = playerColor;
        }

        private void OnEnable() { GameManager.OnActivePlayerChanged += OnActivePlayerChanged; }

        private void OnDisable() { GameManager.OnActivePlayerChanged -= OnActivePlayerChanged; }
    }
}
