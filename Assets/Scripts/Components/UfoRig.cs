using UnityEngine;

namespace Components
{
    public class UfoRig : MonoBehaviour
    {
        [SerializeField]
        private float _rigSmoothTime, _ufoSmoothTime;

        [SerializeField]
        private Transform _ufo;

        [SerializeField]
        private Transform _positionBelowBoard;

        [SerializeField]
        private Transform _positionOverBoard;

        private Vector3 _rigWorldTarget;
        private Vector3 _rigWorldVelocity;

        private Vector3 _ufoLocalTarget;
        private Vector3 _ufoLocalVelocity;

        private void Update()
        {
            transform.position = Vector3.SmoothDamp(transform.position, _rigWorldTarget, ref _rigWorldVelocity, _rigSmoothTime);
            _ufo.transform.localPosition = Vector3.SmoothDamp(_ufo.transform.localPosition, _ufoLocalTarget, ref _ufoLocalVelocity, _rigSmoothTime);
        }

        public void TeleportToTarget()
        {
            transform.position = _rigWorldTarget;
            _rigWorldVelocity = Vector3.zero;
            _ufo.transform.localPosition = _ufoLocalTarget;
            _ufoLocalVelocity = Vector3.zero;
        }

        public void SetTargetSection(int section)
        {
            var boardSpaceTargetPosition =
                Vector2Int.one *
                (WordBoardGenerator.SectionStride * section
                 + WordBoardGenerator.SectionSize / 2);

            _rigWorldTarget = new Vector3(boardSpaceTargetPosition.x, 0f, -boardSpaceTargetPosition.y);
        }

        public void SetUfoTargetOverBoard(bool ufoIsOverBoard)
        {
            _ufoLocalTarget = ufoIsOverBoard ? _positionOverBoard.localPosition :  _positionBelowBoard.localPosition;
        }
    }
}