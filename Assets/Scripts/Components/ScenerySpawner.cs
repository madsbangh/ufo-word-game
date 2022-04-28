using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Components
{
	public class ScenerySpawner : MonoBehaviour
	{
		[SerializeField]
		private Transform _sceneryParent;

		[Header("Scenery Prefabs")]
		[SerializeField]
		private PrefabCategory[] _orderedPrefabCategories = Array.Empty<PrefabCategory>();

		private readonly Dictionary<Vector2Int, GameObject> _spawnedObjects
			= new Dictionary<Vector2Int, GameObject>();


		private WordBoard _wordBoard;
		private int _previousWindowCoordinateMin;
		private int _previousWindowCoordinateMax;
		private int _windowPadding;

		public void Initialize(WordBoard wordBoard, int windowPadding)
		{
			_wordBoard = wordBoard;
			_windowPadding = windowPadding;
			
			DestroySpawnedScenery();
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

		public void SetSection(int sectionIndex)
		{
			var minimumCoordinate = WordBoardGenerator.SectionStride * sectionIndex -
			                        _windowPadding;
			var maximumCoordinate = WordBoardGenerator.SectionStride * sectionIndex +
			                        WordBoardGenerator.SectionSize + _windowPadding;

			// Spawn new objects in new window excluding old window
			for (var x = minimumCoordinate; x <= maximumCoordinate; x++)
			{
				for (var y = minimumCoordinate; y <= maximumCoordinate; y++)
				{
					if (x > _previousWindowCoordinateMax ||
					    y > _previousWindowCoordinateMax)
					{
						GenerateTile(new Vector2Int(x, y));
					}
				}
			}
			
			// Cleanup objects outside of window
			foreach (var position in _spawnedObjects.Keys.ToArray())
			{
				if (position.x < minimumCoordinate ||
				    position.y < minimumCoordinate)
				{
					var objectToDestroy = _spawnedObjects[position];

					if (objectToDestroy)
					{
						Destroy(objectToDestroy);
					}

					_spawnedObjects.Remove(position);
				}
			}

			_previousWindowCoordinateMin = minimumCoordinate;
			_previousWindowCoordinateMax = maximumCoordinate;
		}

		private void GenerateTile(Vector2Int position)
		{
			if (_wordBoard.HasLetterTile(position))
			{
				return;
			}

			foreach (var category in _orderedPrefabCategories)
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


		[Serializable]
		private class PrefabCategory
		{
			public string Name = string.Empty;
			public GameObject[] Prefabs = Array.Empty<GameObject>();

			[Range(0f, 1f)]
			public float Chance;

			public bool MustBeNextToLetterTile;
		}
	}
}