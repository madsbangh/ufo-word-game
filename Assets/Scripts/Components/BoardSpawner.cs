using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Components
{
    public class BoardSpawner : MonoBehaviour
    {
        [SerializeField]
        private Transform _boardParent;

        [SerializeField]
        private LetterTile _letterTilePrefab;

        [SerializeField]
        private float _tileUpdateInterval;
        
        [SerializeField]
        private AudioController _audioController;

        private readonly Dictionary<Vector2Int, LetterTile> _spawnedLetterTiles
            = new Dictionary<Vector2Int, LetterTile>();

        private WordBoard _wordBoard;
		private readonly SortedSet<Vector2Int> _tilesToUpdate = new SortedSet<Vector2Int>(new DiagonalComparer());

        private class DiagonalComparer : IComparer<Vector2Int>
        {
            public int Compare(Vector2Int first, Vector2Int second)
            {
                var xComparison = (first.x + first.y).CompareTo(second.x + second.y);
                return xComparison != 0 ? xComparison : first.y.CompareTo(second.y);
            }
        }

        private IEnumerator Start()
        {
            var waitForInterval = new WaitForSeconds(_tileUpdateInterval);
            var waitForTile = new WaitUntil(_tilesToUpdate.Any);
            while (true)
            {
                yield return waitForInterval;
                yield return waitForTile;

                var position = _tilesToUpdate.Min;
                _tilesToUpdate.Remove(position);
                UpdateOrSpawnLetterTile(_wordBoard, position, true);
            }
        }

        public void Initialize(WordBoard wordBoard)
        {
            _wordBoard = wordBoard;
            wordBoard.LetterTileChanged += WordBoard_TileChanged;

            // Clean up previous world (if any)
            DestroySpawnedBoard();

            // Spawn associated prefabs for all items on the word board
            foreach (var positionToSpawnTileOn in wordBoard.AllLetterTilePositions)
            {
                UpdateOrSpawnLetterTile(wordBoard, positionToSpawnTileOn, false);
            }
        }

        private LetterTile SpawnLetterTile(Vector2Int position)
        {
            var spawnedLetterTile = Instantiate(_letterTilePrefab, _boardParent);
            _spawnedLetterTiles.Add(position, spawnedLetterTile);
            spawnedLetterTile.Position = position;
            return spawnedLetterTile;
        }

        private void UpdateOrSpawnLetterTile(WordBoard wordBoard, Vector2Int position, bool playSounds)
        {
            if (_wordBoard.HasLetterTile(position) == false)
            {
                if (_spawnedLetterTiles.ContainsKey(position))
                {
                    Destroy(_spawnedLetterTiles[position].gameObject);
                }
            }
            else
            {
                var spawnedLetterTile = _spawnedLetterTiles.ContainsKey(position)
                    ? _spawnedLetterTiles[position]
                    : SpawnLetterTile(position);

                var tileData = wordBoard.GetLetterTile(position);
                spawnedLetterTile.Letter = tileData.Letter;
                spawnedLetterTile.State = tileData.Progress;
				if (playSounds && tileData.Progress == TileState.Revealed)
				{
					_audioController.TilePing();
				}
            }
        }

        private void OnDestroy()
        {
            _wordBoard.LetterTileChanged -= WordBoard_TileChanged;

            DestroySpawnedBoard();
        }

        private void DestroySpawnedBoard()
        {
            foreach (var spawnedLetterTile in _spawnedLetterTiles.Values)
            {
                if (spawnedLetterTile)
                {
                    Destroy(spawnedLetterTile.gameObject);
                }
            }

            _spawnedLetterTiles.Clear();
        }

        private void WordBoard_TileChanged(Vector2Int position)
        {
            _tilesToUpdate.Add(position);
        }
    }
}