using System;
using Cinemachine;
using DG.Tweening;
using NaughtyAttributes;
using NaughtyAttributes.Test;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static _game.Scripts.Enums;

namespace _game.Scripts.Controls
{
    public class PlayController : MonoBehaviour
    {
        public float TargetAngle { get; set; } = 45f;
        public float Power { get; private set; }
        [field: SerializeField, MinMaxSlider(0, 10)]
        public Vector2 PowerDistanceMinMax { get; private set; }
        [field: SerializeField, MinMaxSlider(0, 10)]
        public Vector2 PowerBarMinMax { get; private set; }

        [SerializeField] private Transform _aimPosition;
        [SerializeField] private Transform _powerBarRotationPivot;
        [SerializeField] private Transform _highlightRing;

        [Header("Settings")]
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _zoomSpeedScroll;
        [SerializeField] private float _cameraRotationSpeed;
        [SerializeField, MinMaxSlider(0.1f, 10)]
        private Vector2 _zoomMinMax;
        [SerializeField] private Vector2 _cameraSensitivity = new Vector2(300, 300);
        [SerializeField] private float _stopVelocityThreshold;
        [SerializeField] private float _minPowerThreshold;
        [SerializeField] private LayerMask _golfTrackLayer;

        private Player _player;
        private CinemachineFramingTransposer _framingTransposer;
        private Rigidbody _rb;
        private float _zoomInput;
        private Vector2 _mousePosition = new Vector2(960, 540);
        private Vector2 _joystickInput;
        private bool _isAiming, _shotBall, _tookTurn, _isInTheHole;
        private Camera _mainCamera;
        private Vector3 _startTurnPos;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            _framingTransposer = _player.PlayCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

        // Method to interpolate values on an InSine curve
        private float Interpolate(float inputPower)
        {
            // Ensure that inputPower is within the specified range
            inputPower = Mathf.Clamp(inputPower, PowerBarMinMax.x, PowerBarMinMax.y);
            // Normalize inputPower within the range [0, 1]
            float t = Mathf.InverseLerp(PowerBarMinMax.x, PowerBarMinMax.y, inputPower);
            // Use Mathf.Lerp to interpolate on the InSine curve
            float interpolatedValue = Mathf.Lerp(0, Power, t);

            return interpolatedValue;
        }

        private bool IsPlaying
        {
            get
            {
                if (_player)
                    return _player.GamePhase == GamePhase.Play;
                return false;
            }
        }

        private void Update()
        {
            //Set Player components positions to ball
            _powerBarRotationPivot.position = transform.position;
            _highlightRing.position = transform.position;

            if (!IsPlaying) return;
            if (!_player.Active) return;

            //Handle aiming when holding down left mouse button
            HandleAiming();
            HandleZooming();
            if (_rb.velocity.magnitude <= _stopVelocityThreshold)
            {
                _rb.velocity = Vector3.zero;
            }
        }

        private void FixedUpdate()
        {
            float ballSpeed = _rb.velocity.magnitude;
            //Turn on highlight ring if play isn't moving
            _highlightRing.gameObject.SetActive(ballSpeed < 0.01f);

            if (!_player.Active) return;

            if (ballSpeed > 0.01f)
            {
                if (_shotBall)
                {
                    _tookTurn = true;
                    _shotBall = false;
                }
            }
            else
            {
                if (_isInTheHole)
                {
                    _player.Finished = true;
                    gameObject.layer = (int)Layer.Ghost;
                    _player.Active = false;
                    _rb.isKinematic = true;
                    _player.InvokeTookTurn(true);
                }
                else if (_tookTurn)
                {
                    bool touchingTrack = Physics.Raycast(transform.position, Vector3.down, 10, _golfTrackLayer);
                    if (!touchingTrack)
                        _rb.position = _startTurnPos;
                    else
                        _startTurnPos = _rb.position;
                    _tookTurn = false;
                    _player.InvokeTookTurn();
                }

            }
        }

        private Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            Plane xy = new Plane(Vector3.down, new Vector3(0, z, 0));
            xy.Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }

