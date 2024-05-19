using System;
using System.Collections.Generic;
using System.Linq;
using GameStateAndData;
using UnityEngine;

namespace GameStateAndData
{
    public partial interface ISaveDataVisitor
    {
        void Visit(WordBoard wordBoard);
    }
}

public interface IObservableWordBoard
{
    event LetterTileChangedHandler LetterTileChanged;
}

public delegate void LetterTileChangedHandler(Vector2Int position);

public class WordBoard : IObservableWordBoard, ISaveDataVisitable
{
    public event LetterTileChangedHandler LetterTileChanged;

    private readonly GameDataDictionaryField<Vector2Int, LetterTile> _letterTiles;
    private readonly GameDataDictionaryField<Vector2Int, TileBlockedInfo> _blockerTiles;

    public IEnumerable<Vector2Int> AllLetterTilePositions => _letterTiles.Keys;

    public IEnumerable<Vector2Int> AllLetterAndBlockerTilePositions => _blockerTiles.Keys.Union(_letterTiles.Keys);

    public bool HasLetterTile(Vector2Int position) => _letterTiles.ContainsKey(position);

    public LetterTile GetLetterTile(Vector2Int position) => _letterTiles[position];

    public WordBoard(IDirtiable dirtyWhenChanged)
    {
        _letterTiles = new LetterTilesGameDataField(dirtyWhenChanged);
        _blockerTiles = new BlockerTilesGameDataField(dirtyWhenChanged);
    }

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

    public void SetWord(WordPlacement placement, string uppercaseLetters, TileState state, bool alsoSetBlockerTiles)
    {
        var stride = placement.Direction.ToStride();
        var sideOffset = new Vector2Int(stride.y, stride.x);
        for (int i = 0; i < uppercaseLetters.Length; i++)
        {
            var tilePosition = placement.Position + i * stride;
            char letter = uppercaseLetters[i];
            SetLetterTile(tilePosition, letter, state);
            if (alsoSetBlockerTiles)
            {
                // Block same-direction words along this word and next to it
                bool horizontal = placement.Direction == WordDirection.Horizontal;
                bool vertical = placement.Direction == WordDirection.Vertical;
                SetBlockerTile(tilePosition, horizontal, vertical);
                SetBlockerTile(tilePosition - sideOffset, horizontal, vertical);
                SetBlockerTile(tilePosition + sideOffset, horizontal, vertical);
            }
        }

        if (alsoSetBlockerTiles)
        {
            // Block in both directions on the end-caps
            SetBlockerTile(placement.Position - stride, true, true);
            SetBlockerTile(placement.Position + stride * uppercaseLetters.Length, true, true);
        }
    }

    public void RevealTile(Vector2Int position)
    {
        if (HasLetterTile(position))
        {
            var tile = GetLetterTile(position);
            SetLetterTile(position, tile.Letter, TileState.Revealed);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(position), "No tile to reveal at the given position.");
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

    // public void Accept(Visitor stream)
    // {
    // 	stream.Visit(ref _blockerTiles);
    // 	stream.Visit(ref _letterTiles);
    // }

    public struct LetterTile
    {
        public char Letter;
        public TileState Progress;
    }

    public struct TileBlockedInfo
    {
        public bool HorizontallyBlocked;
        public bool VerticallyBlocked;
    }

    public void Accept(ISaveDataVisitor visitor)
    {
        visitor.Visit(_letterTiles);
        visitor.Visit(_blockerTiles);
    }
}