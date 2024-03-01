using System;
using System.Collections;
using System.Linq;
using _game.Scripts.Controls;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using DG.Tweening;
using static _game.Scripts.Enums;
using static _game.Scripts.ExtensionMethods;
using Random = UnityEngine.Random;

namespace _game.Scripts.Building
{
    public class BuildController : MonoBehaviour
    {
        public float TargetAngle { get; set; } = 45f;
        public bool CanPlace { get; set; } = true;
        public Player Player { get; private set; }

        [Header("Settings")]
        [SerializeField] private float _zoomSpeedScroll;
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _cameraRotationSpeed;
        [SerializeField, MinMaxSlider(0.1f, 10)]
        private Vector2 _zoomMinMax;
        [SerializeField, OnValueChanged("UpdateCameraSensitivity")]
        private Vector2 _cameraSensitivity = new Vector2(300, 300);
        [SerializeField] private float _moveSpeed = 0.1f;
        [SerializeField] private Color _canPlaceColor;
        [SerializeField] private Color _canNotPlaceColor;

        [Space]
        [SerializeField] private Material _previewMaterial;
        [SerializeField] private ObstaclesDatabaseSO _obstacles;
        [SerializeField] private LayerMask _layerMask;

        private Vector3 _pos;
        private RaycastHit _hit;
        private Camera _mainCamera;
        private ObstacleBase _pendingObject;
        private int _selectedObstacleIndex = -1;
        private int _spectatingPlayerIndex;
        private CinemachineFramingTransposer _framingTransposer;

        private Vector2 _mousePosition = new Vector2(960, 540);
        private Vector2 _movementInput, _joystickInput;
        private float _rotateInput, _zoomInput;

        public static event Action<string, Color> OnSpectatingPlayerChanged;

        private void Awake() { Player = GetComponent<Player>(); }

        private void Start()
        {
            _mainCamera = Camera.main;
            _framingTransposer = Player.BuildCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            ResetSpectatingPlayerKey();
            Cursor.lockState = CursorLockMode.Confined;
        }

        private bool IsBuilding
        {
            get
            {
                if (Player)
                    return Player.GamePhase == GamePhase.Build;
                return false;
            }
        }

        private void Update()
        {
            if (!IsBuilding) return;
            if (!Player.Active) return;

            MoveCursor();
            HandleMovement();
            HandleZooming();

            _previewMaterial.color = CanPlace ? _canPlaceColor : _canNotPlaceColor;

            if (_selectedObstacleIndex < 0) return;

            _pendingObject.transform.position = _pos;
            _pendingObject.transform.Rotate(Vector3.up, _rotateInput);
        }

        private void MoveCursor()
        {
            if (!Player.PlayerCursor) return;

            Player.PlayerCursor.anchoredPosition = _mousePosition;
            _mousePosition.x = Mathf.Clamp(_mousePosition.x + _joystickInput.x * _cameraSensitivity.x * Time.deltaTime, 0, Screen.width);
            _mousePosition.y = Mathf.Clamp(_mousePosition.y + _joystickInput.y * _cameraSensitivity.y * Time.deltaTime, 0, Screen.height);
        }

        private void HandleMovement()
        {
            //Multiply input with direction of followPoint and move it in resulting direction
            Vector3 moveDirection = Player.BuildCameraFollowPoint.transform.forward * _movementInput.y + Player.BuildCameraFollowPoint.transform.right * _movementInput.x;
            Player.BuildCameraFollowPoint.position += _moveSpeed * Time.deltaTime * moveDirection;
        }

        private void HandleZooming()
        {
            //Handle zooming camera
            _framingTransposer.m_CameraDistance += _zoomInput * _zoomSpeed * Time.deltaTime;
            _framingTransposer.m_CameraDistance = Mathf.Clamp(_framingTransposer.m_CameraDistance, _zoomMinMax.x, _zoomMinMax.y);
        }

        private void FixedUpdate()
        {
            Ray ray = _mainCamera.ScreenPointToRay(_mousePosition);
            //Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            if (Physics.Raycast(ray, out _hit, 1000, _layerMask))
            {
                _pos = _hit.point;
            }
        }

        public int GetRandomActiveObstacleId() { return Random.Range(0, _obstacles.EnabledObstacles.Count); }

        public void StartBuild()
        {
            OnSpectatingPlayerChanged?.Invoke(Player.PlayerName, Player.Color);
            StartPlacement(Player.NextObstacleId);

            //StartCoroutine(StartPlacementWithDelay(Player.NextObstacleId, Player.GameManager.RandomizeDuration));
        }

        private IEnumerator StartPlacementWithDelay(int index, float delay)
        {
            yield return new WaitForSeconds(delay);
            StartPlacement(index);
        }

        public void StartPlacement(int index)
        {
            //_selectedObstacleIndex = _obstacles.Obstacles.FindIndex(data => data.ID == index);
            _selectedObstacleIndex = index;
            if (_selectedObstacleIndex < 0 || _selectedObstacleIndex >= _obstacles.EnabledObstacles.Count)
            {
                Debug.LogError($"Cannot start placement! No active obstacle at {index} found.");
                return;
            }
            _pendingObject = Instantiate(_obstacles.EnabledObstacles[_selectedObstacleIndex].Prefab, _pos, Quaternion.identity);
            SetLayerRecursively(_pendingObject.gameObject, Layer.ObstaclePreview);
            _pendingObject.BuildController = this;
        }