        private void HandleAiming()
        {
            MoveCursor();

            Vector3 mouseWorldPos = GetWorldPositionOnPlane(_mousePosition, transform.position.y);
            _powerBarRotationPivot.LookAt(mouseWorldPos, Vector3.up);

            if (_isAiming && _rb.velocity.magnitude < 0.01f)
            {
                _powerBarRotationPivot.gameObject.SetActive(true);
                Power = Vector3.Distance(_aimPosition.position, mouseWorldPos).RemapClamped(PowerDistanceMinMax.x, PowerDistanceMinMax.y, PowerBarMinMax.x, PowerBarMinMax.y);
            }
            else
            {
                //If PowerBar is active (play was aiming) add force to ball in direction of pivot times inputPower multiplied by fraction of PowerBar
                if (Power > _minPowerThreshold)
                {
                    _rb.AddForce(_powerBarRotationPivot.forward * Power, ForceMode.Impulse);
                    _shotBall = true;
                    _player.ShotsTaken++;
                    _player.ShotsTakenTotal++;
                    gameObject.layer = (int)Layer.Player;
                }

                //Turn off PowerBar rotation and reset scale
                _powerBarRotationPivot.gameObject.SetActive(false);
                Power = 0f;
            }
        }

        private void MoveCursor()
        {
            if (!_player.PlayerCursor) return;

            _player.PlayerCursor.anchoredPosition = _mousePosition;
            _mousePosition.x = Mathf.Clamp(_mousePosition.x + _joystickInput.x * _cameraSensitivity.x * Time.deltaTime, 0, Screen.width);
            _mousePosition.y = Mathf.Clamp(_mousePosition.y + _joystickInput.y * _cameraSensitivity.y * Time.deltaTime, 0, Screen.height);
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

        private void OnRoundStart(int round)
        {
            _isInTheHole = false;
            _tookTurn = false;
            _shotBall = false;
            if (_player.Map.GetRoundStartLocation(round, _player.PlayerID, out Transform startLocation))
            {
                Transform ballPrefab = transform.parent;
                ballPrefab.SetParent(startLocation.parent.parent);
                ballPrefab.position = startLocation.position;
                ballPrefab.localRotation = Quaternion.Euler(Vector3.zero);
                _startTurnPos = startLocation.position;

                transform.localPosition = Vector3.zero;
                _rb.position = transform.position;
                //_rb.position = startLocation.position;
            }
            _rb.isKinematic = false;
        }
        private void OnRoundEnd(int round) { _rb.isKinematic = true; }

        public void OnZoomScroll(InputAction.CallbackContext ctx)
        {
            //Handle zooming camera
            _framingTransposer.m_CameraDistance += ctx.ReadValue<float>() * _zoomSpeedScroll;
            _framingTransposer.m_CameraDistance = Mathf.Clamp(_framingTransposer.m_CameraDistance, _zoomMinMax.x, _zoomMinMax.y);
        }

        public void OnZooming(InputAction.CallbackContext ctx) { _zoomInput = ctx.ReadValue<float>(); }

        public void IsAiming(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
                _isAiming = true;
            if (ctx.canceled)
                _isAiming = false;
        }

        public void OnRotateCamera(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                float currentAngle = _player.PlayCamera.transform.rotation.eulerAngles.y;

                TargetAngle += 90 * ctx.ReadValue<float>();

                float angleDiff = Math.Abs(TargetAngle - currentAngle);
                angleDiff %= 360;

                _player.PlayCamera.transform.DOKill();
                _player.PlayCamera.transform.DORotate(new Vector3(0, angleDiff * ctx.ReadValue<float>(), 0), angleDiff.Remap(0, 90, 0, _cameraRotationSpeed), RotateMode.WorldAxisAdd).SetEase(Ease.Linear);
                _powerBarRotationPivot.DOKill();
                _powerBarRotationPivot.DORotate(new Vector3(0, angleDiff * ctx.ReadValue<float>(), 0), angleDiff.Remap(0, 90, 0, _cameraRotationSpeed), RotateMode.WorldAxisAdd).SetEase(Ease.Linear);

                if (TargetAngle > 360)
                    TargetAngle = 45;
                if (TargetAngle < 0)
                    TargetAngle = 315;
            }
        }

        public void GetMousePosition(InputAction.CallbackContext ctx) { _mousePosition = ctx.ReadValue<Vector2>(); }

        public void OnJoystickMove(InputAction.CallbackContext ctx) { _joystickInput = ctx.ReadValue<Vector2>(); }

        public void CancelAiming(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            //Cancel aiming
            Power = 0;
            _isAiming = false;
            //Turn off PowerBar and reset scale
            _powerBarRotationPivot.gameObject.SetActive(false);
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
