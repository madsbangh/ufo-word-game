using TMPro;
using UnityEngine;

namespace Components
{
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private float _bumpStrength;
        [SerializeField] private float _smoothTime;

        private float _currentScale;
        private float _currentScaleVelocity;

        public void SetScore(int score, bool playEffects)
        {
            _scoreText.text = score.ToString();

            if (playEffects)
            {
                _currentScaleVelocity = _bumpStrength;
            }
        }

        private void Update()
        {
            _currentScale = Mathf.SmoothDamp(_currentScale, 1f, ref _currentScaleVelocity, _smoothTime);
            _scoreText.transform.localScale = Vector3.one * _currentScale;
        }
    }
}