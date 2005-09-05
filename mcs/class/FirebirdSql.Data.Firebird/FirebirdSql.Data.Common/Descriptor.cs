/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.IO;

namespace FirebirdSql.Data.Common
{
	/// <summary>
	/// Descriptor of query input and output parameters.
	/// </summary>
	/// <remarks>
	/// This is similar to the XSQLDA structure described 
	/// in the Interbase 6.0 API docs.
	/// </remarks>
	internal sealed class Descriptor : ICloneable
	{
		#region Fields

		private short version;
		private short count;
		private short actualCount;
		private DbField[] fields;

		#endregion

		#region Properties

		public short Version
		{
			get
			{
				return this.version;
			}
			set
			{
				this.version = value;
			}
		}

		public short Count
		{
			get { return this.count; }
		}

		public short ActualCount
		{
			get { return this.actualCount; }
			set { this.actualCount = value; }
		}

		#endregion

		#region Indexers

		public DbField this[int index]
		{
			get { return this.fields[index]; }
		}

		#endregion

		#region Constructors

		public Descriptor(int count)
		{
			this.version		= IscCodes.SQLDA_VERSION1;
			this.count			= (short)count;
			this.actualCount	= (short)count;
			this.fields			= new DbField[count];

			for (int i = 0; i < this.fields.Length; i++)
			{
				this.fields[i] = new DbField();
			}
		}

		#endregion

		#region ICloneable Methods

		public object Clone()
		{
			Descriptor descriptor = new Descriptor(this.Count);
			descriptor.Version = this.version;

			for (int i = 0; i < descriptor.Count; i++)
			{
				descriptor[i].DataType	= this.fields[i].DataType;
				descriptor[i].NumericScale = this.fields[i].NumericScale;
				descriptor[i].SubType	= this.fields[i].SubType;
				descriptor[i].Length	= this.fields[i].Length;
				descriptor[i].Value		= this.fields[i].Value;
				descriptor[i].NullFlag	= this.fields[i].NullFlag;
				descriptor[i].Name		= this.fields[i].Name;
				descriptor[i].Relation	= this.fields[i].Relation;
				descriptor[i].Owner		= this.fields[i].Owner;
				descriptor[i].Alias		= this.fields[i].Alias;
			}

			return descriptor;
		}

		#endregion

		#region Methods

		public void ResetValues()
		{
			for (int i = 0; i < this.fields.Length; i++)
			{
				this.fields[i].Value = null;
			}
		}

		public byte[] ToBlrArray()
		{
			MemoryStream blr = new MemoryStream();
			int par_count = this.Count * 2;

			blr.WriteByte(IscCodes.blr_version5);
			blr.WriteByte(IscCodes.blr_begin);
			blr.WriteByte(IscCodes.blr_message);
			blr.WriteByte(0);
			blr.WriteByte((byte)(par_count & 255));
			blr.WriteByte((byte)(par_count >> 8));

			for (int i = 0; i < this.fields.Length; i++)
			{
				int dtype = this.fields[i].SqlType;
				int len = this.fields[i].Length;

				switch (dtype)
				{
					case IscCodes.SQL_VARYING:
						blr.WriteByte(IscCodes.blr_varying);
						blr.WriteByte((byte)(len & 255));
						blr.WriteByte((byte)(len >> 8));
						break;

					case IscCodes.SQL_TEXT:
						blr.WriteByte(IscCodes.blr_text);
						blr.WriteByte((byte)(len & 255));
						blr.WriteByte((byte)(len >> 8));
						break;

					case IscCodes.SQL_DOUBLE:
						blr.WriteByte(IscCodes.blr_double);
						break;

					case IscCodes.SQL_FLOAT:
						blr.WriteByte(IscCodes.blr_float);
						break;

					case IscCodes.SQL_D_FLOAT:
						blr.WriteByte(IscCodes.blr_d_float);
						break;

					case IscCodes.SQL_TYPE_DATE:
						blr.WriteByte(IscCodes.blr_sql_date);
						break;

					case IscCodes.SQL_TYPE_TIME:
						blr.WriteByte(IscCodes.blr_sql_time);
						break;

					case IscCodes.SQL_TIMESTAMP:
						blr.WriteByte(IscCodes.blr_timestamp);
						break;

					case IscCodes.SQL_BLOB:
						blr.WriteByte(IscCodes.blr_quad);
						blr.WriteByte(0);
						break;

					case IscCodes.SQL_ARRAY:
						blr.WriteByte(IscCodes.blr_quad);
						blr.WriteByte(0);
						break;

					case IscCodes.SQL_LONG:
						blr.WriteByte(IscCodes.blr_long);
						blr.WriteByte((byte)this.fields[i].NumericScale);
						break;

					case IscCodes.SQL_SHORT:
						blr.WriteByte(IscCodes.blr_short);
						blr.WriteByte((byte)this.fields[i].NumericScale);
						break;

					case IscCodes.SQL_INT64:
						blr.WriteByte(IscCodes.blr_int64);
						blr.WriteByte((byte)this.fields[i].NumericScale);
						break;

					case IscCodes.SQL_QUAD:
						blr.WriteByte(IscCodes.blr_quad);
						blr.WriteByte((byte)this.fields[i].NumericScale);
						break;
				}

				blr.WriteByte(IscCodes.blr_short);
				blr.WriteByte(0);
			}

			blr.WriteByte(IscCodes.blr_end);
			blr.WriteByte(IscCodes.blr_eoc);

			return blr.ToArray();
		}

		#endregion
	}
}