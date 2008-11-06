// created on 4/3/2003 at 19:45

// Npgsql.NpgsqlBinaryRow.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using NpgsqlTypes;

namespace Npgsql
{
	/// <summary>
	/// This is the abstract base class for NpgsqlAsciiRow and NpgsqlBinaryRow.
	/// </summary>
	internal abstract class NpgsqlRow : IStreamOwner
	{
		protected static readonly ResourceManager resman = new ResourceManager(MethodBase.GetCurrentMethod().DeclaringType);
		public abstract object this[int index] { get; }
		public abstract int NumFields { get; }
		public abstract bool IsDBNull(int index);
		public abstract void Dispose();
		public abstract long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length);
		public abstract long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length);
	}

	internal sealed class CachingRow : NpgsqlRow
	{
		private readonly List<object> _data = new List<object>();
		private readonly ForwardsOnlyRow _inner;

		public CachingRow(ForwardsOnlyRow fo)
		{
			_inner = fo;
		}

		public override object this[Int32 index]
		{
			get
			{
				if ((index < 0) || (index >= NumFields))
				{
					throw new IndexOutOfRangeException("this[] index value");
				}
				while (_data.Count <= index)
				{
					_data.Add(_inner[_data.Count]);
				}
				return _data[index];
			}
		}

		public override int NumFields
		{
			get { return _inner.NumFields; }
		}

		public override bool IsDBNull(int index)
		{
			return this[index] == DBNull.Value;
		}

		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			byte[] source = (byte[]) this[i];
			if (buffer == null)
			{
				return source.Length - fieldOffset;
			}
			long finalLength = Math.Max(0, Math.Min(length, source.Length - fieldOffset));
			Array.Copy(source, fieldOffset, buffer, bufferoffset, finalLength);
			return finalLength;
		}

		public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			string source = (string) this[i];
			if (buffer == null)
			{
				return source.Length - fieldoffset;
			}
			long finalLength = Math.Max(0, Math.Min(length, source.Length - fieldoffset));
			Array.Copy(source.ToCharArray(), fieldoffset, buffer, bufferoffset, finalLength);
			return finalLength;
		}

		public override void Dispose()
		{
			_inner.Dispose();
		}
	}

	internal sealed class ForwardsOnlyRow : NpgsqlRow
	{
		private int _lastIndex = -1;
		private readonly RowReader _reader;

		public ForwardsOnlyRow(RowReader reader)
		{
			_reader = reader;
		}

		private void SetIndex(int index, bool allowCurrent)
		{
			if (index < 0 || index >= NumFields)
			{
				throw new IndexOutOfRangeException();
			}
			if (allowCurrent && _reader.CurrentlyStreaming ? index < _lastIndex : index <= _lastIndex)
			{
				throw new InvalidOperationException(
					string.Format(resman.GetString("Row_Sequential_Field_Error"), index, _lastIndex + 1));
			}
			_reader.Skip(index - _lastIndex - 1);
			_lastIndex = index;
		}

		public override object this[int index]
		{
			get
			{
				SetIndex(index, false);
				return _reader.GetNext();
			}
		}

		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			if (buffer == null)
			{
				throw new NotSupportedException();
			}
			if (!_reader.CanGetByteStream(i))
			{
				throw new InvalidCastException();
			}
			SetIndex(i, true);
			_reader.SkipBytesTo(fieldOffset);
			return _reader.Read(buffer, bufferoffset, length);
		}

		public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			if (buffer == null)
			{
				throw new NotSupportedException();
			}
			if (!_reader.CanGetCharStream(i))
			{
				throw new InvalidCastException();
			}
			SetIndex(i, true);
			_reader.SkipCharsTo(fieldoffset);
			return _reader.Read(buffer, bufferoffset, length);
		}

		public override int NumFields
		{
			get { return _reader.NumFields; }
		}

		public override bool IsDBNull(int index)
		{
			if (_lastIndex > -1)
			{
				SetIndex(index - 1, true);
			}
			return _reader.IsNextDBNull;
		}

		public override void Dispose()
		{
			_reader.Dispose();
		}
	}

	/// <summary>
	/// Reads a row, field by field, allowing a DataRow to be built appropriately.
	/// </summary>
	internal abstract class RowReader : IStreamOwner
	{
		/// <summary>
		/// Reads part of a field, as needed (for <see cref="System.Data.IDataRecord.GetChars()"/>
		/// and <see cref="System.Data.IDataRecord.GetBytes()"/>
		/// </summary>
		protected abstract class Streamer : IStreamOwner
		{
			protected readonly Stream _stream;
			protected int _remainingBytes;
			private int _alreadyRead = 0;

			protected Streamer(Stream stream, int remainingBytes)
			{
				_stream = stream;
				_remainingBytes = remainingBytes;
			}

			public int AlreadyRead
			{
				get { return _alreadyRead; }
				protected set { _alreadyRead = value; }
			}

			public void Dispose()
			{
				PGUtil.EatStreamBytes(_stream, _remainingBytes);
			}
		}

		/// <summary>
		/// Adds further functionality to stream that is dependant upon the type of data read.
		/// </summary>
		protected abstract class Streamer<T> : Streamer
		{
			protected Streamer(Stream stream, int remainingBytes)
				: base(stream, remainingBytes)
			{
			}

			public abstract int DoRead(T[] output, int outputIdx, int length);
			public abstract int DoSkip(int length);

			public int Read(T[] output, int outputIdx, int length)
			{
				int ret = DoRead(output, outputIdx, length);
				AlreadyRead += ret;
				return ret;
			}

			private void Skip(int length)
			{
				AlreadyRead += DoSkip(length);
			}

			public void SkipTo(long position)
			{
				if (position < AlreadyRead)
				{
					throw new InvalidOperationException();
				}
				Skip((int) position - AlreadyRead);
			}
		}

		/// <summary>
		/// Completes the implementation of Streamer for char data.
		/// </summary>
		protected sealed class CharStreamer : Streamer<char>
		{
			public CharStreamer(Stream stream, int remainingBytes)
				: base(stream, remainingBytes)
			{
			}

			public override int DoRead(char[] output, int outputIdx, int length)
			{
				return PGUtil.ReadChars(_stream, output, length, ref _remainingBytes, outputIdx);
			}

			public override int DoSkip(int length)
			{
				return PGUtil.SkipChars(_stream, length, ref _remainingBytes);
			}
		}

		/// <summary>
		/// Completes the implementation of Streamer for byte data.
		/// </summary>
		protected sealed class ByteStreamer : Streamer<byte>
		{
			public ByteStreamer(Stream stream, int remainingBytes)
				: base(stream, remainingBytes)
			{
			}

			public override int DoRead(byte[] output, int outputIdx, int length)
			{
				return PGUtil.ReadEscapedBytes(_stream, output, length, ref _remainingBytes, outputIdx);
			}

			public override int DoSkip(int length)
			{
				return PGUtil.SkipEscapedBytes(_stream, length, ref _remainingBytes);
			}
		}

		protected static readonly Encoding UTF8Encoding = Encoding.UTF8;
		private readonly NpgsqlRowDescription _rowDesc;
		private readonly Stream _stream;
		private Streamer _streamer;
		private int _currentField = -1;

		public RowReader(NpgsqlRowDescription rowDesc, Stream stream)
		{
			_rowDesc = rowDesc;
			_stream = stream;
		}

		protected Streamer CurrentStreamer
		{
			get { return _streamer; }
			set
			{
				if (_streamer != null)
				{
					_streamer.Dispose();
				}
				_streamer = value;
			}
		}

		public bool CurrentlyStreaming
		{
			get { return _streamer != null; }
		}

		public bool CanGetByteStream(int index)
		{
//TODO: Add support for byte[] being read as a stream of bytes.
			return _rowDesc[index].TypeInfo.NpgsqlDbType == NpgsqlDbType.Bytea;
		}

		public bool CanGetCharStream(int index)
		{
//TODO: Add support for arrays of string types?
			return _rowDesc[index].TypeInfo.Type.Equals(typeof (string));
		}

		protected Streamer<byte> CurrentByteStreamer
		{
			get
			{
				if (CurrentStreamer == null)
				{
					if (!CanGetByteStream(_currentField + 1))
					{
						throw new InvalidCastException();
					}
					++_currentField;
					return (CurrentStreamer = new ByteStreamer(Stream, GetNextFieldCount())) as ByteStreamer;
				}
				else if (!(CurrentStreamer is Streamer<byte>))
				{
					throw new InvalidOperationException();
				}
				else
				{
					return CurrentStreamer as ByteStreamer;
				}
			}
		}

		protected Streamer<char> CurrentCharStreamer
		{
			get
			{
				if (CurrentStreamer == null)
				{
					if (!CanGetCharStream(_currentField + 1))
					{
						throw new InvalidCastException();
					}
					++_currentField;
					return (CurrentStreamer = new CharStreamer(Stream, GetNextFieldCount())) as CharStreamer;
				}
				else if (!(CurrentStreamer is Streamer<char>))
				{
					throw new InvalidOperationException();
				}
				else
				{
					return CurrentStreamer as CharStreamer;
				}
			}
		}

		protected Stream Stream
		{
			get { return _stream; }
		}

		protected NpgsqlRowDescription.FieldData FieldData
		{
			get { return _rowDesc[_currentField]; }
		}

		public int NumFields
		{
			get { return _rowDesc.NumFields; }
		}

		protected int CurrentField
		{
			get { return _currentField; }
		}

		protected abstract object ReadNext();

		public object GetNext()
		{
			if (++_currentField == _rowDesc.NumFields)
			{
				throw new IndexOutOfRangeException();
			}
			return ReadNext();
		}

		public abstract bool IsNextDBNull { get; }
		protected abstract void SkipOne();

		public void Skip(int count)
		{
			if (count > 0)
			{
				if (_currentField + count >= _rowDesc.NumFields)
				{
					throw new IndexOutOfRangeException();
				}
				while (count-- > 0)
				{
					++_currentField;
					SkipOne();
				}
			}
		}

		protected abstract int GetNextFieldCount();

		public int Read(byte[] output, int outputIdx, int length)
		{
			return CurrentByteStreamer.Read(output, outputIdx, length);
		}

		public void SkipBytesTo(long position)
		{
			CurrentByteStreamer.SkipTo(position);
		}

		public int Read(char[] output, int outputIdx, int length)
		{
			return CurrentCharStreamer.Read(output, outputIdx, length);
		}

		public void SkipCharsTo(long position)
		{
			CurrentCharStreamer.SkipTo(position);
		}

		public void Dispose()
		{
			CurrentStreamer = null;
			Skip(_rowDesc.NumFields - _currentField - 1);
		}
	}
}