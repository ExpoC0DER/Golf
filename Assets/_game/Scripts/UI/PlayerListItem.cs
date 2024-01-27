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
        [HideInInspector] public Player Player;
        
        [SerializeField] private Image _colorImage;
        [SerializeField] private FlexibleColorPicker _colorPicker;
        [SerializeField] private TMP_Text _placeHolder;
        
        private void Start()
        {
            Color randomColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f);
            _colorImage.color = randomColor;
            _colorPicker.SetColor(randomColor);
            SetPlayerColor(randomColor);
            _placeHolder.text = Player.PlayerName;
        }

        public void SetPlayerColor(Color newColor) { Player.SetColor(newColor); }

        public void SetPlayerName(string newName) { Player.PlayerName = string.IsNullOrWhiteSpace(newName) ? _placeHolder.text : newName; }
    }
}
