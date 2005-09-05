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
using System.Text;

namespace FirebirdSql.Data.Common
{
	internal sealed class DbField
	{
		#region Fields

		private short		dataType;
		private short		numericScale;
		private short		subType;
		private short		length;
		private short		nullFlag;
		private string		name;
		private string		relation;
		private string		owner;
		private string		alias;
		private int			charCount;
		private DbValue		dbValue;
		private Charset		charset;
		private ArrayBase	arrayHandle;

		#endregion

		#region Properties

		public DbDataType DbDataType
		{
			get { return this.GetDbDataType(); }
		}

		public int SqlType
		{
			get { return this.dataType & ~1; }
		}

		public short DataType
		{
			get { return this.dataType; }
			set { this.dataType = value; }
		}

		public short NumericScale
		{
			get { return this.numericScale; }
			set { this.numericScale = value; }
		}

		public short SubType
		{
			get { return this.subType; }
			set
			{
				this.subType = value;
				if (this.IsCharacter())
				{
					// Bits 0-7 of sqlsubtype is charset_id (127 is a special value -
					// current attachment charset).
					// Bits 8-17 hold collation_id for this value.
					byte[] cs = BitConverter.GetBytes(value);

					int index = Charset.SupportedCharsets.IndexOf(cs[0]);
					if (index != -1)
					{
						this.charset = Charset.SupportedCharsets[index];
					}
					else
					{
						this.charset = Charset.SupportedCharsets[0];
					}
				}
			}
		}

		public short Length
		{
			get { return this.length; }
			set
			{
				this.length = value;
				if (this.IsCharacter())
				{
					this.charCount = this.length / this.charset.BytesPerCharacter;
				}
			}
		}

		public short NullFlag
		{
			get { return this.nullFlag; }
			set { this.nullFlag = value; }
		}

		public string Name
		{
			get { return this.name; }
			set { this.name = value.Trim(); }
		}

		public string Relation
		{
			get { return this.relation; }
			set { this.relation = value.Trim(); }
		}

		public string Owner
		{
			get { return this.owner; }
			set { this.owner = value.Trim(); }
		}

		public string Alias
		{
			get { return this.alias; }
			set { this.alias = value.Trim(); }
		}

		public Charset Charset
		{
			get { return this.charset; }
		}

		public int CharCount
		{
			get { return this.charCount; }
		}

		public ArrayBase ArrayHandle
		{
			get
			{
				if (this.IsArray())
				{
					return this.arrayHandle;
				}
				else
				{
					throw new IscException("Field is not an array type");
				}
			}

			set
			{
				if (this.IsArray())
				{
					this.arrayHandle = value;
				}
				else
				{
					throw new IscException("Field is not an array type");
				}
			}
		}

		public DbValue DbValue
		{
			get { return this.dbValue; }
		}

		public object Value
		{
			get { return this.dbValue.Value; }
			set { this.dbValue.Value = value; }
		}

		#endregion

		#region Constructors

		public DbField()
		{
			this.charCount	= -1;
			this.name		= String.Empty;
			this.relation	= String.Empty;
			this.owner		= String.Empty;
			this.alias		= String.Empty;
			this.dbValue	= new DbValue(this, DBNull.Value);
		}

		#endregion

		#region Methods

		public bool IsNumeric()
		{
			if (this.dataType == 0)
			{
				return false;
			}

			switch (this.DbDataType)
			{
				case DbDataType.SmallInt:
				case DbDataType.Integer:
				case DbDataType.BigInt:
				case DbDataType.Numeric:
				case DbDataType.Decimal:
				case DbDataType.Float:
				case DbDataType.Double:
					return true;

				default:
					return false;
			}
		}

		public bool IsDecimal()
		{
			if (this.dataType == 0)
			{
				return false;
			}

			switch (this.DbDataType)
			{
				case DbDataType.Numeric:
				case DbDataType.Decimal:
					return true;

				default:
					return false;
			}
		}

		public bool IsLong()
		{
			if (this.dataType == 0)
			{
				return false;
			}

			switch (this.DbDataType)
			{
				case DbDataType.Binary:
				case DbDataType.Text:
					return true;

				default:
					return false;
			}
		}

