using _game.Scripts.Controls;
using UnityEngine;

namespace _game.Scripts.UI
{
    public class PlayerJoinScreen : MonoBehaviour
    {
        [SerializeField] private PlayerListItem _playerListItemPrefab;
        [SerializeField] private RectTransform _playerListContent;
        [SerializeField]

        private void OnPlayerJoined(PlayerController playerController)
        {
            PlayerListItem newPlayerListItem = Instantiate(_playerListItemPrefab, _playerListContent);
            newPlayerListItem.PlayerController = playerController;
        }

        private void OnEnable()
        {
            PlayerController.OnPlayerJoined += OnPlayerJoined;
            //PlayerController.OnPlayerLeft += RemovePlayer;
        }

        private void OnDestroy()
        {
            PlayerController.OnPlayerJoined -= OnPlayerJoined;
            //PlayerController.OnPlayerLeft -= RemovePlayer;
        }
    }
}
