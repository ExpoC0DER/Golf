using System;
using UnityEngine;

namespace _game.Scripts.Controls
{
    public class PlayerCursors : MonoBehaviour
    {
        [SerializeField] private RectTransform _playerCursorPrefab;
        
        private void OnPlayerJoined(Player player)
        {
            RectTransform playerCursor = Instantiate(_playerCursorPrefab, transform);
            player.PlayerCursor = playerCursor;
            playerCursor.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
            playerCursor.gameObject.SetActive(false);
        }

        private void OnEnable() { Player.OnPlayerJoined += OnPlayerJoined; }

        private void OnDisable() { Player.OnPlayerJoined -= OnPlayerJoined; }
    }
}
