using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 ToWorldPosition(this Vector2Int boardPosition) =>
		new Vector3(boardPosition.x, 0f, -boardPosition.y);

	public static Vector2 ToBoardPosition(this Vector3 worldPosition) =>
		new Vector2(worldPosition.x, -worldPosition.z);
}