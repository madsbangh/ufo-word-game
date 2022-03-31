namespace SaveGame
{
	public interface ISerializable
	{
		void Serialize(ReadOrWriteFileStream stream);
	}
}
