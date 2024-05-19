using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Misc;
using GameStateAndData;
using SaveGame;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Components
{
    public class GameController : MonoBehaviour
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

        private WordBoardGenerator _wordBoardGenerator;
        private GameState _gameState;
        private HashSet<string> _allAllowedWords;
        private float _showUseAHintHintTimer;
        private bool _useAHintHintShown;

        public ReadOnlyGameState ReadOnlyGameState => _gameState.Readonly;
        
        private void Awake()
        {
            var allWords = WordUtility.ParseFilterAndProcessWordList(_bigWordListAsset.text);
            _allAllowedWords = new HashSet<string>(allWords);

            var commonWords = WordUtility.ParseFilterAndProcessWordList(_commonWordListAsset.text);

            if (!TryStartGameFromSaveFile(commonWords))
            {
                StartGameFromScratch(commonWords);
            }
            
            SetupSceneObjects();

            GameEvents.NpcHoisted += GameEvents_NPCHoisted;
            _letterRing.WordSubmitted += LetterRing_WordSubmitted;
            _hintDisplay.OnHintButtonClicked.AddListener(HintDisplay_OnHintButtonClicked);
        }

        private void OnDestroy()
        {
            GameEvents.NpcHoisted -= GameEvents_NPCHoisted;
            _letterRing.WordSubmitted -= LetterRing_WordSubmitted;
            _hintDisplay.OnHintButtonClicked.RemoveListener(HintDisplay_OnHintButtonClicked);
        }

        private void Update()
        {
            if (!_useAHintHintShown &&
                !_gameState.FirstEverHintUsed.Value &&
                _gameState.BonusHintPoints.Value >= HintPointsRequiredPerHint)
            {
                _showUseAHintHintTimer += Time.deltaTime;
                if (_showUseAHintHintTimer >= _showUseAHintHintDelay)
                {
                    _useAHintHint.Show();
                    _useAHintHintShown = true;
                }
            }
        }

        private void LateUpdate()
        {
            if (!_gameState.Dirty) return;
            SaveGameUtility.SaveGame(_gameState);
            _gameState.Dirty = false;
        }

        private void HintDisplay_OnHintButtonClicked()
        {
            if (_gameState.BonusHintPoints.Value >= HintPointsRequiredPerHint)
            {
                UseHint();
            }
        }

        private void UseHint()
        {
            if (!_gameState.FirstEverHintUsed.Value)
            {
                _useAHintHint.Dismiss();
                _gameState.FirstEverHintUsed.Value = true;
            }

            var tileToReveal = GetRandomHiddenTile();
            if (tileToReveal.HasValue)
            {
                _gameState.BonusHintPoints.Value -= HintPointsRequiredPerHint;
                _hintDisplay.SetHintPoints(_gameState.BonusHintPoints.Value, true, false);
                _gameState.WordBoard.RevealTile(tileToReveal.Value);

                var wordsFullyRevealedByHint = _gameState.CurrentSectionWords.Items
                    .Where(word => WordContainsTile(word, tileToReveal.Value))
                    .Where(WordIsFullyRevealed);

                foreach (var (word, placement) in wordsFullyRevealedByHint.ToArray())
                {
                    PlaceWordAndCompleteSectionIfNeeded(word, placement);
                }
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

                if (!_gameState.WordBoard.HasLetterTile(position))
                {
                    throw new ArgumentOutOfRangeException(nameof(wordPlacementPair),
                        "The given word falls outside the existing board tiles.");
                }

                if (_gameState.WordBoard.GetLetterTile(position).Progress != TileState.Revealed)
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2Int? GetRandomHiddenTile()
        {
            foreach (var wordPlacementPair in _gameState.CurrentSectionWords.Items)
            {
                var wordLength = wordPlacementPair.Key.Length;
                var randomStartIndex = Random.Range(0, wordLength - 1);
                for (var i = 0; i < wordLength; i++)
                {
                    var candidatePosition =
                        wordPlacementPair.Value.Position +
                        wordPlacementPair.Value.Direction.ToStride() *
                        ((i + randomStartIndex) % wordLength);

                    if (_gameState.WordBoard.GetLetterTile(candidatePosition).Progress == TileState.Hidden)
                    {
                        return candidatePosition;
                    }
                }
            }

            return null;
        }

        private void StartGameFromScratch(string[] commonWords)
        {
            _gameState = SaveGameUtility.CreateNewGameState();
            _wordBoardGenerator = new WordBoardGenerator(commonWords, _gameState.WordBoard);
            ProgressToNextSection();
        }


        private bool TryStartGameFromSaveFile(string[] commonWords)
        {
            try
            {
                if (!SaveGameUtility.TryLoadGame(out _gameState))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                UnityEngine.Debug.LogError("Failed to load save file because it seems to be corrupted");
                ShowCorruptedSaveGameErrorDialog();
                return false;
            }
            
            _wordBoardGenerator = new WordBoardGenerator(commonWords, _gameState.WordBoard);

            _letterRing.SetLetters(_gameState.CurrentSectionLetters.Value);
            for (var i = _gameState.CurrentSectionIndex.Value; i <= _gameState.NewestGeneratedSectionIndex.Value; i++)
            {
                _npcSpawner.SpawnNpcsForSection(i, _gameState.WordBoard);
            }

            if (_gameState.CurrentSectionWords.Keys.Count != 0) return true;
            
            // If we loaded into a completed board
            // Immediately progress to next section
            ProgressToNextSection();
            _ufoRig.TeleportToTarget();
            _cameraRig.TeleportToTarget();
            return true;
        }

        private void SetupSceneObjects()
        {
            _scoreDisplay.SetScore(_gameState.Score.Value, false);
            _hintDisplay.SetHintPoints(_gameState.BonusHintPoints.Value, false, false);

            _boardSpawner.Initialize(_gameState.WordBoard);
            _scenerySpawner.Initialize(_gameState.WordBoard, CalculateScenerySpawnerWindowPadding());

            _scenerySpawner.SetSection(_gameState.CurrentSectionIndex.Value);

            _cameraRig.SetTargetSection(_gameState.CurrentSectionIndex.Value);
            _cameraRig.SetCameraOverBoard(false);
            _cameraRig.TeleportToTarget();

            _ufoRig.SetTargetSection(_gameState.CurrentSectionIndex.Value);
            _ufoRig.SetUfoTargetOverBoard(false);
            _ufoRig.TeleportToTarget();
        }

        private static int CalculateScenerySpawnerWindowPadding()
        {
            return WordBoardGenerator.SectionStride * (WordBoardGenerator.SectionsAheadAndBehind - 1);
        }

        private void GameEvents_NPCHoisted()
        {
            _gameState.Score.Value++;
            _scoreDisplay.SetScore(_gameState.Score.Value, true);
            _audioController.Score();
        }

        private void LetterRing_WordSubmitted(string word)
        {
            _showUseAHintHintTimer = 0f;
            if (_gameState.CurrentSectionWords.TryGetValue(word, out var boardWordPlacement))
            {
                _gameState.BonusHintPoints.Value++;
                _hintDisplay.SetHintPoints(1, true, true);
                PlaceWordAndCompleteSectionIfNeeded(word, boardWordPlacement);
                _previewWordAnimator.HideWord();
            }
            else if (_allAllowedWords.Contains(word))
            {
                if (!_gameState.RecentlyFoundWords.Contains(word))
                {
                    _gameState.BonusHintPoints.Value += 2;
                    _ufoAnimator.PlayFoundBonusWord();
                    _flyingWordEffect.PlayMoveToTransformEffect(GetHintIndicatorWorldSpacePosition(), word, true, () =>
                    {
                        _hintDisplay.SetHintPoints(2, true, true);
                    });
                    _previewWordAnimator.HideWord();
                    MarkWordAsRecentlyFound(word);
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
            _gameState.WordBoard.SetWord(boardWordPlacement, word, TileState.Revealed, false);
            _gameState.CurrentSectionWords.Remove(word);

            var wordMiddlePosition =
                (boardWordPlacement.Position +
                 boardWordPlacement.Direction.ToStride() * word.Length / 2)
                .ToWorldPosition();

            Action onEffectCompleted;
            if (_gameState.CurrentSectionWords.Keys.Count == 0)
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

            _gameState.FirstEverWordCompleted.Value = true;
            GameEvents.NotifyBoardWordCompleted(word);
        }

        private void MarkWordAsRecentlyFound(string word)
        {
            _gameState.RecentlyFoundWords.Enqueue(word);
            if (_gameState.RecentlyFoundWords.Items.Count > _recentlyFoundWordBufferLength)
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
            foreach (var npc in _npcSpawner.PopNpcsInSection(_gameState.CurrentSectionIndex.Value))
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

            _scenerySpawner.SetSection(_gameState.CurrentSectionIndex.Value);

            _cameraRig.SetTargetSection(_gameState.CurrentSectionIndex.Value);
            _ufoRig.SetTargetSection(_gameState.CurrentSectionIndex.Value);
        }

        private void ProgressToNextSection()
        {
            // Dequeue and generate sections
            do
            {
                _gameState.CurrentSectionIndex.Value++;
                while (_gameState.NewestGeneratedSectionIndex.Value <
                       _gameState.CurrentSectionIndex.Value + WordBoardGenerator.SectionsAheadAndBehind)
                {
                    GenerateAndEnqueueSection();
                }

                var section = _gameState.GeneratedFutureSections.Dequeue();
                _gameState.CurrentSectionLetters.Value = section.Letters;
                _gameState.CurrentSectionWords.Clear();
                foreach (var (word, placement) in section.Words)
                {
                    _gameState.CurrentSectionWords.Add(word, placement);
                }
            } while (!_gameState.CurrentSectionLetters.Value.Any());

            _letterRing.SetLetters(_gameState.CurrentSectionLetters.Value);

            if (!_gameState.FirstEverWordCompleted.Value)
            {
                var firstShortestWord = _gameState
                    .CurrentSectionWords.Keys
                    .OrderBy(w => w.Length)
                    .First();

                _spellWordTutorial.Show(firstShortestWord);
            }

            UnlockCurrentSectionWords();

            ClearTilesBelowSection(_gameState.CurrentSectionIndex.Value - WordBoardGenerator.SectionsAheadAndBehind);
        }

        private void UnlockCurrentSectionWords()
        {
            foreach (var word in _gameState.CurrentSectionWords.Keys)
            {
                _gameState.WordBoard.SetWord(_gameState.CurrentSectionWords[word], word, TileState.Hidden, false);
            }
        }

        private void ClearTilesBelowSection(int sectionIndex)
        {
            foreach (var position in _gameState.WordBoard.AllLetterAndBlockerTilePositions.ToArray())
            {
                var minPosition = sectionIndex * WordBoardGenerator.SectionStride;
                if (position.x < minPosition || position.y < minPosition)
                {
                    _gameState.WordBoard.FullyClearTile(position);
                }
            }
        }

        private void GenerateAndEnqueueSection()
        {
            _gameState.NewestGeneratedSectionIndex.Value++;

            var generatedSectionWords =
                _wordBoardGenerator.GenerateSection(_gameState.NewestGeneratedSectionIndex.Value,
                    _gameState.RecentlyFoundWords, out var letters);
            letters = WordUtility.ShuffleLetters(letters);
            _gameState.GeneratedFutureSections.Enqueue(new GameState.Section(generatedSectionWords, letters));

            _npcSpawner.SpawnNpcsForSection(_gameState.NewestGeneratedSectionIndex.Value, _gameState.WordBoard);
        }

        public void DebugCompleteOneWord()
        {
            if (_gameState.CurrentSectionWords.Keys.Any())
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
            _gameState.BonusHintPoints.Value++;
            _hintDisplay.SetHintPoints(_gameState.BonusHintPoints.Value, true, false);
        }

        internal void SelectUfoSkin(int index)
        {
            _gameState.SelectedUfoIndex.Value = index;
        }

        internal void UnlockUfoSkin(int index)
        {
            _gameState.UnlockedUfoIndices.Add(index);
        }
    }
}