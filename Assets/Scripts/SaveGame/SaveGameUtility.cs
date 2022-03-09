using System.IO;
using UnityEngine;

namespace SaveGame
{
	public static class SaveGameUtility
	{
		private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "savegame");

		public static bool SaveFileExists => File.Exists(SaveFilePath);

		public static void Save(params ISerializable[] serializedObjects)
		{
			ReadFromOrWriteToSaveFile(serializedObjects, true);
		}

		public static void Load(params ISerializable[] serializedObjects)
		{
			ReadFromOrWriteToSaveFile(serializedObjects, false);
		}

		private static void ReadFromOrWriteToSaveFile(ISerializable[] serializedObjects, bool write)
		{
			using (var stream = new ReadOrWriteFileStream(SaveFilePath, write))
			{
				foreach (var serialized in serializedObjects)
				{
					serialized.Serialize(stream);
				}
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
