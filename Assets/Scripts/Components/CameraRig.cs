using UnityEngine;

namespace Components
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private float _rigSmoothTime;
        [SerializeField] private float _cameraSmoothTime;
        [SerializeField] private Transform _overBoardTarget;
        [SerializeField] private Transform _belowBoardTarget;
        [SerializeField] private Camera _camera;

        private Vector2 _boardSpaceTargetPosition;
        private Vector2 _boardSpaceVelocity;
        private Vector3 _cameraLocalSpaceVelocity;
        private Vector3 _cameraLocalSpaceTargetPosition;
        private float _cameraRotationOverBoardFactor;
        private float _cameraRotationOverBoardFactorTarget;
        private float _cameraRotationOverBoardFactorDelta;

        private void Update()
        {
            DoMoveRig();
            DoMoveCamera();
        }

        public void SetTargetSection(int section) =>
            _boardSpaceTargetPosition =
                Vector2Int.one *
                (WordBoardGenerator.SectionStride * section
                 + WordBoardGenerator.SectionSize / 2);

        public void SetCameraOverBoard(bool cameraIsOverBoard)
        {
            _cameraLocalSpaceTargetPosition =
                cameraIsOverBoard ? _overBoardTarget.localPosition : _belowBoardTarget.localPosition;
            _cameraRotationOverBoardFactorTarget = cameraIsOverBoard ? 1f : 0f;
        }

        public void TeleportToTarget()
        {
            _boardSpaceVelocity = Vector2.zero;
            var t = transform;
            t.position = new Vector3(_boardSpaceTargetPosition.x, t.position.y, -_boardSpaceTargetPosition.y);

            _cameraLocalSpaceVelocity = Vector3.zero;
            var ct = _camera.transform;
            ct.localPosition = _cameraLocalSpaceTargetPosition;
        }

        private void DoMoveRig()
        {
            var position = transform.position;
            var boardSpacePosition = new Vector2(position.x, -position.z);
            boardSpacePosition = Vector2.SmoothDamp(boardSpacePosition, _boardSpaceTargetPosition,
                ref _boardSpaceVelocity, _rigSmoothTime);
            transform.position = new Vector3(boardSpacePosition.x, position.y, -boardSpacePosition.y);
        }

        private void DoMoveCamera()
        {
            var localPosition = Vector3.SmoothDamp(_camera.transform.localPosition,
                _cameraLocalSpaceTargetPosition, ref _cameraLocalSpaceVelocity, _rigSmoothTime);
            _camera.transform.localPosition = localPosition;

            _cameraRotationOverBoardFactor = Mathf.SmoothDamp(_cameraRotationOverBoardFactor,
                _cameraRotationOverBoardFactorTarget, ref _cameraRotationOverBoardFactorDelta, _cameraSmoothTime);
            _camera.transform.localRotation = Quaternion.Slerp(_belowBoardTarget.localRotation, 
                _overBoardTarget.localRotation, _cameraRotationOverBoardFactor);
        }
    }
}