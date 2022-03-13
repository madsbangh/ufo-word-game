using System.IO;
using UnityEngine;

namespace SaveGame
{
	public static class SaveGameUtility
	{
		private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "savegame_v0");

		public static bool SaveFileExists => File.Exists(SaveFilePath);

		public static ReadOrWriteFileStream MakeSaveContext()
		{
			return new ReadOrWriteFileStream(SaveFilePath, true);
		}

		public static ReadOrWriteFileStream MakeLoadContext()
		{
			return new ReadOrWriteFileStream(SaveFilePath, false);
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
