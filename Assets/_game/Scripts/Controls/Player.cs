using System;
using _game.Scripts.Building;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

        private readonly int _materialColorReference = Shader.PropertyToID("_BaseColor");
        private BuildController _buildController;
        private Enums.GamePhase _gamePhase = Enums.GamePhase.Play;
        private PlayController _playController;
        private PlayerInput _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playController = GetComponent<PlayController>();
            _buildController = GetComponent<BuildController>();
        }


        public static event Action<Player> OnPlayerJoined;
        public static event Action<int> OnPlayerLeft;

        public void SetColor(Color newColor)
        {
            Color = newColor;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor(_materialColorReference, newColor);
            GetComponent<Renderer>().SetPropertyBlock(mpb);
        }

        private void OnActivePlayerChanged(int playerId)
        {
            if (PlayerID == playerId)
            {
                SetActiveCameraPriority(10);
                Active = true;
            }
            else
            {
                SetActiveCameraPriority(0);
                Active = false;
            }
        }
        private void OnGamePhaseChanged(Enums.GamePhase gamePhase)
        {
            _gamePhase = gamePhase;
            switch (gamePhase)
            {
                case Enums.GamePhase.Play:
                    _playerInput.SwitchCurrentActionMap(Enums.ActionMap.Player.ToString());
                    _playController.IsPlaying = true;
                    _buildController.IsBuilding = false;
                    break;
                case Enums.GamePhase.Build:
                    _playerInput.SwitchCurrentActionMap(Enums.ActionMap.Build.ToString());
                    _playController.IsPlaying = false;
                    _buildController.IsBuilding = true;
                    _buildController.StartPlacement(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gamePhase), gamePhase, null);
            }
        }

        public void StartGame() { GameManager.StartGame(); }

        private void OnGameStart(int round)
        {
            if (round != 0) return;
            _playerInput.SwitchCurrentActionMap(Enums.ActionMap.Player.ToString());
            Cursor.visible = false;
        }
        private void OnRoundEnd(int round) { _playerInput.SwitchCurrentActionMap(Enums.ActionMap.Menu.ToString()); }

        private void SetActiveCameraPriority(int newPriority)
        {
            BuildCameraFollowPoint.position = PlayCamera.transform.position;
            if (_gamePhase == Enums.GamePhase.Play)
            {
                PlayCamera.m_Priority = newPriority;
                BuildCamera.m_Priority = 0;
            }
            if (_gamePhase == Enums.GamePhase.Build)
            {
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
