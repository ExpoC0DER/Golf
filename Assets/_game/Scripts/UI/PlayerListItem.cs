using System;
using _game.Scripts.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _game.Scripts.UI
{
    public class PlayerListItem : MonoBehaviour
    {
        public Player Player { get; set; }

        [SerializeField] private Image _bg;
        [SerializeField] private Image _idBg;
        [SerializeField] private TMP_Text _idText;

        private void OnEnable()
        {
            if (Player)
                _idText.text = "P" + Player.PlayerID;
        }

        private void Update()
        {
            if (Player)
                SetColor(Player.Color);
        }

        public void SetColor(Color newColor)
        {
            _bg.color = newColor;
            _idBg.color = newColor;
        }
    }
}
