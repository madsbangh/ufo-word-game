using UnityEngine;

public readonly struct WordPlacement
{
    public readonly Vector2Int Position;
    public readonly WordDirection Direction;

    public WordPlacement(Vector2Int position, WordDirection direction)
    {
        Position = position;
        Direction = direction;
    }
}