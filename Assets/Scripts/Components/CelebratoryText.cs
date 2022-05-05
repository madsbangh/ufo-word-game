using System;
using System.Collections;
using EasyButtons;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Components
{
    public class CelebratoryText : MonoBehaviour
    {
        [SerializeField] private string[] _strings;
        [SerializeField] private AnimationCurve _animationScaleCurve;
        [SerializeField] private float _animationDuration;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private ParticleSystem _particles;
        
        private Coroutine _currentAnimateCoroutine;

        private void Start()
        {
            _text.enabled = false;
        }

        [Button("Test", Mode = ButtonMode.EnabledInPlayMode)]
        public void Celebrate()
        {
            _text.text = _strings[Random.Range(0, _strings.Length)];

            if (_currentAnimateCoroutine != null)
            {
                StopCoroutine(_currentAnimateCoroutine);
            }
            
            _particles.Play();
            _currentAnimateCoroutine = StartCoroutine(AnimateCoroutine());
        }

        private IEnumerator AnimateCoroutine()
        {
            _text.enabled = true;

            var t = 0f;
            while (t <= _animationDuration)
            {
                t += Time.deltaTime;
                _text.transform.localScale = Vector3.one * _animationScaleCurve.Evaluate(t / _animationDuration);
                yield return null;
            }
            
            _text.enabled = false;
        }
    }
}
