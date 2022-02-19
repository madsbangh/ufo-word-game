using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Debug
{
	public class AnagramTester : MonoBehaviour
	{
		[SerializeField]
		private Button _testButton;

		[SerializeField]
		private TMPro.TMP_InputField _testInputField;

		[SerializeField]
		private TMPro.TMP_Text _testOutputText;

		[SerializeField]
		private TextAsset _wordListAsset;

		private PossibleWordsFinder _wordSearcher;

		private void Awake()
		{
			var uppercaseWords = WordUtility.ParseFilterAndProcessWordList(_wordListAsset.text);

			_wordSearcher = new PossibleWordsFinder(uppercaseWords);
		}

		private void Start()
		{
			_testButton.onClick.AddListener(Test);
		}

		private void OnDestroy()
		{
			_testButton.onClick.RemoveListener(Test);
		}

		private void Test()
		{
			_testInputField.text = _testInputField.text.Substring(0, Mathf.Min(_testInputField.text.Length, WordUtility.MaximumWordLength));

			var sortedUppercaseLetters = WordUtility.SortLetters(_testInputField.text.ToUpper());

			var possibleWords = new HashSet<string>();
			_wordSearcher.GetPossibleWordsFromContainedLetters(sortedUppercaseLetters, possibleWords);

			var sb = new StringBuilder();
			foreach (var word in possibleWords)
			{
				sb.AppendLine(word);
			}

			_testOutputText.text = sb.ToString();
		}
	}
}