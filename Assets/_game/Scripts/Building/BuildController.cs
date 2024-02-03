using System;
using System.Linq;
using _game.Scripts.Controls;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static _game.Scripts.Enums;

namespace _game.Scripts.Building
{
    public class BuildController : MonoBehaviour
    {
        [HideInInspector] public bool IsBuilding;
        [HideInInspector] public bool CanPlace = true;

        [FormerlySerializedAs("_zoomSpeed")]
        [Header("Settings")]
        [SerializeField] private float _zoomSpeedScroll;
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

        private Player _player;
        private CinemachinePOV _pov;
        private Vector3 _pos;
        private RaycastHit _hit;
        private Camera _mainCamera;
        private PlacementCheck _pendingObject;
        private int _selectedObstacleIndex = -1;
        private int _spectatingPlayerIndex;

        private Vector2 _mousePos;
        private float _rotateInput;
        private Vector3 _movementInput;

        private void Start()
        {
            _mainCamera = Camera.main;
            _player = GetComponent<Player>();
            _pov = _player.BuildCamera.GetCinemachineComponent<CinemachinePOV>();
            ResetSpectatingPlayerKey();
            UpdateCameraSensitivity();
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void Update()
        {
            if (!IsBuilding) return;
            if (!_player.Active) return;

            HandleMovement();

            _previewMaterial.color = CanPlace ? _canPlaceColor : _canNotPlaceColor;

            if (_selectedObstacleIndex < 0) return;

            _pendingObject.transform.position = _pos;
            _pendingObject.transform.Rotate(Vector3.up, _rotateInput);
        }

        private void HandleMovement()
        {
            //Rotate followPoint towards point on front of camera
            Vector3 cameraForwardPos = _mainCamera.transform.position + _mainCamera.transform.forward;
            _player.BuildCameraFollowPoint.LookAt(new Vector3(cameraForwardPos.x, _player.BuildCameraFollowPoint.position.y, cameraForwardPos.z));
            //Multiply input with direction of followPoint and move it in resulting direction
            Vector3 moveDirection = _player.BuildCameraFollowPoint.transform.forward * _movementInput.z + _player.BuildCameraFollowPoint.transform.up * _movementInput.y + _player.BuildCameraFollowPoint.transform.right * _movementInput.x;
            Vector3 cameraPos = _player.BuildCameraFollowPoint.position + _moveSpeed * Time.deltaTime * moveDirection;
            cameraPos.y = Mathf.Clamp(cameraPos.y, _zoomMinMax.x, _zoomMinMax.y);
            _player.BuildCameraFollowPoint.position = cameraPos;

        }

        private void FixedUpdate()
        {
            //Ray ray = _mainCamera.ScreenPointToRay(_mousePos);
            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            if (Physics.Raycast(ray, out _hit, 1000, _layerMask))
            {
                _pos = _hit.point;
            }
        }

        public void StartPlacement(int index)
        {
            _selectedObstacleIndex = _obstacles.ObstaclesData.FindIndex(data => data.ID == index);
            if (_selectedObstacleIndex < 0)
            {
                Debug.LogError($"No ID found {index}");
                return;
            }
            _pendingObject = Instantiate(_obstacles.ObstaclesData[_selectedObstacleIndex].Prefab, _pos, Quaternion.identity);
            _pendingObject.gameObject.layer = (int)Layer.ObstaclePreview;
            _pendingObject.BuildController = this;
        }

        private void EndPlacement()
        {
            //Set back default material before reset
            _pendingObject.gameObject.layer = (int)Layer.Obstacle;
            _pendingObject.Place();
            _pendingObject = null;
            _selectedObstacleIndex = -1;
            _player.GameManager.SwitchPlayer(Iterate.Next);
            ResetSpectatingPlayerKey();
        }

        private void UpdateCameraSensitivity()
        {
            if (!Application.isPlaying) return;
            _pov.m_HorizontalAxis.m_MaxSpeed = _cameraSensitivity.x;
            _pov.m_VerticalAxis.m_MaxSpeed = _cameraSensitivity.y;
        }

        /// <summary>
        /// Sets spectatingPlayerIndex to one based on playerId
        /// </summary>
        private void ResetSpectatingPlayerKey()
        {
            int key = Array.IndexOf(_player.GameManager.Players.Keys.ToArray(), _player.PlayerID);
            _spectatingPlayerIndex = key;
        }

        /// <summary>
        /// Set position and rotation of BuildCamera to that of different player
        /// </summary>
        /// <param name="index">Index of player in order from first starting at 0</param>
        private void SwitchPlayerCam(int index)
        {
            //Get playerId of player at index
            int playerId = _player.GameManager.Players.Keys.ToArray()[index];
            //Set current BuildCamera pos and rot to that of selected player's playCamera
            Transform playerCam = _player.GameManager.Players[playerId].PlayCamera.transform;
            _player.BuildCameraFollowPoint.position = playerCam.position;
            _player.BuildCamera.ForceCameraPosition(playerCam.position, playerCam.rotation);
        }

        public void OnLeftClick(InputAction.CallbackContext ctx)
        {
            if (_selectedObstacleIndex < 0 || !CanPlace || !ctx.performed || EventSystem.current.IsPointerOverGameObject()) return;

            EndPlacement();
        }

        public void OnCameraMove(InputAction.CallbackContext ctx) { _movementInput = ctx.ReadValue<Vector3>(); }

        public void OnLook(InputAction.CallbackContext ctx)
        {
            Vector2 mouseDelta = ctx.ReadValue<Vector2>();
            _pov.m_VerticalAxis.m_InputAxisValue = mouseDelta.y;
            _pov.m_HorizontalAxis.m_InputAxisValue = mouseDelta.x;
        }

        public void OnZooming(InputAction.CallbackContext ctx)
        {
            Vector3 cameraPos = _player.BuildCameraFollowPoint.position;
            cameraPos.y += ctx.ReadValue<float>() * _zoomSpeedScroll;
            cameraPos.y = Mathf.Clamp(cameraPos.y, _zoomMinMax.x, _zoomMinMax.y);
            _player.BuildCameraFollowPoint.position = cameraPos;
        }

        public void GetMousePos(InputAction.CallbackContext ctx) { _mousePos = ctx.ReadValue<Vector2>(); }
        public void OnRotate(InputAction.CallbackContext ctx) { _rotateInput = ctx.ReadValue<float>(); }

        public void OnSwitchSpectatingPlayer(InputAction.CallbackContext ctx)
        {
            if(!ctx.performed) return;
            
            int index = (int)ctx.ReadValue<float>();
            if (index == _spectatingPlayerIndex) return;
            if (index >= _player.GameManager.Players.Count) return;

            _spectatingPlayerIndex = index;
            SwitchPlayerCam(index);
        }

        public void OnIterateSpectatingPlayer(InputAction.CallbackContext ctx)
        {
            if(!ctx.performed) return;
            
            _spectatingPlayerIndex += (int)ctx.ReadValue<float>();

            if (_spectatingPlayerIndex >= _player.GameManager.Players.Count)
                _spectatingPlayerIndex = 0;
            if (_spectatingPlayerIndex < 0)
                _spectatingPlayerIndex = _player.GameManager.Players.Count - 1;
            
            SwitchPlayerCam(_spectatingPlayerIndex);
        }
    }
}
