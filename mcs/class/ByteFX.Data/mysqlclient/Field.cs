// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;

namespace ByteFX.Data.MySqlClient
{
	internal enum ColFlags : int
	{
		NOT_NULL		= 1,
		PRIMARY_KEY		= 2,
		UNIQUE_KEY		= 4,
		MULTIPLE_KEY	= 8,
		BLOB			= 16,
		UNSIGNED		= 32,
		ZERO_FILL		= 64,
		BINARY			= 128,
		ENUM			= 256,
		AUTO_INCREMENT	= 512,
		TIMESTAMP		= 1024,
		SET				= 2048,
		NUMBER			= 32768
	};
	
	/// <summary>
	/// Summary description for Field.
	/// </summary>
	internal class MySqlField : Common.Field
	{
		  MySqlDbType			colType;
		public	ColFlags		colFlags;
		public	int				colDecimals;
//		System.Text.Encoding	encoding;
		private static NumberFormatInfo		numberFormat = null;


		public MySqlField()
		{
//			this.encoding = encoding;
			if (numberFormat == null)
			{
				numberFormat = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
				numberFormat.NumberDecimalSeparator = ".";
			}
		}

		public void ReadSchemaInfo( Packet packet )
		{	
			tableName = packet.ReadLenString();
			colName = packet.ReadLenString();
			colLen = (int)packet.ReadNBytes();
			colType = (MySqlDbType)packet.ReadNBytes();
			packet.ReadByte();									// this is apparently 2 -- not sure what it is for
			colFlags = (ColFlags)packet.ReadInteger(2);		//(short)(d.ReadByte() & 0xff);
			colDecimals = packet.ReadByte();
		}

		public object GetValue() 
		{
			return value;
		}

		public int NumericPrecision()
		{
			if (colType == MySqlDbType.Decimal)
				return ((SqlDecimal)value).Precision;
			return -1;
		}

		public int NumericScale()
		{
			if (colType == MySqlDbType.Decimal)
				return ((SqlDecimal)value).Scale; 
			return -1;
		}

		public bool IsAutoIncrement()
		{
			return (colFlags & ColFlags.AUTO_INCREMENT) > 0;
		}

		public bool IsNumeric()
		{
			return (colFlags & ColFlags.NUMBER) > 0;
		}

		public bool AllowsNull()
		{
			return (colFlags & ColFlags.NOT_NULL) == 0;
		}

		public bool IsUnique()
		{
			return (colFlags & ColFlags.UNIQUE_KEY) > 0;
		}

		public bool IsPrimaryKey()
		{
			return (colFlags & ColFlags.PRIMARY_KEY) > 0;
		}

		public bool IsBlob() 
		{
			return (colFlags & ColFlags.BLOB) > 0;
		}

		public bool IsBinary()
		{
			return (colFlags & ColFlags.BINARY) > 0;
		}

		public bool IsUnsigned()
		{
			return (colFlags & ColFlags.UNSIGNED) > 0;
		}

		public void SetValueData( Packet p, Encoding encoding )
		{
			int len = p.ReadLenInteger();
			if (len == -1)
			{
				value = DBNull.Value;
				return;
			}

			// read in the data
			byte[] data = new byte[ len ];
			p.ReadBytes( data, 0, len );

			// if it is a blob and binary, then GetBytes is the way to go
			if ( IsBlob() && IsBinary() ) 
			{
				dbType = DbType.Binary;
				value = data;
				return;
			}

			string sValue = encoding.GetString( data );//Chars(data, offset, count );
			//string sValue = new string(_Chars);

			switch(colType)
			{
				case MySqlDbType.Byte:
					if (IsUnsigned())
						value = Byte.Parse( sValue );
					else
						value = SByte.Parse( sValue );
					break;

				case MySqlDbType.Short:
					if (IsUnsigned())
						value = UInt16.Parse( sValue );
					else
						value = Int16.Parse( sValue );
					break;
					
				case MySqlDbType.Long : 
				case MySqlDbType.Int24:
					if (IsUnsigned())
						value = UInt32.Parse( sValue );
					else
						value = Int32.Parse( sValue );
					break;

				case MySqlDbType.LongLong:
					if (IsUnsigned())
						value = UInt64.Parse( sValue );
					else
						value = Int64.Parse( sValue );
					break;

				case MySqlDbType.Decimal:
					value = Decimal.Parse( sValue , numberFormat );
					break;

				case MySqlDbType.Float:
					value = Convert.ToSingle( sValue, numberFormat );
					break;

				case MySqlDbType.Double:
					value = Convert.ToDouble( sValue, numberFormat );
					break;

				case MySqlDbType.Date:
					ParseDateValue( "0000-00-00", "yyyy-MM-dd", sValue );
					break;

				case MySqlDbType.Datetime:
					ParseDateValue( "0000-00-00 00:00:00", "yyyy-MM-dd HH:mm:ss", sValue );
					break;

				case MySqlDbType.Time:
					if (sValue.Equals("00:00:00"))
						value = DBNull.Value;
					else
						value = TimeSpan.Parse(sValue);
					break;

				case MySqlDbType.Timestamp:
					string pattern;
					string null_value = "00000000000000";
					switch (ColumnLength) 
					{
						case 2:  pattern = "yy"; break;
						case 4:  pattern = "yyMM"; break;
						case 6:  pattern = "yyMMdd"; break;
						case 8:  pattern = "yyyyMMdd"; break;
						case 10: pattern = "yyMMddHHmm"; break;
						case 12: pattern = "yyMMddHHmmss"; break;
						case 14: 
						default: pattern = "yyyyMMddHHmmss"; break;
					}

					if (ColumnLength > 2 && sValue.Equals( null_value.Substring(0, ColumnLength)))
						value = DBNull.Value;
					else
						value = DateTime.ParseExact( sValue, pattern, new System.Globalization.DateTimeFormatInfo());
					break;

				case MySqlDbType.String:
				case MySqlDbType.VarChar:
				case MySqlDbType.Blob:
				case MySqlDbType.TinyBlob:
				case MySqlDbType.LongBlob:
				case MySqlDbType.MediumBlob: 
					value = sValue;
					break;

				default:
					throw new NotSupportedException();
			}
		}

