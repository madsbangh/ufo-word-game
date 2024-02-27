using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Components
{
    public class UfoLetterRing : MonoBehaviour
    {
        public event Action<string> WordSubmitted;

        [SerializeField]
        private float _radius;

        [SerializeField]
        private UfoLetter _letterPrefab;

        [SerializeField]
        private LineRenderer _lineBetweenLetters;

        [SerializeField]
        private LineRenderer _lineBetweenActiveLetterAndPointer;

        [SerializeField]
        private TMP_Text _previewWord;

        [SerializeField]
        private AudioController _audioController;

        [SerializeField]
        private PreviewWordAnimator _previewWordAnimator;

        [SerializeField]
        private Transform _tutorialHand;

        private readonly List<UfoLetter> _letterPool = new();
        private readonly Stack<UfoLetter> _currentlyChosenLetters = new();

        private Transform _activeLetterToDrawLineFrom;

        private bool _isControlledByTutorial;

        public IEnumerable<UfoLetter> ActiveLetters => _letterPool.Where(l => l.gameObject.activeSelf);

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            var t = transform;
            Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, new Vector3(1f, 0f, 1f));
            Gizmos.DrawWireSphere(Vector3.zero, _radius);
        }

        private void Start()
        {
            UpdatePreviewWord();
        }

        private void Update()
        {
            if (_activeLetterToDrawLineFrom != null)
            {
                if (_isControlledByTutorial)
                {
                    UpdateLineFromLetterToTutorialPointer();
                }
                else
                {
                    UpdateLineFromLetterToCursor();
                }
            }
            else
            {
                _lineBetweenActiveLetterAndPointer.positionCount = 0;
            }
        }

        private void UpdateLineFromLetterToCursor()
        {
            var ray = Camera.main!.ScreenPointToRay(
                Input.touchCount > 0
                ? (Vector3)Input.GetTouch(0).position
                : Input.mousePosition);

            var t = transform;
            var plane = new Plane(t.up, t.position);

            if (plane.Raycast(ray, out var distance))
            {
                var hitLocalPosition = transform.InverseTransformPoint(ray.GetPoint(distance));
                _lineBetweenActiveLetterAndPointer.positionCount = 2;
                _lineBetweenActiveLetterAndPointer.SetPositions(new[]
                {
                    _activeLetterToDrawLineFrom.localPosition,
                    hitLocalPosition
                });
            }
        }

        private void UpdateLineFromLetterToTutorialPointer()
        {
            var localPosition = transform.InverseTransformPoint(_tutorialHand.position);
            _lineBetweenActiveLetterAndPointer.positionCount = 2;
            _lineBetweenActiveLetterAndPointer.SetPositions(new[]
            {
                _activeLetterToDrawLineFrom.localPosition,
                localPosition
            });
        }

        public bool TryStartTutorialLine(UfoLetter letter)
        {
            if (_currentlyChosenLetters.Any())
            {
                return false;
            }

            _isControlledByTutorial = true;
            _currentlyChosenLetters.Push(letter);
            _activeLetterToDrawLineFrom = letter.transform;
            letter.Selected = true;
            return true;
        }

        public bool TryAddTutorialLineSegment(UfoLetter letter)
        {
            if (!_isControlledByTutorial)
            {
                return false;
            }

            _currentlyChosenLetters.Push(letter);
            _activeLetterToDrawLineFrom = letter.transform;
            letter.Selected = true;
            UpdateLineBetweenLetters();
            return true;
        }

        public void EndTutorialLineIfAny()
        {
            if (!_isControlledByTutorial)
            {
                return;
            }

            foreach (var letter in _currentlyChosenLetters)
            {
                letter.Selected = false;
            }

            _currentlyChosenLetters.Clear();
            _activeLetterToDrawLineFrom = null;
            _isControlledByTutorial = false;
            UpdateLineBetweenLetters();
        }

        public void SetLetters(string letters)
        {
            // Spawn any missing pooled letter objects
            for (var i = _letterPool.Count; i < letters.Length; i++)
            {
                var spawnedLetter = Instantiate(_letterPrefab, transform);
                _letterPool.Add(spawnedLetter);
                spawnedLetter.Entered += UfoLetter_Entered;
                spawnedLetter.Pressed += UfoLetter_Pressed;
                spawnedLetter.Released += UfoLetter_Released;
            }

            // Setup letter objects to match the given letters
            for (var i = 0; i < letters.Length; i++)
            {
                var letterObject = _letterPool[i];
                letterObject.gameObject.SetActive(true);
                letterObject.transform.localPosition = CalculatePositionOnRing(i, letters.Length);
                letterObject.Letter = letters[i];
            }

            // Disable remaining letter objects
            for (var i = letters.Length; i < _letterPool.Count; i++)
            {
                _letterPool[i].gameObject.SetActive(false);
            }
        }

        private void UfoLetter_Pressed(UfoLetter letter, PointerEventData eventData)
        {
            EndTutorialLineIfAny();

            if (eventData.pointerId is not -1 and not 0)
            {
                return;
            }

            letter.Selected = true;
            _currentlyChosenLetters.Push(letter);
            _activeLetterToDrawLineFrom = letter.transform;
            UpdatePreviewWord();

            _audioController.AddLetter(1);
        }

        private void UfoLetter_Released(UfoLetter letter, PointerEventData eventData)
        {
            if (_isControlledByTutorial) return;

            if (eventData.pointerId is not -1 and not 0)
            {
                return;
            }

            WordSubmitted?.Invoke(_previewWord.text);

            // Deselect all letters
            while (_currentlyChosenLetters.Count > 0)
            {
                _currentlyChosenLetters.Pop().Selected = false;
            }

            // Unset active letter for line drawing
            _activeLetterToDrawLineFrom = null;

            UpdateLineBetweenLetters();
        }

        private void UfoLetter_Entered(UfoLetter letter, PointerEventData eventData)
        {
            if (_isControlledByTutorial) return;

            if (eventData.pointerId is not -1 and not 0)
            {
                return;
            }

            // Don't do anything if nothing is selected
            if (_currentlyChosenLetters.Count == 0) return;

            if (!_currentlyChosenLetters.Contains(letter))
            {
                // Select current letter if it was not in the stack
                letter.Selected = true;
                _currentlyChosenLetters.Push(letter);
                _activeLetterToDrawLineFrom = letter.transform;

                _audioController.AddLetter(_currentlyChosenLetters.Count);
            }
            else if (_currentlyChosenLetters.Skip(1).FirstOrDefault() == letter)
            {
                // Deselect top letter if current was second in the stack
                _currentlyChosenLetters.Pop().Selected = false;
                _activeLetterToDrawLineFrom = letter.transform;

                _audioController.RemoveLetter(_currentlyChosenLetters.Count);
            }

            UpdateLineBetweenLetters();
            UpdatePreviewWord();
        }

        private void UpdateLineBetweenLetters()
        {
            var positions = _currentlyChosenLetters
                .Select(l => l.transform.localPosition)
                .ToArray();

            _lineBetweenLetters.positionCount = positions.Length;
            _lineBetweenLetters.SetPositions(positions);
        }

        private void UpdatePreviewWord()
        {
            var word = _currentlyChosenLetters
                .Select(l => l.Letter)
                .Reverse()
                .ToArray()
                .ArrayToString();

            _previewWord.text = word;

            _previewWordAnimator.ResetWord();
        }

        private Vector3 CalculatePositionOnRing(int index, int count)
        {
            return Quaternion.Euler(0f, index * 360f / count, 0f) * new Vector3(0f, 0f, _radius);
        }
    }
}