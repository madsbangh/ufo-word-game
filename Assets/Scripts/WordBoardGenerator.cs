using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SectionWords = System.Collections.Generic.Dictionary<string, WordPlacement>;

public class WordBoardGenerator
{
	public const int SectionSize = 9;
	public const int SectionStride = 5;
	public const int SectionsAheadAndBehind = 4;

	private const int MinimumWordsPerSection = 4;
	private const int MaximumWordsPerSection = 6;
	private const int MaximumFreeWordPlacementAttempts = 100;
	private const int MinimumSectionLargestWordLetterCount = 5;

	private readonly PossibleWordsFinder _possibleWordsFinder;
	private readonly WordBoard _wordBoard;

	public WordBoardGenerator(string[] words, WordBoard wordBoard)
	{
		_possibleWordsFinder = new PossibleWordsFinder(words);
		_wordBoard = wordBoard;
	}

	public SectionWords GenerateSection(int sectionIndex, Queue<string> alreadySeenWords, out string letters)
	{
		var result = new SectionWords();

		var sectionStartCoordinate = sectionIndex * SectionStride;
		var sectionEndCoordinate = sectionStartCoordinate + SectionSize;

		var sortedUppercaseLetters = ChooseCandidateWordsAndLetters(alreadySeenWords, out var candidateWords);

		var tilesInSectionByLetter = GetExistingTilesInSectionByLetter(sortedUppercaseLetters, sectionStartCoordinate);

		var spectrumEnvelope = WordUtility.GetSpectrum(string.Empty);
		while (candidateWords.Any())
		{
			var word = candidateWords.Dequeue();

			if (TryPlaceWordInSection(word, tilesInSectionByLetter, sectionStartCoordinate, sectionEndCoordinate,
					out WordPlacement placement))
			{
				result.Add(word, placement);

				var spectrum = WordUtility.GetSpectrum(word);
				for (var i = 0; i < spectrum.Length; i++)
				{
					spectrumEnvelope[i] = Mathf.Max(spectrumEnvelope[i], spectrum[i]);
				}

				if (result.Count == MaximumWordsPerSection) break;
			}
		}

		letters = WordUtility.GetSortedUppercaseLetters(spectrumEnvelope);

		foreach (var word in result.Keys)
		{
			alreadySeenWords.Enqueue(word);
		}
		
		return result;
	}

	private Dictionary<char, HashSet<Vector2Int>> GetExistingTilesInSectionByLetter(
		string sortedUppercaseLetters,
		int sectionStartCoordinate)
	{
		var tilesInSectionByLetter = new Dictionary<char, HashSet<Vector2Int>>();

		foreach (var letter in sortedUppercaseLetters.Distinct())
			tilesInSectionByLetter[letter] = new HashSet<Vector2Int>();

		foreach (var position in _wordBoard.AllLetterTilePositions.Where(IsAfterSectionStart))
		{
			var letter = _wordBoard.GetLetterTile(position).Letter;
			if (sortedUppercaseLetters.Contains(letter)) tilesInSectionByLetter[letter].Add(position);
		}

		return tilesInSectionByLetter;

		bool IsAfterSectionStart(Vector2Int position)
		{
			return position.x >= sectionStartCoordinate &&
				   position.y >= sectionStartCoordinate;
		}
	}

	private string ChooseCandidateWordsAndLetters(Queue<string> alreadySeenWords, out Queue<string> resultSortedCandidateWords)
	{
		var resultCandidateWordsSet = new HashSet<string>();
		string sortedUppercaseLetters;
		do
		{
			do
			{
				var index = Random.Range(0, _possibleWordsFinder.AllLetterSets.Count());
				sortedUppercaseLetters = _possibleWordsFinder.AllLetterSets.ElementAt(index);
			} while (sortedUppercaseLetters.Length < MinimumSectionLargestWordLetterCount);

			resultCandidateWordsSet.Clear();

			_possibleWordsFinder.GetPossibleWordsFromContainedLetters(sortedUppercaseLetters, resultCandidateWordsSet);
			resultCandidateWordsSet.ExceptWith(alreadySeenWords);
		} while (resultCandidateWordsSet.Count < MinimumWordsPerSection);

		resultSortedCandidateWords = new Queue<string>(resultCandidateWordsSet
			.OrderBy(w => w.Length)
			.Reverse());

		return sortedUppercaseLetters;
	}

