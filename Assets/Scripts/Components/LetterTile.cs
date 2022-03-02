using System;
using TMPro;
using UnityEngine;

namespace Components
{
	public class LetterTile : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _letter;

		[SerializeField]
		private SpriteRenderer _sprite;

		[SerializeField]
		private Color _lockedColor, _hiddenColor, _revealedColor;

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
				switch (value)
				{
					case TileState.Locked:
						_sprite.color = _lockedColor;
						_letter.gameObject.SetActive(false);
						break;
					case TileState.Hidden:
						_sprite.color = _hiddenColor;
						_letter.gameObject.SetActive(false);
						break;
					case TileState.Revealed:
						_sprite.color = _revealedColor;
						_letter.gameObject.SetActive(true);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value));
				}
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