        private void EndPlacement()
        {
            //Set back default material before reset
            SetLayerRecursively(_pendingObject.gameObject, Layer.Obstacle);
            _pendingObject.Place();
            _pendingObject = null;
            _selectedObstacleIndex = -1;
            Player.GameManager.SwitchPlayer(Iterate.Next);
            ResetSpectatingPlayerKey();
        }

        private void CancelPlacement()
        {
            if (_pendingObject)
                Destroy(_pendingObject.gameObject);
            _pendingObject = null;
            _selectedObstacleIndex = -1;
        }

        private void SetLayerRecursively(GameObject objectToSet, Layer layer)
        {
            if (objectToSet.layer != (int)Layer.IgnorePreviewRender)
                objectToSet.layer = (int)layer;
            foreach (Transform child in objectToSet.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        /// <summary>
        /// Sets spectatingPlayerIndex to one based on playerId
        /// </summary>
        private void ResetSpectatingPlayerKey()
        {
            if (!Player.GameManager) return;

            int key = Array.IndexOf(Player.GameManager.Players.Keys.ToArray(), Player.PlayerID);
            _spectatingPlayerIndex = key;
        }

        /// <summary>
        /// Set position and rotation of BuildCamera to that of different player
        /// </summary>
        /// <param name="index">Index of player in order from first starting at 0</param>
        private void SwitchPlayerCam(int index)
        {
            //Get playerId of player at index
            int playerId = Player.GameManager.Players.Keys.ToArray()[index];

            //Set current BuildCamera pos and rot to that of selected player's playCamera
            Player player = Player.GameManager.GetPlayer(playerId);
            Player.BuildCameraFollowPoint.position = player.transform.position;

            Transform playerCam = player.PlayCamera.transform;
            Player.BuildCamera.ForceCameraPosition(playerCam.position, playerCam.rotation);

            OnSpectatingPlayerChanged?.Invoke(player.PlayerName, player.Color);
        }

        private void OnRoundEnd(int round) { CancelPlacement(); }

        public void OnLeftClick(InputAction.CallbackContext ctx)
        {
            if (_selectedObstacleIndex < 0 || !CanPlace || !ctx.performed || EventSystem.current.IsPointerOverGameObject()) return;

            EndPlacement();
        }

        public void OnCameraMove(InputAction.CallbackContext ctx) { _movementInput = ctx.ReadValue<Vector2>(); }

        public void OnZoomScroll(InputAction.CallbackContext ctx)
        {
            //Handle zooming camera
            _framingTransposer.m_CameraDistance += ctx.ReadValue<float>() * _zoomSpeedScroll;
            _framingTransposer.m_CameraDistance = Mathf.Clamp(_framingTransposer.m_CameraDistance, _zoomMinMax.x, _zoomMinMax.y);
        }

        public void OnZooming(InputAction.CallbackContext ctx) { _zoomInput = ctx.ReadValue<float>(); }

        public void OnRotate(InputAction.CallbackContext ctx) { _rotateInput = ctx.ReadValue<float>(); }

        public void OnRotateCamera(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                float currentAngle = Player.BuildCamera.transform.rotation.eulerAngles.y;

                TargetAngle += 90 * ctx.ReadValue<float>();

                float angleDiff = Math.Abs(TargetAngle - currentAngle);
                angleDiff %= 360;

                Player.BuildCamera.transform.DOKill();
                Player.BuildCamera.transform.DORotate(new Vector3(0, angleDiff * ctx.ReadValue<float>(), 0), angleDiff.Remap(0, 90, 0, _cameraRotationSpeed), RotateMode.WorldAxisAdd).SetEase(Ease.Linear);
                Player.BuildCameraFollowPoint.DOKill();
                Player.BuildCameraFollowPoint.DORotate(new Vector3(0, angleDiff * ctx.ReadValue<float>(), 0), angleDiff.Remap(0, 90, 0, _cameraRotationSpeed), RotateMode.WorldAxisAdd).SetEase(Ease.Linear);

                if (TargetAngle > 360)
                    TargetAngle = 45;
                if (TargetAngle < 0)
                    TargetAngle = 315;
            }
        }

        public void GetMousePosition(InputAction.CallbackContext ctx) { _mousePosition = ctx.ReadValue<Vector2>(); }

        public void OnJoystickMove(InputAction.CallbackContext ctx) { _joystickInput = ctx.ReadValue<Vector2>(); }

        public void OnSwitchSpectatingPlayer(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            int index = (int)ctx.ReadValue<float>();
            if (index == _spectatingPlayerIndex) return;
            if (index >= Player.GameManager.Players.Count) return;

            _spectatingPlayerIndex = index;
            SwitchPlayerCam(index);
        }

        public void OnIterateSpectatingPlayer(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            _spectatingPlayerIndex += (int)ctx.ReadValue<float>();

            if (_spectatingPlayerIndex >= Player.GameManager.Players.Count)
                _spectatingPlayerIndex = 0;
            if (_spectatingPlayerIndex < 0)
                _spectatingPlayerIndex = Player.GameManager.Players.Count - 1;

            SwitchPlayerCam(_spectatingPlayerIndex);
        }

        private void OnEnable() { GameManager.OnRoundEnd += OnRoundEnd; }

        private void OnDisable() { GameManager.OnRoundEnd -= OnRoundEnd; }
    }
}
