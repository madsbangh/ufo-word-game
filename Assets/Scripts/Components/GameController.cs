using EasyButtons;
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
        private NpcSpawner _npcSpawner;

        [SerializeField]
        private TextAsset _wordListAsset;

        [SerializeField]
        private CameraRig _cameraRig;

        [SerializeField]
        private UfoLetterRing _letterRing;

        [SerializeField]
        private int _pastSectionCount, _futureSectionCount;

        [SerializeField]
        private UfoRig _ufoRig;

        private readonly Queue<(string, SectionWords)> _generatedFutureSections = new Queue<(string, SectionWords)>();

        private WordBoard _wordBoard;
        private WordBoardGenerator _wordBoardGenerator;
        private int _currentSectionIndex;
        private int _newestGeneratedSectionIndex;
        private SectionWords _currentSectionWords;

        private void Start()
        {
            _wordBoard = new WordBoard();
            _wordBoardGenerator = new WordBoardGenerator(_wordListAsset, _wordBoard);
            _boardSpawner.Initialize(_wordBoard);
            _scenerySpawner.Initialize(_wordBoard, 1 - WordBoardGenerator.SectionStride * _pastSectionCount);
            _letterRing.WordSubmitted += LetterRing_WordSubmitted;

            _currentSectionIndex = -1;
            _newestGeneratedSectionIndex = -1;
            ProgressToNextSection();

            _scenerySpawner.ExpandToSection(_currentSectionIndex + _futureSectionCount - 1);

            _cameraRig.SetTargetSection(_currentSectionIndex);
            _cameraRig.TeleportToTarget();

            _ufoRig.SetTargetSection(_currentSectionIndex);
            _ufoRig.SetUfoTargetBelowBoard();
            _ufoRig.TeleportToTarget();
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
                    ProgressToNextSection();

                    _scenerySpawner.ExpandToSection(_currentSectionIndex + _futureSectionCount - 1);
                    _scenerySpawner.CleanupBeforeSection(_currentSectionIndex - _pastSectionCount + 1);

                    _cameraRig.SetTargetSection(_currentSectionIndex);
                    _ufoRig.SetTargetSection(_currentSectionIndex);
                }
            }
        }

        private void ProgressToNextSection()
		{
			// Dequeue and generate sections
			string letters;
			do
			{
				_currentSectionIndex++;
				while (_newestGeneratedSectionIndex < _currentSectionIndex + _futureSectionCount)
				{
					GenerateAndEnqueueSection();
				}
				(letters, _currentSectionWords) = _generatedFutureSections.Dequeue();

			} while (!letters.Any());

			_letterRing.SetLetters(letters);

			UnlockCurrentSectionWords();

			ClearTilesBelowSection(_currentSectionIndex - _pastSectionCount);
		}

		private void UnlockCurrentSectionWords()
		{
			foreach (var word in _currentSectionWords.Keys)
			{
				(Vector2Int position, WordDirection direction) placement = _currentSectionWords[word];
				_wordBoard.SetWord(placement.position, placement.direction, word, TileState.Hidden, false);
			}
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
            _newestGeneratedSectionIndex++;

            var generatedSectionWords = _wordBoardGenerator.GenerateSection(_newestGeneratedSectionIndex, out var letters);
            letters = WordUtility.ShuffleLetters(letters);
            _generatedFutureSections.Enqueue((letters, generatedSectionWords));

            _npcSpawner.SpawnNpcsForSection(_newestGeneratedSectionIndex, _wordBoard);
        }

        [Button("Cheat: Log Words", Mode = ButtonMode.EnabledInPlayMode)]
        private void DebugLogWords()
        {
            foreach (var word in _currentSectionWords.Keys)
            {
                UnityEngine.Debug.Log(word);
            }
        }

        [Button("Cheat: Almost Complete Section", Mode = ButtonMode.EnabledInPlayMode)]
        private void DebugAlmostCompleteSection()
        {
            foreach (var word in _currentSectionWords.Keys.Skip(1).ToArray())
            {
                LetterRing_WordSubmitted(word);
            }

            DebugLogWords();
        }

        [Button("Cheat: Complete Section", Mode = ButtonMode.EnabledInPlayMode)]
        private void DebugCompleteSection()
        {
            foreach (var word in _currentSectionWords.Keys.ToArray())
            {
                LetterRing_WordSubmitted(word);
            }
        }
    }
}