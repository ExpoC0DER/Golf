using System;
using System.Collections.Generic;
using _game.Scripts.Controls;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _game.Scripts.UI
{
    public class PlayerJoinScreen : MonoBehaviour
    {
        [SerializeField] private PlayerListItem _playerListItemPrefab;
        [SerializeField] private RectTransform _playerListContent;
        [SerializeField] private List<ColorPicker> _colorPickers;
        [SerializeField] private UnityEvent _onUiSelect;
        [SerializeField] private UnityEvent _onUiBack;

        private void Start() { UpdateColors(); }

        [Button("Update Colors")]
        private void UpdateColors()
        {
            foreach (ColorPicker colorPicker in _colorPickers)
            {
                colorPicker.UpdateImageColors();
            }
        }

        private void OnPlayerAdded(Player player)
        {
            PlayerListItem newPlayerListItem = Instantiate(_playerListItemPrefab, _playerListContent);
            newPlayerListItem.Player = player;
            int colorId = -1;
            while (colorId == -1)
            {
                colorId = Random.Range(0, 8);
                if (_colorPickers[colorId].PlayerId != -1)
                    colorId = -1;
                else
                {
                    newPlayerListItem.SetColor(_colorPickers[colorId].Color);
                    player.Color = _colorPickers[colorId].Color;
                    _colorPickers[colorId].PlayerId = player.PlayerID;
                }
            }
        }

        private void OnMoveColor(Vector2 uiInput, Player player)
        {

            int currentColorId = GetCurrentColorId(player);
            if (currentColorId == -1) return;

            _colorPickers[currentColorId].PlayerId = -1;

            currentColorId += (int)(uiInput.x + uiInput.y * 4);

            if (currentColorId >= _colorPickers.Count)
                currentColorId -= _colorPickers.Count;
            if (currentColorId < 0)
                currentColorId += _colorPickers.Count;

            _colorPickers[currentColorId].PlayerId = player.PlayerID;
            player.Color = _colorPickers[currentColorId].Color;
            _colorPickers[currentColorId].transform.DOShakePosition(0.5f, 10);
        }

        private int GetCurrentColorId(Player player)
        {
            int i = -1;
            foreach (ColorPicker item in _colorPickers)
            {
                i++;
                if (item.PlayerId == player.PlayerID)
                {
                    break;
                }
            }
            return i;
        }

        private void OnUiSelectCallback() { _onUiSelect?.Invoke(); }

        private void OnUiBackCallback() { _onUiBack?.Invoke(); }

        private void OnEnable()
        {
            GameManager.OnPlayerAdded += OnPlayerAdded;
            Player.OnUiNavigation += OnMoveColor;
            Player.OnUiSelect += OnUiSelectCallback;
            Player.OnUiBack += OnUiBackCallback;
            //Player.OnPlayerLeft += RemovePlayer;
        }

        private void OnDisable()
        {
            GameManager.OnPlayerAdded -= OnPlayerAdded;
            Player.OnUiNavigation -= OnMoveColor;
            Player.OnUiSelect -= OnUiSelectCallback;
            Player.OnUiBack -= OnUiBackCallback;
            //Player.OnPlayerLeft -= RemovePlayer;
        }
    }
}
