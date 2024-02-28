using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Misc;
using SaveGame;
using UnityEngine;
using Random = UnityEngine.Random;
using SectionWords = System.Collections.Generic.Dictionary<string, WordPlacement>;

namespace Components
{
    public partial class GameController : MonoBehaviour
    {
        public const int HintPointsRequiredPerHint = 3;

        [SerializeField] private TextAsset _commonWordListAsset;
        [SerializeField] private TextAsset _bigWordListAsset;
        [SerializeField] private BoardSpawner _boardSpawner;
        [SerializeField] private ScenerySpawner _scenerySpawner;
        [SerializeField] private NpcSpawner _npcSpawner;
        [SerializeField] private CameraRig _cameraRig;
        [SerializeField] private UfoRig _ufoRig;
        [SerializeField] private UFOAnimator _ufoAnimator;
        [SerializeField] private UfoLetterRing _letterRing;
        [SerializeField] private ScoreDisplay _scoreDisplay;
        [SerializeField] private HintDisplay _hintDisplay;
        [SerializeField] private int _recentlyFoundWordBufferLength;
        [SerializeField] private CelebratoryText _celebratoryText;
        [SerializeField] private FlyingWordEffect _flyingWordEffect;
        [SerializeField] private Transform _hintFlyingWordTarget;
        [SerializeField] private AudioController _audioController;
        [SerializeField] private PreviewWordAnimator _previewWordAnimator;
        [SerializeField] private SpellWordTutorial _spellWordTutorial;
        [SerializeField] private HintBubble _useAHintHint;
        [SerializeField] private float _showUseAHintHintDelay;

        private WordBoard _wordBoard;
        private WordBoardGenerator _wordBoardGenerator;
        private GameState _gameState;
        private HashSet<string> _allAllowedWords;
        private float _showUseAHintHintTimer;
        private bool _useAHintHintShown;

        private void Start()
        {
            var allWords = WordUtility.ParseFilterAndProcessWordList(_bigWordListAsset.text);
            _allAllowedWords = new HashSet<string>(allWords);

            var commonWords = WordUtility.ParseFilterAndProcessWordList(_commonWordListAsset.text);
            _wordBoard = new WordBoard();
            _wordBoardGenerator = new WordBoardGenerator(commonWords, _wordBoard);

            if (SaveGameUtility.SaveFileExists)
            {
                StartGameFromSaveFile();
            }
            else
            {
                StartGameFromScratch();
            }

            SetupSceneObjects();

            GameEvents.NPCHoisted += GameEvents_NPCHoisted;
            _letterRing.WordSubmitted += LetterRing_WordSubmitted;
        }

        private void OnDestroy()
        {
            _letterRing.WordSubmitted -= LetterRing_WordSubmitted;
        }

        private void OnEnable()
        {
            _hintDisplay.OnHintButtonClicked.AddListener(HintDisplay_OnHintButtonClicked);
        }

        private void OnDisable()
        {
            _hintDisplay.OnHintButtonClicked.RemoveListener(HintDisplay_OnHintButtonClicked);
        }

        private void Update()
        {
            if (!_useAHintHintShown &&
                !_gameState.FirstEverHintUsed &&
                _gameState.BonusHintPoints >= HintPointsRequiredPerHint)
            {
                _showUseAHintHintTimer += Time.deltaTime;
                if (_showUseAHintHintTimer >= _showUseAHintHintDelay)
                {
                    _useAHintHint.Show();
                    _useAHintHintShown = true;
                }
            }
        }

        private void HintDisplay_OnHintButtonClicked()
        {
            if (_gameState.BonusHintPoints >= HintPointsRequiredPerHint)
            {
                UseHint();
            }
        }

