//
// OdbcTypeMap.cs : Mapping between different odbc types
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
// * Type mapping between various odbc driver types.
// For further infomartion between these mapping visit following msdn site
//
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/odbc/htm/
//        odbcc_data_types.asp
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/odbc/htm/
//        odbcconverting_data_from_c_to_sql_data_types.asp
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/odbc/htm/
//        odbcconverting_data_from_sql_to_c_data_types.asp
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/odbc/htm/
//        odbcparameter_data_types.asp
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/
//        html/frlrfsystemdataodbcodbctypeclasstopic.asp
//
//
// OdbcType             SQL_C_TYPE              SQL_TYPE
// ===================================================================
// BigInt		SQL_C_TYPE.SBIGINT	SQL_TYPE.BIGINT	    
// Binary		SQL_C_TYPE.BINARY	SQL_TYPE.BINARY	    
// Bit			SQL_C_TYPE.BIT		SQL_TYPE.BIT	    
// Char			SQL_C_TYPE.CHAR		SQL_TYPE.CHAR	    
// Date			SQL_C_TYPE.TYPE_DATE	SQL_TYPE.TYPE_DATE  
// DateTime		SQL_C_TYPE.TIMESTAMP	SQL_TYPE.TIMESTAMP  
// Decimal		SQL_C_TYPE.NUMERIC	SQL_TYPE.NUMERIC    
// Double		SQL_C_TYPE.DOUBLE	SQL_TYPE.DOUBLE	    
// Image		SQL_C_TYPE.BINARY	SQL_TYPE.BINARY	    
// Int			SQL_C_TYPE.LONG		SQL_TYPE.INTEGER    
// NChar		SQL_C_TYPE.WCHAR	SQL_TYPE.WCHAR	    
// NText		SQL_C_TYPE.WCHAR	SQL_TYPE.WLONGVARCHAR
// Numeric		SQL_C_TYPE.NUMERIC	SQL_TYPE.NUMERIC    
// NVarChar		SQL_C_TYPE.WCHAR	SQL_TYPE.WVARCHAR   
// Real			SQL_C_TYPE.FLOAT	SQL_TYPE.REAL	    
// SignedBigInt		SQL_C_TYPE.SBIGINT	SQL_TYPE.BIGINT	    
// SmallDateTime	SQL_C_TYPE.TIMESTAMP	SQL_TYPE.TIMESTAMP  
// SmallInt		SQL_C_TYPE.SHORT	SQL_TYPE.SMALLINT   
// Text			SQL_C_TYPE.WCHAR	SQL_TYPE.LONGVARCHAR
// Time			SQL_C_TYPE.TYPE_TIME	SQL_TYPE.TYPE_TIME  
// Timestamp		SQL_C_TYPE.BINARY	SQL_TYPE.BINARY	    
// TinyInt		SQL_C_TYPE.UTINYINT	SQL_TYPE.TINYINT    
// UniqueIdentifier	SQL_C_TYPE.GUID		SQL_TYPE.GUID	    
// VarBinary		SQL_C_TYPE.BINARY	SQL_TYPE.VARBINARY  
// VarChar		SQL_C_TYPE.WCHAR	SQL_TYPE.WVARCHAR   
//====================================================================


using System.Data;
using System.Collections;
using System.Data.Common;

namespace System.Data.Odbc
{ 
	internal struct OdbcTypeMap
	{
		public DbType         DbType;
		public OdbcType       OdbcType;
		public SQL_C_TYPE     NativeType;
		public SQL_TYPE       SqlType;

		private static Hashtable maps = new Hashtable ();

		public OdbcTypeMap (DbType dbType, OdbcType odbcType, 
				SQL_C_TYPE nativeType, SQL_TYPE sqlType)
		{
			DbType          = dbType;
			OdbcType        = odbcType;
			SqlType         = sqlType;
			NativeType      = nativeType;
		}


		public static Hashtable Maps {
			get { return maps; }
		}

