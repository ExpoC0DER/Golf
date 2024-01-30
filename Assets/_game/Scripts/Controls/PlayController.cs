using System;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Layer = _game.Scripts.Enums.Layer;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.Serialization;

namespace _game.Scripts.Controls
{
    public class PlayController : MonoBehaviour
    {
        [HideInInspector] public bool IsPlaying = true;

        [SerializeField] private Transform _powerBar;
        [SerializeField] private Material _powerBarMat;
        [SerializeField] private Transform _powerBarRotationPivot;
        [SerializeField] private Transform _highlightRing;

        [Header("Settings")]
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _zoomSpeedScroll;
        [SerializeField, MinMaxSlider(0.1f, 10)]
        private Vector2 _zoomMinMax;
        [SerializeField] private Vector2 _cameraSensitivity = new Vector2(300, 300);
        [SerializeField] private float _cameraPrecisionSensitivity = 30;
        [SerializeField, MinMaxSlider(0.1f, 100)]
        private Vector2 _powerBarMinMax;
        [SerializeField] private float _powerSensitivity = 0.1f;
        [SerializeField] private float _power;

        private Player _player;
        private CinemachineFramingTransposer _framingTransposer;
        private CinemachinePOV _pov;
        private Rigidbody _rb;
        private float _powerInput, _aimRotationInput, _zoomInput;
        private bool _isAiming, _shotBall, _tookTurn, _isInTheHole;

        public static event Action<int> OnPlayerFinished;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _framingTransposer = _player.PlayCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            _pov = _player.PlayCamera.GetCinemachineComponent<CinemachinePOV>();
            _rb = GetComponent<Rigidbody>();
        }

        // Method to interpolate values on an InSine curve
        private float InterpolateInSine(float inputPower)
        {
            // Ensure that inputPower is within the specified range
            inputPower = Mathf.Clamp(inputPower, _powerBarMinMax.x, _powerBarMinMax.y);
            // Normalize inputPower within the range [0, 1]
            float t = Mathf.InverseLerp(_powerBarMinMax.x, _powerBarMinMax.y, inputPower);
            // Use Mathf.Lerp to interpolate on the InSine curve
            float interpolatedValue = Mathf.Lerp(0, _power, InSine(t));

            return interpolatedValue;
        }

        // InSine function for interpolation
        private static float InSine(float t) { return 1 - Mathf.Cos((t * Mathf.PI) / 2); }

        private void Update()
        {
            if (!IsPlaying) return;

            //Set Player components positions to ball
            _powerBarRotationPivot.position = transform.position;
            _highlightRing.position = transform.position;

            if (!_player.Active) return;

            //Handle aiming when holding down left mouse button
            HandleAiming();
            HandleZooming();
        }

        private void FixedUpdate()
        {
            float ballSpeed = _rb.velocity.magnitude;
            //Turn on highlight ring if play isn't moving
            _highlightRing.gameObject.SetActive(ballSpeed < 0.01f);

            if (!_player.Active) return;

            if (_tookTurn && ballSpeed < 0.01f)
            {
                _tookTurn = false;
                _player.GameManager.SwitchPlayer(1);
            }

            if (_shotBall && ballSpeed > 0.01f)
            {
                _tookTurn = true;
                _shotBall = false;
            }

            if (_isInTheHole && ballSpeed < 0.01f)
            {
                _player.Finished = true;
                gameObject.layer = (int)Layer.Ghost;
                _player.Active = false;
                OnPlayerFinished?.Invoke(_player.PlayerID);
            }
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
                //If PowerBar is active (play was aiming) add force to ball in direction of pivot times inputPower multiplied by fraction of PowerBar
                if (_powerBarRotationPivot.gameObject.activeSelf)
                {
                    _rb.AddForce(_powerBarRotationPivot.forward * InterpolateInSine(_powerBar.localScale.z), ForceMode.Impulse);
                    _shotBall = true;
                    _player.ShotsTaken++;
                    _player.ShotsTakenTotal++;
                    gameObject.layer = (int)Layer.Player;
                }

                //Set vertical and horizontal camera movement back to normal
                _pov.m_VerticalAxis.m_MaxSpeed = _cameraSensitivity.y;
                _pov.m_HorizontalAxis.m_MaxSpeed = _cameraSensitivity.x;

                //Turn off PowerBar rotation and reset scale
                _powerBarRotationPivot.gameObject.SetActive(false);
                SetPowerBarScale(-_powerBarMinMax.y);
            }
        }

        private void HandleZooming()
        {
            //Handle zooming camera
            _framingTransposer.m_CameraDistance += _zoomInput * _zoomSpeed * Time.deltaTime;
            _framingTransposer.m_CameraDistance = Mathf.Clamp(_framingTransposer.m_CameraDistance, _zoomMinMax.x, _zoomMinMax.y);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Hole"))
            {
                _isInTheHole = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Hole"))
            {
                _isInTheHole = false;
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
            float hue = powerBarLocalScale.z.Remap(_powerBarMinMax.x, _powerBarMinMax.y, 0.42f, 0f);
            Color newColor = Color.HSVToRGB(hue, 1, 1);
            _powerBarMat.SetColor("_BaseColor", newColor);
            _powerBarMat.SetColor("_EmissionColor", newColor);
        }

        private void OnRoundStart(int round)
        {
            _isInTheHole = false;
            _player.Finished = false;
            if (_player.Map.GetRoundStartLocation(round, out Transform startLocation))
                _rb.position = startLocation.position;
            _rb.isKinematic = false;
        }
        private void OnRoundEnd(int round)
        {
            _player.ShotsTaken = 0;
            _rb.isKinematic = true;
        }

        public void OnLook(InputAction.CallbackContext ctx)
        {
            Vector2 mouseDelta = ctx.ReadValue<Vector2>();
            _pov.m_VerticalAxis.m_InputAxisValue = mouseDelta.y;
            if (!_isAiming)
                _pov.m_HorizontalAxis.m_InputAxisValue = mouseDelta.x;
        }

        public void OnZoomScroll(InputAction.CallbackContext ctx)
        {
            //Handle zooming camera
            _framingTransposer.m_CameraDistance += ctx.ReadValue<float>() * _zoomSpeedScroll;
            _framingTransposer.m_CameraDistance = Mathf.Clamp(_framingTransposer.m_CameraDistance, _zoomMinMax.x, _zoomMinMax.y);
        }
        
        public void OnZooming(InputAction.CallbackContext ctx)
        {
            _zoomInput = ctx.ReadValue<float>();
        }

        public void IsAiming(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
                _isAiming = true;
            if (ctx.canceled)
                _isAiming = false;
        }

        public void GetPower(InputAction.CallbackContext ctx) { _powerInput = ctx.ReadValue<float>(); }

        public void GetAimRotation(InputAction.CallbackContext ctx)
        {
            _aimRotationInput = ctx.ReadValue<float>();
            if (_isAiming)
                _pov.m_HorizontalAxis.m_InputAxisValue = _aimRotationInput;
        }

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

        private void OnEnable()
        {
            GameManager.OnRoundStart += OnRoundStart;
            GameManager.OnRoundEnd += OnRoundEnd;
        }

        private void OnDisable()
        {
            GameManager.OnRoundStart -= OnRoundStart;
            GameManager.OnRoundEnd -= OnRoundEnd;
        }
    }
}
