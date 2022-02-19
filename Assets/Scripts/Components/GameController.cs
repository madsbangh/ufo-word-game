using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SectionWords = System.Collections.Generic.Dictionary<string, (UnityEngine.Vector2Int, WordDirection)>;

namespace Components
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private WorldSpawner _worldSpawner;

        [SerializeField]
        private TextAsset _wordListAsset;

        [SerializeField]
        private CameraRig _cameraRig;

        [SerializeField]
        private UfoLetterRing _letterRing;
        
        [SerializeField]
        private int _futureGeneratedSectionCount;
        
        [SerializeField]
        private int _pastSectionCount;

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
            _worldSpawner.Initialize(_wordBoard);
            _letterRing.WordSubmitted += LetterRing_WordSubmitted;

            for (var i = 0; i < _futureGeneratedSectionCount + 1; i++)
            {
                GenerateAndEnqueueSection();
            }

            ActivateNextSection();
            
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
                    ActivateNextSection();
                    ClearTilesBelowSection(_currentSectionIndex - _pastSectionCount);
                    
                    _cameraRig.SetTargetSection(_currentSectionIndex);
                    _ufo.SetTargetSection(_currentSectionIndex);
                }
            }
        }

        private void ActivateNextSection()
        {
            string letters;
            (letters, _currentSectionWords) = _generatedFutureSections.Dequeue();
            _letterRing.SetLetters(letters);

            foreach (var word in _currentSectionWords.Keys)
            {
                (Vector2Int position, WordDirection direction) placement = _currentSectionWords[word]; 
                _wordBoard.SetWord(placement.position, placement.direction, word, TileState.Hidden, alsoSetBlockerTiles: false);
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
        
        [ContextMenu("Clear Board")]
        private void DebugClearBoard()
        {
            foreach (var position in _wordBoard.AllLetterAndBlockerTilePositions.ToArray())
            {
                _wordBoard.FullyClearTile(position);
            }
        }
    }
}