		static OdbcTypeMap ()
		{
			maps [OdbcType.BigInt]    = new OdbcTypeMap (DbType.Int64,
								 OdbcType.BigInt,
								 SQL_C_TYPE.SBIGINT,
								 SQL_TYPE.BIGINT);
			maps [OdbcType.Binary]    = new OdbcTypeMap (DbType.Binary,
								 OdbcType.Binary,
								 SQL_C_TYPE.BINARY,
								 SQL_TYPE.BINARY);
			maps [OdbcType.Bit]       = new OdbcTypeMap (DbType.Boolean, 
								 OdbcType.Bit,
								 SQL_C_TYPE.BIT,
								 SQL_TYPE.BIT);
			maps [OdbcType.Char]      = new OdbcTypeMap (DbType.String, 
								 OdbcType.Char,
								 SQL_C_TYPE.CHAR,
								 SQL_TYPE.CHAR);
			maps [OdbcType.Date]      = new OdbcTypeMap (DbType.Date, 
								 OdbcType.Date,
								 SQL_C_TYPE.DATE,
								 SQL_TYPE.DATE);
			maps [OdbcType.DateTime]  = new OdbcTypeMap (DbType.DateTime, 
								 OdbcType.DateTime,
								 SQL_C_TYPE.TIMESTAMP,
								 SQL_TYPE.TIMESTAMP);
			maps [OdbcType.Decimal]   = new OdbcTypeMap (DbType.Decimal, 
								 OdbcType.Decimal,
								 SQL_C_TYPE.NUMERIC,
								 SQL_TYPE.NUMERIC);
			maps [OdbcType.Double]    = new OdbcTypeMap (DbType.Double, 
								 OdbcType.Double,
								 SQL_C_TYPE.DOUBLE,
								 SQL_TYPE.DOUBLE);
			maps [OdbcType.Image]     = new OdbcTypeMap (DbType.Binary, 
								 OdbcType.Image,
								 SQL_C_TYPE.BINARY,
								 SQL_TYPE.BINARY);
			maps [OdbcType.Int]       = new OdbcTypeMap (DbType.Int32, 
								 OdbcType.Int,
								 SQL_C_TYPE.LONG,
								 SQL_TYPE.INTEGER);
			maps [OdbcType.NChar]     = new OdbcTypeMap (DbType.String, 
								 OdbcType.NChar,
								     SQL_C_TYPE.WCHAR,
								     SQL_TYPE.WCHAR);
			maps [OdbcType.NText]     = new OdbcTypeMap (DbType.String, 
								 OdbcType.NText,
								     SQL_C_TYPE.WCHAR, // change
								     SQL_TYPE.WLONGVARCHAR); //change
			// Currently, NUMERIC types works only with NUMERIC SQL Type to CHAR C Type mapping (pgsql). Other databases return 
			// SQL_TYPE.DECIMAL in place of numeric types.
			maps [OdbcType.Numeric]   = new OdbcTypeMap (DbType.Decimal, 
								 OdbcType.Numeric,
								 SQL_C_TYPE.CHAR,
								 SQL_TYPE.NUMERIC);
			maps [OdbcType.NVarChar]  = new OdbcTypeMap (DbType.String, 
								 OdbcType.NVarChar,
								     SQL_C_TYPE.WCHAR,
								     SQL_TYPE.WVARCHAR);
			maps [OdbcType.Real]      = new OdbcTypeMap (DbType.Single, 
								 OdbcType.Real,
								 SQL_C_TYPE.FLOAT,
								 SQL_TYPE.REAL);
			maps [OdbcType.SmallDateTime] = new OdbcTypeMap (DbType.DateTime, 
								     OdbcType.SmallDateTime,
								     SQL_C_TYPE.TIMESTAMP,
								     SQL_TYPE.TIMESTAMP);
			maps [OdbcType.SmallInt]  = new OdbcTypeMap (DbType.Int16, 
								 OdbcType.SmallInt,
								 SQL_C_TYPE.SHORT,
								 SQL_TYPE.SMALLINT);
			maps [OdbcType.Text]      = new OdbcTypeMap (DbType.String, 
								 OdbcType.Text,
								     SQL_C_TYPE.CHAR, //change
								 SQL_TYPE.LONGVARCHAR);
			maps [OdbcType.Time]      = new OdbcTypeMap (DbType.DateTime, 
								 OdbcType.Time,
								 SQL_C_TYPE.TIME,
								 SQL_TYPE.TIME);
			maps [OdbcType.Timestamp] = new OdbcTypeMap (DbType.DateTime, 
								 OdbcType.Timestamp,
								 SQL_C_TYPE.BINARY,
								 SQL_TYPE.BINARY);
			maps [OdbcType.TinyInt]   = new OdbcTypeMap (DbType.SByte, 
								 OdbcType.TinyInt,
								 SQL_C_TYPE.UTINYINT,
								 SQL_TYPE.TINYINT);
			maps [OdbcType.UniqueIdentifier] = new OdbcTypeMap (DbType.Guid, 
									OdbcType.UniqueIdentifier,
									SQL_C_TYPE.GUID,
									SQL_TYPE.GUID);
			maps [OdbcType.VarBinary] = new OdbcTypeMap (DbType.Binary, 
								 OdbcType.VarBinary,
								 SQL_C_TYPE.BINARY,
								 SQL_TYPE.VARBINARY);
			maps [OdbcType.VarChar]   = new OdbcTypeMap (DbType.String, 
								 OdbcType.VarChar,
								 SQL_C_TYPE.CHAR,
								 SQL_TYPE.VARCHAR);
 		}

	}
}