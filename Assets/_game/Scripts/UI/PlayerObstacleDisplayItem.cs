using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UI
{
    public class PlayerObstacleDisplayItem : MonoBehaviour
    {
        [SerializeField] private Image _obstacleIcon;
        [SerializeField] private Image _obstacleIconBg;
        [SerializeField] private Image _nameBg;
        [SerializeField] private TMP_Text _playerName;

        public void SetObstacleIcon(Sprite sprite) { _obstacleIcon.sprite = sprite; }
        
        public void SetPlayerNameAndColor(string name, Color color)
        {
            _playerName.text = name;
            _obstacleIconBg.color = color;
            _nameBg.color = color;
        }
    }
}
