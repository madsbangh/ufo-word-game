using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SaveGame
{
	public class ReadOrWriteFileStream : IDisposable
	{
		private readonly FileStream _stream;
		private readonly BinaryReader _reader;
		private readonly BinaryWriter _writer;
		private readonly bool _isWriteMode;
		private readonly int _fileFormatVersion;

		public ReadOrWriteFileStream(string path, bool isWriteMode, int fileFormatVersion)
		{
			_isWriteMode = isWriteMode;
			_fileFormatVersion = fileFormatVersion;

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

		public int FileFormatVersion => _fileFormatVersion;

		public void Visit(ref bool value)
		{
			if (_isWriteMode) _writer.Write(value);
			else value = _reader.ReadBoolean();
		}

		public void Visit(ref byte value)
		{
			if (_isWriteMode) _writer.Write(value);
			else value = _reader.ReadByte();
		}

		public void Visit(ref char value)
		{
			if (_isWriteMode) _writer.Write(value);
			else value = _reader.ReadChar();
		}

		public void Visit(ref int value)
		{
			if (_isWriteMode) _writer.Write(value);
			else value = _reader.ReadInt32();
		}

		public void Visit(ref Vector2Int value)
		{
			if (_isWriteMode) WriteVector2Int(value);
			else value = ReadVector2Int();
		}

		public void Visit(ref string value)
		{
			if (_isWriteMode) _writer.Write(value ?? string.Empty);
			else value = _reader.ReadString();
		}

		public void Visit(ref HashSet<bool> set) =>
			Visit(ref set, ReadBoolean, WriteBoolean);

		public void Visit(ref HashSet<byte> set) =>
			Visit(ref set, ReadByte, WriteByte);

		public void Visit(ref HashSet<char> set) =>
			Visit(ref set, ReadChar, WriteChar);

		public void Visit(ref HashSet<int> set) =>
			Visit(ref set, ReadInt, WriteInt);

		public void Visit(ref HashSet<Vector2Int> set) =>
			Visit(ref set, ReadVector2Int, WriteVector2Int);

		public void Visit(ref HashSet<string> set) =>
			Visit(ref set, ReadString, WriteString);

		private void Visit<T>(ref HashSet<T> set, Func<T> readHandler, Action<T> writeHandler)
		{
			if (_isWriteMode)
			{
				_writer.Write(set.Count);
				foreach (var item in set)
				{
					writeHandler(item);
				}
			}
			else
			{
				var count = _reader.ReadInt32();
				set = new HashSet<T>();
				for (int i = 0; i < count; i++)
				{
					set.Add(readHandler());
				}
			}
		}

		// * to Vector2Int
		public void Visit(ref Dictionary<bool, Vector2Int> dictionary)
			=> Visit(ref dictionary, ReadBoolean, WriteBoolean, ReadVector2Int, WriteVector2Int);

		public void Visit(ref Dictionary<byte, Vector2Int> dictionary)
			=> Visit(ref dictionary, ReadByte, WriteByte, ReadVector2Int, WriteVector2Int);

		public void Visit(ref Dictionary<char, Vector2Int> dictionary)
			=> Visit(ref dictionary, ReadChar, WriteChar, ReadVector2Int, WriteVector2Int);

		public void Visit(ref Dictionary<int, Vector2Int> dictionary)
			=> Visit(ref dictionary, ReadInt, WriteInt, ReadVector2Int, WriteVector2Int);

		public void Visit(ref Dictionary<Vector2Int, Vector2Int> dictionary)
			=> Visit(ref dictionary, ReadVector2Int, WriteVector2Int, ReadVector2Int, WriteVector2Int);

		public void Visit(ref Dictionary<string, Vector2Int> dictionary)
			=> Visit(ref dictionary, ReadString, WriteString, ReadVector2Int, WriteVector2Int);

		// * to string
		public void Visit(ref Dictionary<bool, string> collection)
			=> Visit(ref collection, ReadBoolean, WriteBoolean, ReadString, WriteString);

		public void Visit(ref Dictionary<byte, string> dictionary)
			=> Visit(ref dictionary, ReadByte, WriteByte, ReadString, WriteString);

		public void Visit(ref Dictionary<char, string> dictionary)
			=> Visit(ref dictionary, ReadChar, WriteChar, ReadString, WriteString);

		public void Visit(ref Dictionary<int, string> dictionary)
			=> Visit(ref dictionary, ReadInt, WriteInt, ReadString, WriteString);

		public void Visit(ref Dictionary<Vector2Int, string> dictionary)
			=> Visit(ref dictionary, ReadVector2Int, WriteVector2Int, ReadString, WriteString);

		public void Visit(ref Dictionary<string, string> dictionary)
			=> Visit(ref dictionary, ReadString, WriteString, ReadString, WriteString);

		// * to Serializable
		public void Visit<T>(ref Dictionary<bool, T> dictionary) where T : ISerializable, new()
			=> Visit(ref dictionary, ReadBoolean, WriteBoolean, ReadSerializable<T>, WriteSerializable);

		public void Visit<T>(ref Dictionary<byte, T> dictionary) where T : ISerializable, new()
			=> Visit(ref dictionary, ReadByte, WriteByte, ReadSerializable<T>, WriteSerializable);

		public void Visit<T>(ref Dictionary<char, T> dictionary) where T : ISerializable, new()
			=> Visit(ref dictionary, ReadChar, WriteChar, ReadSerializable<T>, WriteSerializable);

		public void Visit<T>(ref Dictionary<int, T> dictionary) where T : ISerializable, new()
			=> Visit(ref dictionary, ReadInt, WriteInt, ReadSerializable<T>, WriteSerializable);

		public void Visit<T>(ref Dictionary<Vector2Int, T> dictionary) where T : ISerializable, new()
			=> Visit(ref dictionary, ReadVector2Int, WriteVector2Int, ReadSerializable<T>, WriteSerializable);

		public void Visit<T>(ref Dictionary<string, T> dictionary) where T : ISerializable, new()
			=> Visit(ref dictionary, ReadString, WriteString, ReadSerializable<T>, WriteSerializable);

		public void Visit(ref Queue<bool> queue) =>
			Visit(ref queue, ReadBoolean, WriteBoolean);

		public void Visit(ref Queue<byte> queue) =>
			Visit(ref queue, ReadByte, WriteByte);

		public void Visit(ref Queue<char> queue) =>
			Visit(ref queue, ReadChar, WriteChar);

		public void Visit(ref Queue<int> queue) =>
			Visit(ref queue, ReadInt, WriteInt);

		public void Visit(ref Queue<Vector2Int> queue) =>
			Visit(ref queue, ReadVector2Int, WriteVector2Int);

		public void Visit(ref Queue<string> queue) =>
			Visit(ref queue, ReadString, WriteString);
		
		private void Visit<T>(ref Queue<T> queue, Func<T> readHandler, Action<T> writeHandler)
		{
			if (_isWriteMode)
			{
				_writer.Write(queue.Count);
				foreach (var item in queue)
				{
					writeHandler(item);
				}
			}
			else
			{
				var count = _reader.ReadInt32();
				queue = new Queue<T>(count);
				for (int i = 0; i < count; i++)
				{
					queue.Enqueue(readHandler());
				}
			}
		}
		
		private void Visit<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary,
			Func<TKey> keyReadHandler, Action<TKey> keyWriteHandler,
			Func<TValue> valueReadHandler, Action<TValue> valueWriteHandler)
		{
			if (_isWriteMode)
			{
				if (dictionary == null)
				{
					_writer.Write(0);
					return;
				}
				_writer.Write(dictionary.Count);
				foreach (var pair in dictionary)
				{
					keyWriteHandler(pair.Key);
					valueWriteHandler(pair.Value);
				}
			}
			else
			{
				var count = _reader.ReadInt32();
				dictionary = new Dictionary<TKey, TValue>(count);
				for (int i = 0; i < count; i++)
				{
					dictionary.Add(
						keyReadHandler(),
						valueReadHandler());
				}
			}
		}

		public void Visit<T>(ref HashSet<T> set) where T : ISerializable, new()
		{
			if (_isWriteMode)
			{
				if (set == null)
				{
					_writer.Write(0);
					return;
				}
				_writer.Write(set.Count);
				foreach (var item in set)
				{
					WriteSerializable(item);
				}
			}
			else
			{
				var count = _reader.ReadInt32();
				set = new HashSet<T>();
				for (int i = 0; i < count; i++)
				{
					set.Add(ReadSerializable<T>());
				}
			}
		}

		public void Visit<T>(ref Queue<T> queue) where T : ISerializable, new()
		{
			if (_isWriteMode)
			{
				if (queue == null)
				{
					_writer.Write(0);
					return;
				}
				_writer.Write(queue.Count);
				foreach (var item in queue)
				{
					WriteSerializable(item);
				}
			}
			else
			{
				var count = _reader.ReadInt32();
				queue = new Queue<T>(count);
				for (int i = 0; i < count; i++)
				{
					queue.Enqueue(ReadSerializable<T>());
				}
			}
		}

		private bool ReadBoolean()
		{
			Debug.Assert(!_isWriteMode, "The stream is in write mode.");
			return _reader.ReadBoolean();
		}

		private void WriteBoolean(bool value)
		{
			Debug.Assert(_isWriteMode, "The stream is not in write mode.");
			_writer?.Write(value);
		}

		private byte ReadByte()
		{
			Debug.Assert(!_isWriteMode, "The stream is in write mode.");
			return _reader.ReadByte();
		}

		private void WriteByte(byte value)
		{
			Debug.Assert(_isWriteMode, "The stream is not in write mode.");
			_writer?.Write(value);
		}

		private char ReadChar()
		{
			Debug.Assert(!_isWriteMode, "The stream is in write mode.");
			return _reader.ReadChar();
		}

		private void WriteChar(char value)
		{
			Debug.Assert(_isWriteMode, "The stream is not in write mode.");
			_writer?.Write(value);
		}

		private int ReadInt()
		{
			Debug.Assert(!_isWriteMode, "The stream is in write mode.");
			return _reader.ReadInt32();
		}

		private void WriteInt(int value)
		{
			Debug.Assert(_isWriteMode, "The stream is not in write mode.");
			_writer?.Write(value);
		}

		private Vector2Int ReadVector2Int()
		{
			return new Vector2Int
			{
				x = _reader.ReadInt32(),
				y = _reader.ReadInt32()
			};
		}

		private void WriteVector2Int(Vector2Int value)
		{
			_writer.Write(value.x);
			_writer.Write(value.y);
		}

		private string ReadString()
		{
			Debug.Assert(!_isWriteMode, "The stream is in write mode.");
			return _reader.ReadString();
		}

		private void WriteString(string value)
		{
			Debug.Assert(_isWriteMode, "The stream is not in write mode.");
			_writer?.Write(value ?? string.Empty);
		}

		private T ReadSerializable<T>() where T : ISerializable, new()
		{
			Debug.Assert(!_isWriteMode, "The stream is in write mode.");
			var result = new T();
			result.Serialize(this);
			return result;
		}

		private void WriteSerializable<T>(T serializable) where T : ISerializable
		{
			Debug.Assert(_isWriteMode, "The stream is not in write mode.");
			serializable.Serialize(this);
		}
	}
}