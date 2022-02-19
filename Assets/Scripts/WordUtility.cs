using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public static class WordUtility
{
    public const int MinimumWordLength = 3;
    public const int MaximumWordLength = 9;

    public static string SortLetters(string input)
    {
        char[] characters = input.ToCharArray();
        Array.Sort(characters);
        return new string(characters);
    }

    public static string ShuffleLetters(string input)
    {
        var characters = input.ToCharArray();
        for (var first = 0; first < characters.Length; first++)
        {
            var last = Random.Range(first, characters.Length);
            (characters[first], characters[last]) = (characters[last], characters[first]);
        }

        return new string(characters);
    }

    public static int[] GetSpectrum(string uppercaseWord, int alphabetLetterCount)
    {
        var spectrum = new int[alphabetLetterCount];
        foreach (int c in uppercaseWord)
        {
            spectrum[c - 'A']++;
        }

        return spectrum;
    }

    public static string GetSortedUppercaseLetters(int[] spectrum)
    {
        var result = new StringBuilder();
        for (var letterIndex = 0; letterIndex < spectrum.Length; letterIndex++)
        {
            for (var count = 0; count < spectrum[letterIndex]; count++)
            {
                result.Append((char) ('A' + letterIndex));
            }
        }

        return result.ToString();
    }

    public static string[] ParseFilterAndProcessWordList(string wordList) => wordList
        .Split((char[]) null, StringSplitOptions.RemoveEmptyEntries)
        .Where(IsWordValid)
        .Select(word => word.ToUpper())
        .ToArray();

    public static Vector2Int ToStride(this WordDirection direction) =>
        direction == WordDirection.Horizontal
            ? Vector2Int.right
            : Vector2Int.up;

    private static bool IsWordValid(string word) =>
        word.Length >= MinimumWordLength
        && word.Length <= MaximumWordLength
        && word.All(char.IsLetter);
}