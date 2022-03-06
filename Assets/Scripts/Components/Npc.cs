using System.Collections;
using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Animator))]
    public class Npc : MonoBehaviour
    {
        private const int WalkPositionsCount = 4;

        private static readonly int SpeedParameterId = Animator.StringToHash("Speed");
        private static readonly int HoistTriggerId = Animator.StringToHash("Hoist");
        private static readonly int CycleOffsetParameterId = Animator.StringToHash("Cycle Offset");

        [SerializeField] private float _hoistTime;

        private Animator _animator;
        private Vector2 _walkAreaCenter;

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

            _walkAreaCenter = transform.position.ToBoardPosition();

            while (true)
            {
                var randomTravelVector = new Vector2(Random.value - 0.5f, Random.value - 0.5f);
                var walkPosition = _walkAreaCenter + randomTravelVector;
                yield return StartCoroutine(WalkToPosition(walkPosition, Random.Range(0.25f, 1f)));

                yield return new WaitForSeconds(Random.Range(1f, 5f));
            }
        }

        public void Hoist(Transform tractorBeamOrigin)
        {
            StopAllCoroutines();
            StartCoroutine(HoistCoroutine(tractorBeamOrigin));
        }

        private IEnumerator WalkToPosition(Vector2 boardSpacePosition, float speed)
        {
            _animator.SetFloat(SpeedParameterId, speed);
            var travelVector = boardSpacePosition - transform.position.ToBoardPosition();
            var distance = travelVector.magnitude;
            var velocity = travelVector.normalized * speed;
            var duration = distance / speed;
            var endTime = Time.time + duration;
            while (Time.time < endTime)
            {
                var deltaTime = Time.deltaTime;
                transform.Translate(velocity.x * deltaTime, 0f, -velocity.y * deltaTime);
                yield return null;
            }

            _animator.SetFloat(SpeedParameterId, 0f);
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