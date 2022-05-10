using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Components
{
    public class NpcSpawner : MonoBehaviour
    {
        [SerializeField]
        private Npc _npcPrefab;

        [SerializeField]
        private int _minSpawnsPerSection, _maxSpawnsPerSection;

        [SerializeField]
        private Transform _npcsParent;

        private readonly Dictionary<int, HashSet<Npc>> _npcsBySection = new Dictionary<int, HashSet<Npc>>();

        public void SpawnNpcsForSection(int sectionIndex, WordBoard wordBoard)
        {
            if (_npcsBySection.ContainsKey(sectionIndex))
            {
                return;
            }

            var count = Random.Range(_minSpawnsPerSection, _maxSpawnsPerSection + 1);

            var npcs = new HashSet<Npc>();

            foreach (var tile in wordBoard.AllLetterTilePositions
                 .Where(tilePosition => TileIsInSection(tilePosition, sectionIndex))
                 .OrderBy(_ => Random.value)
                 .Take(count))
            {
                npcs.Add(
                    Instantiate(_npcPrefab,
                        tile.ToWorldPosition() +
                        new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f),
                        Quaternion.identity,
                        _npcsParent));
            }

            _npcsBySection.Add(sectionIndex, npcs);
        }

        public HashSet<Npc> PopNpcsInSection(int sectionIndex)
        {
            var npcs = _npcsBySection[sectionIndex];
            _npcsBySection.Remove(sectionIndex);
            return npcs;
        }

        private static bool TileIsInSection(Vector2Int position, int sectionIndex)
        {
            var minimumCoordinate =
                WordBoardGenerator.SectionStride * sectionIndex + WordBoardGenerator.SectionSize / 4;
            var maximumCoordinate = minimumCoordinate + WordBoardGenerator.SectionSize * 3 / 4;

            return position.x >= minimumCoordinate
                   && position.y >= minimumCoordinate
                   && position.x < maximumCoordinate
                   && position.y < maximumCoordinate;
        }
    }
}