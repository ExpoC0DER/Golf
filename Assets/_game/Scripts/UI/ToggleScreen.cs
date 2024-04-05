using System;
using System.Collections.Generic;
using _game.Scripts.Controls;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _game.Scripts.UI
{
    public class ToggleScreen : MonoBehaviour
    {
        [SerializeField] private List<EventToggle> _eventToggles;
        [SerializeField] private UnityEvent _onUiBack;

        private int _selectedToggleId;

        private void OnSwitchToggle(Vector2 uiInput, Player player)
        {
            _selectedToggleId += (int)uiInput.x;
            _selectedToggleId = Mathf.Clamp(_selectedToggleId, 0, _eventToggles.Count - 1);

            _eventToggles[_selectedToggleId].Toggle.isOn = true;
            _eventToggles[_selectedToggleId].Toggle.transform.DOKill(true);
            _eventToggles[_selectedToggleId].Toggle.transform.DOShakePosition(0.5f,10);
        }

        private void OnUiSelect() { _eventToggles[_selectedToggleId].Event?.Invoke(); }
        private void OnUiBackCallback() { _onUiBack?.Invoke(); }

        private void OnEnable()
        {
            Player.OnUiNavigation += OnSwitchToggle;
            Player.OnUiSelect += OnUiSelect;
            Player.OnUiBack += OnUiBackCallback;
        }

        private void OnDisable()
        {
            Player.OnUiNavigation -= OnSwitchToggle;
            Player.OnUiSelect -= OnUiSelect;
            Player.OnUiBack -= OnUiBackCallback;
        }

        [Serializable]
        private struct EventToggle
        {
            [field: SerializeField] public Toggle Toggle { get; private set; }
            [field: SerializeField] public UnityEvent Event { get; private set; }
        }
    }
}
