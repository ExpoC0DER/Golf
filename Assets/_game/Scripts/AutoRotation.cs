using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

namespace _game.Scripts
{
    public class AutoRotation : MonoBehaviour
    {
        [Flags] private enum Axis
        {
            None = 0,
            X = 1,
            Y = 2,
            Z = 4
        }

        private Vector3 _startingRotation;

        [SerializeField, EnumFlags] private Axis _rotationAxis;
        [SerializeField] private float _speed;

        private void Awake() { _startingRotation = transform.rotation.eulerAngles; }

        private void Update()
        {
            if ((_rotationAxis & Axis.X) == Axis.X)
            {
                transform.Rotate(Vector3.forward, _speed * Time.deltaTime, Space.Self);
            }
            if ((_rotationAxis & Axis.Y) == Axis.Y)
            {
                transform.Rotate(Vector3.up, _speed * Time.deltaTime, Space.Self);
            }
            if ((_rotationAxis & Axis.Z) == Axis.Z)
            {
                transform.Rotate(Vector3.right, _speed * Time.deltaTime, Space.Self);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                transform.rotation = Quaternion.Euler(_startingRotation);
        }
    }
}
