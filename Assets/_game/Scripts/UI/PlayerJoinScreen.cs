using _game.Scripts.Controls;
using UnityEngine;

namespace _game.Scripts.UI
{
    public class PlayerJoinScreen : MonoBehaviour
    {
        [SerializeField] private PlayerListItem _playerListItemPrefab;
        [SerializeField] private RectTransform _playerListContent;

        private void OnPlayerJoined(Player player)
        {
            PlayerListItem newPlayerListItem = Instantiate(_playerListItemPrefab, _playerListContent);
            newPlayerListItem.Player = player;
        }

        private void OnEnable()
        {
            Player.OnPlayerJoined += OnPlayerJoined;
            //Player.OnPlayerLeft += RemovePlayer;
        }

        private void OnDestroy()
        {
            Player.OnPlayerJoined -= OnPlayerJoined;
            //Player.OnPlayerLeft -= RemovePlayer;
        }
    }
}
