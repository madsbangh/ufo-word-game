using UnityEngine;

namespace Components
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private float _smoothTime;
        [SerializeField] private Vector2Int _offset;

        private Vector2 _boardSpaceTargetPosition;
        private Vector2 _boardSpaceVelocity;

        private void Update()
        {
            var position = transform.position;
            var boardSpacePosition = new Vector2(position.x, -position.z);
            boardSpacePosition = Vector2.SmoothDamp(boardSpacePosition, _boardSpaceTargetPosition,
                ref _boardSpaceVelocity, _smoothTime);
            transform.position = new Vector3(boardSpacePosition.x, position.y, -boardSpacePosition.y);
        }

        public void SetTargetSection(int section) =>
            _boardSpaceTargetPosition =
                Vector2Int.one *
                (WordBoardGenerator.SectionStride * section
                 + WordBoardGenerator.SectionSize / 2) +
                _offset;

        public void TeleportToTarget()
        {
            _boardSpaceVelocity = Vector2.zero;
            var t = transform;
            t.position = new Vector3(_boardSpaceTargetPosition.x, t.position.y, -_boardSpaceTargetPosition.y);
        }
    }
}