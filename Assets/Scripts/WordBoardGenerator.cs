using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordBoardGenerator
{
    public const int SectionSize = 9;
    public const int SectionStride = 3;

    private const int MinimumWordsPerSection = 4;
    private const int MaximumWordsPerSection = 6;
    private const int MaximumFreeWordPlacementAttempts = 100;
    private const int MinimumSectionLargestWordLetterCount = 5;

    private readonly PossibleWordsFinder _anagramFinder;
    private readonly WordBoard _wordBoard;

    public WordBoardGenerator(TextAsset wordListAsset, WordBoard wordBoard)
    {
        var words = WordUtility.ParseFilterAndProcessWordList(wordListAsset.text);
        _anagramFinder = new PossibleWordsFinder(words);
        _wordBoard = wordBoard;
    }

    public Dictionary<string, (Vector2Int, WordDirection)> GenerateSection(int sectionIndex, out string letters)
    {
        // Get a set of all possible words that can be written with the given letters
        // Note: this can potentially loop forever in case there are no letter sets with at least the minimum required amount of containing words
        string sortedUppercaseLetters;

        IEnumerable<string> candidateWords = new HashSet<string>();
        {
            var candidateWordsTyped = (HashSet<string>) candidateWords;
            do
            {
                do
                {
                    var index = Random.Range(0, _anagramFinder.AllLetterSets.Count());
                    sortedUppercaseLetters = _anagramFinder.AllLetterSets.ElementAt(index);
                } while (sortedUppercaseLetters.Length < MinimumSectionLargestWordLetterCount);

                candidateWordsTyped.Clear();

                _anagramFinder.GetPossibleWordsFromContainedLetters(sortedUppercaseLetters,
                    (HashSet<string>) candidateWords);
            } while (candidateWordsTyped.Count < MinimumWordsPerSection);
        }

        candidateWords = candidateWords
            .OrderBy(w => w.Length)
            .ToList();

        // Find start and end coordinates (both X and Y) of current board section
        var sectionStartCoordinate = sectionIndex * SectionStride;
        var sectionEndCoordinate = sectionStartCoordinate + SectionSize;

        // Get only the letter tiles that come after the start of this section
        var tilesInSection
            = new HashSet<Vector2Int>(
                _wordBoard.AllLetterTilePositions
                    .Where(p =>
                        p.x >= sectionStartCoordinate &&
                        p.y >= sectionStartCoordinate));

        // Create a dictionary of existing letter tile positions grouped by which letter they have
        var tilesInSectionByLetter = new Dictionary<char, HashSet<Vector2Int>>();

        foreach (var letter in sortedUppercaseLetters.Distinct())
            tilesInSectionByLetter[letter] = new HashSet<Vector2Int>();

        // Populate the above mentioned grouping
        foreach (var position in tilesInSection)
        {
            var letter = _wordBoard.GetLetterTile(position).Letter;
            if (sortedUppercaseLetters.Contains(letter))
            {
                tilesInSectionByLetter[letter].Add(position);
            }
        }

        var done = false;
        var sectionWordsAndPlacements = new Dictionary<string, (Vector2Int, WordDirection)>();
        var spectrumEnvelope = WordUtility.GetSpectrum(string.Empty, _anagramFinder.AlphabetLetterCount);
        var firstDirection = WordDirection.Horizontal;
        var secondDirection = WordDirection.Vertical;
        while (done == false)
        {
            if (candidateWords.Any() == false)
            {
                done = true;
                continue;
            }

            string word;
            {
                var candidateWordsTyped = (List<string>) candidateWords;
                word = candidateWordsTyped.Last();
                candidateWordsTyped.RemoveAt(candidateWordsTyped.Count - 1);
            }

            // Swap the first and second direction, so we start with the opposite as last every time
            (firstDirection, secondDirection) = (secondDirection, firstDirection);
            
            // Try to place word first on existing pivots, then freely, first in the wanted direction, then the other
            (Vector2Int position, WordDirection direction) placement;
            if (TryPlaceWordOnPivotInSection(word, tilesInSectionByLetter, sectionStartCoordinate,
                    sectionEndCoordinate, firstDirection, out placement)
                || TryPlaceWordOnPivotInSection(word, tilesInSectionByLetter, sectionStartCoordinate,
                    sectionEndCoordinate, secondDirection, out placement)
                || TryPlaceWordFreelyInSection(word, tilesInSectionByLetter, sectionStartCoordinate,
                    sectionEndCoordinate, firstDirection, out placement)
                || TryPlaceWordFreelyInSection(word, tilesInSectionByLetter, sectionStartCoordinate,
                    sectionEndCoordinate, secondDirection, out placement))
            {
                sectionWordsAndPlacements.Add(word, (placement.position, placement.direction));
                var spectrum = WordUtility.GetSpectrum(word, _anagramFinder.AlphabetLetterCount);
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrumEnvelope[i] = Mathf.Max(spectrumEnvelope[i], spectrum[i]);
                }

                if (sectionWordsAndPlacements.Count == MaximumWordsPerSection)
                {
                    done = true;
                }
            }
            else
            {
                done = true;
            }
        }

        letters = WordUtility.GetSortedUppercaseLetters(spectrumEnvelope);
        return sectionWordsAndPlacements;
    }

    private bool TryPlaceWordOnPivotInSection(string word, Dictionary<char, HashSet<Vector2Int>> tilesInSectionByLetter,
        int sectionStartCoordinate, int sectionEndCoordinate, WordDirection direction,
        out (Vector2Int, WordDirection) placement)
    {
        // First, try to place the word on the board so it overlaps with an existing word
        for (var letterIndex = 0; letterIndex < word.Length; letterIndex++)
        {
            var letter = word[letterIndex];
            var stride = direction.ToStride();
            foreach (var pivot in tilesInSectionByLetter[letter])
            {
                var positionCandidate = pivot - letterIndex * stride;
                if (TryPlaceWord(word, positionCandidate, direction, sectionStartCoordinate,
                        sectionEndCoordinate))
                {
                    placement = (positionCandidate, direction);
                    return true;
                }
            }
        }

        placement = default;
        return false;
    }

    private bool TryPlaceWordFreelyInSection(string word, Dictionary<char, HashSet<Vector2Int>> tilesInSectionByLetter,
        int sectionStartCoordinate, int sectionEndCoordinate, WordDirection direction,
        out (Vector2Int, WordDirection) placement)
    {
        // No overlapping word placement possible, try to place it freely
        for (var i = 0; i < MaximumFreeWordPlacementAttempts; i++)
        {
            var range = Vector2Int.one * (sectionEndCoordinate - sectionStartCoordinate);
            range -= direction.ToStride() * word.Length;
            var positionCandidate = new Vector2Int(
                Random.Range(sectionStartCoordinate, sectionStartCoordinate + range.x),
                Random.Range(sectionStartCoordinate, sectionStartCoordinate + range.y)
            );

            if (TryPlaceWord(word, positionCandidate, direction, sectionStartCoordinate,
                    sectionEndCoordinate))
            {
                placement = (positionCandidate, direction);
                return true;
            }
        }

        placement = default;
        return false;
    }

    private bool TryPlaceWord(string word, Vector2Int position, WordDirection direction, int sectionStartCoordinate,
        int sectionEndCoordinate)
    {
        var stride = direction.ToStride();

        // Check that word does not exceed section boundaries
        if (position.x < sectionStartCoordinate
            || position.y < sectionStartCoordinate
            || position.x + word.Length * stride.x > sectionEndCoordinate
            || position.y + word.Length * stride.y > sectionEndCoordinate)
            return false;

        // Check for letter tiles before beginning of word
        if (_wordBoard.HasLetterTile(position - stride))
        {
            return false;
        }

        // Check for letter tiles after end of word
        if (_wordBoard.HasLetterTile(position + word.Length * stride))
        {
            return false;
        }

        // Check along word itself for:
        // - blocker hints
        // - existing letters that do not match 
        for (var i = 0; i < word.Length; i++)
        {
            var letterPosition = position + i * stride;

            if (_wordBoard.IsTileBlocked(letterPosition, direction))
            {
                return false;
            }

            if (_wordBoard.HasLetterTile(letterPosition)
                && _wordBoard.GetLetterTile(letterPosition).Letter != word[i])
                return false;
        }

        // All checks passed.  Place word and return true.
        _wordBoard.SetWord(position, direction, word, TileState.Locked, true);
        return true;
    }
}