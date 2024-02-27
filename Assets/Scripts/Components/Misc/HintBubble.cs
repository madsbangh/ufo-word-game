using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Components.Misc
{
    public class HintBubble : MonoBehaviour
    {
        [SerializeField]
        private float _bobAmplitude;

        [SerializeField]
        private float _bobFrequency;

        [SerializeField]
        private float _scaleDuration;

        private Vector3 _initialLocalPosition;

        private void Start()
        {
            _initialLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            transform.localPosition = _initialLocalPosition + _bobAmplitude * Mathf.Sin(Time.time * _bobFrequency) * Vector3.up;
        }

        [ContextMenu("Show")]
        public void Show()
        {
            StopAllCoroutines();
            StartCoroutine(ScaleCoroutine(1f));
        }

        [ContextMenu("Dismiss")]
        public void Dismiss()
        {
            StopAllCoroutines();
            StartCoroutine(ScaleCoroutine(0f));
        }

        private IEnumerator ScaleCoroutine(float targetScale)
        {
            var startScale = transform.localScale.x;

            if (_scaleDuration > 0f)
            {
                for (float t = 0f; t < 1f; t += Time.deltaTime / _scaleDuration)
                {
                    var scale = Mathf.SmoothStep(startScale, targetScale, t);
                    transform.localScale = Vector3.one * scale;
                    yield return null;
                }
            }

            transform.localScale = Vector3.one * targetScale;
        }
    }
}