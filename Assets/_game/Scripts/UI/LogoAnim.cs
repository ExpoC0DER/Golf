using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UI
{
    public class LogoAnim : MonoBehaviour
    {
        [SerializeField] private float _colorSpeed;
        [SerializeField] private float _scaleSpeed;
        [SerializeField] private float _maxScale;
        [SerializeField] private Transform _colorTransform;
        private Image _logo;
        private float _hue;
        private ColorPicker[] _colors;
        private float _t;

        private void Awake() { _logo = GetComponent<Image>(); }
        private void Start() { _colors = _colorTransform.GetComponentsInChildren<ColorPicker>(); }

        private void Update()
        {
            _t += Time.deltaTime * _colorSpeed;
            _t %= 1;
            float scaledTime = _t * _colors.Length - 1;
            Color oldColor = _colors[(int)scaledTime].Color;
            Color newColor = (int)scaledTime + 1 < _colors.Length ? _colors[(int)(scaledTime + 1f)].Color : _colors[0].Color;
            float newT = scaledTime - Mathf.Floor(scaledTime);

            _logo.color = Color.Lerp(oldColor, newColor, newT);
            _logo.transform.localScale = Vector3.one * (Mathf.PingPong(Time.time * _scaleSpeed, _maxScale - 1) + 1);
        }
    }
}
