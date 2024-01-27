using System;
using _game.Scripts.Building;
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
                GetActiveCamera(_gamePhase).m_Priority = 10;
                Active = true;
            }
            else
            {
                GetActiveCamera(_gamePhase).m_Priority = 0;
                Active = false;
            }
        }
        private void OnGamePhaseChanged(Enums.GamePhase gamePhase)
        {
            _gamePhase = gamePhase;
            switch (gamePhase)
            {
                case Enums.GamePhase.Play:
                    _playerInput.SwitchCurrentActionMap("Player");
                    _playController.IsPlaying = true;
                    _buildController.IsBuilding = false;
                    break;
                case Enums.GamePhase.Build:
                    _playerInput.SwitchCurrentActionMap("Build");
                    _playController.IsPlaying = false;
                    _buildController.IsBuilding = true;
                    _buildController.StartPlacement(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gamePhase), gamePhase, null);
            }
        }

        //Get relevant camera depending on gamePhase
        private CinemachineVirtualCamera GetActiveCamera(Enums.GamePhase gamePhase) { return gamePhase == Enums.GamePhase.Play ? PlayCamera : BuildCamera; }

        private void OnEnable()
        {
            OnPlayerJoined?.Invoke(this);
            GameManager.OnActivePlayerChanged += OnActivePlayerChanged;
            // GameManager.OnRoundStart += OnRoundStart;
            // GameManager.OnRoundEnd += OnRoundEnd;
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
        }

        private void OnDisable()
        {
            OnPlayerLeft?.Invoke(PlayerID);
            GameManager.OnActivePlayerChanged -= OnActivePlayerChanged;
            // GameManager.OnRoundStart -= OnRoundStart;
            // GameManager.OnRoundEnd -= OnRoundEnd;
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged;
        }
    }
}
