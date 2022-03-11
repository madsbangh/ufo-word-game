using EasyButtons;
using SaveGame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SectionWords = System.Collections.Generic.Dictionary<string, (UnityEngine.Vector2Int, WordDirection)>;

namespace Components
{
	public class GameController : MonoBehaviour, ISerializable
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

		private readonly Queue<(string, SectionWords)> _generatedFutureSections = new Queue<(string, SectionWords)>();

		private WordBoard _wordBoard;
		private WordBoardGenerator _wordBoardGenerator;
		private int _currentSectionIndex;
		private int _newestGeneratedSectionIndex;
		private SectionWords _currentSectionWords;
		private string _currentSectionLetters;

		private void Start()
		{
			_wordBoard = new WordBoard();
			_wordBoardGenerator = new WordBoardGenerator(_wordListAsset, _wordBoard);

			if (SaveGameUtility.SaveFileExists)
			{
				LoadGame();
				_letterRing.SetLetters(_currentSectionLetters);
				for (int i = _currentSectionIndex; i <= _newestGeneratedSectionIndex; i++)
				{
					_npcSpawner.SpawnNpcsForSection(i, _wordBoard);
				}
			}
			else
			{
				_currentSectionIndex = -1;
				_newestGeneratedSectionIndex = -1;
				ProgressToNextSection();
			}

			_boardSpawner.Initialize(_wordBoard);
			_scenerySpawner.Initialize(_wordBoard, 1 - WordBoardGenerator.SectionStride * _pastSectionCount);
			_letterRing.WordSubmitted += LetterRing_WordSubmitted;

			_scenerySpawner.ExpandToSection(_currentSectionIndex + _futureSectionCount - 1);

			_cameraRig.SetTargetSection(_currentSectionIndex);
			_cameraRig.SetCameraOverBoard(false);
			_cameraRig.TeleportToTarget();

			_ufoRig.SetTargetSection(_currentSectionIndex);
			_ufoRig.SetUfoTargetOverBoard(false);
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

			foreach (var npc in _npcSpawner.PopNpcsInSection(_currentSectionIndex))
			{
				npc.Hoist(_ufoRig.TractorBeamOrigin);
			}

			yield return new WaitForSeconds(_afterHoistSeconds);

			_cameraRig.SetCameraOverBoard(false);
			_ufoRig.SetUfoTargetOverBoard(false);

			ProgressToNextSection();

			_scenerySpawner.ExpandToSection(_currentSectionIndex + _futureSectionCount - 1);
			_scenerySpawner.CleanupBeforeSection(_currentSectionIndex - _pastSectionCount + 1);

			_cameraRig.SetTargetSection(_currentSectionIndex);
			_ufoRig.SetTargetSection(_currentSectionIndex);

			SaveGame();
		}

		private void SaveGame()
		{
			SaveGameUtility.Save(this, _wordBoard);
		}

		private void LoadGame()
		{
			SaveGameUtility.Load(this, _wordBoard);
		}

		private void ProgressToNextSection()
		{
			// Dequeue and generate sections
			do
			{
				_currentSectionIndex++;
				while (_newestGeneratedSectionIndex < _currentSectionIndex + _futureSectionCount)
				{
					GenerateAndEnqueueSection();
				}

				(_currentSectionLetters, _currentSectionWords) = _generatedFutureSections.Dequeue();
			} while (!_currentSectionLetters.Any());

			_letterRing.SetLetters(_currentSectionLetters);

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

			var generatedSectionWords =
				_wordBoardGenerator.GenerateSection(_newestGeneratedSectionIndex, out var letters);
			letters = WordUtility.ShuffleLetters(letters);
			_generatedFutureSections.Enqueue((letters, generatedSectionWords));

			_npcSpawner.SpawnNpcsForSection(_newestGeneratedSectionIndex, _wordBoard);
		}

		public void Serialize(ReadOrWriteFileStream stream)
		{
			stream.Serialize(ref _currentSectionIndex);
			stream.Serialize(ref _newestGeneratedSectionIndex);
			stream.Serialize(ref _currentSectionLetters);

			if (stream.IsWriteMode)
			{
				WriteSectionWords(stream, _currentSectionWords);
				stream.Write(_generatedFutureSections.Count);
				foreach (var (letters, sectionWords) in _generatedFutureSections)
				{
					stream.Write(letters);
					WriteSectionWords(stream, sectionWords);
				}
			}
			else
			{
				_currentSectionWords = ReadSectionWords(stream);
				var count = stream.ReadInt32();
				_generatedFutureSections.Clear();
				for (int i = 0; i < count; i++)
				{
					_generatedFutureSections.Enqueue(
						(stream.ReadString(),
						ReadSectionWords(stream)));
				}
			}
		}

		private static void WriteSectionWords(ReadOrWriteFileStream stream, SectionWords sectionWords)
		{
			stream.Write(sectionWords.Count);
			foreach (var pair in sectionWords)
			{
				stream.Write(pair.Key);
				stream.Write(pair.Value.Item1);
				stream.Write((int)pair.Value.Item2);
			}
		}

		private static SectionWords ReadSectionWords(ReadOrWriteFileStream stream)
		{
			var count = stream.ReadInt32();
			var sectionWords = new SectionWords(count);
			for (int i = 0; i < count; i++)
			{
				sectionWords.Add(
					stream.ReadString(),
					(stream.ReadVector2Int(),
					(WordDirection)stream.ReadInt32()));
			}
			return sectionWords;
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

		[ContextMenu("Delete Save File")]
		private void DebugDeleteSaveFile()
		{
			SaveGameUtility.DeleteSaveFile();
		}
	}
}