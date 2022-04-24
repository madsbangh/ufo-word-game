using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyButtons;
using SaveGame;
using UnityEngine;
using SectionWords = System.Collections.Generic.Dictionary<string, WordPlacement>;

namespace Components
{
	public class GameController : MonoBehaviour
	{
		[SerializeField] private TextAsset _wordListAsset;
		[SerializeField] private BoardSpawner _boardSpawner;
		[SerializeField] private ScenerySpawner _scenerySpawner;
		[SerializeField] private NpcSpawner _npcSpawner;
		[SerializeField] private CameraRig _cameraRig;
		[SerializeField] private UfoRig _ufoRig;
		[SerializeField] private UFOAnimator _ufoAnimator;
		[SerializeField] private UfoLetterRing _letterRing;
		[SerializeField] private ScoreDisplay _scoreDisplay;
		[SerializeField] private int _pastSectionCount, _futureSectionCount;
		[SerializeField] float _beforeHoistSeconds;
		[SerializeField] float _afterHoistSeconds;

		private WordBoard _wordBoard;
		private WordBoardGenerator _wordBoardGenerator;
		private GameState _gameState;

		private void Start()
		{
			_wordBoard = new WordBoard();
			_wordBoardGenerator = new WordBoardGenerator(_wordListAsset, _wordBoard);

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
		}

		private void OnDestroy()
		{
			_letterRing.WordSubmitted -= LetterRing_WordSubmitted;
		}

		private void StartGameFromScratch()
		{
			_gameState.GeneratedFutureSections = new Queue<Section>();
			_gameState.CurrentSectionWords = new SectionWords();
			_gameState.CurrentSectionIndex = -1;
			_gameState.NewestGeneratedSectionIndex = -1;
			ProgressToNextSection();
		}

		private void StartGameFromSaveFile()
		{
			LoadGame();
			_scoreDisplay.SetScore(_gameState.Score, false);
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
			_letterRing.WordSubmitted += LetterRing_WordSubmitted;

			_scenerySpawner.SetSection(_gameState.CurrentSectionIndex);

			_cameraRig.SetTargetSection(_gameState.CurrentSectionIndex);
			_cameraRig.SetCameraOverBoard(false);
			_cameraRig.TeleportToTarget();

			_ufoRig.SetTargetSection(_gameState.CurrentSectionIndex);
			_ufoRig.SetUfoTargetOverBoard(false);
			_ufoRig.TeleportToTarget();
		}

		private int CalculateScenerySpawnerWindowPadding()
		{
			var minPaddingSections = Mathf.Min(_pastSectionCount, _futureSectionCount);
			return WordBoardGenerator.SectionStride * minPaddingSections - 1;
		}

		private void GameEvents_NPCHoisted()
		{
			_gameState.Score++;
			_scoreDisplay.SetScore(_gameState.Score, true);
		}

		private void LetterRing_WordSubmitted(string word)
		{
			if (_gameState.CurrentSectionWords.TryGetValue(word, out var boardWordPlacement))
			{
				_wordBoard.SetWord(boardWordPlacement, word, TileState.Revealed, false);
				_gameState.CurrentSectionWords.Remove(word);

				if (_gameState.CurrentSectionWords.Count == 0)
				{
					StartCoroutine(BoardCompletedCoroutine());
				}
				else
				{
					_ufoAnimator.PlayHappy();
				}

				SaveGame();
			}
			else if (word.Length > 1)
			{
				_ufoAnimator.PlaySad();
			}
		}

		private IEnumerator BoardCompletedCoroutine()
		{
			_ufoAnimator.PlayWin();
			_cameraRig.SetCameraOverBoard(true);
			_ufoRig.SetUfoTargetOverBoard(true);

			yield return new WaitForSeconds(_beforeHoistSeconds);

			foreach (var npc in _npcSpawner.PopNpcsInSection(_gameState.CurrentSectionIndex))
			{
				npc.Hoist(_ufoRig.TractorBeamOrigin);
			}

			yield return new WaitForSeconds(_afterHoistSeconds);

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
				while (_gameState.NewestGeneratedSectionIndex < _gameState.CurrentSectionIndex + _futureSectionCount)
				{
					GenerateAndEnqueueSection();
				}

				var section = _gameState.GeneratedFutureSections.Dequeue();
				(_gameState.CurrentSectionLetters, _gameState.CurrentSectionWords) = (section.Letters, section.Words);
				
			} while (!_gameState.CurrentSectionLetters.Any());

			_letterRing.SetLetters(_gameState.CurrentSectionLetters);

			UnlockCurrentSectionWords();

			ClearTilesBelowSection(_gameState.CurrentSectionIndex - _pastSectionCount);
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
				_wordBoardGenerator.GenerateSection(_gameState.NewestGeneratedSectionIndex, out var letters);
			letters = WordUtility.ShuffleLetters(letters);
			_gameState.GeneratedFutureSections.Enqueue(new Section { Letters = letters, Words = generatedSectionWords });

			_npcSpawner.SpawnNpcsForSection(_gameState.NewestGeneratedSectionIndex, _wordBoard);
		}

		[Button("Cheat: Log Words", Mode = ButtonMode.EnabledInPlayMode)]
		private void DebugLogWords()
		{
			foreach (var word in _gameState.CurrentSectionWords.Keys)
			{
				UnityEngine.Debug.Log(word);
			}
		}

		[Button("Cheat: Complete One word", Mode = ButtonMode.EnabledInPlayMode)]
		private void DebugCompleteOneWord()
		{
			if (_gameState.CurrentSectionWords.Any())
			{
				LetterRing_WordSubmitted(_gameState.CurrentSectionWords.Keys.First());
			}
		}

		[Button("Cheat: Complete Section", Mode = ButtonMode.EnabledInPlayMode)]
		private void DebugCompleteSection()
		{
			foreach (var word in _gameState.CurrentSectionWords.Keys.ToArray())
			{
				LetterRing_WordSubmitted(word);
			}
		}

		[ContextMenu("Delete Save File")]
		private void DebugDeleteSaveFile()
		{
			SaveGameUtility.DeleteSaveFile();
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

		private struct GameState : ISerializable
		{
			public Queue<Section> GeneratedFutureSections;
			public SectionWords CurrentSectionWords;
			public int CurrentSectionIndex;
			public int NewestGeneratedSectionIndex;
			public string CurrentSectionLetters;
			public int Score;

			public void Serialize(ReadOrWriteFileStream stream)
			{
				stream.Visit(ref CurrentSectionIndex);
				stream.Visit(ref NewestGeneratedSectionIndex);
				stream.Visit(ref CurrentSectionLetters);
				stream.Visit(ref CurrentSectionWords);
				stream.Visit(ref GeneratedFutureSections);
				stream.Visit(ref Score);
			}
		}
	}
}