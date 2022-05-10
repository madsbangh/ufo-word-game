using TMPro;
using UnityEngine;

namespace Components
{
    public class ScoreDisplay : ScaleBumpableDisplayBase
    {
        [SerializeField] private TMP_Text _scoreText;

        public void SetScore(int score, bool playEffects)
        {
            _scoreText.text = score.ToString();

            if (playEffects)
            {
                BumpScale();
            }
        }
        
        protected override void Update()
        {
            base.Update();
            _scoreText.transform.localScale = Vector3.one * CurrentScale;
        }
    }
}