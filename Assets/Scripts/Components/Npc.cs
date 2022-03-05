using System.Collections;
using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Animator))]
    public class Npc : MonoBehaviour
    {
        private static readonly int SpeedParameterId = Animator.StringToHash("Speed");
        private static readonly int HoistTriggerId = Animator.StringToHash("Hoist");
        private static readonly int CycleOffsetParameterId = Animator.StringToHash("Cycle Offset");

        [SerializeField] private float _hoistTime;
        
        private Animator _animator;

        private float Speed
        {
            set => _animator.SetFloat(SpeedParameterId, value);
        }
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private IEnumerator Start()
        {
            _animator.SetFloat(CycleOffsetParameterId, Random.value);
            yield break;
        }

        public void Hoist(Transform tractorBeamOrigin)
        {
            StopAllCoroutines();
            StartCoroutine(HoistCoroutine(tractorBeamOrigin));
        }

        private IEnumerator HoistCoroutine(Transform tractorBeamOrigin)
        {
            yield return new WaitForSeconds(Random.value);
            
            _animator.SetTrigger(HoistTriggerId);

            var npcTransform = transform;
            var initialScale = npcTransform.localScale;
            var initialPosition = npcTransform.position;
            
            var secondsRemaining = _hoistTime;
            while (secondsRemaining > 0f)
            {
                var deltaTime = Time.deltaTime;

                var timeRemainingNormalized = secondsRemaining / _hoistTime;
                npcTransform.localScale = initialScale * Mathf.Sqrt(timeRemainingNormalized);
                var t = Mathf.SmoothStep(1f, 0f, timeRemainingNormalized);
                npcTransform.position = Vector3.Lerp(initialPosition, tractorBeamOrigin.position, t);
                
                secondsRemaining -= deltaTime;
                
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}