using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameStateAndData
{
    public class ReadStreamVisitor : ISaveDataVisitor, IDisposable
    {
        private readonly BinaryReader _reader;

        public ReadStreamVisitor(BinaryReader reader)
        {
            _reader = reader;
        }

        public void Visit(GameDataField<bool> dataField)
        {
            dataField.Value = _reader.ReadBoolean();
        }

        public void Visit(GameDataField<string> dataField)
        {
            dataField.Value = _reader.ReadString();
        }

        public void Visit(GameDataField<int> dataField)
        {
            dataField.Value = _reader.ReadInt32();
        }

        public void Visit(GameDataHashSetField<int> dataField)
        {
            ReadHashSet(dataField, _reader.ReadInt32);
        }

        public void Visit(GameDataQueueField<string> dataField)
        {
            ReadQueue(dataField, _reader.ReadString);
        }

        public void Visit(GameDataField<GameState.Section> dataField)
        {
            dataField.Value = ReadSection();
        }

        public void Visit(GameDataQueueField<GameState.Section> dataField)
        {
            ReadQueue(dataField, ReadSection);
        }

        public void Visit(GameDataDictionaryField<string, WordPlacement> dataField)
        {
            ReadDictionary(dataField, _reader.ReadString, ReadWordPlacement);
        }

        public void Visit(GameDataDictionaryField<Vector2Int, WordBoard.LetterTile> dataField)
        {
            ReadDictionary(dataField, ReadVector2Int, ReadLetterTile);
        }

        public void Visit(WordBoard wordBoard)
        {
            wordBoard.Accept(this);
        }

        public void Visit(GameDataDictionaryField<Vector2Int, WordBoard.TileBlockedInfo> dataField)
        {
            ReadDictionary(dataField, ReadVector2Int, ReadBlockerTile);
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        private void ReadHashSet<T>(GameDataHashSetField<T> dataField, Func<T> readT)
        {
            var count = _reader.ReadInt32();
            dataField.Clear();
            for (var i = 0; i < count; i++)
            {
                dataField.Add(readT());
            }
        }

        private void ReadQueue<T>(GameDataQueueField<T> dataField, Func<T> readT)
        {
            var count = _reader.ReadInt32();
            dataField.Clear();
            for (var i = 0; i < count; i++)
            {
                dataField.Enqueue(readT());
            }
        }

        private GameState.Section ReadSection()
        {
            var letters = _reader.ReadString();
            var words = ReadDictionary(_reader.ReadString, ReadWordPlacement);
            return new GameState.Section(words, letters);
        }

        private void ReadDictionary<TKey, TValue>(
            GameDataDictionaryField<TKey, TValue> dataField,
            Func<TKey> readKey,
            Func<TValue> readValue)
        {
            var count = _reader.ReadInt32();
            dataField.Clear();
            for (var i = 0; i < count; i++)
            {
                dataField.Add(readKey(), readValue());
            }
        }

        private IReadOnlyDictionary<TKey,TValue> ReadDictionary<TKey, TValue>(Func<TKey> readKey, Func<TValue> readValue)
        {
            var count = _reader.ReadInt32();
            var dictionary = new Dictionary<TKey, TValue>(count);
            for (var i = 0; i < count; i++)
            {
                dictionary.Add(readKey(), readValue());
            }

            return dictionary;
        }

        private WordPlacement ReadWordPlacement()
        {
            return new WordPlacement(
                ReadVector2Int(),
                (WordDirection)_reader.ReadInt32());
        }

        private Vector2Int ReadVector2Int()
        {
            return new Vector2Int(_reader.ReadInt32(), _reader.ReadInt32());
        }

        private WordBoard.TileBlockedInfo ReadBlockerTile()
        {
            return new WordBoard.TileBlockedInfo
            {
                HorizontallyBlocked = _reader.ReadBoolean(),
                VerticallyBlocked = _reader.ReadBoolean()
            };
        }

        private WordBoard.LetterTile ReadLetterTile()
        {
            return new WordBoard.LetterTile
            {
                Letter = _reader.ReadChar(),
                Progress = (TileState)_reader.ReadInt32()
            };
        }
    }
}