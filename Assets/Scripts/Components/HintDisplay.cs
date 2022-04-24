using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class HintDisplay : ScaleBumpableDisplayBase
    {
        [SerializeField] private Image _hintImageToFill;
        [SerializeField] private Image _hintImageToScale;
        [SerializeField] private GameObject _lightStreaksToEnable;
        
        public void SetHintPoints(int points, bool playEffects)
        {
            _hintImageToFill.fillAmount = (float) points / GameController.HintPointsRequiredPerHint;

            _lightStreaksToEnable.SetActive(points >= GameController.HintPointsRequiredPerHint);
            
            if (playEffects)
            {
                BumpScale();
            }
        }

        protected override void Update()
        {
            base.Update();
            _hintImageToScale.transform.localScale = Vector3.one * CurrentScale;
        }
    }
}