        private void UseHint()
        {
            bool dismissedHint;

            if (!_gameState.FirstEverHintUsed)
            {
                _useAHintHint.Dismiss();
                _gameState.FirstEverHintUsed = true;
                dismissedHint = true;
            }
            else
            {
                dismissedHint = false;
            }

            var tileToReveal = GetRandomHiddenTile();
            if (tileToReveal.HasValue)
            {
                _gameState.BonusHintPoints -= HintPointsRequiredPerHint;
                _hintDisplay.SetHintPoints(_gameState.BonusHintPoints, true, false);
                _wordBoard.RevealTile(tileToReveal.Value);

                var wordsFullyRevealedByHint = _gameState.CurrentSectionWords
                    .Where(word => WordContainsTile(word, tileToReveal.Value))
                    .Where(WordIsFullyRevealed);

                foreach ((string word, var placement) in wordsFullyRevealedByHint.ToArray())
                {
                    PlaceWordAndCompleteSectionIfNeeded(word, placement);
                }
            }

            if (dismissedHint || tileToReveal.HasValue)
            {
                SaveGame();
            }

            _audioController.UseHint();
        }

        private static bool WordContainsTile(
            KeyValuePair<string, WordPlacement> wordPlacementPair,
            Vector2Int tilePosition)
        {
            for (var i = 0; i < wordPlacementPair.Key.Length; i++)
            {
                if (tilePosition ==
                    wordPlacementPair.Value.Position +
                    wordPlacementPair.Value.Direction.ToStride() * i)
                {
                    return true;
                }
            }

            return false;
        }

