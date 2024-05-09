using System;
using _game.Scripts.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts
{
    public class PowerBar : MonoBehaviour
    {
        [SerializeField] private Image _line;
        [SerializeField] private Image _arrow;
        [SerializeField] private PlayController _playController;

        private RectTransform _arrowTransform;
        private RectTransform _lineTransform;

        private void Awake()
        {
            _arrowTransform = _arrow.GetComponent<RectTransform>();
            _lineTransform = _line.GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!_playController) return;

            SetPower(_playController.Power);
        }

        private void SetPower(float power)
        {
            _lineTransform.sizeDelta = new Vector2(0.02f, _playController.PowerDistanceMinMax.y);
            _line.fillAmount = power.RemapClamped(_playController.PowerBarMinMax.x, _playController.PowerBarMinMax.y, 0f, 1f);
            _arrowTransform.anchoredPosition = new Vector2(0, power.RemapClamped(_playController.PowerBarMinMax.x, _playController.PowerBarMinMax.y, _playController.PowerDistanceMinMax.x, _playController.PowerDistanceMinMax.y));

            float hue = Mathf.Clamp(power.Remap(_playController.PowerBarMinMax.x, _playController.PowerBarMinMax.y, 0.42f, 0f), 0f, 0.42f);
            Color newColor = Color.HSVToRGB(hue, 1, 1);
            _line.color = newColor;
            _arrow.color = newColor;
        }

        private void OnDisable() { SetPower(0f); }
    }
}
