using UnityEngine;
using SectionWords = System.Collections.Generic.IReadOnlyDictionary<string, WordPlacement>;

namespace GameStateAndData
{
    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataField<bool> dataField);
    }

    public class BoolGameDataField : GameDataField<bool>
    {
        public BoolGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataField<int> dataField);
    }

    public class IntGameDataField : GameDataField<int>
    {
        public IntGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataField<string> dataField);
    }

    public class StringGameDataField : GameDataField<string>
    {
        public StringGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataHashSetField<int> dataField);
    }

    public class IntHashSetGameDataField : GameDataHashSetField<int>
    {
        public IntHashSetGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataQueueField<string> dataField);
    }

    public class StringQueueGameDataField : GameDataQueueField<string>
    {
        public StringQueueGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataDictionaryField<string, WordPlacement> dataField);
    }

    public class SectionWordsGameDataField : GameDataDictionaryField<string, WordPlacement>
    {
        public SectionWordsGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataQueueField<GameState.Section> dataField);
    }

    public class SectionQueueGameDataField : GameDataQueueField<GameState.Section>
    {
        public SectionQueueGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    
    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataDictionaryField<Vector2Int, WordBoard.LetterTile> dataField);
    }

    public class LetterTilesGameDataField : GameDataDictionaryField<Vector2Int, WordBoard.LetterTile>
    {
        public LetterTilesGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    
    public partial interface ISaveDataVisitor
    {
        void Visit(GameDataDictionaryField<Vector2Int, WordBoard.TileBlockedInfo> dataField);
    }

    public class BlockerTilesGameDataField : GameDataDictionaryField<Vector2Int, WordBoard.TileBlockedInfo>
    {
        public BlockerTilesGameDataField(IDirtiable dirtyWhenChanged) : base(dirtyWhenChanged)
        {
        }

        public override void Accept(ISaveDataVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}