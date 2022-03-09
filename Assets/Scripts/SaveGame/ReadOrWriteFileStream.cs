using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SaveGame
{
	public class ReadOrWriteFileStream : IDisposable
	{
		private FileStream _stream;
		private BinaryReader _reader;
		private BinaryWriter _writer;
		private bool _isWriteMode;

		public bool IsWriteMode => _isWriteMode;

		public ReadOrWriteFileStream(string path, bool isWriteMode)
		{
			_isWriteMode = isWriteMode;

			_stream = File.Open(path, FileMode.OpenOrCreate, _isWriteMode ? FileAccess.Write : FileAccess.Read);
			
			if (isWriteMode)
			{
				_writer = new BinaryWriter(_stream, Encoding.UTF8, false);
			}
			else
			{
				_reader = new BinaryReader(_stream, Encoding.UTF8, false);
			}
		}

		public void Dispose()
		{
			(_isWriteMode ? (IDisposable)_writer : _reader).Dispose();
			_stream.Dispose();
		}

		public void Serialize(ref bool value)
		{
			if (_isWriteMode)
			{
				Write(value);
			}
			else
			{
				value = ReadBoolean();
			}
		}

		public void Serialize(ref byte value)
		{
			if (_isWriteMode)
			{
				Write(value);
			}
			else
			{
				value = ReadByte();
			}
		}

		public void Serialize(ref char value)
		{
			if (_isWriteMode)
			{
				Write(value);
			}
			else
			{
				value = ReadChar();
			}
		}

		public void Serialize(ref int value)
		{
			if (_isWriteMode)
			{
				Write(value);
			}
			else
			{
				value = ReadInt32();
			}
		}

		public void Serialize(ref Vector2Int value)
		{
			if (_isWriteMode)
			{
				Write(value);
			}
			else
			{
				value = ReadVector2Int();
			}
		}

		public void Serialize(ref string value)
		{
			if (_isWriteMode)
			{
				Write(value);
			}
			else
			{
				value = ReadString();
			}
		}

		public void Write(bool value) => _writer.Write(value);
		public void Write(byte value) => _writer.Write(value);
		public void Write(char value) => _writer.Write(value);
		public void Write(int value) => _writer.Write(value);
		public void Write(string value) => _writer.Write(value);
		public void Write(Vector2Int value)
		{
			_writer.Write(value.x);
			_writer.Write(value.y);
		}

		public bool ReadBoolean() => _reader.ReadBoolean();
		public byte ReadByte() => _reader.ReadByte();
		public char ReadChar() => _reader.ReadChar();
		public int ReadInt32() => _reader.ReadInt32();
		public string ReadString() => _reader.ReadString();
		public Vector2Int ReadVector2Int()
		{
			return new Vector2Int(
				_reader.ReadInt32(),
				_reader.ReadInt32());
		}
	}
}