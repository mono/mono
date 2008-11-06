// created on 12/6/2002 at 20:29

// Npgsql.NpgsqlRowDescription.cs
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
using NpgsqlTypes;

namespace Npgsql
{
	/// <summary>
	/// This class represents a RowDescription message sent from
	/// the PostgreSQL.
	/// </summary>
	///
	internal abstract class NpgsqlRowDescription : IServerResponseObject
	{
		/// <summary>
		/// This struct represents the internal data of the RowDescription message.
		/// </summary>
		public abstract class FieldData
		{
			private string _name; // Protocol 2/3
			private int _typeOID; // Protocol 2/3
			private short _typeSize; // Protocol 2/3
			private int _typeModifier; // Protocol 2/3
			private int _tableOID; // Protocol 3
			private short _columnAttributeNumber; // Protocol 3
			private FormatCode _formatCode; // Protocol 3. 0 text, 1 binary
			private NpgsqlBackendTypeInfo _typeInfo; // everything we know about this field type

			public string Name
			{
				get { return _name; }
				protected set { _name = value; }
			}

			public int TypeOID
			{
				get { return _typeOID; }
				protected set { _typeOID = value; }
			}

			public short TypeSize
			{
				get { return _typeSize; }
				protected set { _typeSize = value; }
			}

			public int TypeModifier
			{
				get { return _typeModifier; }
				protected set { _typeModifier = value; }
			}

			public int TableOID
			{
				get { return _tableOID; }
				protected set { _tableOID = value; }
			}

			public short ColumnAttributeNumber
			{
				get { return _columnAttributeNumber; }
				protected set { _columnAttributeNumber = value; }
			}

			public FormatCode FormatCode
			{
				get { return _formatCode; }
				protected set { _formatCode = value; }
			}

			public NpgsqlBackendTypeInfo TypeInfo
			{
				get { return _typeInfo; }
				protected set { _typeInfo = value; }
			}
		}

		private readonly FieldData[] fields_data;
		private readonly Dictionary<string, int> field_name_index_table;
		private readonly Dictionary<string, int> caseInsensitiveNameIndexTable;

		protected NpgsqlRowDescription(Stream stream, NpgsqlBackendTypeMapping type_mapping)
		{
			int num = ReadNumFields(stream);
			fields_data = new FieldData[num];
			field_name_index_table = new Dictionary<string, int>(num, StringComparer.InvariantCulture);
			caseInsensitiveNameIndexTable = new Dictionary<string, int>(num, StringComparer.InvariantCultureIgnoreCase);
			for (int i = 0; i != num; ++i)
			{
				FieldData fd = BuildFieldData(stream, type_mapping);
				fields_data[i] = fd;
				if (!field_name_index_table.ContainsKey(fd.Name))
				{
					field_name_index_table.Add(fd.Name, i);
					if (!caseInsensitiveNameIndexTable.ContainsKey(fd.Name))
					{
						caseInsensitiveNameIndexTable.Add(fd.Name, i);
					}
				}
			}
		}

		protected abstract FieldData BuildFieldData(Stream stream, NpgsqlBackendTypeMapping typeMapping);
		protected abstract int ReadNumFields(Stream stream);

		public FieldData this[int index]
		{
			get { return fields_data[index]; }
		}

		public int NumFields
		{
			get { return (Int16) fields_data.Length; }
		}

		public int FieldIndex(String fieldName)
		{
			int ret = -1;
			return
				field_name_index_table.TryGetValue(fieldName, out ret)
					? ret
					: (caseInsensitiveNameIndexTable.TryGetValue(fieldName, out ret) ? ret : -1);
		}
	}

	internal sealed class NpgsqlRowDescriptionV2 : NpgsqlRowDescription
	{
		public NpgsqlRowDescriptionV2(Stream stream, NpgsqlBackendTypeMapping typeMapping)
			: base(stream, typeMapping)
		{
		}

		private sealed class FieldDataV2 : FieldData
		{
			public FieldDataV2(Stream stream, NpgsqlBackendTypeMapping typeMapping)
			{
				Name = PGUtil.ReadString(stream);
				TypeInfo = typeMapping[TypeOID = PGUtil.ReadInt32(stream)];
				TypeSize = PGUtil.ReadInt16(stream);
				TypeModifier = PGUtil.ReadInt32(stream);
			}
		}

		protected override FieldData BuildFieldData(Stream stream, NpgsqlBackendTypeMapping type_mapping)
		{
			return new FieldDataV2(stream, type_mapping);
		}

		protected override int ReadNumFields(Stream stream)
		{
			return PGUtil.ReadInt16(stream);
		}
	}

	internal sealed class NpgsqlRowDescriptionV3 : NpgsqlRowDescription
	{
		private sealed class FieldDataV3 : FieldData
		{
			public FieldDataV3(Stream stream, NpgsqlBackendTypeMapping typeMapping)
			{
				Name = PGUtil.ReadString(stream);
				TableOID = PGUtil.ReadInt32(stream);
				ColumnAttributeNumber = PGUtil.ReadInt16(stream);
				TypeInfo = typeMapping[TypeOID = PGUtil.ReadInt32(stream)];
				TypeSize = PGUtil.ReadInt16(stream);
				TypeModifier = PGUtil.ReadInt32(stream);
				FormatCode = (FormatCode) PGUtil.ReadInt16(stream);
			}
		}

		public NpgsqlRowDescriptionV3(Stream stream, NpgsqlBackendTypeMapping typeMapping)
			: base(stream, typeMapping)
		{
		}

		protected override FieldData BuildFieldData(Stream stream, NpgsqlBackendTypeMapping typeMapping)
		{
			return new FieldDataV3(stream, typeMapping);
		}

		protected override int ReadNumFields(Stream stream)
		{
			PGUtil.EatStreamBytes(stream, 4);
			return PGUtil.ReadInt16(stream);
		}
	}
}