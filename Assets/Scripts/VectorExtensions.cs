using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 ToWorldPosition(this Vector2Int boardPosition) =>
		new Vector3(boardPosition.x, 0f, -boardPosition.y);
}