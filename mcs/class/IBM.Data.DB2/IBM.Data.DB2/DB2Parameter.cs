
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Data;
using System.Runtime.InteropServices;

namespace IBM.Data.DB2
{

	public sealed class DB2Parameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		private DbType dbType = DbType.Object;
		private DB2Type db2Type = DB2Type.Invalid;
		private short db2DataType = DB2Constants.SQL_UNKNOWN_TYPE;
		private short db2LastUsedDataType = DB2Constants.SQL_UNKNOWN_TYPE;

		private ParameterDirection direction;
		private short db2Direction = DB2Constants.SQL_PARAM_INPUT;
		private bool nullable = true;
		private string parameterName;
		private string sourceColumn;
		private DataRowVersion sourceVersion;
		private object dataVal;
		private byte scale, precision;
		private int size;
		internal IntPtr internalBuffer;
		internal IntPtr internalLengthBuffer;
		internal int requiredMemory;

		#region Contructors and destructors
		public DB2Parameter()
		{
			direction = ParameterDirection.Input;
			sourceVersion = DataRowVersion.Current;
		} 

		public DB2Parameter(string name, DB2Type type)
		{
			direction = ParameterDirection.Input;
			sourceVersion = DataRowVersion.Current;
			this.ParameterName = name;
			this.DB2Type = type;
		} 

		public DB2Parameter(string name, DB2Type type, int size)
		{
			direction = ParameterDirection.Input;
			sourceVersion = DataRowVersion.Current;
			this.ParameterName = name;
			this.DB2Type = type;
			this.Size = size;
		}

		public DB2Parameter(string name, DB2Type type, int size, string sourceColumn)
		{
			direction = ParameterDirection.Input;
			sourceVersion = DataRowVersion.Current;
			this.ParameterName = name;
			this.DB2Type = type;
			this.Size = size;
			this.SourceColumn = sourceColumn;
		}

		public DB2Parameter(string parameterName, object value)
		{
			direction = ParameterDirection.Input;
			sourceVersion = DataRowVersion.Current;
			this.ParameterName = parameterName;
			this.Value = value;
		}

