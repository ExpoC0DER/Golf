using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UI
{
    public class ColorPicker : MonoBehaviour
    {
        [field: SerializeField, OnValueChanged(nameof(UpdateImageColors))]
        public Color Color { get; private set; }
        [SerializeField] private Image _bgImage;
        [SerializeField] private Image _idImage;
        [SerializeField] private TMP_Text _idText;

        [SerializeField, ReadOnly] private int _playerId = -1;
        public int PlayerId
        {
            get { return _playerId; }
            set
            {
                _playerId = value;

                _idImage.gameObject.SetActive(value != -1);
                _idText.text = "P" + value;
            }
        }

        public void UpdateImageColors()
        {
            _bgImage.color = Color;
            _idImage.color = Color;
        }
    }
}
