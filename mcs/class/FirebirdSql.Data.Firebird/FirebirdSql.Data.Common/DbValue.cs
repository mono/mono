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
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	internal sealed class DbValue
	{
		#region Fields

		private StatementBase	statement;
		private DbField			field;
		private object			value;

		#endregion

		#region Properties

		public DbField Field
		{
			get { return this.field; }
		}

		public object Value
		{
			get { return this.GetValue(); }
			set { this.value = value; }
		}

		#endregion

		#region Constructor

		public DbValue(DbField field, object value)
		{
			this.field = field;
			this.value = (value == null) ? System.DBNull.Value : value;
		}

		public DbValue(StatementBase statement, DbField field)
		{
			this.statement	= statement;
			this.field		= field;
			this.value		= field.Value;
		}

		public DbValue(StatementBase statement, DbField field, object value)
		{
			this.statement	= statement;
			this.field		= field;
			this.value		= (value == null) ? System.DBNull.Value : value;
		}

		#endregion

		#region Methods

		public bool IsDBNull()
		{
			if (this.value == null || this.value == System.DBNull.Value)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public string GetString()
		{
			if (this.Field.DbDataType == DbDataType.Text && this.value is long)
			{
				this.value = this.GetClobData((long)this.value);
			}

			return this.value.ToString();
		}

		public char GetChar()
		{
			return Convert.ToChar(this.value, CultureInfo.CurrentCulture);
		}

		public bool GetBoolean()
		{
			return Convert.ToBoolean(this.value, CultureInfo.InvariantCulture);
		}

		public byte GetByte()
		{
			return Convert.ToByte(this.value, CultureInfo.InvariantCulture);
		}

		public short GetInt16()
		{
			return Convert.ToInt16(this.value, CultureInfo.InvariantCulture);
		}

		public int GetInt32()
		{
			return Convert.ToInt32(this.value, CultureInfo.InvariantCulture);
		}

		public long GetInt64()
		{
			return Convert.ToInt64(this.value, CultureInfo.InvariantCulture);
		}

		public decimal GetDecimal()
		{
			return Convert.ToDecimal(this.value, CultureInfo.InvariantCulture);
		}

		public float GetFloat()
		{
			return Convert.ToSingle(this.value, CultureInfo.InvariantCulture);
		}

		public Guid GetGuid()
		{
			if (this.Value is Guid)
			{
				return (Guid)this.Value;
			}
			else if (this.Value is byte[])
			{
				return new Guid((byte[])this.value);
			}

			throw new InvalidOperationException("Incorrect Guid value");
		}

		public double GetDouble()
		{
			return Convert.ToDouble(this.value, CultureInfo.InvariantCulture);
		}

		public DateTime GetDateTime()
		{
			return Convert.ToDateTime(this.value, CultureInfo.CurrentCulture.DateTimeFormat);
		}

		public Array GetArray()
		{
			if (this.value is long)
			{
				this.value = this.GetArrayData((long)this.value);
			}

			return (Array)this.value;
		}

		public byte[] GetBinary()
		{
			if (this.value is long)
			{
				this.value = this.GetBlobData((long)this.value);
			}
			return (byte[])this.value;
		}

		public int EncodeDate()
		{
			return TypeEncoder.EncodeDate(this.GetDateTime());
		}

		public int EncodeTime()
		{
			return TypeEncoder.EncodeTime(this.GetDateTime());
		}

		#endregion

		#region Private Methods

		private object GetValue()
		{
			if (this.IsDBNull())
			{
				return System.DBNull.Value;
			}

			switch (this.field.DbDataType)
			{
				case DbDataType.Text:
					if (this.statement == null)
					{
						return this.GetInt64();
					}
					else
					{
						return this.GetString();
					}

				case DbDataType.Binary:
					if (this.statement == null)
					{
						return this.GetInt64();
					}
					else
					{
						return this.GetBinary();
					}

				case DbDataType.Array:
					if (this.statement == null)
					{
						return this.GetInt64();
					}
					else
					{
						return this.GetArray();
					}

				default:
					return this.value;
			}
		}

		private string GetClobData(long blobId)
		{
			BlobBase clob = this.statement.CreateBlob(blobId);

			return clob.ReadString();
		}

		private byte[] GetBlobData(long blobId)
		{
			BlobBase blob = this.statement.CreateBlob(blobId);

			return blob.Read();
		}

		private Array GetArrayData(long handle)
		{
			if (this.field.ArrayHandle == null)
			{
				this.field.ArrayHandle = this.statement.CreateArray(handle, this.Field.Relation, this.Field.Name);
			}

			ArrayBase gdsArray = this.statement.CreateArray(this.field.ArrayHandle.Descriptor);
			
			gdsArray.Handle			= handle;
			gdsArray.DB				= this.statement.DB;
			gdsArray.Transaction	= this.statement.Transaction;

			return gdsArray.Read();
		}

		#endregion
	}
}