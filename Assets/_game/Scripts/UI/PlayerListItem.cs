using System;
using _game.Scripts.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _game.Scripts.UI
{
    public class PlayerListItem : MonoBehaviour
    {
        [SerializeField] private Image _colorImage;
        [SerializeField] private FlexibleColorPicker _colorPicker;
        [SerializeField] private TMP_Text _placeHolder;

        public PlayerController PlayerController { get; set; }

        private void Start()
        {
            Color randomColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f);
            _colorImage.color = randomColor;
            _colorPicker.SetColor(randomColor);
            SetPlayerColor(randomColor);
            _placeHolder.text = PlayerController.PlayerName;
        }

        public void SetPlayerColor(Color newColor) { PlayerController.SetColor(newColor); }

        public void SetPlayerName(string newName) { PlayerController.PlayerName = string.IsNullOrWhiteSpace(newName) ? _placeHolder.text : newName; }
    }
}
