using System;
using _game.Scripts.Controls;
using UnityEngine;

namespace _game.Scripts.UI
{
    public class Controls : MonoBehaviour
    {
        [SerializeField] private GameObject[] _controlsSprites;
        [SerializeField] private GameObject _controlWindow;
        [SerializeField] private GameObject _controlsKey;

        private Enums.GamePhase _gamePhase;
        private bool _isOn = false;

        private void ToggleControlsKeyboard()
        {
            _isOn = !_isOn;
            if (_isOn)
            {
                if (_gamePhase == Enums.GamePhase.Build)
                {
                    _controlWindow.SetActive(true);
                    _controlsSprites[3].SetActive(true);
                }
                if (_gamePhase == Enums.GamePhase.Play)
                {
                    _controlWindow.SetActive(true);
                    _controlsSprites[2].SetActive(true);
                }
            }
            else
            {
                _controlWindow.SetActive(false);
                TurnOffAllSprites();
            }
        }

        private void ToggleControlsController()
        {
            _isOn = !_isOn;
            if (_isOn)
            {
                if (_gamePhase == Enums.GamePhase.Build)
                {
                    _controlWindow.SetActive(true);
                    _controlsSprites[1].SetActive(true);
                }
                if (_gamePhase == Enums.GamePhase.Play)
                {
                    _controlWindow.SetActive(true);
                    _controlsSprites[0].SetActive(true);
                }
            }
            else
            {
                _controlWindow.SetActive(false);
                TurnOffAllSprites();
            }
        }

        private void OnGamePhaseChanged(Enums.GamePhase gamePhase)
        {
            _gamePhase = gamePhase;
            if (_gamePhase is not (Enums.GamePhase.Build or Enums.GamePhase.Play))
            {
                TurnOffAllSprites();
                _controlsKey.SetActive(false);
            }
            else
            {
                _controlsKey.SetActive(true);
            }
        }

        private void TurnOffAllSprites()
        {
            _isOn = false;
            _controlWindow.SetActive(_isOn);
            foreach (GameObject sprite in _controlsSprites)
            {
                sprite.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
            Player.OnShowControlsKeyboard += ToggleControlsKeyboard;
            Player.OnShowControlsController += ToggleControlsController;
        }

        private void OnDisable()
        {
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged; 
            Player.OnShowControlsKeyboard -= ToggleControlsKeyboard;
            Player.OnShowControlsController -= ToggleControlsController;
        }
    }
}
