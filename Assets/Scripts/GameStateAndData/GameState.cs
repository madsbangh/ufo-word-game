using SectionWords = System.Collections.Generic.IReadOnlyDictionary<string, WordPlacement>;

namespace GameStateAndData
{
    public class ReadOnlyGameState
    {
        private readonly GameState _gameState;

        public ReadOnlyGameState(GameState gameState)
        {
            _gameState = gameState;
        }
        
        public IObservableCollection<GameState.Section> GeneratedFutureSections => _gameState.GeneratedFutureSections;
        public IObservable<string> CurrentSectionLetters => _gameState.CurrentSectionLetters;
        public IObservableDictionary<string, WordPlacement> CurrentSectionWords => _gameState.CurrentSectionWords;
        public IObservable<int> CurrentSectionIndex => _gameState.CurrentSectionIndex;
        public IObservable<int> NewestGeneratedSectionIndex => _gameState.NewestGeneratedSectionIndex;
        public IObservableCollection<string> RecentlyFoundWords => _gameState.RecentlyFoundWords;
        public IObservable<int> Score => _gameState.Score;
        public IObservable<int> BonusHintPoints => _gameState.BonusHintPoints;
        public IObservable<bool> FirstEverWordCompleted => _gameState.FirstEverWordCompleted;
        public IObservable<bool> FirstEverHintUsed => _gameState.FirstEverHintUsed;
        public IObservable<int> SelectedUfoIndex => _gameState.SelectedUfoIndex;
        public IObservableCollection<int> UnlockedUfoIndices => _gameState.UnlockedUfoIndices;
    }

    public class GameState : IDirtiable
    {
        public readonly ReadOnlyGameState Readonly;

        public readonly GameDataQueueField<Section> GeneratedFutureSections;
        public readonly GameDataField<string> CurrentSectionLetters;
        public readonly GameDataDictionaryField<string, WordPlacement> CurrentSectionWords;
        public readonly GameDataField<int> CurrentSectionIndex;
        public readonly GameDataField<int> NewestGeneratedSectionIndex;
        public readonly GameDataQueueField<string> RecentlyFoundWords;
        public readonly GameDataField<int> Score;
        public readonly GameDataField<int> BonusHintPoints;
        public readonly GameDataField<bool> FirstEverWordCompleted;
        public readonly GameDataField<bool> FirstEverHintUsed;
        public readonly GameDataField<int> SelectedUfoIndex;
        public readonly GameDataHashSetField<int> UnlockedUfoIndices;

        public readonly WordBoard WordBoard;

        public bool Dirty { get; set; }

        public GameState()
        {
            Readonly = new ReadOnlyGameState(this);
            
            GeneratedFutureSections = new SectionQueueGameDataField(this);
            CurrentSectionLetters = new StringGameDataField(this);
            CurrentSectionWords = new SectionWordsGameDataField(this);
            CurrentSectionIndex = new IntGameDataField(this);
            NewestGeneratedSectionIndex = new IntGameDataField(this);
            RecentlyFoundWords = new StringQueueGameDataField(this);
            Score = new IntGameDataField(this);
            BonusHintPoints = new IntGameDataField(this);
            FirstEverWordCompleted = new BoolGameDataField(this);
            FirstEverHintUsed = new BoolGameDataField(this);
            SelectedUfoIndex = new IntGameDataField(this);
            UnlockedUfoIndices = new IntHashSetGameDataField(this);

            WordBoard = new WordBoard(this);
        }

        public readonly struct Section
        {
            public readonly string Letters;
            public readonly SectionWords Words;

            public Section(SectionWords words, string letters)
            {
                Words = words;
                Letters = letters;
            }
        }

        public void MarkDirty()
        {
            Dirty = true;
        }
    }
}