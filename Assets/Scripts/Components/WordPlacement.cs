using SaveGame;
using UnityEngine;

public struct WordPlacement : ISerializable
{
	public Vector2Int Position;

	private int _direction;

	public WordDirection Direction
	{
		get => (WordDirection)_direction;
		set => _direction = (int)value;
	}

	public void Serialize(ReadOrWriteFileStream stream)
	{
		stream.Serialize(ref Position);
		stream.Serialize(ref _direction);
	}
}