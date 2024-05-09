using System;
using System.IO;
using UnityEngine;

namespace SaveGame
{
    public static class SaveGameUtility
    {
        private static readonly string LegacySaveFilePath0 = Path.Combine(Application.persistentDataPath, "savegame_v0");
        private static readonly string LegacySaveFilePath1 = Path.Combine(Application.persistentDataPath, "savegame_v1");
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "savegame_v2");
        private const int LegacyFileFormatVersion0 = 0;
        private const int LegacyFileFormatVersion1 = 1;
        public const int FileFormatVersion = 2;

        public static bool SaveFileExists => File.Exists(SaveFilePath) || File.Exists(LegacySaveFilePath1) || File.Exists(LegacySaveFilePath0);

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
            else if (File.Exists(LegacySaveFilePath1))
            {
                return new ReadOrWriteFileStream(LegacySaveFilePath1, false, LegacyFileFormatVersion1);
            }
            else if (File.Exists(LegacySaveFilePath0))
            {
                return new ReadOrWriteFileStream(LegacySaveFilePath0, false, LegacyFileFormatVersion0);
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

            if (File.Exists(LegacySaveFilePath0))
            {
                File.Delete(LegacySaveFilePath0);
            }

            if (File.Exists(LegacySaveFilePath1))
            {
                File.Delete(LegacySaveFilePath1);
            }
        }
    }
}
