using System.Collections.Generic;
using UnityEngine;

namespace _game.Scripts.UI
{
    public class PlayerLobbyScreen : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _screens;

        public void GoToScreen(GameObject goToScreen)
        {
            foreach (GameObject screen in _screens)
                screen.SetActive(screen == goToScreen);
        }
    }
}
