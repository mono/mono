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

namespace ByteFX.Data.MySQLClient
{
	

	internal enum MySQLFieldType : byte
	{
		DECIMAL		=   0,
		TINY        =   1,
		BYTE		=   1,
		SHORT       =   2,
		LONG        =   3,
		FLOAT       =   4,
		DOUBLE      =   5,
		NULL        =   6,
		TIMESTAMP   =   7,
		LONGLONG    =   8,
		INT24       =   9,
		DATE        =  10,
		TIME        =  11,
		DATETIME    =  12,
		YEAR        =  13,
		NEWDATE     =  14,
		ENUM        = 247,
		SET         = 248,
		TINY_BLOB   = 249,
		MEDIUM_BLOB = 250,
		LONG_BLOB   = 251,
		BLOB        = 252,
		VAR_STRING  = 253,
		STRING      = 254
	};

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
	internal class MySQLField : Common.Field
	{
		  MySQLFieldType	m_FieldType;
		public	ColFlags		ColumnFlags;
		public	int				ColumnDecimals;
		object					m_Value;

		public MySQLField()
		{
		}

		public void ReadSchemaInfo( Driver d )
		{	
			d.ReadPacket();

			m_TableName = d.ReadLenString();
			m_ColName = d.ReadLenString();
			m_ColLen = d.ReadNBytes();
			m_FieldType = (MySQLFieldType)d.ReadNBytes();
			d.ReadByte();									// this is apparently 2 -- not sure what it is for
			ColumnFlags = (ColFlags)d.ReadInteger(2);		//(short)(d.ReadByte() & 0xff);
			ColumnDecimals = d.ReadByte();
		}

		public object GetValue() 
		{
			return m_Value;
		}

		public int NumericPrecision()
		{
			if (m_FieldType == MySQLFieldType.DECIMAL)
				return ColumnLength;
			return -1;
		}

		public int NumericScale()
		{
			if (m_FieldType == MySQLFieldType.DECIMAL)
				return ColumnDecimals;
			return -1;
		}

		public bool IsAutoIncrement()
		{
			return (ColumnFlags & ColFlags.AUTO_INCREMENT) > 0;
		}

		public bool IsNumeric()
		{
			return (ColumnFlags & ColFlags.NUMBER) > 0;
		}

		public bool AllowsNull()
		{
			return (ColumnFlags & ColFlags.NOT_NULL) == 0;
		}

		public bool IsUnique()
		{
			return (ColumnFlags & ColFlags.UNIQUE_KEY) > 0;
		}

		public bool IsPrimaryKey()
		{
			return (ColumnFlags & ColFlags.PRIMARY_KEY) > 0;
		}

		public bool IsBlob() 
		{
			return (ColumnFlags & ColFlags.BLOB) > 0;
		}

		public bool IsBinary()
		{
			return (ColumnFlags & ColFlags.BINARY) > 0;
		}

		public bool IsUnsigned()
		{
			return (ColumnFlags & ColFlags.UNSIGNED) > 0;
		}

		public void SetValueData( byte[] data )
		{
			if (data == null) 
			{
				m_Value = null;
				return;
			}

			// if it is a blob and binary, then GetBytes is the way to go
			if ( IsBlob() && IsBinary() ) 
			{
				m_DbType = DbType.Binary;
				return;
			}

			char[] _Chars = System.Text.Encoding.ASCII.GetChars( data );
			string sValue = new string(_Chars);

			switch(m_FieldType)
			{
				case MySQLFieldType.BYTE:
					if (IsUnsigned()) 
						m_Value = Byte.Parse( sValue );
					else 
						m_Value = SByte.Parse( sValue );
					break;

				case MySQLFieldType.SHORT:
					if (IsUnsigned()) 
						m_Value = UInt16.Parse( sValue );
					else 
						m_Value = Int16.Parse( sValue );
					break;

				case MySQLFieldType.INT24:
				case MySQLFieldType.LONG : 
					if (IsUnsigned()) 
						m_Value = UInt32.Parse( sValue );
					else 
						m_Value = Int32.Parse( sValue );
					break;

				case MySQLFieldType.LONGLONG:
					if (IsUnsigned()) 
						m_Value = UInt64.Parse( sValue );
					else 
						m_Value = Int64.Parse( sValue );
					break;

				case MySQLFieldType.FLOAT:
					m_Value = Single.Parse( sValue );
					break;

				case MySQLFieldType.DOUBLE:
					m_Value = Double.Parse( sValue );
					break;

				case MySQLFieldType.DECIMAL:
					m_Value = Decimal.Parse( sValue );
					break;
			
				case MySQLFieldType.DATE:
					if (sValue == "0000-00-00")
						m_Value = null;
					else
						m_Value = DateTime.ParseExact( sValue, "yyyy-MM-dd", new System.Globalization.DateTimeFormatInfo());
						
					break;

				case MySQLFieldType.DATETIME:
					if (sValue == "0000-00-00 00:00:00")
						m_Value = null;
					else
						m_Value = DateTime.ParseExact( sValue, "yyyy-MM-dd HH:mm:ss", new System.Globalization.DateTimeFormatInfo());

					break;

				case MySQLFieldType.TIME:
					m_Value = TimeSpan.Parse(sValue);
					break;

				case MySQLFieldType.TIMESTAMP:
					string pattern;
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
					m_Value = DateTime.ParseExact( sValue, pattern, new System.Globalization.DateTimeFormatInfo());
					break;

				case MySQLFieldType.STRING:
				case MySQLFieldType.VAR_STRING:
				case MySQLFieldType.BLOB:
				case MySQLFieldType.TINY_BLOB:
				case MySQLFieldType.LONG_BLOB:
				case MySQLFieldType.MEDIUM_BLOB:
					m_Value = sValue;
					break;

				default:
					throw new NotSupportedException();
			}
		}

