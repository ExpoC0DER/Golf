using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _game.Scripts.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextFade : MonoBehaviour
    {
        [SerializeField] private float _textFadeDuration;
        private TMP_Text _text;

        private void Awake() { _text = GetComponent<TMP_Text>(); }

        private void FadeText(Enums.GamePhase gamePhase)
        {
            if (gamePhase == Enums.GamePhase.Play)
            {
                _text.alpha = 1;
                _text.DOFade(0, _textFadeDuration).SetDelay(0.5f).SetEase(Ease.InQuad);
            }
        }


        private void OnEnable() { GameManager.OnGamePhaseChanged += FadeText; }

        private void OnDisable() { GameManager.OnGamePhaseChanged -= FadeText; }
    }
}
