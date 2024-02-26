using System;
using System.IO;
using UnityEngine;

namespace SaveGame
{
    public static class SaveGameUtility
    {
        private static readonly string LegacySaveFilePath = Path.Combine(Application.persistentDataPath, "savegame_v0");
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "savegame_v1");
        private const int LegacyFileFormatVersion = 0;
        private const int FileFormatVersion = 1;

        public static bool SaveFileExists => File.Exists(SaveFilePath) || File.Exists(LegacySaveFilePath);

        public static ReadOrWriteFileStream MakeSaveContext()
        {
            return new ReadOrWriteFileStream(SaveFilePath, true, FileFormatVersion);
        }

        public static ReadOrWriteFileStream MakeLoadContext()
        {
            if (File.Exists(SaveFilePath))
            {
                return new ReadOrWriteFileStream(SaveFilePath, false, FileFormatVersion);
            }
            else if (File.Exists(LegacySaveFilePath))
            {
                return new ReadOrWriteFileStream(LegacySaveFilePath, false, LegacyFileFormatVersion);
            }
            else
            {
                throw new InvalidOperationException("Can not make a load context when no save file exists.");
            }
        }

        public static void DeleteSaveFile()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }
        }
    }
}
