using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScenerySpawner : MonoBehaviour
{
	[SerializeField]
	private Transform _sceneryParent;

	[Header("Scenery Prefabs")]

	[SerializeField]
	private PrefabCategory[] _orderedPefabCategories = new PrefabCategory[0];

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

		foreach (var category in _orderedPefabCategories)
		{
			if (Random.value < category.Chance)
			{
				if (!category.MustBeNextToLetterTile || IsNextToLetterTile(position, _wordBoard))
				{
					SpawnPrefab(position, category.Prefabs);
					break;
				}
			}
		}
	}

	private void SpawnPrefab(Vector2Int position, GameObject[] prefabsToChooseFrom)
	{
		var randomIndex = Random.Range(0, prefabsToChooseFrom.Length);
		var spawnedObject = Instantiate(prefabsToChooseFrom[randomIndex],
			position.ToWorldPosition(),
			Quaternion.identity,
			_sceneryParent);

		_spawnedObjects.Add(position, spawnedObject);
	}

	private static bool IsNextToLetterTile(Vector2Int position, WordBoard wordBoard) =>
		wordBoard.HasLetterTile(position + Vector2Int.up) ||
		wordBoard.HasLetterTile(position + Vector2Int.left) ||
		wordBoard.HasLetterTile(position + Vector2Int.right) ||
		wordBoard.HasLetterTile(position + Vector2Int.down);

	[System.Serializable]
	private class PrefabCategory
	{
		public string Name = string.Empty;
		public GameObject[] Prefabs = new GameObject[0];
		[Range(0f, 1f)]
		public float Chance;
		public bool MustBeNextToLetterTile;
	}
}
