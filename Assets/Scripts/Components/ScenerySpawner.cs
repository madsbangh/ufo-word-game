using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScenerySpawner : MonoBehaviour
{
	[SerializeField]
	private Transform _sceneryParent;

	[Range(0f, 1f)]
	[SerializeField]
	private float _houseDensity;

	[Header("Scenery Prefabs")]

	[SerializeField]
	private GameObject[] _housePrefabs = new GameObject[0];

	private int _minimumActiveCoordinate, _maximumActiveCoordinate;

	private readonly Dictionary<Vector2Int, GameObject> _spawnedObjects
			= new Dictionary<Vector2Int, GameObject>();

	private WordBoard _wordBoard;
	private int _pastSections, _futureSections;

	public void Initialize(WordBoard wordBoard, int pastSections, int futureSections)
	{
		_wordBoard = wordBoard;
		_pastSections = pastSections;
		_futureSections = futureSections;

		DestroySpawnedScenery();

		_minimumActiveCoordinate = -WordBoardGenerator.SectionStride * pastSections;
		_maximumActiveCoordinate = _minimumActiveCoordinate;
	}

	private void DestroySpawnedScenery()
	{
		foreach (var spawnedObject in _spawnedObjects.Values)
		{
			if (spawnedObject)
			{
				Destroy(spawnedObject.gameObject);
			}
		}

		_spawnedObjects.Clear();
	}

	public void ExpandToSection(int sectionIndex)
	{
		var previousMaximumActiveCoordinate = _maximumActiveCoordinate;

		_maximumActiveCoordinate =
			WordBoardGenerator.SectionStride *
			(sectionIndex + _futureSections) +
			WordBoardGenerator.SectionSize;

		// Expand area below previous active erea
		for (int x = _minimumActiveCoordinate; x < previousMaximumActiveCoordinate; x++)
		{
			for (int y = previousMaximumActiveCoordinate; y < _maximumActiveCoordinate; y++)
			{
				GenerateTile(new Vector2Int(x, y));
			}
		}

		// Expand area to the right of previous and new active area
		for (int x = previousMaximumActiveCoordinate; x < _maximumActiveCoordinate; x++)
		{
			for (int y = _minimumActiveCoordinate; y < _maximumActiveCoordinate; y++)
			{
				GenerateTile(new Vector2Int(x, y));
			}
		}
	}

	public void CleanupBeforeSection(int sectionIndex)
	{
		_minimumActiveCoordinate =
			WordBoardGenerator.SectionStride *
			(sectionIndex - _pastSections);

		foreach (var position in _spawnedObjects.Keys.ToArray())
		{
			if (position.x < _minimumActiveCoordinate ||
				position.y < _minimumActiveCoordinate)
			{
				var objectToDestroy = _spawnedObjects[position];

				if (objectToDestroy)
				{
					Destroy(objectToDestroy);
				}

				_spawnedObjects.Remove(position);
			}
		}
	}

	private void GenerateTile(Vector2Int position)
	{
		if (_wordBoard.HasLetterTile(position))
		{
			return;
		}

		if (Random.value < _houseDensity)
		{
			if (IsNextToLetterTile(position, _wordBoard))
			{
				SpawnHouse(position);
				return;
			}
		}

		// TODO: Spawn trees etc.
	}

	private void SpawnHouse(Vector2Int position)
	{
		var randomIndex = Random.Range(0, _housePrefabs.Length);
		var house = Instantiate(_housePrefabs[randomIndex],
			position.ToWorldPosition(),
			Quaternion.identity,
			_sceneryParent);

		_spawnedObjects.Add(position, house);
	}

	private static bool IsNextToLetterTile(Vector2Int position, WordBoard wordBoard) =>
		wordBoard.HasLetterTile(position + Vector2Int.up) ||
		wordBoard.HasLetterTile(position + Vector2Int.left) ||
		wordBoard.HasLetterTile(position + Vector2Int.right) ||
		wordBoard.HasLetterTile(position + Vector2Int.down);
}