		public string GetFieldTypeName() 
		{
			switch (m_FieldType) 
			{
				case MySQLFieldType.DECIMAL:		return "DECIMAL";
				case MySQLFieldType.TINY:			return "TINY";
				case MySQLFieldType.SHORT:			return "SHORT";
				case MySQLFieldType.LONG:			return "LONG";
				case MySQLFieldType.FLOAT:			return "FLOAT";
				case MySQLFieldType.DOUBLE:			return "DOUBLE";
				case MySQLFieldType.NULL:			return "NULL";
				case MySQLFieldType.TIMESTAMP:		return "TIMESTAMP";
				case MySQLFieldType.LONGLONG:		return "LONGLONG";
				case MySQLFieldType.INT24:			return "INT24";
				case MySQLFieldType.DATE:			return "DATE";
				case MySQLFieldType.TIME:			return "TIME";
				case MySQLFieldType.DATETIME:		return "DATETIME";
				case MySQLFieldType.YEAR:			return "YEAR";
				case MySQLFieldType.NEWDATE:		return "NEWDATE";
				case MySQLFieldType.ENUM:			return "ENUM";
				case MySQLFieldType.SET:			return "SET";
				case MySQLFieldType.TINY_BLOB:		return "TINY_BLOB";
				case MySQLFieldType.MEDIUM_BLOB:	return "MEDIUM_BLOB";
				case MySQLFieldType.LONG_BLOB:		return "LONG_BLOB";
				case MySQLFieldType.BLOB:			return "BLOB";
				case MySQLFieldType.VAR_STRING:	return "VAR_STRING";
				case MySQLFieldType.STRING:		return "STRING";
			}
			return "Unknown typeid";
		}


		public Type GetFieldType() 
		{
			switch (m_FieldType) 
			{
				case MySQLFieldType.BYTE:		return Type.GetType("System.Byte");

				case MySQLFieldType.SHORT:		return Type.GetType("System.Int16");

				case MySQLFieldType.LONG:
				case MySQLFieldType.INT24:		return Type.GetType("System.Int32", false, true);

				case MySQLFieldType.FLOAT:		return Type.GetType("System.Single");
				case MySQLFieldType.DOUBLE:		return Type.GetType("System.Double");

				case MySQLFieldType.TIME:		return Type.GetType("System.TimeSpan");

				case MySQLFieldType.DATE:
				case MySQLFieldType.DATETIME:
				case MySQLFieldType.TIMESTAMP:	return Type.GetType("System.DateTime");

				case MySQLFieldType.LONGLONG:	return Type.GetType("System.Int64");

				case MySQLFieldType.DECIMAL:	return Type.GetType("System.Decimal");

				case MySQLFieldType.VAR_STRING:	
				case MySQLFieldType.STRING:		return Type.GetType("System.String");

				case MySQLFieldType.TINY_BLOB:
				case MySQLFieldType.MEDIUM_BLOB:
				case MySQLFieldType.LONG_BLOB:
				case MySQLFieldType.BLOB:		return Type.GetType("System.Array");

				default:
				case MySQLFieldType.NULL:		return Type.GetType("System.null", false, true);
			}
		}

		public DbType GetDbType() 
		{
			switch (m_FieldType) 
			{
				case MySQLFieldType.DECIMAL:		return DbType.Decimal;
				case MySQLFieldType.TINY:			return DbType.Byte;
				case MySQLFieldType.SHORT:			
					if (IsUnsigned())
						return DbType.UInt16;
					else
						return DbType.Int16;

				case MySQLFieldType.LONG:			
					if (IsUnsigned())
						return DbType.UInt32;
					else
						return DbType.Int32;

				case MySQLFieldType.FLOAT:			return DbType.Double;
				case MySQLFieldType.DOUBLE:			return DbType.Double;
				case MySQLFieldType.NULL:			return DbType.Object;

				case MySQLFieldType.LONGLONG:		
					if (IsUnsigned())
						return DbType.UInt64;
					else
						return DbType.Int64;

				case MySQLFieldType.INT24:			
					if (IsUnsigned())
						return DbType.UInt32;
					else
						return DbType.Int32;
				case MySQLFieldType.DATE:			
				case MySQLFieldType.YEAR:
				case MySQLFieldType.NEWDATE:
					return DbType.Date;

				case MySQLFieldType.TIME:			
					return DbType.Time;
				case MySQLFieldType.DATETIME:		
				case MySQLFieldType.TIMESTAMP:
					return DbType.DateTime;

				case MySQLFieldType.ENUM:			return DbType.UInt32;
				case MySQLFieldType.SET:			return DbType.Object;

				case MySQLFieldType.TINY_BLOB:		
				case MySQLFieldType.MEDIUM_BLOB:
				case MySQLFieldType.LONG_BLOB:
				case MySQLFieldType.BLOB:
					if (IsBinary()) return DbType.Binary;
					return DbType.String;
				case MySQLFieldType.VAR_STRING:
					return DbType.String;
				case MySQLFieldType.STRING:
					return DbType.StringFixedLength;
			}
			throw new Exception("unknown MySQLFieldType");
		}

	}

}
