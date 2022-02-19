using System.Collections.Generic;

public class PossibleWordsTrie
{
	private readonly Node _root;
	private readonly int _alphabetLetterCount;

	public PossibleWordsTrie(IEnumerable<string> uppercaseWords, int alphabetLetterCount)
	{
		_root = new Node();
		_alphabetLetterCount = alphabetLetterCount;

		foreach (var uppercaseWord in uppercaseWords)
		{
			var keySpectrum = WordUtility.GetSpectrum(uppercaseWord, alphabetLetterCount);

			var node = _root;
			foreach (var childKey in keySpectrum)
			{
				// All non-leaf nodes have only children, but no word sets
				node.Children ??= new Dictionary<int, Node>();
				if (node.Children.ContainsKey(childKey) == false)
				{
					node.Children.Add(childKey, new Node());
				}

				node = node.Children[childKey];
			}

			// Last node is a leaf node, and only has a set of words
			if (node.UppercaseWords == null)
			{
				node.UppercaseWords = new HashSet<string>();
			}

			node.UppercaseWords.Add(uppercaseWord);
		}
	}

	public void GetPossibleWords(string sortedUppercaseLetters, HashSet<string> result)
	{
		var keySpectrum = WordUtility.GetSpectrum(sortedUppercaseLetters, _alphabetLetterCount);

		GetPossibleWordsFromSpectrumRecursive(keySpectrum, 0, _root, result);
	}

	private static void GetPossibleWordsFromSpectrumRecursive(IReadOnlyList<int> fullSpectrum, int currentSpectrumIndex, Node currentNode, ISet<string> result)
	{
		// Add possible words to the result set if any
		if (currentNode.UppercaseWords != null)
		{
			result.UnionWith(currentNode.UppercaseWords);
		}
		else
		{
			// Recursively get any words where the current letter frequency is equal to or less than the frequency in the provided key
			for (int frequency = 0; frequency <= fullSpectrum[currentSpectrumIndex]; frequency++)
			{
				if (currentNode.Children.TryGetValue(frequency, out var childNode))
				{
					GetPossibleWordsFromSpectrumRecursive(fullSpectrum, currentSpectrumIndex + 1, childNode, result);
				}
			}
		}
	}

	private class Node
	{
		public Dictionary<int, Node> Children;
		public HashSet<string> UppercaseWords;
	}
}