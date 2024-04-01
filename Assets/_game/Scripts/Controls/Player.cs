using System;
using System.Collections;
using System.Collections.Generic;
using _game.Scripts.Building;
using _game.Scripts.UI;
using static _game.Scripts.Enums;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _game.Scripts.Controls
{
    [RequireComponent(typeof(PlayController), typeof(BuildController))]
    public class Player : MonoBehaviour
    {
        [field: SerializeField] public CinemachineVirtualCamera PlayCamera { get; private set; }
        [field: SerializeField] public CinemachineVirtualCamera BuildCamera { get; private set; }
        //[field: SerializeField] public CinemachineVirtualCamera Camera { get; private set; }
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
        [field: SerializeField, ReadOnly] public RectTransform PlayerCursor { get; set; }
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Marker _marker;

        public GameManager GameManager { get; set; }

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
        public static event Action<int, int> OnRandomObstacleGenerated;

        public static event Action<Vector2, Player> OnUiNavigation;
        public static event Action OnUiSelect;
        public static event Action OnUiBack;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playController = GetComponent<PlayController>();
            _buildController = GetComponent<BuildController>();
        }

        public void SetColor(Color newColor)
        {
            Color = newColor;
            PlayerCursor.GetComponent<Image>().color = newColor;
            _marker.Color = newColor;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor(_materialColorReference, newColor);
            _renderer.SetPropertyBlock(mpb);
        }

        private void SetActiveCameraPriority(int newPriority)
        {
            //Camera.m_Priority = newPriority;
            if (GamePhase is GamePhase.Play)
            {
                //Set camera position to player cam
                PlayCamera.ForceCameraPosition(BuildCamera.transform.position, BuildCamera.transform.rotation);

                //Update play target angle
                _playController.TargetAngle = BuildCamera.transform.rotation.eulerAngles.y;

                //Switch camera priority
                PlayCamera.m_Priority = newPriority;
                BuildCamera.m_Priority = 0;
            }
            if (GamePhase is GamePhase.Build or GamePhase.Intermission)
            {
                //Set camera position and pivot to player cam
                BuildCameraFollowPoint.position = transform.position;
                BuildCamera.ForceCameraPosition(PlayCamera.transform.position, PlayCamera.transform.rotation);

                //Update build target angle and pivot rotation
                Vector3 playerCamRotation = PlayCamera.transform.rotation.eulerAngles;
                _buildController.TargetAngle = playerCamRotation.y;
                BuildCameraFollowPoint.rotation = Quaternion.Euler(0, playerCamRotation.y, 0);

                //Switch camera priority
                BuildCamera.m_Priority = newPriority;
                PlayCamera.m_Priority = 0;
            }
        }

        private void OnActivePlayerChanged(int playerId)
        {
            if (PlayerID == playerId)
            {
                SetActiveCameraPriority(10);
                PlayerCursor.gameObject.SetActive(true);
                Active = true;
                if (GamePhase is GamePhase.Build or GamePhase.Intermission)
                    _buildController.StartBuild();
            }
            else
            {
                SetActiveCameraPriority(0);
                PlayerCursor.gameObject.SetActive(false);
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
                    OnRandomObstacleGenerated?.Invoke(PlayerID, NextObstacleId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gamePhase), gamePhase, null);
            }
        }

        private void OnRoundStart(int round)
        {
            if (round == 0)
            {
                Map.SetGrassColor(PlayerID, Color);
                SetColor(Color);
            }
            Finished = false;
            GamePhase = GamePhase.Play;
            gameObject.layer = (int)Layer.Player;
            ShotsTaken = 0;
            _playerInput.SwitchCurrentActionMap(ActionMap.Player.ToString());
        }
        private void OnRoundEnd(int round) { _playerInput.SwitchCurrentActionMap(ActionMap.Menu.ToString()); }

        public void OnUiNavigationCallback(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            OnUiNavigation?.Invoke(ctx.ReadValue<Vector2>(), this);
        }

        public void OnUiSelectCallback(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            OnUiSelect?.Invoke();
        }
        
        public void OnUiBackCallback(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            OnUiBack?.Invoke();
        }

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
