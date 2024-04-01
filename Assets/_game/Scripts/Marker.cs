using System;
using Cinemachine;
using UnityEngine;

namespace _game.Scripts
{
    public class Marker : MonoBehaviour
    {
        public Color Color { set { _sprite.color = value; } }
        
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Transform _followPoint;
        [SerializeField] private CinemachineVirtualCamera _buildCam;
        [SerializeField] private GameObject _mark;
        [SerializeField] private float _scaleFactor;
        [SerializeField] private float _showDistanceThreshold;

        private CinemachineFramingTransposer _framingTransposer;
        private Transform _camera;
        private bool _showMark;

        private void Start()
        {
            _camera = Camera.main.transform;
            _framingTransposer = _buildCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

        private void Update()
        {
            if (!_showMark || _framingTransposer.m_CameraDistance < _showDistanceThreshold)
            {
                _mark.SetActive(false);
                return;
            }

            _mark.SetActive(true);
            transform.position = _followPoint.position;

            Vector3 lookTargetPos = _camera.position;
            lookTargetPos.y = transform.position.y;
            transform.LookAt(lookTargetPos, transform.up);
            transform.localScale = Vector3.one * (_scaleFactor * _framingTransposer.m_CameraDistance);
        }

        private void OnGamePhaseChanged(Enums.GamePhase gamePhase) { _showMark = gamePhase == Enums.GamePhase.Build; }

        private void OnEnable() { GameManager.OnGamePhaseChanged += OnGamePhaseChanged; }

        private void OnDisable() { GameManager.OnGamePhaseChanged -= OnGamePhaseChanged; }
    }
}
