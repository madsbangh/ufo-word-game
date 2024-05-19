using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameStateAndData;
using UnityEngine;

namespace SaveGame
{
    public static class SaveGameUtility
    {
        private static readonly GameLoaderSaver GameLoaderSaver = new();

        private static readonly List<IGameLoader> LoadersToTry = new()
        {
            GameLoaderSaver,
            new GameLoaderLegacyV2(),
            new GameLoaderLegacyV1()
        };

        public static bool TryLoadGame(out GameState gameState)
        {
            foreach (var loader in LoadersToTry.Where(loader => loader.CanLoadGame))
            {
                gameState = loader.LoadGame();
                return true;
            }

            gameState = default;
            return false;
        }

        public static void SaveGame(GameState gameState)
        {
            GameLoaderSaver.SaveGame(gameState);
        }

        public static void DeleteSaveFile()
        {
            foreach (var loader in LoadersToTry)
            {
                loader.DeleteGame();
            }
        }

        internal static void ApplyVisitorToLegacyVersion1AndAbove(ISaveDataVisitor visitor, GameState gameState)
        {
            visitor.Visit(gameState.CurrentSectionIndex);
            visitor.Visit(gameState.NewestGeneratedSectionIndex);
            visitor.Visit(gameState.CurrentSectionLetters);
            visitor.Visit(gameState.CurrentSectionWords);
            visitor.Visit(gameState.GeneratedFutureSections);
            visitor.Visit(gameState.Score);
            visitor.Visit(gameState.RecentlyFoundWords);
            visitor.Visit(gameState.BonusHintPoints);
        }

        internal static void ApplyVisitorToLegacyVersion2AndAbove(ISaveDataVisitor visitor, GameState gameState)
        {
            visitor.Visit(gameState.FirstEverWordCompleted);
            visitor.Visit(gameState.FirstEverHintUsed);
        }

        internal static void ApplyVisitorToNewestVersionAndAbove(ISaveDataVisitor visitor, GameState gameState)
        {
            visitor.Visit(gameState.SelectedUfoIndex);
            visitor.Visit(gameState.UnlockedUfoIndices);
        }

        internal static void ApplyVisitorToWordBoard(ISaveDataVisitor visitor, GameState gameState)
        {
            visitor.Visit(gameState.WordBoard);
        }

        public static GameState CreateNewGameState()
        {
            var newGame = new GameState
            {
                CurrentSectionIndex =
                {
                    Value = -1
                },
                NewestGeneratedSectionIndex =
                {
                    Value = -1
                },
                SelectedUfoIndex =
                {
                    Value = 0
                }
            };

            newGame.UnlockedUfoIndices.Add(0);
            return newGame;
        }
    }

    internal class GameLoaderSaver : IGameLoader
    {
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "savegame_v2");

        public bool CanLoadGame => File.Exists(SaveFilePath);

        public GameState LoadGame()
        {
            using var readerVisitor = new ReadStreamVisitor(FileStreamUtilities.MakeReader(SaveFilePath));
            var gameState = SaveGameUtility.CreateNewGameState();
            ApplyVisitorToGame(readerVisitor, gameState);
            gameState.Dirty = false;
            return gameState;
        }

        public void SaveGame(GameState gameState)
        {
            using var writerVisitor = new WriteStreamVisitor(FileStreamUtilities.MakeWriter(SaveFilePath));
            ApplyVisitorToGame(writerVisitor, gameState);
        }

        private static void ApplyVisitorToGame(ISaveDataVisitor visitor, GameState gameState)
        {
            SaveGameUtility.ApplyVisitorToLegacyVersion1AndAbove(visitor, gameState);
            SaveGameUtility.ApplyVisitorToLegacyVersion2AndAbove(visitor, gameState);
            SaveGameUtility.ApplyVisitorToNewestVersionAndAbove(visitor, gameState);
            SaveGameUtility.ApplyVisitorToWordBoard(visitor, gameState);
        }

        public void DeleteGame()
        {
            File.Delete(SaveFilePath);
        }
    }

    internal class GameLoaderLegacyV2 : IGameLoader
    {
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "savegame_v1");

        public bool CanLoadGame => File.Exists(SaveFilePath);

        public GameState LoadGame()
        {
            using var readerVisitor = new ReadStreamVisitor(FileStreamUtilities.MakeReader(SaveFilePath));
            var gameState = SaveGameUtility.CreateNewGameState();
            SaveGameUtility.ApplyVisitorToLegacyVersion1AndAbove(readerVisitor, gameState);
            SaveGameUtility.ApplyVisitorToLegacyVersion2AndAbove(readerVisitor, gameState);
            SaveGameUtility.ApplyVisitorToWordBoard(readerVisitor, gameState);
            gameState.Dirty = false;
            return gameState;
        }

        public void DeleteGame()
        {
            File.Delete(SaveFilePath);
        }
    }

    internal class GameLoaderLegacyV1 : IGameLoader
    {
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "savegame_v0");

        public bool CanLoadGame => File.Exists(SaveFilePath);

        public GameState LoadGame()
        {
            using var readerVisitor = new ReadStreamVisitor(FileStreamUtilities.MakeReader(SaveFilePath));
            var gameState = SaveGameUtility.CreateNewGameState();
            SaveGameUtility.ApplyVisitorToLegacyVersion1AndAbove(readerVisitor, gameState);
            SaveGameUtility.ApplyVisitorToWordBoard(readerVisitor, gameState);
            gameState.Dirty = false;
            return gameState;
        }

        public void DeleteGame()
        {
            File.Delete(SaveFilePath);
        }
    }

    public interface IGameLoader
    {
        bool CanLoadGame { get; }

        GameState LoadGame();

        void DeleteGame();
    }
}