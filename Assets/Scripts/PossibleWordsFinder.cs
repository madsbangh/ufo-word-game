using System.Collections.Generic;
using System.Linq;

public class PossibleWordsFinder
{
	public readonly IEnumerable<string> AllLetterSets;

	private readonly PossibleWordsTrie _possibleWordsStructure;

	public PossibleWordsFinder(string[] uppercaseWords)
	{
		_possibleWordsStructure = GeneratePossibleWordsStructure(uppercaseWords);

		var allLetterSets = new HashSet<string>();
		foreach (var word in uppercaseWords)
		{
			var key = WordUtility.SortLetters(word);
			allLetterSets.Add(key);
		}
		AllLetterSets = allLetterSets;
	}

	public void GetPossibleWordsFromContainedLetters(string sortedUppercaseLetters, HashSet<string> result)
	{
		_possibleWordsStructure.GetPossibleWords(sortedUppercaseLetters, result);
	}

	private static PossibleWordsTrie GeneratePossibleWordsStructure(string[] uppercaseWords)
	{
		return new PossibleWordsTrie(uppercaseWords);
	}
}