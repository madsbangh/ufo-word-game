using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Animator))]
    public class PreviewWordAnimator : MonoBehaviour
    {
        private static readonly int ResetHash = Animator.StringToHash("Reset");
        private static readonly int ShakeHash = Animator.StringToHash("Shake");
        private static readonly int FadeHash = Animator.StringToHash("Fade");
        private static readonly int HideHash = Animator.StringToHash("Hide");
        
        private Animator _animator;

        private void Awake() => _animator = GetComponent<Animator>();
        private void Start() => ResetWord();
        
        public void ResetWord() => _animator.SetTrigger(ResetHash);
        public void ShakeWord() => _animator.SetTrigger(ShakeHash);
        public void FadeWord() => _animator.SetTrigger(FadeHash);
        public void HideWord() => _animator.SetTrigger(HideHash);
    }
}