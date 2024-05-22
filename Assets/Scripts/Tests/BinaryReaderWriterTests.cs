using System.IO;
using GameStateAndData;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class BinaryReaderWriterTests
    {
        private class DummyDirtiable : IDirtiable
        {
            public void MarkDirty()
            {
                // Ignore
            }
        }

        [Test]
        public void WordBoard_SavedAndLoaded_AreEqual()
        {
            // Setup
            var wordBoard = new WordBoard(new DummyDirtiable());
            wordBoard.SetWord(
                new WordPlacement(
                    new Vector2Int(1, 2),
                    WordDirection.Vertical),
                "TEST",
                TileState.Revealed,
                false);

            // Act
            var stream = new MemoryStream();
            using var writerVisitor = new WriteStreamVisitor(new BinaryWriter(stream));
            writerVisitor.Visit(wordBoard);

            stream.Seek(0, SeekOrigin.Begin);

            using var readerVisitor = new ReadStreamVisitor(new BinaryReader(stream));
            var newWordBoard = new WordBoard(new DummyDirtiable());
            readerVisitor.Visit(newWordBoard);

            // Assert
            Assert.AreEqual(wordBoard.AllLetterTilePositions, newWordBoard.AllLetterTilePositions);
            Assert.AreEqual(wordBoard.AllLetterAndBlockerTilePositions, newWordBoard.AllLetterAndBlockerTilePositions);
            foreach (var position in wordBoard.AllLetterTilePositions)
            {
                Assert.AreEqual(wordBoard.GetLetterTile(position), newWordBoard.GetLetterTile(position));
            }
        }
    }
}