        private bool WordIsFullyRevealed(KeyValuePair<string, WordPlacement> wordPlacementPair)
        {
            for (var i = 0; i < wordPlacementPair.Key.Length; i++)
            {
                var position =
                    wordPlacementPair.Value.Position +
                    wordPlacementPair.Value.Direction.ToStride() * i;

                if (!_wordBoard.HasLetterTile(position))
                {
                    throw new ArgumentOutOfRangeException(nameof(wordPlacementPair),
                        "The given word falls outside the existing board tiles.");
                }

                if (_wordBoard.GetLetterTile(position).Progress != TileState.Revealed)
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2Int? GetRandomHiddenTile()
        {
            foreach (var wordPlacementPair in _gameState.CurrentSectionWords)
            {
                var wordLength = wordPlacementPair.Key.Length;
                var randomStartIndex = Random.Range(0, wordLength - 1);
                for (var i = 0; i < wordLength; i++)
                {
                    var candidatePosition =
                        wordPlacementPair.Value.Position +
                        wordPlacementPair.Value.Direction.ToStride() *
                        ((i + randomStartIndex) % wordLength);

                    if (_wordBoard.GetLetterTile(candidatePosition).Progress == TileState.Hidden)
                    {
                        return candidatePosition;
                    }
                }
            }

            return null;
        }

        private void StartGameFromScratch()
        {
            _hintDisplay.SetHintPoints(0, false, false);
            _scoreDisplay.SetScore(0, false);
            _gameState.GeneratedFutureSections = new Queue<Section>();
            _gameState.CurrentSectionWords = new SectionWords();
            _gameState.RecentlyFoundWords = new Queue<string>();
            _gameState.CurrentSectionIndex = -1;
            _gameState.NewestGeneratedSectionIndex = -1;
            ProgressToNextSection();
        }

        private void StartGameFromSaveFile()
        {
            LoadGame();
            _scoreDisplay.SetScore(_gameState.Score, false);
            _hintDisplay.SetHintPoints(_gameState.BonusHintPoints, false, false);
            _letterRing.SetLetters(_gameState.CurrentSectionLetters);
            for (int i = _gameState.CurrentSectionIndex; i <= _gameState.NewestGeneratedSectionIndex; i++)
            {
                _npcSpawner.SpawnNpcsForSection(i, _wordBoard);
            }

            // If we loaded into a completed board
            if (_gameState.CurrentSectionWords.Count == 0)
            {
                // Immediately progress to next section
                ProgressToNextSection();
                _ufoRig.TeleportToTarget();
                _cameraRig.TeleportToTarget();
            }
        }

        private void SetupSceneObjects()
        {
            _boardSpawner.Initialize(_wordBoard);
            _scenerySpawner.Initialize(_wordBoard, CalculateScenerySpawnerWindowPadding());

            _scenerySpawner.SetSection(_gameState.CurrentSectionIndex);

            _cameraRig.SetTargetSection(_gameState.CurrentSectionIndex);
            _cameraRig.SetCameraOverBoard(false);
            _cameraRig.TeleportToTarget();

            _ufoRig.SetTargetSection(_gameState.CurrentSectionIndex);
            _ufoRig.SetUfoTargetOverBoard(false);
            _ufoRig.TeleportToTarget();
        }

        private static int CalculateScenerySpawnerWindowPadding()
        {
            return WordBoardGenerator.SectionStride * (WordBoardGenerator.SectionsAheadAndBehind - 1);
        }

        private void GameEvents_NPCHoisted()
        {
            _gameState.Score++;
            _scoreDisplay.SetScore(_gameState.Score, true);
            _audioController.Score();
        }

        private void LetterRing_WordSubmitted(string word)
        {
            _showUseAHintHintTimer = 0f;
            if (_gameState.CurrentSectionWords.TryGetValue(word, out var boardWordPlacement))
            {
                _gameState.BonusHintPoints++;
                _hintDisplay.SetHintPoints(1, true, true);
                PlaceWordAndCompleteSectionIfNeeded(word, boardWordPlacement);
                _previewWordAnimator.HideWord();
                SaveGame();
            }
            else if (_allAllowedWords.Contains(word))
            {
                if (!_gameState.RecentlyFoundWords.Contains(word))
                {
                    _gameState.BonusHintPoints += 2;
                    _ufoAnimator.PlayFoundBonusWord();
                    _flyingWordEffect.PlayMoveToTransformEffect(GetHintIndicatorWorldSpacePosition(), word, true, () =>
                    {
                        _hintDisplay.SetHintPoints(2, true, true);
                    });
                    _previewWordAnimator.HideWord();
                    MarkWordAsRecentlyFound(word);
                    SaveGame();
                }
                else
                {
                    _ufoAnimator.PlayAlreadyFoundWord();
                    _previewWordAnimator.FadeWord();
                }
            }
            else if (word.Length > 1)
            {
                _ufoAnimator.PlaySad();
                _previewWordAnimator.ShakeWord();
            }
            else
            {
                _previewWordAnimator.HideWord();
            }
        }

        private Vector3 GetHintIndicatorWorldSpacePosition()
        {
            var transformPosition = _hintFlyingWordTarget.position;
            var ray = Camera.main!.ScreenPointToRay(transformPosition);
            var plane = new Plane(Vector3.up, 1f);
            plane.Raycast(ray, out var d);
            return ray.GetPoint(d);
        }

        private void PlaceWordAndCompleteSectionIfNeeded(string word, WordPlacement boardWordPlacement)
        {
            _wordBoard.SetWord(boardWordPlacement, word, TileState.Revealed, false);
            _gameState.CurrentSectionWords.Remove(word);

            var wordMiddlePosition =
                (boardWordPlacement.Position +
                 boardWordPlacement.Direction.ToStride() * word.Length / 2)
                .ToWorldPosition();

            Action onEffectCompleted;
            if (_gameState.CurrentSectionWords.Count == 0)
            {
                onEffectCompleted = () => StartCoroutine(BoardCompletedCoroutine());
            }
            else
            {
                onEffectCompleted = null;
                _ufoAnimator.PlayHappy();
                _audioController.SpellWord();
            }

            _flyingWordEffect.PlayMoveToTransformEffect(wordMiddlePosition, word, false, onEffectCompleted);

            _gameState.FirstEverWordCompleted = true;
            GameEvents.NotifyBoardWordCompleted(word);
        }

        private void MarkWordAsRecentlyFound(string word)
        {
            _gameState.RecentlyFoundWords.Enqueue(word);
            if (_gameState.RecentlyFoundWords.Count > _recentlyFoundWordBufferLength)
            {
                _gameState.RecentlyFoundWords.Dequeue();
            }
        }

        private IEnumerator BoardCompletedCoroutine()
        {
            _ufoAnimator.PlayHappy();

            _audioController.Celebrate();
            yield return _celebratoryText.Celebrate();

            _audioController.FlyUp();

            yield return new WaitForSeconds(0.3f);

            _ufoAnimator.PlayWin();
            _cameraRig.SetCameraOverBoard(true);
            _ufoRig.SetUfoTargetOverBoard(true);

            yield return new WaitForSeconds(1f);

            _audioController.TractorBeam();
            foreach (var npc in _npcSpawner.PopNpcsInSection(_gameState.CurrentSectionIndex))
            {
                npc.Hoist(_ufoRig.TractorBeamOrigin);
            }

            yield return new WaitForSeconds(0.4f);

            _audioController.FlyDown();

            yield return new WaitForSeconds(1f);

            _audioController.RandomPostSuctionSound();
            _cameraRig.SetCameraOverBoard(false);
            _ufoRig.SetUfoTargetOverBoard(false);

            ProgressToNextSection();

            _scenerySpawner.SetSection(_gameState.CurrentSectionIndex);

            _cameraRig.SetTargetSection(_gameState.CurrentSectionIndex);
            _ufoRig.SetTargetSection(_gameState.CurrentSectionIndex);

            SaveGame();
        }

        private void SaveGame()
        {
            using var context = SaveGameUtility.MakeSaveContext();
            Serialize(context);
        }

        private void LoadGame()
        {
            using var context = SaveGameUtility.MakeLoadContext();
            try
            {
                Serialize(context);
            }
            catch (EndOfStreamException)
            {
                // Incompatible save file. Just reset game for now...
                UnityEngine.Debug.LogWarning("Recreated save file due to incompatibility!");
                SaveGameUtility.DeleteSaveFile();
                StartGameFromScratch();
            }
        }

        private void Serialize(ReadOrWriteFileStream context)
        {
            _gameState.Serialize(context);
            _wordBoard.Serialize(context);
        }

        private void ProgressToNextSection()
        {
            // Dequeue and generate sections
            do
            {
                _gameState.CurrentSectionIndex++;
                while (_gameState.NewestGeneratedSectionIndex <
                       _gameState.CurrentSectionIndex + WordBoardGenerator.SectionsAheadAndBehind)
                {
                    GenerateAndEnqueueSection();
                }

                var section = _gameState.GeneratedFutureSections.Dequeue();
                (_gameState.CurrentSectionLetters, _gameState.CurrentSectionWords) = (section.Letters, section.Words);
            } while (!_gameState.CurrentSectionLetters.Any());

            _letterRing.SetLetters(_gameState.CurrentSectionLetters);

            if (!_gameState.FirstEverWordCompleted)
            {
                string firstShortestWord = _gameState
                    .CurrentSectionWords
                    .Select(p => p.Key)
                    .OrderBy(w => w.Length)
                    .First();

                _spellWordTutorial.Show(firstShortestWord);
            }

            UnlockCurrentSectionWords();

            ClearTilesBelowSection(_gameState.CurrentSectionIndex - WordBoardGenerator.SectionsAheadAndBehind);
        }

        private void UnlockCurrentSectionWords()
        {
            foreach (var word in _gameState.CurrentSectionWords.Keys)
            {
                _wordBoard.SetWord(_gameState.CurrentSectionWords[word], word, TileState.Hidden, false);
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
            _gameState.NewestGeneratedSectionIndex++;

            var generatedSectionWords =
                _wordBoardGenerator.GenerateSection(_gameState.NewestGeneratedSectionIndex,
                    _gameState.RecentlyFoundWords, out var letters);
            letters = WordUtility.ShuffleLetters(letters);
            _gameState.GeneratedFutureSections.Enqueue(new Section
            { Letters = letters, Words = generatedSectionWords });

            _npcSpawner.SpawnNpcsForSection(_gameState.NewestGeneratedSectionIndex, _wordBoard);
        }

        public void DebugCompleteOneWord()
        {
            if (_gameState.CurrentSectionWords.Any())
            {
                LetterRing_WordSubmitted(_gameState.CurrentSectionWords.Keys.First());
            }
        }

        public void DebugCompleteSection()
        {
            foreach (var word in _gameState.CurrentSectionWords.Keys.ToArray())
            {
                LetterRing_WordSubmitted(word);
            }
        }

        public void DebugGiveHint()
        {
            _gameState.BonusHintPoints++;
            SaveGame();
            _hintDisplay.SetHintPoints(_gameState.BonusHintPoints, true, false);
        }

        private struct Section : ISerializable
        {
            public string Letters;
            public SectionWords Words;

            public void Serialize(ReadOrWriteFileStream stream)
            {
                stream.Visit(ref Letters);
                stream.Visit(ref Words);
            }
        }
    }
}