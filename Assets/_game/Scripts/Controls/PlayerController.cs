using Cinemachine;
using NaughtyAttributes;
using UnityEngine;

namespace _game.Scripts.Controls
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _playerCamera;
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

        private InputManager _inputManager;
        private CinemachineFramingTransposer _framingTransposer;
        private CinemachinePOV _pov;
        private Rigidbody _rb;

        private void Awake()
        {
            _framingTransposer = _playerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            _pov = _playerCamera.GetCinemachineComponent<CinemachinePOV>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Start() { _inputManager = InputManager.Instance; }

        private void Update()
        {
            //Set Player components positions to ball
            _powerBarRotationPivot.position = transform.position;
            _highlightRing.position = transform.position;

            //Handle zooming camera
            _framingTransposer.m_CameraDistance += _inputManager.GetScrollDelta() * _zoomSpeed;
            _framingTransposer.m_CameraDistance = Mathf.Clamp(_framingTransposer.m_CameraDistance, _zoomMinMax.x, _zoomMinMax.y);

            //Cancel aiming also sets IsAiming false
            if (_inputManager.CanceledAimingThisFrame())
            {
                //Turn off PowerBar rotation and reset scale
                _powerBarRotationPivot.gameObject.SetActive(false);
                SetPowerBarScale(-_powerBarMinMax.y);
            }

            //Handle aiming when holding down left mouse button
            if (_inputManager.IsAiming() && _rb.velocity.magnitude < 0.01f)
            {
                //Lock vertical camera movement and set horizontal sensitivity to more precise
                _pov.m_VerticalAxis.m_MaxSpeed = 0;
                _pov.m_HorizontalAxis.m_MaxSpeed = _cameraPrecisionSensitivity;

                //Turn on PowerBar, scale it by mouseYDelta, and rotate facing camera 
                _powerBarRotationPivot.gameObject.SetActive(true);
                SetPowerBarScale(_inputManager.GetMouseYDelta() * +_powerSensitivity);
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

            //Turn on highlight ring if player isn't moving
            _highlightRing.gameObject.SetActive(_rb.velocity.magnitude < 0.01f);
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
    }
}
