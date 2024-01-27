using System;
using _game.Scripts.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _game.Scripts.Building
{
    public class BuildController : MonoBehaviour
    {
        [HideInInspector] public bool IsBuilding;
        [HideInInspector] public bool CanPlace = true;
        
        [field: SerializeField] public Material PreviewMaterial { get; private set; }
        [SerializeField] private Color _canPlaceColor;
        [SerializeField] private Color _canNotPlaceColor;

        [SerializeField] private ObstaclesDatabaseSO _obstacles;
        [SerializeField] private LayerMask _layerMask;

        private PlayerInput _playerInput;
        private Vector3 _pos;
        private RaycastHit _hit;
        private Camera _mainCamera;
        private PlacementCheck _pendingObject;
        private int _selectedObstacleIndex = -1;

        private Vector2 _mousePos;
        private float _rotateInput;

        private void Start()
        {
            _mainCamera = Camera.main;
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            if (!IsBuilding) return;

            PreviewMaterial.color = CanPlace ? _canPlaceColor : _canNotPlaceColor;

            if (_selectedObstacleIndex < 0) return;

            _pendingObject.transform.position = _pos;
            _pendingObject.transform.Rotate(Vector3.up, _rotateInput);
        }

        private void FixedUpdate()
        {
            Ray ray = _mainCamera.ScreenPointToRay(_mousePos);
            if (Physics.Raycast(ray, out _hit, 1000, _layerMask))
            {
                _pos = _hit.point;
            }
        }

        public void OnLeftClick(InputAction.CallbackContext ctx)
        {
            if (_selectedObstacleIndex < 0 || !CanPlace || !ctx.performed || EventSystem.current.IsPointerOverGameObject()) return;

            EndPlacement();
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
            _pendingObject.gameObject.layer = (int)Enums.Layer.ObstaclePreview;
            _pendingObject.BuildController = this;
        }

        private void EndPlacement()
        {
            //Set back default material before reset
            _pendingObject.gameObject.layer = (int)Enums.Layer.Obstacle;
            _pendingObject = null;
            _selectedObstacleIndex = -1;
        }

        public void GetMousePos(InputAction.CallbackContext ctx) { _mousePos = ctx.ReadValue<Vector2>(); }
        public void OnRotate(InputAction.CallbackContext ctx) { _rotateInput = ctx.ReadValue<float>(); }
    }
}
