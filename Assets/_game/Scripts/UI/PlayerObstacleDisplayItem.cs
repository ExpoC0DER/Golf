using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UI
{
    public class PlayerObstacleDisplayItem : MonoBehaviour
    {
        [SerializeField] private Image _obstacleIcon;
        [SerializeField] private TMP_Text _playerName;

        public void SetObstacleIcon(Sprite sprite) { _obstacleIcon.sprite = sprite; }

        public void SetPlayerName(string name) { _playerName.text = name; }

        public void SetPlayerNameColor(Color color) { _playerName.color = color; }

        public void SetPlayerNameAndColor(string name, Color color)
        {
            SetPlayerName(name);
            SetPlayerNameColor(color);
        }
    }
}
