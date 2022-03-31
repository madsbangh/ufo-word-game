using EasyButtons;
using SaveGame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
				LoadGame();
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
			else
			{
				_gameState.GeneratedFutureSections = new Queue<Section>();
				_gameState.CurrentSectionWords = new SectionWords();
				_gameState.CurrentSectionIndex = -1;
				_gameState.NewestGeneratedSectionIndex = -1;
				ProgressToNextSection();
			}

			_boardSpawner.Initialize(_wordBoard);
			_scenerySpawner.Initialize(_wordBoard, 1 - WordBoardGenerator.SectionStride * _pastSectionCount);
			_letterRing.WordSubmitted += LetterRing_WordSubmitted;

			_scenerySpawner.ExpandToSection(_gameState.CurrentSectionIndex + _futureSectionCount - 1);

			_cameraRig.SetTargetSection(_gameState.CurrentSectionIndex);
			_cameraRig.SetCameraOverBoard(false);
			_cameraRig.TeleportToTarget();

			_ufoRig.SetTargetSection(_gameState.CurrentSectionIndex);
			_ufoRig.SetUfoTargetOverBoard(false);
			_ufoRig.TeleportToTarget();
		}

		private void OnDestroy()
		{
			_letterRing.WordSubmitted -= LetterRing_WordSubmitted;
		}

		private void LetterRing_WordSubmitted(string word)
		{
			if (_gameState.CurrentSectionWords.TryGetValue(word, out WordPlacement boardWordPlacement))
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

			_scenerySpawner.ExpandToSection(_gameState.CurrentSectionIndex + _futureSectionCount - 1);
			_scenerySpawner.CleanupBeforeSection(_gameState.CurrentSectionIndex - _pastSectionCount + 1);

			_cameraRig.SetTargetSection(_gameState.CurrentSectionIndex);
			_ufoRig.SetTargetSection(_gameState.CurrentSectionIndex);

			SaveGame();
		}

		private void SaveGame()
		{
			using var context = SaveGameUtility.MakeSaveContext();
			_gameState.Serialize(context);
			_wordBoard.Serialize(context);
		}

		private void LoadGame()
		{
			using var context = SaveGameUtility.MakeLoadContext();
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

				Section section = _gameState.GeneratedFutureSections.Dequeue();
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

		[Button("Cheat: Almost Complete Section", Mode = ButtonMode.EnabledInPlayMode)]
		private void DebugAlmostCompleteSection()
		{
			foreach (var word in _gameState.CurrentSectionWords.Keys.Skip(1).ToArray())
			{
				LetterRing_WordSubmitted(word);
			}

			DebugLogWords();
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
				stream.Serialize(ref Letters);
				stream.Serialize(ref Words);
			}
		}

		private struct GameState : ISerializable
		{
			public Queue<Section> GeneratedFutureSections;
			public SectionWords CurrentSectionWords;
			public int CurrentSectionIndex;
			public int NewestGeneratedSectionIndex;
			public string CurrentSectionLetters;

			public void Serialize(ReadOrWriteFileStream stream)
			{
				stream.Serialize(ref CurrentSectionIndex);
				stream.Serialize(ref NewestGeneratedSectionIndex);
				stream.Serialize(ref CurrentSectionLetters);
				stream.Serialize(ref CurrentSectionWords);
				stream.Serialize(ref GeneratedFutureSections);
			}
		}
	}
}