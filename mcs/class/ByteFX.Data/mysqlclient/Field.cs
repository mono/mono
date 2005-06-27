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
using ByteFX.Data.Common;

namespace ByteFX.Data.MySqlClient
{
	internal enum ColumnFlags : int
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
		protected	MySqlDbType	colType;
		protected	ColumnFlags	colFlags;
		protected	int			colDecimals;
		protected	Encoding	encoding;
		private static NumberFormatInfo		numberFormat = null;

		protected	byte[]		buffer;
		protected	long		bufIndex;
		protected	long		bufLength;

		public MySqlField(Encoding encoding)
		{
			this.encoding = encoding;
			if (numberFormat == null)
			{
				numberFormat = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
				numberFormat.NumberDecimalSeparator = ".";
			}
		}

		public byte[] Buffer 
		{
			get { return buffer; }
		}

		public long	BufferIndex 
		{
			get { return bufIndex; }
		}

		public long BufferLength 
		{
			get { return bufLength; }
		}

		/// <summary>
		/// CopyBuffer makes a copy of the byte buffer given to us while
		/// the rowset was being read
		/// </summary>
		private void CopyBuffer()
		{
			byte[] newbuf = new byte[ bufLength ];
			long oldIndex = bufIndex;
			for (long i=0; i < bufLength; i++)
				newbuf[i] = buffer[ oldIndex++ ];
			bufIndex = 0;
			buffer = newbuf;
			value = newbuf;
		}

		/// <summary>
		/// GetValue returns an object that represents the value of this field.
		/// </summary>
		/// <returns></returns>
		public object GetValue() 
		{
			// if our value is a byte buffer and we are using only 
			// a part of that buffer, then we need to make a copy
			if (value is byte[] && (bufIndex > 0 || bufLength < buffer.Length))
				CopyBuffer();
			return value;
		}

		public MySqlDbType	Type 
		{
			get { return colType; }
			set { colType = value; }
		}

		public int NumericPrecision
		{
			get 
			{
				if (colType == MySqlDbType.Decimal)
					return colLen;
				return -1;
			}

		}

		public int NumericScale
		{
			get 
			{
				if (colType == MySqlDbType.Decimal)
					return colDecimals;
				return -1;
			}
			set 
			{
				colDecimals = value;
			}
		}

		public ColumnFlags Flags 
		{ 
			get { return colFlags; }
			set { colFlags = value; }
		}

		public bool IsAutoIncrement()
		{
			return (colFlags & ColumnFlags.AUTO_INCREMENT) > 0;
		}

		public bool IsNumeric()
		{
			return (colFlags & ColumnFlags.NUMBER) > 0;
		}

		public bool AllowsNull()
		{
			return (colFlags & ColumnFlags.NOT_NULL) == 0;
		}

		public bool IsUnique()
		{
			return (colFlags & ColumnFlags.UNIQUE_KEY) > 0;
		}

		public bool IsPrimaryKey()
		{
			return (colFlags & ColumnFlags.PRIMARY_KEY) > 0;
		}

		public bool IsBlob() 
		{
			return (colFlags & ColumnFlags.BLOB) > 0;
		}

		public bool IsBinary()
		{
			return (colFlags & ColumnFlags.BINARY) > 0;
		}

		public bool IsUnsigned()
		{
			return (colFlags & ColumnFlags.UNSIGNED) > 0;
		}

		public void SetValueData( byte[] buf, long index, long len, DBVersion version )
		{
			if (len == -1)
			{
				value = DBNull.Value;
				buffer = null;
				return;
			}

			buffer = buf;
			bufIndex = index;
			bufLength = len;

			// if it is a blob and binary, then GetBytes is the way to go
			if ( IsBlob() && IsBinary() ) 
			{
				dbType = DbType.Binary;
				value = buffer;
				return;
			}

			string sValue = encoding.GetString( buf, (int)index, (int)len );

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
					
				case MySqlDbType.Int : 
				case MySqlDbType.Int24:
					if (IsUnsigned())
						value = UInt32.Parse( sValue );
					else
						value = Int32.Parse( sValue );
					break;

				case MySqlDbType.BigInt:
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

				case MySqlDbType.Year:
					value = Int32.Parse( sValue );
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
					// MySql 4.1.0 and later use DateTime format for timestamp
					if (version.isAtLeast(4,1,0))  
					{
						ParseDateValue( "0000-00-00 00:00:00", "yyyy-MM-dd HH:mm:ss", sValue );
						return;
					}

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
				case MySqlDbType.Int:			return "INTEGER";
				case MySqlDbType.Float:			return "FLOAT";
				case MySqlDbType.Double:		return "DOUBLE";
				case MySqlDbType.Null:			return "NULL";
				case MySqlDbType.Timestamp:		return "TIMESTAMP";
				case MySqlDbType.BigInt:		return "BIGINT";
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

				case MySqlDbType.Int:
				case MySqlDbType.Int24:		return IsUnsigned() ? typeof(System.UInt32) : typeof(System.Int32);

				case MySqlDbType.BigInt:	return IsUnsigned() ? typeof(System.UInt64) : typeof(System.Int64);

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
				case MySqlDbType.Blob:		
					if ((colFlags & ColumnFlags.BINARY) != 0)
						return typeof(System.Array);
					else
						return typeof(System.String);

				case MySqlDbType.Year:
					return typeof(System.Int32);

				default:
				case MySqlDbType.Null:		return typeof(System.DBNull);
			}
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

				case MySqlDbType.Int:			
					if (IsUnsigned())
						return DbType.UInt32;
					else
						return DbType.Int32;

				case MySqlDbType.Float:			return DbType.Single;
				case MySqlDbType.Double:		return DbType.Double;
				case MySqlDbType.Null:			return DbType.Object;

				case MySqlDbType.BigInt:		
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
