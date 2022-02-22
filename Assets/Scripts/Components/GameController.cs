using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SectionWords = System.Collections.Generic.Dictionary<string, (UnityEngine.Vector2Int, WordDirection)>;

namespace Components
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private BoardSpawner _boardSpawner;

        [SerializeField]
        private ScenerySpawner _scenerySpawner;

        [SerializeField]
        private TextAsset _wordListAsset;

        [SerializeField]
        private CameraRig _cameraRig;

        [SerializeField]
        private UfoLetterRing _letterRing;

        [SerializeField]
        private int _pastSectionCount, _futureSectionCount;

        [SerializeField]
        private Ufo _ufo;

        private readonly Queue<(string, SectionWords)> _generatedFutureSections = new Queue<(string, SectionWords)>();

        private WordBoard _wordBoard;
        private WordBoardGenerator _wordBoardGenerator;
        private int _currentSectionIndex;
        private SectionWords _currentSectionWords;

        private void Start()
        {
            _wordBoard = new WordBoard();
            _wordBoardGenerator = new WordBoardGenerator(_wordListAsset, _wordBoard);
            _boardSpawner.Initialize(_wordBoard);
            _scenerySpawner.Initialize(_wordBoard, _pastSectionCount, _futureSectionCount);
            _letterRing.WordSubmitted += LetterRing_WordSubmitted;

            for (var i = 0; i < _futureSectionCount + 1; i++)
            {
                GenerateAndEnqueueSection();
            }

            while (!ActivateNextSection())
            {
                _currentSectionIndex++;
                GenerateAndEnqueueSection();
            }

            _scenerySpawner.ExpandToSection(_currentSectionIndex);

            _cameraRig.SetTargetSection(_currentSectionIndex);
            _cameraRig.TeleportToTarget();

            _ufo.SetTargetSection(_currentSectionIndex);
            _ufo.TeleportToTarget();
        }

        private void OnDestroy()
        {
            _letterRing.WordSubmitted -= LetterRing_WordSubmitted;
        }

        private void LetterRing_WordSubmitted(string word)
        {
            if (_currentSectionWords.TryGetValue(word, out (Vector2Int position, WordDirection direction) boardWord))
            {
                _wordBoard.SetWord(boardWord.position, boardWord.direction, word, TileState.Revealed, false);
                _currentSectionWords.Remove(word);

                if (_currentSectionWords.Count == 0)
                {
                    _currentSectionIndex++;

                    GenerateAndEnqueueSection();
                    while (!ActivateNextSection())
                    {
                        _currentSectionIndex++;
                        GenerateAndEnqueueSection();
                    }
                    ClearTilesBelowSection(_currentSectionIndex - _pastSectionCount);

                    _scenerySpawner.ExpandToSection(_currentSectionIndex);
                    // TODO: Do this as the new poisition is reached, and also clear all the way up to current
                    _scenerySpawner.CleanupBeforeSection(_currentSectionIndex - _pastSectionCount);

                    _cameraRig.SetTargetSection(_currentSectionIndex);
                    _ufo.SetTargetSection(_currentSectionIndex);
                }
            }
        }

        private bool ActivateNextSection()
        {
            string letters;
            (letters, _currentSectionWords) = _generatedFutureSections.Dequeue();

            if (!letters.Any())
            {
                return false;
            }
            
            _letterRing.SetLetters(letters);

            foreach (var word in _currentSectionWords.Keys)
            {
                (Vector2Int position, WordDirection direction) placement = _currentSectionWords[word];
                _wordBoard.SetWord(placement.position, placement.direction, word, TileState.Hidden, false);
            }

            return true;
        }

        private void ClearTilesBelowSection(int sectionIndex)
        {
            foreach (var position in _wordBoard.AllLetterAndBlockerTilePositions.ToArray())
            {
                var minPosition = sectionIndex * WordBoardGenerator.SectionStride;
                if (position.x < minPosition || position.y < minPosition)
                {
                    _wordBoard.FullyClearTile(position);
                }
            }
        }

        private void GenerateAndEnqueueSection()
        {
            var generatedSectionWords =
                _wordBoardGenerator.GenerateSection(_currentSectionIndex + _generatedFutureSections.Count,
                    out var letters);
            letters = WordUtility.ShuffleLetters(letters);
            _generatedFutureSections.Enqueue((letters, generatedSectionWords));
        }

        [ContextMenu("Cheat: Log Words")]
        private void DebugLogWords()
        {
            foreach (var word in _currentSectionWords.Keys)
            {
                UnityEngine.Debug.Log(word);
            }
        }

        [ContextMenu("Cheat: Almost Complete Section")]
        private void DebugCompleteSection()
        {
            foreach (var word in _currentSectionWords.Keys.Skip(1).ToArray())
            {
                LetterRing_WordSubmitted(word);
            }

            DebugLogWords();
        }
    }
}