using UnityEngine;
using UnityEngine.InputSystem;

namespace _game.Scripts.Controls
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private PlayerControls _playerControls;

        private bool _isAiming;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            _playerControls = new PlayerControls();

            _playerControls.Player.Aiming.started += (ctx) => SetLeftButtonDown(ctx, true);
            _playerControls.Player.Aiming.canceled += (ctx) => SetLeftButtonDown(ctx, false);
        }

        private void SetLeftButtonDown(InputAction.CallbackContext ctx, bool value) { _isAiming = value; }

        public Vector2 GetMouseDelta() => _playerControls.Player.Look.ReadValue<Vector2>();

        public float GetScrollDelta() => _playerControls.Player.Scroll.ReadValue<float>();

        public float GetMouseYDelta() => _playerControls.Player.MouseDeltaY.ReadValue<float>();

        public bool IsAiming() => _isAiming;

        public bool CanceledAimingThisFrame()
        {
            if (!_playerControls.Player.CancelAiming.triggered)
                return false;

            _isAiming = false;
            return true;
        }

        private void OnEnable() { _playerControls.Enable(); }

        private void OnDisable() { _playerControls.Disable(); }
    }
}
