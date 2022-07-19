using System;
using System.Collections;
using EasyButtons;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Components
{
    public class FlyingWordEffect : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform _startTransform;
        [SerializeField] private AudioController _audioController;

        [Header("Self")]
        [SerializeField] private Transform _textTransform;
        [SerializeField] private Transform _circleTransform;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private TMP_Text _text;
        
        [Header("Settings")]
        [SerializeField] private AnimationCurve _toBoardTranslationXCurve;
        [SerializeField] private AnimationCurve _toBoardTranslationYZCurve;
        [SerializeField] private AnimationCurve _toHintTranslationXCurve;
        [SerializeField] private AnimationCurve _toHintTranslationYZCurve;
        [SerializeField] private AnimationCurve _textScaleCurve;
        [SerializeField] private AnimationCurve _circleScaleCurve;
        [SerializeField] private float _toBoardDuration;
        [SerializeField] private float _toHintDuration;

        private Coroutine _currentCoroutine;

        private void Start()
        {
            _textTransform.gameObject.SetActive(false);
            _circleTransform.gameObject.SetActive(false);
        }

        public void PlayMoveToTransformEffect(Vector3 target, string word, bool isFlyingToHint)
        {
            _text.text = word;
            
            if (_currentCoroutine != null)
            {
                _particleSystem.Clear();
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            _currentCoroutine = StartCoroutine(PlayMoveToTransformEffectCoroutine(target, isFlyingToHint));
        }

        private IEnumerator PlayMoveToTransformEffectCoroutine(Vector3 target, bool isFlyingToHint)
        {
            _textTransform.gameObject.SetActive(true);
            _circleTransform.gameObject.SetActive(true);

            var t = 0f;
            var deltaT = 1f / (isFlyingToHint ? _toHintDuration : _toBoardDuration);

            while (t <= 1f)
            {
                t += deltaT * Time.deltaTime;
                
                var tX = (isFlyingToHint ? _toHintTranslationXCurve : _toBoardTranslationXCurve).Evaluate(t);
                var tYZ = (isFlyingToHint ? _toHintTranslationYZCurve : _toBoardTranslationYZCurve).Evaluate(t);
                var startPosition = _startTransform.position;
                transform.position =
                    new Vector3(
                        Mathf.LerpUnclamped(
                            startPosition.x,
                            target.x,
                            tX),
                        Mathf.LerpUnclamped(
                            startPosition.y,
                            target.y,
                            tYZ),
                        Mathf.LerpUnclamped(
                            startPosition.z,
                            target.z,
                            tYZ));

                var textScale = _textScaleCurve.Evaluate(t);
                _textTransform.localScale = new Vector3(textScale, textScale, textScale);
                
                var circleScale = _circleScaleCurve.Evaluate(t);
                _circleTransform.localScale = new Vector3(circleScale, circleScale, circleScale);

                yield return null;
            }

            _textTransform.gameObject.SetActive(false);
            _circleTransform.gameObject.SetActive(false);
            
            _particleSystem.Play();
            _audioController.LetterImpact();
        }
    }
}