using _game.Scripts.Building;
using DG.Tweening;
using UnityEngine;

namespace _game.Scripts.ObstacleScripts
{
    [RequireComponent(typeof(ObstacleBase))]
    public class Tornado : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _rotationDuration = 5f;
        [SerializeField] private float _strength;
        
        [Space]
        [SerializeField] private Transform _tornadoPivot;
        [SerializeField] private Transform _range;
        
        private ObstacleBase _obstacleBase;

        private void Awake()
        {
            _obstacleBase = GetComponent<ObstacleBase>();
        }

        private void FixedUpdate()
        {
            if(_obstacleBase.BuildController.Player.GameManager.GamePhase != Enums.GamePhase.Play) return;
            
            Vector3 position = _range.position;
            foreach (Rigidbody player in _obstacleBase.PlayersInRange)
            {
                Vector3 playerPosition = player.position;
                float distanceFromCenter = Vector3.Distance(position, playerPosition);
                float scaledStrength = distanceFromCenter.RemapClamped(0, _range.lossyScale.x, _strength, 0);
                player.AddForce((position - playerPosition).normalized * (scaledStrength * Time.deltaTime), ForceMode.Acceleration);
                player.AddForce(Vector3.up * (scaledStrength * Time.deltaTime), ForceMode.Force);
            }
        }

        private void StartLooping() { _tornadoPivot.DORotate(Vector3.up * 360, _rotationDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental); }

        private void EndLooping() { _tornadoPivot.DOKill(); }

        public void OnPlace()
        {
            StartLooping();
        }

        private void OnDisable() { EndLooping(); }
    }
}
