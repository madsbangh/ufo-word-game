using TMPro;
using UnityEngine;

namespace Components
{
	public class LetterTile : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _letter;

		[SerializeField]
		private GameObject _lockedIndicator;

		[SerializeField]
		private GameObject _hiddenIndicator;

		public char Letter
		{
			get => _letter.text[0];
			set
			{
				_letter.text = value.ToString();
				name = _letter.text;
			}
		}

		public TileState State
		{
			set
			{
				_letter.gameObject.SetActive(value == TileState.Revealed);
				_lockedIndicator.SetActive(value == TileState.Locked);
				_hiddenIndicator.SetActive(value == TileState.Hidden);
			}
		}

		public Vector2Int Position
		{
			set
			{
				var t = transform;
				t.position = value.ToWorldPosition();
			}
		}
	}
}
