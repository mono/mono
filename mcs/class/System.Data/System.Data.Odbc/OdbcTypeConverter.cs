//
// OdbcTypeConvert.cs : helps conversion between various odbc types.
//
// Author:
//   Sureshkumar T <tsureshkumar@novell.com>
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
//
// For mapping between types, refer OdbcTypeMap.cs
//
//


using System.Data;
using System.Collections;
using System.Data.Common;

namespace System.Data.Odbc
{ 
	internal class OdbcTypeConverter  
	{
		public static OdbcTypeMap GetTypeMap (OdbcType odbcType)
		{
			return (OdbcTypeMap) OdbcTypeMap.Maps [odbcType];
		}

		public static OdbcTypeMap InferFromValue (object value)
		{

			if (value.GetType ().IsArray)
				if (value.GetType ().GetElementType () == typeof (byte))
					return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Binary];
				else
					return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.VarChar]; // change

			switch (Type.GetTypeCode (value.GetType ())) {
			case TypeCode.Empty:
			case TypeCode.Object:
			case TypeCode.DBNull:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.NVarChar];  //Default to NVarChar as in MS.net. OdbcParameter.Bind() will take care.
			case TypeCode.Boolean:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Bit];
			case TypeCode.Char:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Char];
			case TypeCode.SByte:
				throw new ArgumentException ("infering OdbcType from SByte is not supported");
			case TypeCode.Byte:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.TinyInt];
			case TypeCode.Int16:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.SmallInt];
			case TypeCode.UInt16:
			case TypeCode.Int32:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Int];
			case TypeCode.UInt32:
			case TypeCode.Int64:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.BigInt];
			case TypeCode.UInt64:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Numeric];
			case TypeCode.Single:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Real];
			case TypeCode.Double:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Double];
			case TypeCode.Decimal:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Numeric];
			case TypeCode.DateTime:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.DateTime];
			case TypeCode.String:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.NVarChar];
			}

			// FIXME : Guid
			// FIXME : TimeSpan
			// FIXME : DateTime

			return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.VarChar];
		}

		public static OdbcTypeMap GetTypeMap (SQL_TYPE sqlType) 
		{
			switch (sqlType) {
			case SQL_TYPE.BINARY:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Binary];
			case SQL_TYPE.BIT:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Bit];
			case SQL_TYPE.CHAR:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Char];
			case SQL_TYPE.DATE:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Date];
			case SQL_TYPE.DECIMAL:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Decimal];
			case SQL_TYPE.DOUBLE:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Double];
			case SQL_TYPE.GUID:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.UniqueIdentifier];
			case SQL_TYPE.INTEGER:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Int];
			case SQL_TYPE.INTERVAL_DAY:
			case SQL_TYPE.INTERVAL_DAY_TO_HOUR:
			case SQL_TYPE.INTERVAL_DAY_TO_MINUTE:
			case SQL_TYPE.INTERVAL_DAY_TO_SECOND:
			case SQL_TYPE.INTERVAL_HOUR:
			case SQL_TYPE.INTERVAL_HOUR_TO_MINUTE:
			case SQL_TYPE.INTERVAL_HOUR_TO_SECOND:
			case SQL_TYPE.INTERVAL_MINUTE:
			case SQL_TYPE.INTERVAL_MINUTE_TO_SECOND:
			case SQL_TYPE.INTERVAL_MONTH:
			case SQL_TYPE.INTERVAL_SECOND:
			case SQL_TYPE.INTERVAL_YEAR:
			case SQL_TYPE.INTERVAL_YEAR_TO_MONTH:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.DateTime];
			case SQL_TYPE.LONGVARBINARY:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Image];
			case SQL_TYPE.LONGVARCHAR:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Text];
			case SQL_TYPE.NUMERIC:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Numeric];
			case SQL_TYPE.REAL:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Real];
			case SQL_TYPE.SMALLINT:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.SmallInt];
			case SQL_TYPE.TYPE_TIME:
			case SQL_TYPE.TIME:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.Time];
			case SQL_TYPE.TIMESTAMP:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.DateTime];
			case SQL_TYPE.TINYINT:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.TinyInt];
			case SQL_TYPE.TYPE_DATE:
			case SQL_TYPE.TYPE_TIMESTAMP:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.DateTime];
			case SQL_TYPE.VARBINARY:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.VarBinary];
			case SQL_TYPE.VARCHAR:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.VarChar];
			case SQL_TYPE.WCHAR:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.NChar];
			case SQL_TYPE.WLONGVARCHAR:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.NText];
			case SQL_TYPE.WVARCHAR:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.NVarChar];
			case SQL_TYPE.UNASSIGNED:
				return (OdbcTypeMap)  OdbcTypeMap.Maps [OdbcType.VarChar];
			}
			return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.VarChar];
		}

		public static OdbcTypeMap GetTypeMap (DbType dbType) 
		{
			switch (dbType) {
			case DbType.AnsiString:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.VarChar];
			case DbType.Binary:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Binary];
			case DbType.Byte:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.TinyInt];
			case DbType.Boolean:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Bit];
			case DbType.Currency:
				throw new NotSupportedException ("Infering OdbcType from DbType.Currency is not" +
								 " supported");
			case DbType.Date:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Date];
			case DbType.DateTime:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.DateTime];
			case DbType.Decimal:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Numeric];
			case DbType.Double:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Double];
			case DbType.Guid:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.UniqueIdentifier];
			case DbType.Int16:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.SmallInt];
			case DbType.Int32:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Int];
			case DbType.Int64:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.BigInt];
			case DbType.Object:
				throw new NotSupportedException ("Infering OdbcType from DbType.Object is not" +
								 " supported");
			case DbType.SByte:
				throw new NotSupportedException ("Infering OdbcType from DbType.SByte is not" +
								 " supported");
			case DbType.Single:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Real];
			case DbType.String:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.NVarChar];
			case DbType.Time:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Time];
			case DbType.UInt16:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Int];
			case DbType.UInt32:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.BigInt];
			case DbType.UInt64:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Numeric];
			case DbType.VarNumeric:
				throw new NotSupportedException ("Infering OdbcType from DbType.VarNumeric is not" +
								 " supported");
			case DbType.AnsiStringFixedLength:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.Char];
			case DbType.StringFixedLength:
				return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.NChar];
			}
			return (OdbcTypeMap) OdbcTypeMap.Maps [OdbcType.VarChar];
		}
	}
}
