using System;
using _game.Scripts.Building;
using static _game.Scripts.Enums;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

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

        [HideInInspector] public GameManager GameManager;
        [SerializeField] private Renderer _renderer;

        private readonly int _materialColorReference = Shader.PropertyToID("_BaseColor");
        private BuildController _buildController;
        private Enums.GamePhase _gamePhase = Enums.GamePhase.Play;
        private PlayController _playController;
        private PlayerInput _playerInput;
        public static event Action<Player> OnPlayerJoined;
        public static event Action<int> OnPlayerLeft;

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

        private void OnActivePlayerChanged(int playerId)
        {
            if (PlayerID == playerId)
            {
                SetActiveCameraPriority(10);
                Active = true;
                if (_gamePhase == GamePhase.Build)
                    _buildController.StartPlacement(0);
            }
            else
            {
                SetActiveCameraPriority(0);
                Active = false;
            }
        }
        private void OnGamePhaseChanged(GamePhase gamePhase)
        {
            _gamePhase = gamePhase;
            switch (gamePhase)
            {
                case GamePhase.Play:
                    _playerInput.SwitchCurrentActionMap(ActionMap.Player.ToString());
                    _playController.IsPlaying = true;
                    _buildController.IsBuilding = false;
                    break;
                case GamePhase.Build:
                    _playerInput.SwitchCurrentActionMap(ActionMap.Build.ToString());
                    _playController.IsPlaying = false;
                    _buildController.IsBuilding = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gamePhase), gamePhase, null);
            }
        }

        public void StartGame() { GameManager.StartGame(); }

        private void OnGameStart(int round)
        {
            if (round != 0) return;
            _playerInput.SwitchCurrentActionMap(ActionMap.Player.ToString());
            Map.SetGrassColor(Color);
            Cursor.visible = false;
        }
        private void OnRoundEnd(int round) { _playerInput.SwitchCurrentActionMap(ActionMap.Menu.ToString()); }

        private void SetActiveCameraPriority(int newPriority)
        {
            BuildCameraFollowPoint.position = PlayCamera.transform.position;
            if (_gamePhase == GamePhase.Play)
            {
                PlayCamera.m_Priority = newPriority;
                BuildCamera.m_Priority = 0;
            }
            if (_gamePhase == GamePhase.Build)
            {
                BuildCamera.ForceCameraPosition(PlayCamera.transform.position, PlayCamera.transform.rotation);
                BuildCamera.m_Priority = newPriority;
                PlayCamera.m_Priority = 0;
            }
        }

        private void OnEnable()
        {
            OnPlayerJoined?.Invoke(this);
            GameManager.OnActivePlayerChanged += OnActivePlayerChanged;
            GameManager.OnRoundStart += OnGameStart;
            GameManager.OnRoundEnd += OnRoundEnd;
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
        }

        private void OnDisable()
        {
            OnPlayerLeft?.Invoke(PlayerID);
            GameManager.OnActivePlayerChanged -= OnActivePlayerChanged;
            GameManager.OnRoundStart -= OnGameStart;
            GameManager.OnRoundEnd -= OnRoundEnd;
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged;
        }
    }
}