		public bool IsCharacter()
		{
			if (this.dataType == 0)
			{
				return false;
			}

			switch (this.DbDataType)
			{
				case DbDataType.Char:
				case DbDataType.VarChar:
				case DbDataType.Text:
					return true;

				default:
					return false;
			}
		}

		public bool IsArray()
		{
			if (this.dataType == 0)
			{
				return false;
			}

			switch (this.DbDataType)
			{
				case DbDataType.Array:
					return true;

				default:
					return false;
			}
		}

		public bool IsAliased()
		{
			return (this.Name != this.Alias) ? true : false;
		}

		public bool IsExpression()
		{
			return this.Name.Length == 0 ? true : false;
		}

		public int GetSize()
		{
			if (this.IsLong())
			{
				return System.Int32.MaxValue;
			}
			else
			{
				if (this.IsCharacter())
				{
					return this.CharCount;
				}
				else
				{
					return this.Length;
				}
			}
		}

		public bool AllowDBNull()
		{
			return ((this.DataType & 1) == 1);
		}

		public void SetValue(byte[] buffer)
		{
			if (buffer == null || this.NullFlag == -1)
			{
				this.Value = System.DBNull.Value;
			}
			else
			{
				switch (this.SqlType)
				{
					case IscCodes.SQL_TEXT:
					case IscCodes.SQL_VARYING:
						if (this.DbDataType == DbDataType.Guid)
						{
							this.Value = new Guid(buffer);
						}
						else
						{
							string s = this.Charset.GetString(buffer, 0, buffer.Length);

							if ((this.Length % this.Charset.BytesPerCharacter) == 0 &&
								s.Length > this.CharCount)
							{
								s = s.Substring(0, this.CharCount);
							}

							this.Value = s;
						}
						break;

					case IscCodes.SQL_SHORT:
						if (this.numericScale < 0)
						{
							this.Value = TypeDecoder.DecodeDecimal(
								BitConverter.ToInt16(buffer, 0),
								this.numericScale,
								this.dataType);
						}
						else
						{
							this.Value = BitConverter.ToInt16(buffer, 0);
						}
						break;

					case IscCodes.SQL_LONG:
						if (this.NumericScale < 0)
						{
							this.Value = TypeDecoder.DecodeDecimal(
								BitConverter.ToInt32(buffer, 0),
								this.numericScale,
								this.dataType);
						}
						else
						{
							this.Value = BitConverter.ToInt32(buffer, 0);
						}
						break;

					case IscCodes.SQL_FLOAT:
						this.Value = BitConverter.ToSingle(buffer, 0);
						break;

					case IscCodes.SQL_DOUBLE:
					case IscCodes.SQL_D_FLOAT:
						this.Value = BitConverter.ToDouble(buffer, 0);
						break;

					case IscCodes.SQL_QUAD:
					case IscCodes.SQL_INT64:
					case IscCodes.SQL_BLOB:
					case IscCodes.SQL_ARRAY:
						if (this.NumericScale < 0)
						{
							this.Value = TypeDecoder.DecodeDecimal(
								BitConverter.ToInt64(buffer, 0),
								this.numericScale,
								this.dataType);
						}
						else
						{
							this.Value = BitConverter.ToInt64(buffer, 0);
						}
						break;

					case IscCodes.SQL_TIMESTAMP:
						DateTime date = TypeDecoder.DecodeDate(
							BitConverter.ToInt32(buffer, 0));

						DateTime time = TypeDecoder.DecodeTime(
							BitConverter.ToInt32(buffer, 4));

						this.Value = new System.DateTime(
							date.Year, date.Month, date.Day,
							time.Hour, time.Minute, time.Second, time.Millisecond);
						break;

					case IscCodes.SQL_TYPE_TIME:
						this.Value = TypeDecoder.DecodeTime(BitConverter.ToInt32(buffer, 0));
						break;

					case IscCodes.SQL_TYPE_DATE:
						this.Value = TypeDecoder.DecodeDate(BitConverter.ToInt32(buffer, 0));
						break;

					default:
						throw new NotSupportedException("Unknown data type");
				}
			}
		}

