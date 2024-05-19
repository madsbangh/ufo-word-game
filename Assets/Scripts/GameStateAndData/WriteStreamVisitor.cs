using System;
using System.IO;
using UnityEngine;

namespace GameStateAndData
{
    public class WriteStreamVisitor : ISaveDataVisitor, IDisposable
    {
        private readonly BinaryWriter _writer;

        public WriteStreamVisitor(BinaryWriter writer)
        {
            _writer = writer;
        }

        public void Visit(GameDataQueueField<GameState.Section> dataField)
        {
            _writer.Write(dataField.Items.Count);
            foreach (var section in dataField.Items)
            {
                WriteSection(section);
            }
        }

        public void Visit(GameDataDictionaryField<string, WordPlacement> dataField)
        {
            WriteDictionary(dataField, _writer.Write, WriteWordPlacement);
        }

        public void Visit(GameDataDictionaryField<Vector2Int, WordBoard.LetterTile> dataField)
        {
            WriteDictionary(dataField, WriteVector2Int, WriteLetterTile);
        }

        public void Visit(WordBoard wordBoard)
        {
            wordBoard.Accept(this);
        }

        public void Visit(GameDataDictionaryField<Vector2Int, WordBoard.TileBlockedInfo> dataField)
        {
            WriteDictionary(dataField, WriteVector2Int, WriteTileBlockedInfo);
        }

        public void Visit(GameDataQueueField<string> dataField)
        {
            _writer.Write(dataField.Items.Count);
            foreach (var item in dataField.Items)
            {
                _writer.Write(item);
            }
        }

        public void Visit(GameDataHashSetField<int> dataField)
        {
            _writer.Write(dataField.Items.Count);
            foreach (var item in dataField.Items)
            {
                _writer.Write(item);
            }
        }

        public void Visit(GameDataField<string> dataField)
        {
            _writer.Write(dataField.Value);
        }

        public void Visit(GameDataField<int> dataField)
        {
            _writer.Write(dataField.Value);
        }

        public void Visit(GameDataField<bool> dataField)
        {
            _writer.Write(dataField.Value);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        private void WriteSection(GameState.Section section)
        {
            _writer.Write(section.Letters);
            _writer.Write(section.Words.Count);
            foreach (var (word, placement) in section.Words)
            {
                _writer.Write(word);
                WriteWordPlacement(placement);
            }
        }

        private void WriteWordPlacement(WordPlacement placement)
        {
            WriteVector2Int(placement.Position);
            _writer.Write((int)placement.Direction);
        }

        private void WriteLetterTile(WordBoard.LetterTile letterTile)
        {
            _writer.Write(letterTile.Letter);
            _writer.Write((int)letterTile.Progress);
        }

        private void WriteTileBlockedInfo(WordBoard.TileBlockedInfo blockerTile)
        {
            _writer.Write(blockerTile.HorizontallyBlocked);
            _writer.Write(blockerTile.VerticallyBlocked);
        }

        private void WriteVector2Int(Vector2Int value)
        {
            _writer.Write(value.x);
            _writer.Write(value.y);
        }

        private void WriteDictionary<TKey, TValue>(GameDataDictionaryField<TKey, TValue> dataField,
            Action<TKey> writeKey, Action<TValue> writeValue)
        {
            _writer.Write(dataField.Keys.Count);
            foreach (var key in dataField.Keys)
            {
                writeKey(key);
                writeValue(dataField[key]);
            }
        }
    }
}