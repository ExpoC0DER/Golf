using System;
using System.Collections;
using _game.Scripts.Building;
using static _game.Scripts.Enums;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _game.Scripts.Controls
{
    [RequireComponent(typeof(PlayController), typeof(BuildController))]
    public class Player : MonoBehaviour
    {
        [field: SerializeField] public CinemachineVirtualCamera PlayCamera { get; private set; }
        [field: SerializeField] public CinemachineVirtualCamera BuildCamera { get; private set; }
        [field: SerializeField] public Transform BuildCameraFollowPoint { get; private set; }

        [Header("Player Info")]
        [ReadOnly] public int PlayerID = -1;
        [ReadOnly] public Color Color = Color.white;
        [ReadOnly] public string PlayerName = "Player";
        [ReadOnly] public Map Map;
        [ReadOnly] public bool Finished;
        [ReadOnly] public int ShotsTaken;
        [ReadOnly] public int ShotsTakenTotal;
        [ReadOnly] public bool Active;
        [ReadOnly] public int NextObstacleId = -1;
        [field: SerializeField, ReadOnly] public GamePhase GamePhase { get; private set; } = GamePhase.Play;

        [HideInInspector] public GameManager GameManager;
        [SerializeField] private Renderer _renderer;

        private readonly int _materialColorReference = Shader.PropertyToID("_BaseColor");
        private BuildController _buildController;
        private PlayController _playController;
        private PlayerInput _playerInput;
        
        /// <summary>
        /// Returns Player object of Player who joined
        /// </summary>
        public static event Action<Player> OnPlayerJoined;
        /// <summary>
        /// Returns int PlayerId of player who left
        /// </summary>
        public static event Action<int> OnPlayerLeft;
        /// <summary>
        /// Returns int PlayerId, int NextObstacleId, float duration
        /// </summary>
        public static event Action<int, int, float>OnRandomObstacleGenerated;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playController = GetComponent<PlayController>();
            _buildController = GetComponent<BuildController>();
        }

        public void SetColor(Color newColor)
        {
            Color = newColor;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor(_materialColorReference, newColor);
            _renderer.SetPropertyBlock(mpb);
        }

        private void SetActiveCameraPriority(int newPriority)
        {
            if (GamePhase is GamePhase.Play)
            {
                PlayCamera.m_Priority = newPriority;
                BuildCamera.m_Priority = 0;
            }
            if (GamePhase is GamePhase.Build or GamePhase.Intermission)
            {
                BuildCameraFollowPoint.position = PlayCamera.transform.position;
                BuildCamera.ForceCameraPosition(PlayCamera.transform.position, PlayCamera.transform.rotation);
                BuildCamera.m_Priority = newPriority;
                PlayCamera.m_Priority = 0;
            }
        }

        private void OnActivePlayerChanged(int playerId)
        {
            if (PlayerID == playerId)
            {
                SetActiveCameraPriority(10);
                Active = true;
                if (GamePhase is GamePhase.Build or GamePhase.Intermission)
                    _buildController.StartBuild();
            }
            else
            {
                SetActiveCameraPriority(0);
                Active = false;
            }
        }
        private void OnGamePhaseChanged(GamePhase gamePhase)
        {
            if (Finished) return;

            GamePhase = gamePhase;
            switch (gamePhase)
            {
                case GamePhase.Play:
                    _playerInput.SwitchCurrentActionMap(ActionMap.Player.ToString());
                    break;
                case GamePhase.Build:
                    _playerInput.SwitchCurrentActionMap(ActionMap.Build.ToString());
                    break;
                case GamePhase.Intermission:
                    NextObstacleId = _buildController.GetRandomActiveObstacleId();
                    OnRandomObstacleGenerated?.Invoke(PlayerID, NextObstacleId, GameManager.RandomizeDuration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gamePhase), gamePhase, null);
            }
        }

        private void OnRoundStart(int round)
        {
            if (round == 0)
            {
                Map.SetGrassColor(Color);
                Cursor.visible = false;
            }
            Finished = false;
            GamePhase = GamePhase.Play;
            gameObject.layer = (int)Layer.Player;
            ShotsTaken = 0;
            _playerInput.SwitchCurrentActionMap(ActionMap.Player.ToString());
        }
        private void OnRoundEnd(int round) { _playerInput.SwitchCurrentActionMap(ActionMap.Menu.ToString()); }

        private void OnEnable()
        {
            OnPlayerJoined?.Invoke(this);
            GameManager.OnActivePlayerChanged += OnActivePlayerChanged;
            GameManager.OnRoundStart += OnRoundStart;
            GameManager.OnRoundEnd += OnRoundEnd;
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
        }

        private void OnDisable()
        {
            OnPlayerLeft?.Invoke(PlayerID);
            GameManager.OnActivePlayerChanged -= OnActivePlayerChanged;
            GameManager.OnRoundStart -= OnRoundStart;
            GameManager.OnRoundEnd -= OnRoundEnd;
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged;
        }
    }
}
