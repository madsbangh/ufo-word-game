using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    public class BoardSpawner : MonoBehaviour
    {
        [SerializeField]
        private Transform _boardParent;

        [SerializeField]
        private LetterTile _letterTilePrefab;

        private readonly Dictionary<Vector2Int, LetterTile> _spawnedLetterTiles
            = new Dictionary<Vector2Int, LetterTile>();

        private WordBoard _wordBoard;

        public void Initialize(WordBoard wordBoard)
        {
            _wordBoard = wordBoard;
            wordBoard.LetterTileChanged += WordBoard_TileChanged;

            // Clean up previous world (if any)
            DestroySpawnedWorld();

            // Spawn associated prefabs for all items on the word board
            foreach (var positionToSpawnTileOn in wordBoard.AllLetterTilePositions)
            {
                UpdateOrSpawnLetterTile(wordBoard, positionToSpawnTileOn);
            }
        }

        private LetterTile SpawnLetterTile(Vector2Int position)
        {
            var spawnedLetterTile = Instantiate(_letterTilePrefab, _boardParent);
            _spawnedLetterTiles.Add(position, spawnedLetterTile);
            spawnedLetterTile.Position = position;
            return spawnedLetterTile;
        }

        private void UpdateOrSpawnLetterTile(WordBoard wordBoard, Vector2Int position)
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
            }
        }

        private void OnDestroy()
        {
            _wordBoard.LetterTileChanged -= WordBoard_TileChanged;

            DestroySpawnedWorld();
        }

        private void DestroySpawnedWorld()
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
            UpdateOrSpawnLetterTile(_wordBoard, position);
        }
    }
}