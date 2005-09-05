/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Data;

namespace FirebirdSql.Data.Common
{
	internal sealed class TypeHelper
	{
		#region Constructors

		private TypeHelper()
		{
		}

		#endregion

		#region Static Methods

		public static short GetSize(DbDataType dataType)
		{
			switch (dataType)
			{
				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
					return 8;

				case DbDataType.SmallInt:
					return 2;

				case DbDataType.Integer:
				case DbDataType.Float:
				case DbDataType.Date:
				case DbDataType.Time:
					return 4;

				case DbDataType.BigInt:
				case DbDataType.Double:
				case DbDataType.TimeStamp:
					return 8;

				case DbDataType.Guid:
					return 16;

				default:
					return 0;
			}
		}

		public static int GetFbType(DbDataType dataType, bool isNullable)
		{
			int sqltype = 0;

			switch (dataType)
			{
				case DbDataType.Array:
					sqltype = IscCodes.SQL_ARRAY;
					break;

				case DbDataType.Binary:
				case DbDataType.Text:
					sqltype = IscCodes.SQL_BLOB;
					break;

				case DbDataType.Char:
					sqltype = IscCodes.SQL_TEXT;
					break;

				case DbDataType.VarChar:
					sqltype = IscCodes.SQL_VARYING;
					break;

				case DbDataType.SmallInt:
					sqltype = IscCodes.SQL_SHORT;
					break;

				case DbDataType.Integer:
					sqltype = IscCodes.SQL_LONG;
					break;

				case DbDataType.BigInt:
					sqltype = IscCodes.SQL_INT64;
					break;

				case DbDataType.Float:
					sqltype = IscCodes.SQL_FLOAT;
					break;

				case DbDataType.Guid:
					sqltype = IscCodes.SQL_TEXT;
					break;

				case DbDataType.Double:
					sqltype = IscCodes.SQL_DOUBLE;
					break;

				case DbDataType.Date:
					sqltype = IscCodes.SQL_TYPE_DATE;
					break;

				case DbDataType.Time:
					sqltype = IscCodes.SQL_TYPE_TIME;
					break;

				case DbDataType.TimeStamp:
					sqltype = IscCodes.SQL_TIMESTAMP;
					break;

				default:
					throw new ArgumentException("Invalid data type");
			}

			if (isNullable)
			{
				sqltype++;
			}

			return sqltype;
		}

		public static int GetFbType(int blrType)
		{
			switch (blrType)
			{
				case IscCodes.blr_varying:
				case IscCodes.blr_varying2:
					return IscCodes.SQL_VARYING;

				case IscCodes.blr_text:
				case IscCodes.blr_text2:
				case IscCodes.blr_cstring:
				case IscCodes.blr_cstring2:
					return IscCodes.SQL_TEXT;

				case IscCodes.blr_short:
					return IscCodes.SQL_SHORT;

				case IscCodes.blr_long:
					return IscCodes.SQL_LONG;

				case IscCodes.blr_quad:
					return IscCodes.SQL_QUAD;

				case IscCodes.blr_int64:
				case IscCodes.blr_blob_id:
					return IscCodes.SQL_INT64;

				case IscCodes.blr_double:
					return IscCodes.SQL_DOUBLE;

				case IscCodes.blr_d_float:
					return IscCodes.SQL_D_FLOAT;

				case IscCodes.blr_float:
					return IscCodes.SQL_FLOAT;

				case IscCodes.blr_sql_date:
					return IscCodes.SQL_TYPE_DATE;

				case IscCodes.blr_sql_time:
					return IscCodes.SQL_TYPE_TIME;

				case IscCodes.blr_timestamp:
					return IscCodes.SQL_TIMESTAMP;

				case IscCodes.blr_blob:
					return IscCodes.SQL_BLOB;

				default:
					throw new ArgumentException("Invalid data type");
			}
		}

