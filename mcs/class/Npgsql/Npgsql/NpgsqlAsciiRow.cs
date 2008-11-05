// created on 13/6/2002 at 21:06

// Npgsql.NpgsqlAsciiRow.cs
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
using System.Data;
using System.IO;
using NpgsqlTypes;

namespace Npgsql
{
	/// <summary>
	/// Implements <see cref="RowReader"/> for version 3 of the protocol.
	/// </summary>
	internal sealed class StringRowReaderV3 : RowReader
	{
		private readonly int _messageSize;
		private int? _nextFieldSize = null;

		public StringRowReaderV3(NpgsqlRowDescription rowDesc, Stream inputStream)
			: base(rowDesc, inputStream)
		{
			_messageSize = PGUtil.ReadInt32(inputStream);
			if (PGUtil.ReadInt16(inputStream) != rowDesc.NumFields)
			{
				throw new DataException();
			}
		}

		protected override object ReadNext()
		{
			int fieldSize = GetThisFieldCount();
			if (fieldSize >= _messageSize)
			{
				AbandonShip();
			}
			_nextFieldSize = null;

			// Check if this field is null
			if (fieldSize == -1) // Null value
			{
				return DBNull.Value;
			}

			NpgsqlRowDescription.FieldData field_descr = FieldData;

			byte[] buffer = new byte[fieldSize];
			PGUtil.CheckedStreamRead(Stream, buffer, 0, fieldSize);

			try
			{
				if (field_descr.FormatCode == FormatCode.Text)
				{
					char[] charBuffer = new char[UTF8Encoding.GetCharCount(buffer, 0, buffer.Length)];
					UTF8Encoding.GetChars(buffer, 0, buffer.Length, charBuffer, 0);
					return
						NpgsqlTypesHelper.ConvertBackendStringToSystemType(field_descr.TypeInfo, new string(charBuffer),
						                                                   field_descr.TypeSize, field_descr.TypeModifier);
				}
				else
				{
					return
						NpgsqlTypesHelper.ConvertBackendBytesToSystemType(field_descr.TypeInfo, buffer, fieldSize,
						                                                  field_descr.TypeModifier);
				}
			}
			catch (InvalidCastException ice)
			{
				return ice;
			}
			catch (Exception ex)
			{
				return new InvalidCastException(ex.Message, ex);
			}
		}

		private void AbandonShip()
		{
			//field size will always be smaller than message size
			//but if we fall out of sync with the stream due to an error then we will probably hit
			//such a situation soon as bytes from elsewhere in the stream get interpreted as a size.
			//so if we see this happens, we know we've lost the stream - our best option is to just give up on it,
			//and have the connector recovered later.
			try
			{
				Stream.WriteByte((byte) FrontEndMessageCode.Termination);
				PGUtil.WriteInt32(Stream, 4);
				Stream.Flush();
			}
			catch
			{
			}
			try
			{
				Stream.Close();
			}
			catch
			{
			}
			throw new DataException();
		}

		protected override void SkipOne()
		{
			int fieldSize = GetThisFieldCount();
			if (fieldSize >= _messageSize)
			{
				AbandonShip();
			}
			_nextFieldSize = null;
			PGUtil.EatStreamBytes(Stream, fieldSize);
		}

		public override bool IsNextDBNull
		{
			get { return GetThisFieldCount() == -1; }
		}

		private int GetThisFieldCount()
		{
			return (_nextFieldSize = _nextFieldSize ?? PGUtil.ReadInt32(Stream)).Value;
		}

		protected override int GetNextFieldCount()
		{
			int ret = GetThisFieldCount();
			_nextFieldSize = null;
			return ret;
		}
	}

	/// <summary>
	/// Implements <see cref="RowReader"/> for version 2 of the protocol.
	/// </summary>
	internal sealed class StringRowReaderV2 : RowReader
	{
		/// <summary>
		/// Encapsulates the null mapping bytes sent at the start of a version 2
		/// datarow message, and the process of identifying the nullity of the data
		/// at a particular index
		/// </summary>
		private sealed class NullMap
		{
			private readonly byte[] _map;

			public NullMap(NpgsqlRowDescription desc, Stream inputStream)
			{
				_map = new byte[(desc.NumFields + 7)/8];
				PGUtil.CheckedStreamRead(inputStream, _map, 0, _map.Length);
			}

			public bool IsNull(int index)
			{
				// Get the byte that holds the bit index position.
				// Then check the bit that in MSB order corresponds
				// to the index position.
				return (_map[index/8] & (0x80 >> (index%8))) == 0;
			}
		}

		private readonly NullMap _nullMap;

		public StringRowReaderV2(NpgsqlRowDescription rowDesc, Stream inputStream)
			: base(rowDesc, inputStream)
		{
			_nullMap = new NullMap(rowDesc, inputStream);
		}

		protected override object ReadNext()
		{
			if (_nullMap.IsNull(CurrentField))
			{
				return DBNull.Value;
			}

			NpgsqlRowDescription.FieldData field_descr = FieldData;
			Int32 field_value_size = PGUtil.ReadInt32(Stream) - 4;
			byte[] buffer = new byte[field_value_size];
			PGUtil.CheckedStreamRead(Stream, buffer, 0, field_value_size);
			char[] charBuffer = new char[UTF8Encoding.GetCharCount(buffer, 0, buffer.Length)];
			UTF8Encoding.GetChars(buffer, 0, buffer.Length, charBuffer, 0);
			try
			{
				return
					NpgsqlTypesHelper.ConvertBackendStringToSystemType(field_descr.TypeInfo, new string(charBuffer),
					                                                   field_descr.TypeSize, field_descr.TypeModifier);
			}
			catch (InvalidCastException ice)
			{
				return ice;
			}
			catch (Exception ex)
			{
				return new InvalidCastException(ex.Message, ex);
			}
		}

		public override bool IsNextDBNull
		{
			get { return _nullMap.IsNull(CurrentField + 1); }
		}

		protected override void SkipOne()
		{
			if (!_nullMap.IsNull(CurrentField))
			{
				PGUtil.EatStreamBytes(Stream, PGUtil.ReadInt32(Stream) - 4);
			}
		}

		protected override int GetNextFieldCount()
		{
			return _nullMap.IsNull(CurrentField) ? -1 : PGUtil.ReadInt32(Stream) - 4;
		}
	}
}