		public void FixNull()
		{
			if (this.NullFlag == -1 && this.dbValue.IsDBNull())
			{
				switch (this.DbDataType)
				{
					case DbDataType.Char:
					case DbDataType.VarChar:
						this.Value = String.Empty;
						break;

					case DbDataType.Guid:
						this.Value = Guid.Empty;
						break;

					case DbDataType.SmallInt:
						this.Value = (short)0;
						break;

					case DbDataType.Integer:
						this.Value = (int)0;
						break;

					case DbDataType.BigInt:
					case DbDataType.Binary:
					case DbDataType.Array:
					case DbDataType.Text:
						this.Value = (long)0;
						break;

					case DbDataType.Numeric:
					case DbDataType.Decimal:
						this.Value = (decimal)0;
						break;

					case DbDataType.Float:
						this.Value = (float)0;
						break;

					case DbDataType.Double:
						this.Value = (double)0;
						break;

					case DbDataType.Date:
					case DbDataType.Time:
					case DbDataType.TimeStamp:
						this.Value = new System.DateTime(0 * 10000L + 621355968000000000);
						break;

					default:
						throw new IscException("Unknown sql data type: " + this.DataType);
				}
			}
		}

		public Type GetSystemType()
		{
			switch (this.DbDataType)
			{
				case DbDataType.Char:
				case DbDataType.VarChar:
				case DbDataType.Text:
					return Type.GetType("System.String");

				case DbDataType.SmallInt:
					return Type.GetType("System.Int16");

				case DbDataType.Integer:
					return Type.GetType("System.Int32");

				case DbDataType.BigInt:
					return Type.GetType("System.Int64");

				case DbDataType.Numeric:
				case DbDataType.Decimal:
					return Type.GetType("System.Decimal");

				case DbDataType.Float:
					return Type.GetType("System.Single");

				case DbDataType.Guid:
					return Type.GetType("System.Guid");

				case DbDataType.Double:
					return Type.GetType("System.Double");

				case DbDataType.Date:
				case DbDataType.Time:
				case DbDataType.TimeStamp:
					return Type.GetType("System.DateTime");

				case DbDataType.Binary:
					return typeof(byte[]);

				case DbDataType.Array:
					return Type.GetType("System.Array");

				default:
					throw new SystemException("Invalid data type");
			}
		}

		#endregion

		#region Private Methods

		private DbDataType GetDbDataType()
		{
			// Special case for Guid handling
			if (this.SqlType == IscCodes.SQL_TEXT && this.Length == 16 &&
				(this.Charset != null && this.Charset.Name == "OCTETS"))
			{
				return DbDataType.Guid;
			}

			switch (this.SqlType)
			{
				case IscCodes.SQL_TEXT:
					return DbDataType.Char;

				case IscCodes.SQL_VARYING:
					return DbDataType.VarChar;

				case IscCodes.SQL_SHORT:
					if (this.subType == 2)
					{
						return DbDataType.Decimal;
					}
                    else if (subType == 1)
                    {
                        return DbDataType.Numeric;
                    }
                    else
					{
						return DbDataType.SmallInt;
					}

				case IscCodes.SQL_LONG:
					if (this.subType == 2)
					{
						return DbDataType.Decimal;
					}
                    else if (subType == 1)
                    {
                        return DbDataType.Numeric;
                    }
                    else
					{
						return DbDataType.Integer;
					}

				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					if (this.subType == 2)
					{
						return DbDataType.Decimal;
					}
                    else if (subType == 1)
                    {
                        return DbDataType.Numeric;
                    }
                    else
					{
						return DbDataType.BigInt;
					}

				case IscCodes.SQL_FLOAT:
					return DbDataType.Float;

				case IscCodes.SQL_DOUBLE:
				case IscCodes.SQL_D_FLOAT:
					if (this.subType == 2)
					{
						return DbDataType.Decimal;
					}
                    else if (subType == 1)
                    {
                        return DbDataType.Numeric;
                    }
                    else
					{
						return DbDataType.Double;
					}

				case IscCodes.SQL_BLOB:
					if (this.subType == 1)
					{
						return DbDataType.Text;
					}
					else
					{
						return DbDataType.Binary;
					}

				case IscCodes.SQL_TIMESTAMP:
					return DbDataType.TimeStamp;

				case IscCodes.SQL_TYPE_TIME:
					return DbDataType.Time;

				case IscCodes.SQL_TYPE_DATE:
					return DbDataType.Date;

				case IscCodes.SQL_ARRAY:
					return DbDataType.Array;

				default:
					throw new SystemException("Invalid data type");
			}
		}

		#endregion
	}
}