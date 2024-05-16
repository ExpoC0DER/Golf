using System;
using System.Collections.Generic;
using System.Linq;
using _game.Scripts.Controls;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

namespace _game.Scripts.UI
{
    public class FinalScores : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private GameObject _content;
        [SerializeField] private List<PlayerListItem> _items;

        private void ShowPlacements(SerializedDictionary<int, Player> players)
        {
            _content.SetActive(true);
            int i = 0;
            foreach (KeyValuePair<int, Player> item in players.OrderBy(r => r.Value.ShotsTakenTotal))
            {
                Console.WriteLine("Key: {0}, Value: {1}", item.Key, item.Value);
                _items[i].Player = item.Value;
                _items[i].transform.parent.GetChild(1).GetComponent<Image>().color = item.Value.Color;
                //_items[i].transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1.5f);
                //_items[i].transform.parent.GetComponent<RectTransform>().DOPivotY(0.5f, 1f).SetDelay(i * 0.5f);
                _items[i].transform.parent.gameObject.SetActive(true);
                i++;
            }
        }

        [Button("TestAnim")]
        private void TestAnim()
        {
            int i = 0;
            foreach (PlayerListItem item in _items)
            {
                item.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1.5f);
                item.transform.parent.GetComponent<RectTransform>().DOPivotY(0.5f, 1f).SetDelay(i * 0.5f);
                i++;
            }
        }

        public void RestartScene()
        {
            print("Restart");
            _gameManager.RestartGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnEnable() { GameManager.OnGameEnd += ShowPlacements; }

        private void OnDisable() { GameManager.OnGameEnd -= ShowPlacements; }
    }
}