		public DB2Parameter(string parameterName, DB2Type db2Type, int size, ParameterDirection parameterDirection, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
		{
			this.ParameterName = parameterName;
			this.DB2Type = db2Type;
			this.Size = size;
			this.Direction = parameterDirection;
			this.IsNullable = isNullable;
			this.Precision = precision;
			this.Scale = scale;
			this.SourceColumn = srcColumn;
			this.SourceVersion = srcVersion;
			this.Value = value;
		}

		#endregion

		#region Properties
		#region DbType Property
		public DB2Type DB2Type
		{
			get 
			{
				return db2Type;
			}
			set 
			{
				db2Type = value;
				switch(db2Type)
				{
					case DB2Type.Invalid:        dbType = DbType.Object;            db2DataType = DB2Constants.SQL_UNKNOWN_TYPE;   break;
					case DB2Type.SmallInt:       dbType = DbType.Int16;             db2DataType = DB2Constants.SQL_SMALLINT;       break;
					case DB2Type.Integer:        dbType = DbType.Int32;             db2DataType = DB2Constants.SQL_INTEGER;        break;
					case DB2Type.BigInt:         dbType = DbType.Int64;             db2DataType = DB2Constants.SQL_BIGINT;         break;
					case DB2Type.Real:           dbType = DbType.Single;            db2DataType = DB2Constants.SQL_REAL;           break;
					case DB2Type.Double:         dbType = DbType.Double;            db2DataType = DB2Constants.SQL_DOUBLE;         break;
					case DB2Type.Float:          dbType = DbType.Single;            db2DataType = DB2Constants.SQL_REAL;           break;
					case DB2Type.Decimal:        dbType = DbType.Decimal;           db2DataType = DB2Constants.SQL_DECIMAL;        break;
					case DB2Type.Numeric:        dbType = DbType.VarNumeric;        db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.Date:           dbType = DbType.Date;              db2DataType = DB2Constants.SQL_TYPE_DATE;      break;
					case DB2Type.Time:           dbType = DbType.Time;              db2DataType = DB2Constants.SQL_TYPE_TIME;      break;
					case DB2Type.Timestamp:      dbType = DbType.DateTime;          db2DataType = DB2Constants.SQL_TYPE_TIMESTAMP; break;
					case DB2Type.Char:           dbType = DbType.String;            db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.VarChar:        dbType = DbType.StringFixedLength; db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.LongVarChar:    dbType = DbType.String;            db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.Binary:         dbType = DbType.Binary;            db2DataType = DB2Constants.SQL_VARBINARY;      break;
					case DB2Type.VarBinary:      dbType = DbType.Binary;            db2DataType = DB2Constants.SQL_VARBINARY;      break;
					case DB2Type.LongVarBinary:  dbType = DbType.String;            db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.Graphic:        dbType = DbType.StringFixedLength; db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.VarGraphic:     dbType = DbType.String;            db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.LongVarGraphic: dbType = DbType.String;            db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.Clob:           dbType = DbType.String;            db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.Blob:           dbType = DbType.Binary;            db2DataType = DB2Constants.SQL_VARBINARY;      break;
					case DB2Type.DbClob:         dbType = DbType.String;            db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DB2Type.Datalink:       dbType = DbType.Byte;              db2DataType = DB2Constants.SQL_VARBINARY;      break;
					case DB2Type.RowId:          dbType = DbType.Decimal;           db2DataType = DB2Constants.SQL_DECIMAL;        break;
					case DB2Type.XmlReader:      dbType = DbType.String;            db2DataType = DB2Constants.SQL_WCHAR;          break;
					default:
						throw new NotSupportedException("Value is of unknown data type");
				}
			}
		}
		///
		/// Parameter data type
		/// 
		public DbType DbType
		{
			get 
			{
				return dbType;
			}
			set 
			{
				dbType = value;
				switch(dbType)
				{
					case DbType.AnsiString:             db2Type = DB2Type.VarChar;    db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DbType.AnsiStringFixedLength:  db2Type = DB2Type.Char;       db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DbType.Binary:                 db2Type = DB2Type.Binary;     db2DataType = DB2Constants.SQL_VARBINARY;      break;
					case DbType.Boolean:                db2Type = DB2Type.SmallInt;   db2DataType = DB2Constants.SQL_BIT;            break;
					case DbType.Byte:                   db2Type = DB2Type.SmallInt;   db2DataType = DB2Constants.SQL_UTINYINT;       break;
					case DbType.Currency:               db2Type = DB2Type.Decimal;    db2DataType = DB2Constants.SQL_DECIMAL;        break;
					case DbType.Date:                   db2Type = DB2Type.Date;       db2DataType = DB2Constants.SQL_TYPE_DATE;      break;
					case DbType.DateTime:               db2Type = DB2Type.Timestamp;  db2DataType = DB2Constants.SQL_TYPE_TIMESTAMP; break;
					case DbType.Decimal:                db2Type = DB2Type.Decimal;    db2DataType = DB2Constants.SQL_DECIMAL;        break;
					case DbType.Double:                 db2Type = DB2Type.Double;     db2DataType = DB2Constants.SQL_DOUBLE;         break;
					case DbType.Guid:                   db2Type = DB2Type.Binary;     db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DbType.Int16:                  db2Type = DB2Type.SmallInt;   db2DataType = DB2Constants.SQL_SMALLINT;       break;
					case DbType.Int32:                  db2Type = DB2Type.Integer;    db2DataType = DB2Constants.SQL_INTEGER;        break;
					case DbType.Int64:                  db2Type = DB2Type.BigInt;     db2DataType = DB2Constants.SQL_BIGINT;         break;
					case DbType.Object:                 db2Type = DB2Type.Invalid;    db2DataType = DB2Constants.SQL_UNKNOWN_TYPE;   break;
					case DbType.SByte:                  db2Type = DB2Type.SmallInt;   db2DataType = DB2Constants.SQL_UTINYINT;       break;
					case DbType.Single:                 db2Type = DB2Type.Float;      db2DataType = DB2Constants.SQL_REAL;           break;
					case DbType.String:                 db2Type = DB2Type.VarChar;    db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DbType.StringFixedLength:      db2Type = DB2Type.Char;       db2DataType = DB2Constants.SQL_WCHAR;          break;
					case DbType.Time:                   db2Type = DB2Type.Time;       db2DataType = DB2Constants.SQL_TYPE_TIME;      break;
					case DbType.UInt16:                 db2Type = DB2Type.SmallInt;   db2DataType = DB2Constants.SQL_SMALLINT;       break;
					case DbType.UInt32:                 db2Type = DB2Type.Integer;    db2DataType = DB2Constants.SQL_INTEGER;        break;
					case DbType.UInt64:                 db2Type = DB2Type.BigInt;     db2DataType = DB2Constants.SQL_BIGINT;         break;
					case DbType.VarNumeric:             db2Type = DB2Type.Numeric;    db2DataType = DB2Constants.SQL_WCHAR;          break;
					default:
						throw new NotSupportedException("Value is of unknown data type");
				}
			}
		}

		#endregion
		#region Direction
		///
		/// In or out parameter, or both
		/// 
		public ParameterDirection Direction
		{
			get 
			{
				return direction;
			}
			set 
			{
				direction = value;
				switch(direction)
				{
					default:
					case ParameterDirection.Input:			db2Direction = DB2Constants.SQL_PARAM_INPUT;		break;
					case ParameterDirection.Output:			db2Direction = DB2Constants.SQL_PARAM_OUTPUT;		break;
					case ParameterDirection.InputOutput:	db2Direction = DB2Constants.SQL_PARAM_INPUT_OUTPUT;	break;
					case ParameterDirection.ReturnValue:	db2Direction = DB2Constants.SQL_RETURN_VALUE;		break;
				}
			}
		}
		#endregion
		#region IsNullable
		///
		/// Does this parameter support a null value
		/// 
		public bool IsNullable 
		{
			get 
			{
				return nullable;
			}
			set 
			{
				nullable = value;
			}
		}
		#endregion
		#region ParameterName
		public string ParameterName
		{
			get 
			{
				return parameterName;
			}
			set 
			{
				parameterName = value;
			}
		}
		#endregion
		#region SourceColumn
		///
		/// Gets or sets the name of the source column that is mapped to the DataSet
		/// 
		public string SourceColumn
		{
			get 
			{
				return sourceColumn;
			}
			set 
			{
				sourceColumn = value;
			}
		}
		#endregion
		#region SourceVersion
		///
		/// DataRowVersion property
		/// 
		public DataRowVersion SourceVersion 
		{
			get 
			{
				return sourceVersion;
			}
			set 
			{
				sourceVersion = value;
			}
		}
		#endregion
		#region IDbDataParameter properties
		public byte Precision 
		{
			get 
			{ 
				return precision;
			}
			set 
			{ 
				precision = value; 
			}
		}
		
		public byte Scale 
		{
			get 
			{ 
				return scale;
			}
			set 
			{ 
				scale = value; 
			}
		}
		
		public int Size 
		{
			get 
			{
				return size;
			}
			set 
			{ 
				size = value;
			}
		}
		#endregion
		#region Value
		///
		/// The actual parameter data
		/// 
		public object Value 
		{
			get
			{
				return dataVal;
			}
			set 
			{
				this.dataVal = value;
			}
		}
		#endregion
		#endregion
		
		#region inferType Method
		/// <summary>
		/// Determine the data type based on the value
		/// </summary>
		private void InferType()
		{
			if(Value == null)
				throw new ArgumentException("No DB2Parameter.Value found");

			if(Value is IConvertible)
			{
				switch(((IConvertible)Value).GetTypeCode())
				{
					case TypeCode.Char:      dbType = DbType.Byte;       db2Type = DB2Type.SmallInt;    db2DataType = DB2Constants.SQL_WCHAR;           break;
					case TypeCode.Boolean:   dbType = DbType.Byte;       db2Type = DB2Type.SmallInt;    db2DataType = DB2Constants.SQL_BIT;             break;
					case TypeCode.SByte:
					case TypeCode.Byte:      dbType = DbType.Byte;       db2Type = DB2Type.SmallInt;    db2DataType = DB2Constants.SQL_UTINYINT;        break;
					case TypeCode.UInt16:
					case TypeCode.Int16:     dbType = DbType.Int16;      db2Type = DB2Type.SmallInt;    db2DataType = DB2Constants.SQL_SMALLINT;        break;
					case TypeCode.UInt32:
					case TypeCode.Int32:     dbType = DbType.Int32;      db2Type = DB2Type.Integer;     db2DataType = DB2Constants.SQL_INTEGER;         break;
					case TypeCode.UInt64:
					case TypeCode.Int64:     dbType = DbType.Int64;      db2Type = DB2Type.BigInt;      db2DataType = DB2Constants.SQL_BIGINT;          break;
					case TypeCode.Single:    dbType = DbType.Single;     db2Type = DB2Type.Float;       db2DataType = DB2Constants.SQL_REAL;            break;
					case TypeCode.Double:    dbType = DbType.Double;     db2Type = DB2Type.Double;      db2DataType = DB2Constants.SQL_DOUBLE;          break;
					case TypeCode.Decimal:   dbType = DbType.Decimal;    db2Type = DB2Type.Decimal;     db2DataType = DB2Constants.SQL_DECIMAL;         break;
					case TypeCode.DateTime:  dbType = DbType.DateTime;   db2Type = DB2Type.Timestamp;   db2DataType = DB2Constants.SQL_TYPE_TIMESTAMP;  break;
					case TypeCode.String:    dbType = DbType.String;     db2Type = DB2Type.VarChar;     db2DataType = DB2Constants.SQL_WCHAR;           break;

					case TypeCode.Object:
					case TypeCode.DBNull:
					case TypeCode.Empty:
						throw new SystemException("Unknown data type");
					default:
						throw new SystemException("Value is of unknown data type");
				}
			}
			else if(Value is byte[])
			{
				dbType = DbType.Binary;
				db2Type = DB2Type.VarBinary;
				db2DataType = DB2Constants.SQL_VARBINARY;
			}
			else if(Value is TimeSpan)
			{
				dbType = DbType.Time;
				db2Type = DB2Type.Time;
				db2DataType = DB2Constants.SQL_TYPE_TIME;
			}
			else
			{
				throw new NotSupportedException("Value is of unsupported data type");
			}
		}
		#endregion
		
		internal void CalculateRequiredmemory()
		{
			//if((direction == ParameterDirection.Input) || (direction == ParameterDirection.InputOutput))
			//{
			//	if(Value == null)
			//		throw new ArgumentException("Value missing");
			//}
			if(dbType == DbType.Object)
			{
				if((direction == ParameterDirection.Output) || (direction == ParameterDirection.ReturnValue))
					throw new ArgumentException("Unknown type");

				if((direction != ParameterDirection.Input) || !Convert.IsDBNull(Value))
				{
					InferType();
				}
			}
			if (db2DataType == DB2Constants.SQL_INTEGER)
			{
				requiredMemory = 4;
			}
			if((db2DataType == DB2Constants.SQL_VARBINARY) ||
				(db2DataType == DB2Constants.SQL_WCHAR))
			{
				if(Size <= 0)
				{
					if(direction != ParameterDirection.Input)
						throw new ArgumentException("Size not specified");
					if(Value == DBNull.Value)
						requiredMemory = 0;
					else if(Value is string)
						requiredMemory = ((string)Value).Length;
					else if(Value is byte[])
						requiredMemory = ((byte[])Value).Length;
					else
						throw new ArgumentException("wrong type!?");
				}
				else
				{
					requiredMemory = Size;
				}
				if(db2DataType == DB2Constants.SQL_WCHAR)
					requiredMemory = (requiredMemory * 2) + 2;
				requiredMemory = (requiredMemory | 0xf) + 1;
			}
			requiredMemory = Math.Max(128, requiredMemory);
		}

		#region Bind 
		///
		/// Bind this parameter
		/// 
		internal short Bind(IntPtr hwndStmt, short paramNum)
		{
			int inLength = requiredMemory;
			db2LastUsedDataType = db2DataType;
			short db2CType = DB2Constants.SQL_C_DEFAULT;
			if((direction == ParameterDirection.Input) || (direction == ParameterDirection.InputOutput))
			{
				if(Convert.IsDBNull(Value))
				{
					inLength = DB2Constants.SQL_NULL_DATA;
					if((db2DataType == DB2Constants.SQL_UNKNOWN_TYPE) || 
						(db2DataType == DB2Constants.SQL_DECIMAL))
					{
						db2LastUsedDataType = DB2Constants.SQL_VARGRAPHIC;
						db2CType = DB2Constants.SQL_C_WCHAR;
					}
				}
			}
			if((direction == ParameterDirection.Input) || (direction == ParameterDirection.InputOutput))
			{
				switch (db2DataType) 
				{
					case DB2Constants.SQL_WCHAR:
						string tmpString = Convert.ToString(Value);
						inLength =  tmpString.Length;
						if((Size > 0) && (inLength > Size))
							inLength = Size;
						Marshal.Copy(tmpString.ToCharArray(), 0, internalBuffer, inLength);
						inLength *= 2;
						db2LastUsedDataType = DB2Constants.SQL_VARGRAPHIC;
						db2CType = DB2Constants.SQL_C_WCHAR;
						if(inLength > 32000)
						{
							db2LastUsedDataType = DB2Constants.SQL_TYPE_BLOB;
						}
						break;
					case DB2Constants.SQL_VARBINARY:
						byte[] tmpBytes = (byte[])Value;
						inLength = tmpBytes.Length;
						if((Size > 0) && (inLength > Size))
							inLength = Size;
						Marshal.Copy(tmpBytes, 0, internalBuffer, inLength);
						db2CType = DB2Constants.SQL_TYPE_BINARY;
						break;
					case DB2Constants.SQL_BIT:
					case DB2Constants.SQL_UTINYINT:
					case DB2Constants.SQL_SMALLINT:
						Marshal.WriteInt16(internalBuffer, Convert.ToInt16(Value));
						db2CType = DB2Constants.SQL_C_SSHORT;
						break;
					case DB2Constants.SQL_INTEGER:
						Marshal.WriteInt32(internalBuffer, Convert.ToInt32(Value));
						db2CType = DB2Constants.SQL_C_SLONG;
						break;
					case DB2Constants.SQL_BIGINT:
						Marshal.WriteInt64(internalBuffer, Convert.ToInt64(Value));
						db2CType = DB2Constants.SQL_C_SBIGINT;
						break;
					case DB2Constants.SQL_REAL:
						Marshal.StructureToPtr((float)Convert.ToDouble(Value), internalBuffer, false);
						db2CType = DB2Constants.SQL_C_TYPE_REAL;
						break;
					case DB2Constants.SQL_DOUBLE:
						Marshal.StructureToPtr(Convert.ToDouble(Value), internalBuffer, false);
						db2CType = DB2Constants.SQL_C_DOUBLE;
						break;
					case DB2Constants.SQL_DECIMAL:
						byte[] tmpDecimalData = System.Text.Encoding.UTF8.GetBytes(
							Convert.ToDecimal(Value).ToString(System.Globalization.CultureInfo.InvariantCulture));
						inLength =  Math.Min(tmpDecimalData.Length, requiredMemory);
						Marshal.Copy(tmpDecimalData, 0, internalBuffer, inLength);
						db2LastUsedDataType = DB2Constants.SQL_VARCHAR;
						db2CType = DB2Constants.SQL_C_CHAR;
						break;
					case DB2Constants.SQL_TYPE_DATE:
						DateTime tmpDate = Convert.ToDateTime(Value);
						Marshal.WriteInt16(internalBuffer, 0,  (short)tmpDate.Year);
						Marshal.WriteInt16(internalBuffer, 2,  (short)tmpDate.Month);
						Marshal.WriteInt16(internalBuffer, 4,  (short)tmpDate.Day);
						db2CType = DB2Constants.SQL_C_TYPE_DATE;
						break;
					case DB2Constants.SQL_TYPE_TIMESTAMP:
						DateTime tmpDateTime = Convert.ToDateTime(Value);
						Marshal.WriteInt16(internalBuffer, 0,  (short)tmpDateTime.Year);
						Marshal.WriteInt16(internalBuffer, 2,  (short)tmpDateTime.Month);
						Marshal.WriteInt16(internalBuffer, 4,  (short)tmpDateTime.Day);
						Marshal.WriteInt16(internalBuffer, 6,  (short)tmpDateTime.Hour);
						Marshal.WriteInt16(internalBuffer, 8,  (short)tmpDateTime.Minute);
						Marshal.WriteInt16(internalBuffer, 10, (short)tmpDateTime.Second);
						Marshal.WriteInt32(internalBuffer, 12, (int)((tmpDateTime.Ticks % 10000000) * 100));
						db2CType = DB2Constants.SQL_C_TYPE_TIMESTAMP;
						break;
					case DB2Constants.SQL_TYPE_TIME:
						TimeSpan tmpTime = (TimeSpan)Value;
						Marshal.WriteInt16(internalBuffer, 0,  (short)tmpTime.Hours);
						Marshal.WriteInt16(internalBuffer, 2,  (short)tmpTime.Minutes);
						Marshal.WriteInt16(internalBuffer, 4,  (short)tmpTime.Seconds);
						db2CType = DB2Constants.SQL_C_TYPE_TIME;
						break;
				}
			}
			else
			{
				switch (db2DataType) 
				{
					case DB2Constants.SQL_WCHAR:
						db2LastUsedDataType = DB2Constants.SQL_VARGRAPHIC;
						db2CType = DB2Constants.SQL_C_WCHAR;
						break;
					case DB2Constants.SQL_VARBINARY:
						db2CType = DB2Constants.SQL_TYPE_BINARY;
						break;
					case DB2Constants.SQL_BIT:
					case DB2Constants.SQL_UTINYINT:
					case DB2Constants.SQL_SMALLINT:
						db2CType = DB2Constants.SQL_C_SSHORT;
						break;
					case DB2Constants.SQL_INTEGER:
						db2CType = DB2Constants.SQL_C_SLONG;
						break;
					case DB2Constants.SQL_BIGINT:
						db2CType = DB2Constants.SQL_C_SBIGINT;
						break;
					case DB2Constants.SQL_REAL:
						db2CType = DB2Constants.SQL_C_TYPE_REAL;
						break;
					case DB2Constants.SQL_DOUBLE:
						db2CType = DB2Constants.SQL_C_DOUBLE;
						break;
					case DB2Constants.SQL_DECIMAL:
						db2LastUsedDataType = DB2Constants.SQL_VARCHAR;
						db2CType = DB2Constants.SQL_C_CHAR;
						break;
					case DB2Constants.SQL_TYPE_DATE:
						db2CType = DB2Constants.SQL_C_TYPE_DATE;
						break;
					case DB2Constants.SQL_TYPE_TIMESTAMP:
						db2CType = DB2Constants.SQL_C_TYPE_TIMESTAMP;
						break;
					case DB2Constants.SQL_TYPE_TIME:
						db2CType = DB2Constants.SQL_C_TYPE_TIME;
						break;
				}
			}
			Marshal.WriteInt32(internalLengthBuffer, inLength);
			short sqlRet = DB2CLIWrapper.SQLBindParameter(hwndStmt, paramNum, db2Direction, 
				db2CType, db2LastUsedDataType, Size, Scale,
				internalBuffer, requiredMemory, internalLengthBuffer);

			return sqlRet;
		}
		#endregion
		object ICloneable.Clone ()
		{
			DB2Parameter clone = new DB2Parameter();
			clone.dbType = dbType;
			clone.db2Type = db2Type;
			clone.db2DataType = db2DataType;
			clone.db2LastUsedDataType = db2LastUsedDataType;
			clone.direction = direction;
			clone.db2Direction = db2Direction;
			clone.nullable = nullable;
			clone.parameterName = parameterName;
			clone.sourceColumn = sourceColumn;
			clone.sourceVersion = sourceVersion;
			clone.dataVal = dataVal;
			clone.scale = scale;
			clone.precision = precision;
			clone.size = size;
			if(dataVal is ICloneable)
			{
				clone.dataVal = ((ICloneable)dataVal).Clone();
			}
			return clone;
		}
		
		internal void GetOutValue()
		{
			int length = Marshal.ReadInt32(internalLengthBuffer);
			if(length == DB2Constants.SQL_NULL_DATA)
			{
				dataVal = DBNull.Value;
				return;
			}
			switch(DB2Type) 
			{
				case DB2Type.SmallInt:
					dataVal = Marshal.ReadInt16(internalBuffer);
					break;
				case DB2Type.Integer:
					dataVal = Marshal.ReadInt32(internalBuffer);
					break;
				case DB2Type.BigInt:
					dataVal = Marshal.ReadInt64(internalBuffer);
					break;
				case DB2Type.Double:
					dataVal = Marshal.PtrToStructure(internalBuffer, typeof(Double));
					break;
				case DB2Type.Float:
					dataVal = Marshal.PtrToStructure(internalBuffer, typeof(Single));
					break;
				case DB2Type.Char:
				case DB2Type.VarChar:
				case DB2Type.LongVarChar:
				case DB2Type.Graphic:
				case DB2Type.VarGraphic:
				case DB2Type.LongVarGraphic:
				case DB2Type.Clob:
				case DB2Type.DbClob:
					dataVal = Marshal.PtrToStringUni(internalBuffer, Math.Min(Size, length / 2));
					break;
				case DB2Type.Binary:
				case DB2Type.VarBinary:
				case DB2Type.LongVarBinary:
				case DB2Type.Blob:
				case DB2Type.Datalink:
					length = Math.Min(Size, length);
					dataVal = new byte[length];
					Marshal.Copy(internalBuffer, (byte[])dataVal, 0, length);
					break;
				case DB2Type.Decimal:
					dataVal = decimal.Parse(Marshal.PtrToStringAnsi(internalBuffer, length), 
						System.Globalization.CultureInfo.InvariantCulture);
					break;
				case DB2Type.Timestamp:
					DateTime dtTmp = new DateTime(
						Marshal.ReadInt16(internalBuffer, 0),  // year
						Marshal.ReadInt16(internalBuffer, 2),  // month
						Marshal.ReadInt16(internalBuffer, 4),  // day
						Marshal.ReadInt16(internalBuffer, 6),  // hour
						Marshal.ReadInt16(internalBuffer, 8),  // minute
						Marshal.ReadInt16(internalBuffer, 10));// second
					dataVal = dtTmp.AddTicks(Marshal.ReadInt32(internalBuffer, 12) / 100); // nanoseconds 
					break;
				case DB2Type.Date:
					dataVal = new DateTime(
						Marshal.ReadInt16(internalBuffer, 0),
						Marshal.ReadInt16(internalBuffer, 2),
						Marshal.ReadInt16(internalBuffer, 4));
					break;
				case DB2Type.Time:
					dataVal = new TimeSpan(
						Marshal.ReadInt16(internalBuffer, 0),  // Hour
						Marshal.ReadInt16(internalBuffer, 2),  // Minute
						Marshal.ReadInt16(internalBuffer, 4)); // Second
					break;

				case DB2Type.Invalid:
				case DB2Type.Real:
				case DB2Type.Numeric:
				case DB2Type.RowId:
				case DB2Type.XmlReader:
					throw new NotImplementedException();
				default:
					throw new NotSupportedException("unknown data type");
			}
		}
	}
}
