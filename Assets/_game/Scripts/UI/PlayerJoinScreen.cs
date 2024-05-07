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
        [SerializeField] private float _inputTimeout = 0.25f;
        [Space]
        [SerializeField] private PlayerListItem _playerListItemPrefab;
        [SerializeField] private RectTransform _playerListContent;
        [SerializeField] private List<ColorPicker> _colorPickers;
        [SerializeField] private UnityEvent _onUiSelect;
        [SerializeField] private UnityEvent _onUiBack;

        private float _timeSinceLastInput;

        private void Start() { UpdateColors(); }

        private void FixedUpdate() { _timeSinceLastInput += Time.fixedDeltaTime; }

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
                    newPlayerListItem.SetName("P" + player.PlayerID);
                    player.Color = _colorPickers[colorId].Color;
                    _colorPickers[colorId].PlayerId = player.PlayerID;
                }
            }
        }

        private void OnMoveColor(Vector2 uiInput, Player player)
        {
            int currentColorId = GetCurrentColorId(player);
            if (currentColorId == -1) return;

            if (_timeSinceLastInput < _inputTimeout) return;
            _timeSinceLastInput = 0;

            //Try to move color picker to new cell
            if (!TryMoveColorPicker(ref currentColorId, OverflowClamp(currentColorId + (int)(uiInput.x + uiInput.y * 4), _colorPickers.Count), player))
                //if the cell is occupied try to move it to the next free cell (horizontally only)
                for(int i = 2; i < _colorPickers.Count; i++)
                    //if new unoccupied cell is found stop searching
                    if (TryMoveColorPicker(ref currentColorId, OverflowClamp(currentColorId + (int)(i * uiInput.x), _colorPickers.Count), player))
                        break;

            _colorPickers[currentColorId].transform.DOKill(true);
            _colorPickers[currentColorId].transform.DOShakePosition(0.5f, 10);
        }

        private bool TryMoveColorPicker(ref int currentColorPickerId, int newColorPickerId, Player player)
        {
            //if the cell is occupied return false
            if (_colorPickers[newColorPickerId].PlayerId != -1)
                return false;

            //set old cell to unoccupied and update current id
            _colorPickers[currentColorPickerId].PlayerId = -1;
            currentColorPickerId = newColorPickerId;

            //update new cell to player id and color
            _colorPickers[newColorPickerId].PlayerId = player.PlayerID;
            player.Color = _colorPickers[newColorPickerId].Color;
            return true;
        }

        private static int OverflowClamp(int value, int maxValue)
        {
            if (value >= maxValue)
                value -= maxValue;
            if (value < 0)
                value += maxValue;

            return value;
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
