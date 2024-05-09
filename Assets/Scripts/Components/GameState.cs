using System;
using System.Collections.Generic;
using SaveGame;
using SectionWords = System.Collections.Generic.Dictionary<string, WordPlacement>;

namespace Components
{
    public interface IReadOnlyGameState
    {
        int Score { get; }
        IReadOnlySet<int> UnlockedUfos { get; }
        int SelectedUfo { get; }

        event Action<int> ScoreChanged;
    }

    public partial class GameController
    {
        public IReadOnlyGameState ReadOnlyGameState => _gameState;

        private class GameState : ISerializable, IReadOnlyGameState
        {
            public Queue<Section> GeneratedFutureSections;
            public SectionWords CurrentSectionWords;
            public int CurrentSectionIndex;
            public int NewestGeneratedSectionIndex;
            public string CurrentSectionLetters;
            public Queue<string> RecentlyFoundWords;
            public int BonusHintPoints;
            public bool FirstEverWordCompleted;
            public bool FirstEverHintUsed;

            private int _score;

            public int Score
            {
                get => _score;
                set
                {
                    _score = value;
                    ScoreChanged?.Invoke(value);
                }
            }

            private int _selectedUfo;
            public int SelectedUfo { get => _selectedUfo; set => _selectedUfo = value; }
            public HashSet<int> UnlockedUfosInternal;

            public IReadOnlySet<int> UnlockedUfos => new ReadOnlyHashSet<int>(UnlockedUfosInternal);

            public event Action<int> ScoreChanged;

            public void Serialize(ReadOrWriteFileStream stream)
            {
                stream.Visit(ref CurrentSectionIndex);
                stream.Visit(ref NewestGeneratedSectionIndex);
                stream.Visit(ref CurrentSectionLetters);
                stream.Visit(ref CurrentSectionWords);
                stream.Visit(ref GeneratedFutureSections);
                stream.Visit(ref _score);
                stream.Visit(ref RecentlyFoundWords);
                stream.Visit(ref BonusHintPoints);

                // Version 1.1.1 and below
                if (stream.FileFormatVersion < 1)
                {
                    return;
                }

                stream.Visit(ref FirstEverWordCompleted);
                stream.Visit(ref FirstEverHintUsed);

                // Version 1.1.2 and below
                if (stream.FileFormatVersion < 2)
                {
                    return;
                }

                stream.Visit(ref _selectedUfo);
                stream.Visit(ref UnlockedUfosInternal);
            }

            internal void NotifyLoaded()
            {
                // Notify all listeners
                ScoreChanged?.Invoke(_score);
            }
        }
    }

    public interface IReadOnlySet<T>
    {
        bool Contains(T item);
    }

    public class ReadOnlyHashSet<T> : IReadOnlySet<T>
    {
        private readonly HashSet<T> _hashSet;

        public ReadOnlyHashSet(HashSet<T> hashSet)
        {
            _hashSet = hashSet;
        }

        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }
    }
}