using UnityEngine;

namespace Components
{
    public class ScaleBumpableDisplayBase : MonoBehaviour
    {
        [SerializeField] protected float _bumpStrength;
        [SerializeField] private float _smoothTime;
        
        protected float CurrentScale = 1f;
        
        private float _currentScaleVelocity;

        protected virtual void Update()
        {
            CurrentScale = Mathf.SmoothDamp(CurrentScale, 1f, ref _currentScaleVelocity, _smoothTime);
        }

        protected void BumpScale()
        {
            _currentScaleVelocity = _bumpStrength;
        }
    }
}