using System;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _game.Scripts.Controls
{
    public class PlayerController : MonoBehaviour
    {
        [field: SerializeField, ReadOnly] public int PlayerID { get; set; } = -1;
        [SerializeField, ReadOnly] private bool _active;
        public GameManager GameManager { get; set; }

        [Space, SerializeField] private CinemachineVirtualCamera _playerCamera;
        [SerializeField] private Transform _powerBar;
        [SerializeField] private Transform _powerBarRotationPivot;
        [SerializeField] private Transform _highlightRing;

        [Header("Settings")]
        [SerializeField] private float _zoomSpeed;
        [SerializeField, MinMaxSlider(1, 10)] private Vector2 _zoomMinMax;
        [SerializeField] private Vector2 _cameraSensitivity = new Vector2(300, 300);
        [SerializeField] private float _cameraPrecisionSensitivity = 30;
        [SerializeField, MinMaxSlider(1, 100)] private Vector2 _powerBarMinMax;
        [SerializeField] private float _powerSensitivity = 0.1f;
        [SerializeField] private float _power;

        private CinemachineFramingTransposer _framingTransposer;
        private CinemachinePOV _pov;
        private Rigidbody _rb;
        private bool _isAiming;
        private float _powerInput;

        public static event Action<PlayerController> OnPlayerJoined;
        public static event Action<int> OnPlayerLeft;

        private void Awake()
        {
            _framingTransposer = _playerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            _pov = _playerCamera.GetCinemachineComponent<CinemachinePOV>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            //Set Player components positions to ball
            _powerBarRotationPivot.position = transform.position;
            _highlightRing.position = transform.position;

            //Turn on highlight ring if player isn't moving
            _highlightRing.gameObject.SetActive(_rb.velocity.magnitude < 0.01f);

            if (!_active) return;

            //Handle aiming when holding down left mouse button
            HandleAiming();
        }

        private void HandleAiming()
        {
            if (_isAiming && _rb.velocity.magnitude < 0.01f)
            {
                //Lock vertical camera movement and set horizontal sensitivity to more precise
                _pov.m_VerticalAxis.m_MaxSpeed = 0;
                _pov.m_HorizontalAxis.m_MaxSpeed = _cameraPrecisionSensitivity;

                //Turn on PowerBar, scale it by mouseYDelta, and rotate facing camera 
                _powerBarRotationPivot.gameObject.SetActive(true);
                SetPowerBarScale(_powerInput * +_powerSensitivity);
                _powerBarRotationPivot.rotation = Quaternion.Euler(0, _pov.m_HorizontalAxis.Value, 0);
            }
            else
            {
                //If PowerBar is active (player was aiming) add force to ball in direction of pivot times power multiplied by fraction of PowerBar
                if (_powerBarRotationPivot.gameObject.activeSelf)
                    _rb.AddForce(_powerBarRotationPivot.forward * (_power * (_powerBar.localScale.z / _powerBarMinMax.y)), ForceMode.Impulse);

                //Set vertical and horizontal camera movement back to normal
                _pov.m_VerticalAxis.m_MaxSpeed = _cameraSensitivity.y;
                _pov.m_HorizontalAxis.m_MaxSpeed = _cameraSensitivity.x;

                //Turn off PowerBar rotation and reset scale
                _powerBarRotationPivot.gameObject.SetActive(false);
                SetPowerBarScale(-_powerBarMinMax.y);
            }
        }

        public void OnLook(InputAction.CallbackContext ctx)
        {
            Vector2 mouseDelta = ctx.ReadValue<Vector2>();
            _pov.m_VerticalAxis.m_InputAxisValue = mouseDelta.y;
            _pov.m_HorizontalAxis.m_InputAxisValue = mouseDelta.x;
        }

        public void OnZooming(InputAction.CallbackContext ctx)
        {
            //Handle zooming camera
            _framingTransposer.m_CameraDistance += ctx.ReadValue<float>() * _zoomSpeed;
            _framingTransposer.m_CameraDistance = Mathf.Clamp(_framingTransposer.m_CameraDistance, _zoomMinMax.x, _zoomMinMax.y);
        }

        public void IsAiming(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
                _isAiming = true;
            if (ctx.canceled)
                _isAiming = false;
        }

        public void GetPower(InputAction.CallbackContext ctx) { _powerInput = ctx.ReadValue<float>(); }

        public void CancelAiming(InputAction.CallbackContext ctx)
        {
            //Cancel aiming
            if (ctx.performed)
            {
                _isAiming = false;
                //Turn off PowerBar and reset scale
                _powerBarRotationPivot.gameObject.SetActive(false);
                SetPowerBarScale(-_powerBarMinMax.y);
            }
        }

        /// <summary>
        /// Changes PowerBar length(Z scale) by amount
        /// </summary>
        /// <param name="lenght">Amount to change length by</param>
        private void SetPowerBarScale(float lenght)
        {
            Vector3 powerBarLocalScale = _powerBar.localScale;
            powerBarLocalScale.z += lenght;
            powerBarLocalScale.z = Mathf.Clamp(powerBarLocalScale.z, _powerBarMinMax.x, _powerBarMinMax.y);
            _powerBar.localScale = powerBarLocalScale;
        }

        private void OnActivePlayerChanged(int playerId)
        {
            if (PlayerID == playerId)
            {
                _playerCamera.m_Priority = 10;
                _active = true;
            }
            else
            {
                _playerCamera.m_Priority = 0;
                _active = false;
            }
        }

        private void OnEnable()
        {
            GameManager.OnActivePlayerChanged += OnActivePlayerChanged;
            OnPlayerJoined?.Invoke(this);
        }

        private void OnDisable()
        {
            GameManager.OnActivePlayerChanged -= OnActivePlayerChanged;
            OnPlayerLeft?.Invoke(PlayerID);
        }
    }
}
