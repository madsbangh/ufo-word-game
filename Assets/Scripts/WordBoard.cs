using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordBoard
{
	public event Action<Vector2Int> LetterTileChanged;

	private readonly Dictionary<Vector2Int, LetterTile> _letterTiles = new Dictionary<Vector2Int, LetterTile>();
	private readonly Dictionary<Vector2Int, TileBlockedInfo> _blockerTiles = new Dictionary<Vector2Int, TileBlockedInfo>();

	public IEnumerable<Vector2Int> AllLetterTilePositions => _letterTiles.Keys;
	
	public IEnumerable<Vector2Int> AllLetterAndBlockerTilePositions => _blockerTiles.Keys.Union(_letterTiles.Keys);

	public bool HasLetterTile(Vector2Int position) => _letterTiles.ContainsKey(position);

	public LetterTile GetLetterTile(Vector2Int position) => _letterTiles[position];

	public bool IsTileBlocked(Vector2Int position, WordDirection direction)
	{
		if (_blockerTiles.ContainsKey(position))
		{
			return direction == WordDirection.Horizontal
				? _blockerTiles[position].HorizontallyBlocked
				: _blockerTiles[position].VerticallyBlocked;
		}

		return false;
	}

	public void SetWord(Vector2Int position, WordDirection direction, string uppercaseLetters, TileState state, bool alsoSetBlockerTiles)
	{
		var stride = direction.ToStride();
		var sideOffset = new Vector2Int(stride.y, stride.x);
		for (int i = 0; i < uppercaseLetters.Length; i++)
		{
			var tilePosition = position + i * stride;
			char letter = uppercaseLetters[i];
			SetLetterTile(tilePosition, letter, state);
			if (alsoSetBlockerTiles)
			{
				// Block same-direction words along this word and next to it
				bool horizontal = direction == WordDirection.Horizontal;
				bool vertical = direction == WordDirection.Vertical;
				SetBlockerTile(tilePosition, horizontal, vertical);
				SetBlockerTile(tilePosition - sideOffset, horizontal, vertical);
				SetBlockerTile(tilePosition + sideOffset, horizontal, vertical);
			}
		}

		if (alsoSetBlockerTiles)
		{
			// Block in both directions on the end-caps
			SetBlockerTile(position - stride, true, true);
			SetBlockerTile(position + stride * uppercaseLetters.Length, true, true);
		}
	}

	public void FullyClearTile(Vector2Int position)
	{
		_blockerTiles.Remove(position);
		if (_letterTiles.Remove(position))
		{
			LetterTileChanged?.Invoke(position);
		}
	}

	private void SetLetterTile(Vector2Int position, char letter, TileState progress)
	{
		if (HasLetterTile(position))
		{
			var tile = GetLetterTile(position);
			if ((int)tile.Progress < (int)progress)
			{
				tile.Progress = progress;
				_letterTiles[position] = tile;
				LetterTileChanged?.Invoke(position);
			}
		}
		else
		{
			_letterTiles[position] = new LetterTile
			{
				Letter = letter,
				Progress = progress,
			};
			LetterTileChanged?.Invoke(position);
		}
	}

	private void SetBlockerTile(Vector2Int postition, bool horizontal, bool vertical)
	{
		if (_blockerTiles.TryGetValue(postition, out var blockedInfo) == false)
		{
			blockedInfo = new TileBlockedInfo();
		}

		blockedInfo.HorizontallyBlocked |= horizontal;
		blockedInfo.VerticallyBlocked |= vertical;

		_blockerTiles[postition] = blockedInfo;
	}
	
	public struct LetterTile
	{
		public char Letter;
		public TileState Progress;
	}

	private struct TileBlockedInfo
	{
		public bool HorizontallyBlocked;
		public bool VerticallyBlocked;
	}
}
