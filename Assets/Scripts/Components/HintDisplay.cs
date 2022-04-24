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

        public Button.ButtonClickedEvent OnHintButtonClicked => _useHintButton.onClick;

        private bool _alsoScaleText;
        
        public void SetHintPoints(int points, bool playEffects)
        {
            _hintImageToFill.fillAmount = (float) points / GameController.HintPointsRequiredPerHint;

            _lightStreaksToEnable.SetActive(points >= GameController.HintPointsRequiredPerHint);
            
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