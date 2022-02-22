using UnityEngine;

namespace Components
{
    public class Ufo : MonoBehaviour
    {
        [SerializeField]
        private float _smoothTime;
        [SerializeField]
        private Vector2Int _offsetWhenBelowSection;

        private Vector3 _target;
        private Vector3 _velocity;

        private void Update()
        {
            transform.position = Vector3.SmoothDamp(transform.position, _target, ref _velocity, _smoothTime);
        }

        public void TeleportToTarget()
        {
            transform.position = _target;
            _velocity = Vector3.zero;
        }

        public void SetTargetSection(int section)
        {
            var boardSpaceTargetPosition =
                Vector2Int.one *
                (WordBoardGenerator.SectionStride * section
                 + WordBoardGenerator.SectionSize / 2) +
                _offsetWhenBelowSection;

            _target = new Vector3(boardSpaceTargetPosition.x, transform.position.y, -boardSpaceTargetPosition.y);
        }
    }
}