using System;
using Cinemachine;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static _game.Scripts.Enums;

namespace _game.Scripts.Controls
{
    public class PlayController : MonoBehaviour
    {
        public float TargetAngle { get; set; } = 45f;
        
        [SerializeField] private Transform _powerBar;
        [SerializeField] private Material _powerBarMat;
        [SerializeField] private Transform _powerBarRotationPivot;
        [SerializeField] private Transform _highlightRing;
        [SerializeField] private LineRenderer _line;

        [Header("Settings")]
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _zoomSpeedScroll;
        [SerializeField] private float _cameraRotationSpeed;
        [SerializeField, MinMaxSlider(0.1f, 10)]
        private Vector2 _zoomMinMax;
        [SerializeField] private Vector2 _cameraSensitivity = new Vector2(300, 300);
        [SerializeField, MinMaxSlider(0, 10)]
        private Vector2 _powerDistanceMinMax;
        [SerializeField, MinMaxSlider(0, 10)]
        private Vector2 _powerBarMinMax;

        private Player _player;
        private CinemachineFramingTransposer _framingTransposer;
        private Rigidbody _rb;
        private float _zoomInput, _power;
        private Vector2 _mousePosition = new Vector2(960, 540);
        private Vector2 _joystickInput;
        private bool _isAiming, _shotBall, _tookTurn, _isInTheHole;
        private Camera _mainCamera;

        public static event Action<int> OnPlayerFinished;

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
            inputPower = Mathf.Clamp(inputPower, _powerBarMinMax.x, _powerBarMinMax.y);
            // Normalize inputPower within the range [0, 1]
            float t = Mathf.InverseLerp(_powerBarMinMax.x, _powerBarMinMax.y, inputPower);
            // Use Mathf.Lerp to interpolate on the InSine curve
            float interpolatedValue = Mathf.Lerp(0, _power, t);

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
                _player.GameManager.SwitchPlayer(Iterate.Next);
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
                _rb.isKinematic = true;
                OnPlayerFinished?.Invoke(_player.PlayerID);
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
                SetLineLength(transform.position, mouseWorldPos);

                _powerBarRotationPivot.gameObject.SetActive(true);
                _power = Vector3.Distance(transform.position, mouseWorldPos).RemapClamped(_powerDistanceMinMax.x, _powerDistanceMinMax.y, _powerBarMinMax.x, _powerBarMinMax.y);
                SetPowerBarScale(_power);
            }
            else
            {
                //If PowerBar is active (play was aiming) add force to ball in direction of pivot times inputPower multiplied by fraction of PowerBar
                if (_power > 0.01f)
                {
                    _rb.AddForce(_powerBarRotationPivot.forward * _power, ForceMode.Impulse);
                    _shotBall = true;
                    _player.ShotsTaken++;
                    _player.ShotsTakenTotal++;
                    gameObject.layer = (int)Layer.Player;
                }

                //Turn off PowerBar rotation and reset scale
                _powerBarRotationPivot.gameObject.SetActive(false);
                _power = 0f;
                SetPowerBarScale(0);
            }
        }

        private void MoveCursor()
        {
            if (!_player.PlayerCursor) return;

            _player.PlayerCursor.anchoredPosition = _mousePosition;
            _mousePosition.x = Mathf.Clamp(_mousePosition.x + _joystickInput.x * _cameraSensitivity.x * Time.deltaTime, 0, Screen.width);
            _mousePosition.y = Mathf.Clamp(_mousePosition.y + _joystickInput.y * _cameraSensitivity.y * Time.deltaTime, 0, Screen.height);
        }

        /// <summary>
        /// Sets start and end world position for aiming line
        /// </summary>
        /// <param name="startPos">Start world position</param>
        /// <param name="endPos">End world position</param>
        private void SetLineLength(Vector3 startPos, Vector3 endPos)
        {
            Vector3 dir = endPos - startPos;
            float dist = Mathf.Clamp(Vector3.Distance(startPos, endPos), _powerDistanceMinMax.x, _powerDistanceMinMax.y);
            endPos = startPos + (dir.normalized * dist);

            _line.SetPosition(0, startPos);
            _line.SetPosition(1, endPos);
        }

        private void HandleZooming()
        {
            //Handle zooming camera
            _framingTransposer.m_CameraDistance += _zoomInput * _zoomSpeed * Time.deltaTime;
            _framingTransposer.m_CameraDistance = Mathf.Clamp(_framingTransposer.m_CameraDistance, _zoomMinMax.x, _zoomMinMax.y);
        }

        /// <summary>
        /// Changes PowerBar length(Z scale) by amount
        /// </summary>
        /// <param name="power">Amount to change length by</param>
        private void SetPowerBarScale(float power)
        {
            power = Mathf.Clamp(power, _powerBarMinMax.x, _powerBarMinMax.y);

            Vector3 powerBarLocalScale = _powerBar.localScale;
            _powerBar.localScale = powerBarLocalScale;

            float hue = Mathf.Clamp(power.Remap(_powerBarMinMax.x, _powerBarMinMax.y, 0.42f, 0f), 0, 0.42f);
            Color newColor = Color.HSVToRGB(hue, 1, 1);
            _line.startColor = _line.endColor = newColor;
            _powerBarMat.SetColor("_BaseColor", newColor);
            _powerBarMat.SetColor("_EmissionColor", newColor);
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
            if (_player.Map.GetRoundStartLocation(round, out Transform startLocation))
                _rb.position = startLocation.position;
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
            _isAiming = false;
            //Turn off PowerBar and reset scale
            _powerBarRotationPivot.gameObject.SetActive(false);
            SetPowerBarScale(-_powerBarMinMax.y);
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