		protected void ParseDateValue( string nullpattern, string pattern, string data )
		{
			if ( data.Equals (nullpattern) )
				value = DBNull.Value;
			else
				value = DateTime.ParseExact( data, pattern, new System.Globalization.DateTimeFormatInfo());
		}

		public string GetFieldTypeName() 
		{
			switch (colType) 
			{
				case MySqlDbType.Decimal:		return "DECIMAL";
				case MySqlDbType.Byte:			return "TINY";
				case MySqlDbType.Short:			return "SHORT";
				case MySqlDbType.Long:			return "LONG";
				case MySqlDbType.Float:			return "FLOAT";
				case MySqlDbType.Double:		return "DOUBLE";
				case MySqlDbType.Null:			return "NULL";
				case MySqlDbType.Timestamp:		return "TIMESTAMP";
				case MySqlDbType.LongLong:		return "LONGLONG";
				case MySqlDbType.Int24:			return "INT24";
				case MySqlDbType.Date:			return "DATE";
				case MySqlDbType.Time:			return "TIME";
				case MySqlDbType.Datetime:		return "DATETIME";
				case MySqlDbType.Year:			return "YEAR";
				case MySqlDbType.Newdate:		return "NEWDATE";
				case MySqlDbType.Enum:			return "ENUM";
				case MySqlDbType.Set:			return "SET";
				case MySqlDbType.TinyBlob:		return "TINY_BLOB";
				case MySqlDbType.MediumBlob:	return "MEDIUM_BLOB";
				case MySqlDbType.LongBlob:		return "LONG_BLOB";
				case MySqlDbType.Blob:			return "BLOB";
				case MySqlDbType.VarChar:	return "VAR_STRING";
				case MySqlDbType.String:		return "STRING";
			}
			return "Unknown typeid";
		}


		public Type GetFieldType() 
		{
			switch (colType) 
			{
				case MySqlDbType.Byte:		return IsUnsigned() ? typeof(System.Byte) : typeof(System.SByte);

				case MySqlDbType.Short:		return IsUnsigned() ? typeof(System.UInt16) : typeof(System.Int16);

				case MySqlDbType.Long:
				case MySqlDbType.Int24:		return IsUnsigned() ? typeof(System.UInt32) : typeof(System.Int32);

				case MySqlDbType.LongLong:	return IsUnsigned() ? typeof(System.UInt64) : typeof(System.Int64);

				case MySqlDbType.Float:		return typeof(System.Single);
				case MySqlDbType.Double:		return typeof(System.Double);

				case MySqlDbType.Time:		return typeof(System.TimeSpan);

				case MySqlDbType.Date:
				case MySqlDbType.Datetime:
				case MySqlDbType.Timestamp:	return typeof(System.DateTime);

				case MySqlDbType.Decimal:	return typeof(System.Decimal);

				case MySqlDbType.VarChar:	
				case MySqlDbType.String:		return typeof(System.String);

				case MySqlDbType.TinyBlob:
				case MySqlDbType.MediumBlob:
				case MySqlDbType.LongBlob:
				case MySqlDbType.Blob:		return typeof(System.Array);

				default:
				case MySqlDbType.Null:		return typeof(System.DBNull);
			}
		}

		public MySqlDbType GetMySqlDbType()
		{
			return colType;
		}

		public DbType GetDbType() 
		{
			switch (colType) 
			{
				case MySqlDbType.Decimal:		return DbType.Decimal;
				case MySqlDbType.Byte:			return DbType.Byte;
				case MySqlDbType.Short:			
					if (IsUnsigned())
						return DbType.UInt16;
					else
						return DbType.Int16;

				case MySqlDbType.Long:			
					if (IsUnsigned())
						return DbType.UInt32;
					else
						return DbType.Int32;

				case MySqlDbType.Float:			return DbType.Single;
				case MySqlDbType.Double:		return DbType.Double;
				case MySqlDbType.Null:			return DbType.Object;

				case MySqlDbType.LongLong:		
					if (IsUnsigned())
						return DbType.UInt64;
					else
						return DbType.Int64;

				case MySqlDbType.Int24:			
					if (IsUnsigned())
						return DbType.UInt32;
					else
						return DbType.Int32;
				case MySqlDbType.Date:			
				case MySqlDbType.Year:
				case MySqlDbType.Newdate:
					return DbType.Date;

				case MySqlDbType.Time:			
					return DbType.Time;
				case MySqlDbType.Datetime:		
				case MySqlDbType.Timestamp:
					return DbType.DateTime;

				case MySqlDbType.Enum:			return DbType.UInt32;
				case MySqlDbType.Set:			return DbType.Object;

				case MySqlDbType.TinyBlob:		
				case MySqlDbType.MediumBlob:
				case MySqlDbType.LongBlob:
				case MySqlDbType.Blob:
					if (IsBinary()) return DbType.Binary;
					return DbType.String;
				case MySqlDbType.VarChar:
					return DbType.String;
				case MySqlDbType.String:
					return DbType.StringFixedLength;
			}
			throw new Exception("unknown MySqlDbType");
		}

	}

}
