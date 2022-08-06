using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class HintDisplay : ScaleBumpableDisplayBase
    {
        [SerializeField] private Image _hintImageToFill;
        [SerializeField] private Image _hintImageToScale;
        [SerializeField] private GameObject _lightStreaksToEnable;
        [SerializeField] private Button _useHintButton;
        [SerializeField] private TMP_Text _hintCountText;
        [SerializeField] private Color _backgroundColor;
        [SerializeField] private Color _foregroundColor1;
        [SerializeField] private Color _foregroundColor2;
        
        public Button.ButtonClickedEvent OnHintButtonClicked => _useHintButton.onClick;

        private bool _alsoScaleText;
        
        public void SetHintPoints(int points, bool playEffects)
        {
            _hintImageToFill.fillAmount = (float) points / GameController.HintPointsRequiredPerHint % 1f;

            _lightStreaksToEnable.SetActive(points >= GameController.HintPointsRequiredPerHint);
            _hintImageToScale.color = points >= GameController.HintPointsRequiredPerHint
                ? _foregroundColor1
                : _backgroundColor;
            _hintImageToFill.color = points >= GameController.HintPointsRequiredPerHint
                ? _foregroundColor2
                : _foregroundColor1;
            
            if (playEffects)
            {
                BumpScale();
            }

            _hintCountText.text = points >= GameController.HintPointsRequiredPerHint * 2
                ? $"{points / GameController.HintPointsRequiredPerHint}x"
                : string.Empty;

            _alsoScaleText = points % GameController.HintPointsRequiredPerHint == 0;
        }

        protected override void Update()
        {
            base.Update();
            var scale = Vector3.one * CurrentScale;
            _hintImageToScale.transform.localScale = scale;

            if (_alsoScaleText)
            {
                _hintCountText.transform.localScale = scale;
            }
            else
            {
                _hintCountText.transform.localScale = Vector3.one;
            }
        }
    }
}