		public static DbDataType GetDbDataType(int blrType, int subType, int scale)
		{
			switch (blrType)
			{
				case IscCodes.blr_varying:
				case IscCodes.blr_varying2:
					return DbDataType.VarChar;

				case IscCodes.blr_text:
				case IscCodes.blr_text2:
					return DbDataType.Char;

				case IscCodes.blr_cstring:
				case IscCodes.blr_cstring2:
					return DbDataType.Text;

				case IscCodes.blr_short:
                    if (subType == 2)
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

				case IscCodes.blr_long:
                    if (subType == 2)
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

				case IscCodes.blr_quad:
				case IscCodes.blr_int64:
				case IscCodes.blr_blob_id:
                    if (subType == 2)
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

				case IscCodes.blr_double:
				case IscCodes.blr_d_float:
					return DbDataType.Double;

				case IscCodes.blr_float:
					return DbDataType.Float;

				case IscCodes.blr_sql_date:
					return DbDataType.Date;

				case IscCodes.blr_sql_time:
					return DbDataType.Time;

				case IscCodes.blr_timestamp:
					return DbDataType.TimeStamp;

				case IscCodes.blr_blob:
					if (subType == 1)
					{
						return DbDataType.Text;
					}
					else
					{
						return DbDataType.Binary;
					}

				default:
					throw new ArgumentException("Invalid data type");
			}
		}

		public static string GetDataTypeName(DbDataType dataType)
		{
			switch (dataType)
			{
				case DbDataType.Array:
					return "ARRAY";

				case DbDataType.Binary:
					return "BLOB";

				case DbDataType.Text:
					return "BLOB SUB_TYPE 1";

				case DbDataType.Char:
				case DbDataType.Guid:
					return "CHAR";

				case DbDataType.VarChar:
					return "VARCHAR";

				case DbDataType.SmallInt:
					return "SMALLINT";

				case DbDataType.Integer:
					return "INTEGER";

				case DbDataType.Float:
					return "FLOAT";

				case DbDataType.Double:
					return "DOUBLE PRECISION";

				case DbDataType.BigInt:
					return "BIGINT";

				case DbDataType.Numeric:
					return "NUMERIC";

				case DbDataType.Decimal:
					return "DECIMAL";

				case DbDataType.Date:
					return "DATE";

				case DbDataType.Time:
					return "TIME";

				case DbDataType.TimeStamp:
					return "TIMESTAMP";

				default:
					return null;
			}
		}

		public static DbType GetDbType(DbDataType type)
		{
			switch (type)
			{
				case DbDataType.Array:
				case DbDataType.Binary:
					return DbType.Binary;

				case DbDataType.Text:
				case DbDataType.VarChar:
				case DbDataType.Char:
					return DbType.String;

				case DbDataType.SmallInt:
					return DbType.Int16;

				case DbDataType.Integer:
					return DbType.Int32;

				case DbDataType.BigInt:
					return DbType.Int64;

				case DbDataType.Date:
					return DbType.Date;

				case DbDataType.Time:
					return DbType.Time;

				case DbDataType.TimeStamp:
					return DbType.DateTime;

				case DbDataType.Numeric:
				case DbDataType.Decimal:
					return DbType.Decimal;

				case DbDataType.Float:
					return DbType.Single;

				case DbDataType.Double:
					return DbType.Double;

				case DbDataType.Guid:
					return DbType.Guid;

				default:
					throw new ArgumentException("Invalid data type");
			}
		}

		public static DbDataType GetDbDataType(DbType dbType)
		{
			switch (dbType)
			{
				case DbType.String:
				case DbType.AnsiString:
					return DbDataType.VarChar;

				case DbType.StringFixedLength:
				case DbType.AnsiStringFixedLength:
					return DbDataType.Char;

				case DbType.Boolean:
				case DbType.Byte:
				case DbType.SByte:
				case DbType.Int16:
				case DbType.UInt16:
					return DbDataType.SmallInt;

				case DbType.Int32:
				case DbType.UInt32:
					return DbDataType.Integer;

				case DbType.Int64:
				case DbType.UInt64:
					return DbDataType.BigInt;

				case DbType.Date:
					return DbDataType.Date;

				case DbType.Time:
					return DbDataType.Time;

				case DbType.DateTime:
					return DbDataType.TimeStamp;

				case DbType.Object:
				case DbType.Binary:
					return DbDataType.Binary;

				case DbType.Decimal:
					return DbDataType.Decimal;

				case DbType.Double:
					return DbDataType.Double;

				case DbType.Single:
					return DbDataType.Float;

				case DbType.Guid:
					return DbDataType.Guid;

				default:
					throw new ArgumentException("Invalid data type");
			}
		}

		#endregion
	}
}
