//
// System.Data.Odbc.OdbcType
//
// Author:
//   Brian Ritchie
//
// Copyright (C) Brian Ritchie, 2002
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

using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{ 

// From the ODBC documentation:
//
//	In ODBC 3.x, the identifiers for date, time, and timestamp SQL data types 
//  have changed from SQL_DATE, SQL_TIME, and SQL_TIMESTAMP (with instances of 
//  #define in the header file of 9, 10, and 11) to SQL_TYPE_DATE, SQL_TYPE_TIME,
//  and SQL_TYPE_TIMESTAMP (with instances of #define in the header file of 91, 92, and 93), 
//  respectively.

	// Unmapped SQL Types
	//
	//#define SQL_FLOAT								6
	//	could map to SQL_DOUBLE?
	//#define SQL_INTERVAL							10
	//	could map to SmallDateTime?

	public enum OdbcType
	{
		BigInt=-5,		// SQL_BIGINT
		Binary=-2,		// SQL_BINARY
		Bit=-7,			// SQL_BIT
		Char=1,			// SQL_CHAR
		Date=91,		// SQL_TYPE_DATE
		DateTime=9,		// SQL_DATETIME
		Decimal=3,		// SQL_DECIMAL
		Double=8,		// SQL_DOUBLE
		Image=-4,		// SQL_LONGVARBINARY
		Int=4,			// SQL_INTEGER
		NChar=-95,		// SQL_UNICODE_CHAR
		NText=-97,		// SQL_UNICODE_LONGVARCHAR
		Numeric=2,		// SQL_NUMERIC
		NVarChar=-96,	// SQL_UNICODE_VARCHAR
		Real=7,			// SQL_REAL
		SmallDateTime=0,// ??????????????????????????
		SmallInt=5,		// SQL_SMALLINT
		Time=92,		// SQL_TYPE_TIME
		Text=-1,		// SQL_LONGVARCHAR
		Timestamp=93,	// SQL_TYPE_TIMESTAMP
		TinyInt=-6,		// SQL_TINYINT
		UniqueIdentifier=-11,  // SQL_GUID
		VarBinary=-3,	// SQL_VARBINARY
		VarChar=12		// SQL_VARCHAR
	}
}
