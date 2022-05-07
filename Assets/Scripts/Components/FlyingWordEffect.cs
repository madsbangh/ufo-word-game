using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Components
{
    public class FlyingWordEffect : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform _startTransform;

        [Header("Self")]
        [SerializeField] private Transform _textTransform;
        [SerializeField] private Transform _circleTransform;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private TMP_Text _text;
        
        [Header("Settings")]
        [SerializeField] private AnimationCurve _translationXCurve;
        [SerializeField] private AnimationCurve _translationYZCurve;
        [SerializeField] private AnimationCurve _textScaleCurve;
        [SerializeField] private AnimationCurve _circleScaleCurve;
        [SerializeField] private float _duration;

        private Coroutine _currentCoroutine;

        private void Start()
        {
            _textTransform.gameObject.SetActive(false);
            _circleTransform.gameObject.SetActive(false);
        }

        public Coroutine PlayMoveToTransformEffect(Vector3 target, string word)
        {
            _text.text = word;
            
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            _currentCoroutine = StartCoroutine(PlayMoveToTransformEffectCoroutine(target));

            return _currentCoroutine;
        }

        private IEnumerator PlayMoveToTransformEffectCoroutine(Vector3 target)
        {
            _textTransform.gameObject.SetActive(true);
            _circleTransform.gameObject.SetActive(true);

            var t = 0f;
            var deltaT = 1f / _duration;

            while (t <= 1f)
            {
                t += deltaT * Time.deltaTime;
                
                var tX = _translationXCurve.Evaluate(t);
                var tYZ = _translationYZCurve.Evaluate(t);
                var startPosition = _startTransform.position;
                transform.position =
                    new Vector3(
                        Mathf.Lerp(
                            startPosition.x,
                            target.x,
                            tX),
                        Mathf.Lerp(
                            startPosition.y,
                            target.y,
                            tYZ),
                        Mathf.Lerp(
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
        }
    }
}