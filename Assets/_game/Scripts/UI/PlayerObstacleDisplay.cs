using System.Collections;
using System.Collections.Generic;
using _game.Scripts.Building;
using _game.Scripts.Controls;
using static _game.Scripts.Enums;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _game.Scripts.UI
{
    public class PlayerObstacleDisplay : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Transform _content;
        [SerializeField] private ObstaclesDatabaseSO _obstacles;
        [SerializeField] private PlayerObstacleDisplayItem _prefab;
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private TMP_Text _buildPhaseText;
        [SerializeField] private SerializedDictionary<int, PlayerObstacleDisplayItem> _obstacleDisplayItems;
        [SerializeField] private SerializedDictionary<int, int> _nextObstacleIds = new SerializedDictionary<int, int>();

        [Header("Settings")]
        [SerializeField] private float _textFadeDuration;
        [SerializeField] private float _randomizeDuration;
        [SerializeField] private float _listScaleDelay;
        [SerializeField] private float _listScaleDuration;

        private int _playerCount;

        private void OnGameStart(int round)
        {
            if (round != 0) return;

            foreach (KeyValuePair<int, Player> pair in _gameManager.Players)
            {
                PlayerObstacleDisplayItem newObstacleDisplayItemLarge = Instantiate(_prefab, _content);
                newObstacleDisplayItemLarge.SetPlayerNameAndColor($"P{pair.Value.PlayerID}", pair.Value.Color);
                _obstacleDisplayItems.Add(pair.Key, newObstacleDisplayItemLarge);
            }
        }

        private IEnumerator AnimateRandomObstacle(int playerId, int index)
        {
            PlayerObstacleDisplayItem item = _obstacleDisplayItems[playerId];
            RectTransform itemRect = (RectTransform)item.transform;

            //Restart Scale
            itemRect.localScale = Vector3.one;

            //Switch to random pictures for duration
            int iterator = (int)(_randomizeDuration * 10);
            int lastRandomId = 0;
            for(int i = 0; i < iterator; i++)
            {
                int randomId = Random.Range(0, _obstacles.EnabledObstacles.Count);
                while (randomId == lastRandomId && _obstacles.EnabledObstacles.Count != 1)
                    randomId = Random.Range(0, _obstacles.EnabledObstacles.Count);

                lastRandomId = randomId;
                item.SetObstacleIcon(_obstacles.EnabledObstacles[randomId].Sprite);
                yield return new WaitForSeconds(_randomizeDuration / iterator);
            }
            item.SetObstacleIcon(_obstacles.EnabledObstacles[index].Sprite);

            yield return new WaitForSeconds(_listScaleDelay);

            Vector3 pos = itemRect.position; //Remember starting position
            ////Change pivot to TopRight corner
            itemRect.pivot = Vector2.one;
            itemRect.anchorMin = Vector2.one;
            itemRect.anchorMax = Vector2.one;
            //---------------------------------
            itemRect.position = pos; //Set position back to before changing pivot

            //Animate position and scale
            float xOffset = _obstacleDisplayItems.Count * 120; //Offset from right corner
            Vector2 padding = new Vector2(-20, -20);
            itemRect.DOAnchorPos(new Vector2(playerId * 120 - xOffset, 0) + padding, _listScaleDuration);
            itemRect.DOScale(Vector3.one * 0.35f, _listScaleDuration).OnComplete(() => _gameManager.ChangeGamePhase(GamePhase.Build));
        }

        [Button("TestAnimate")]
        private void TestAnimate()
        {
            _gridLayout.enabled = true;
            Canvas.ForceUpdateCanvases();
            _gridLayout.enabled = false;
            StopAllCoroutines();

            for(int i = 1; i < 9; i++)
            {
                OnStartBuild(i, 1);
            }
        }

        private void OnStartBuild(int playerId, int index)
        {
            if (index < 0 || index >= _obstacles.EnabledObstacles.Count)
            {
                Debug.LogError($"Cannot find enabled obstacle on index {index}!");
                return;
            }
            _playerCount++;
            _nextObstacleIds[playerId] = index;
            if (_playerCount >= _obstacleDisplayItems.Count)
            {
                StopAllCoroutines();

                _buildPhaseText.DOFade(1, _textFadeDuration).SetEase(Ease.InQuad).OnComplete(
                    () =>
                    {
                        _buildPhaseText.alpha = 0;
                        _content.gameObject.SetActive(true);

                        _gridLayout.enabled = true;
                        Canvas.ForceUpdateCanvases();
                        _gridLayout.enabled = false;

                        foreach (KeyValuePair<int, Player> pair in _gameManager.Players)
                        {
                            StartCoroutine(AnimateRandomObstacle(pair.Value.PlayerID, _nextObstacleIds[pair.Value.PlayerID]));
                        }
                        _playerCount = 0;
                    });
            }
            //StartCoroutine(AnimateRandomObstacle(playerId, index, time));
        }

        private void OnGamePhaseChanged(GamePhase gamePhase)
        {
            if (gamePhase == GamePhase.Play)
                _content.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            GameManager.OnRoundStart += OnGameStart;
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
            Player.OnRandomObstacleGenerated += OnStartBuild;
        }

        private void OnDisable()
        {
            GameManager.OnRoundStart -= OnGameStart;
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged;
            Player.OnRandomObstacleGenerated -= OnStartBuild;
        }
    }

}
