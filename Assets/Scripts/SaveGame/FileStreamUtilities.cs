using System.IO;
using System.Text;

namespace SaveGame
{
    public static class FileStreamUtilities
    {
        public static BinaryReader MakeReader(string path) =>
            new(File.Open(path, FileMode.OpenOrCreate, FileAccess.Read), Encoding.UTF8, false);

        public static BinaryWriter MakeWriter(string path) =>
            new(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write), Encoding.UTF8, false);
    }
}