	private bool TryPlaceWordInSection(string word,
		Dictionary<char, HashSet<Vector2Int>> tilesInSectionByLetter,
		int sectionStartCoordinate, int sectionEndCoordinate,
		out WordPlacement placement)
	{
		return TryPlaceWordOnPivotInSection(word,
				   tilesInSectionByLetter,
				   sectionStartCoordinate,
				   sectionEndCoordinate,
				   out placement) ||
			   TryPlaceWordFreelyInSection(word,
				   tilesInSectionByLetter,
				   sectionStartCoordinate,
				   sectionEndCoordinate,
				   out placement);
	}

	private bool TryPlaceWordOnPivotInSection(string word,
		Dictionary<char, HashSet<Vector2Int>> tilesInSectionByLetter,
		int sectionStartCoordinate, int sectionEndCoordinate,
		out WordPlacement placement)
	{
		// First, try to place the word on the board so it overlaps with an existing word
		foreach (var direction in new[] { WordDirection.Horizontal, WordDirection.Vertical })
		{
			for (var letterIndex = 0; letterIndex < word.Length; letterIndex++)
			{
				var letter = word[letterIndex];
				var stride = direction.ToStride();
				foreach (var positionCandidate in tilesInSectionByLetter[letter]
							 .ToArray()
							 .Select(pivot => pivot - letterIndex * stride)
							 .Where(positionCandidate =>
								 TryPlaceWordAtPosition(word, tilesInSectionByLetter,
								 new WordPlacement { Position = positionCandidate, Direction = direction },
									 sectionStartCoordinate, sectionEndCoordinate)))
				{
					placement = new WordPlacement { Position = positionCandidate, Direction = direction };
					return true;
				}
			}
		}

		placement = default;
		return false;
	}

	private bool TryPlaceWordFreelyInSection(string word,
		Dictionary<char, HashSet<Vector2Int>> tilesInSectionByLetter,
		int sectionStartCoordinate, int sectionEndCoordinate,
		out WordPlacement placement)
	{
		var (firstDirection, secondDirection) = Random.value < 0.5f
			? (WordDirection.Horizontal, WordDirection.Vertical)
			: (WordDirection.Vertical, WordDirection.Horizontal);

		foreach (var direction in new[] { firstDirection, secondDirection })
		{
			for (var i = 0; i < MaximumFreeWordPlacementAttempts; i++)
			{
				var range = Vector2Int.one * (sectionEndCoordinate - sectionStartCoordinate);
				range -= direction.ToStride() * word.Length;
				var positionCandidate = new Vector2Int(
					Random.Range(sectionStartCoordinate, sectionStartCoordinate + range.x),
					Random.Range(sectionStartCoordinate, sectionStartCoordinate + range.y)
				);

				var placementCandidate = new WordPlacement { Position = positionCandidate, Direction = direction };
				if (TryPlaceWordAtPosition(word, tilesInSectionByLetter, placementCandidate,
						sectionStartCoordinate,
						sectionEndCoordinate))
				{
					placement = placementCandidate;
					return true;
				}
			}
		}

		placement = default;
		return false;
	}

	private bool TryPlaceWordAtPosition(string word,
		Dictionary<char, HashSet<Vector2Int>> tilesInSectionByLetter,
		WordPlacement placement,
		int sectionStartCoordinate,
		int sectionEndCoordinate)
	{
		var stride = placement.Direction.ToStride();

		// Check that word does not exceed section boundaries
		if (placement.Position.x < sectionStartCoordinate
			|| placement.Position.y < sectionStartCoordinate
			|| placement.Position.x + word.Length * stride.x > sectionEndCoordinate
			|| placement.Position.y + word.Length * stride.y > sectionEndCoordinate)
			return false;

		// Check for letter tiles before beginning of word
		if (_wordBoard.HasLetterTile(placement.Position - stride)) return false;

		// Check for letter tiles after end of word
		if (_wordBoard.HasLetterTile(placement.Position + word.Length * stride)) return false;

		// Check along word itself for:
		// - blocker hints
		// - existing letters that do not match 
		for (var i = 0; i < word.Length; i++)
		{
			var letterPosition = placement.Position + i * stride;

			if (_wordBoard.IsTileBlocked(letterPosition, placement.Direction)) return false;

			if (_wordBoard.HasLetterTile(letterPosition)
				&& _wordBoard.GetLetterTile(letterPosition).Letter != word[i])
				return false;

			// All checks passed.  Mark letter as a pivot.
			tilesInSectionByLetter[word[i]].Add(letterPosition);
		}

		// All checks passed.  Place word and return true.
		_wordBoard.SetWord(placement, word, TileState.Locked, true);
		return true;
	}
}