//
// System.Data.Odbc.OdbcType
//
// Author:
//   Sureshkumar T <tsureshkumar@novell.com> 2005.
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

        public enum OdbcType
        {
                BigInt = 1,
                Binary = 2,
                Bit = 3,
                Char = 4,
                Date = 0x17,
                DateTime = 5,
                Decimal = 6,
                Double = 8,
                Image = 9,
                Int = 10,
                NChar = 11,
                NText = 12,
                Numeric = 7,
                NVarChar = 13,
                Real = 14,
                SmallDateTime = 0x10,
                SmallInt = 0x11,
                Text = 0x12,
                Time = 0x18,
                Timestamp = 0x13,
                TinyInt = 20,
                UniqueIdentifier = 15,
                VarBinary = 0x15,
                VarChar = 0x16
        }

        // From the ODBC documentation:
        //
        //	In ODBC 3.x, the identifiers for date, time, and timestamp SQL data types 
        //  have changed from SQL_DATE, SQL_TIME, and SQL_TIMESTAMP (with instances of 
        //  #define in the header file of 9, 10, and 11) to SQL_TYPE_DATE, SQL_TYPE_TIME,
        //  and SQL_TYPE_TIMESTAMP (with instances of #define in the header file of 91, 92, and 93), 
        //  respectively.
        
        // This internal enum is used as mapping types into database drivers.
        // This is essentially a map between public OdbcType to C types for 
        // Odbc to call into driver. These values are taken from sql.h & sqlext.h.
        internal enum SQL_TYPE : short
        {
                BIGINT				= (-5),
                BINARY				= (-2),
                BIT				= (-7),
                CHAR				= 1,
                DATE				= 9,
                DECIMAL                         = 3,
                DOUBLE				= 8,
                GUID				= (-11),
                INTEGER				= 4,
                INTERVAL_DAY			= (100 + 3),
                INTERVAL_DAY_TO_HOUR		= (100 + 8),
                INTERVAL_DAY_TO_MINUTE		= (100 + 9),
                INTERVAL_DAY_TO_SECOND		= (100 + 10),
                INTERVAL_HOUR			= (100 + 4),
                INTERVAL_HOUR_TO_MINUTE		= (100 + 11),
                INTERVAL_HOUR_TO_SECOND		= (100 + 12),
                INTERVAL_MINUTE			= (100 + 5),
                INTERVAL_MINUTE_TO_SECOND	= (100 + 13),
                INTERVAL_MONTH			= (100 + 2),
                INTERVAL_SECOND			= (100 + 6),
                INTERVAL_YEAR			= (100 + 1),
                INTERVAL_YEAR_TO_MONTH		= (100 + 7),
                LONGVARBINARY                   = (-4),
                LONGVARCHAR                     = (-1),
                NUMERIC                         = 2,
                REAL				= 7,
                SMALLINT			= 5,
                TIME				= 10,
                TIMESTAMP			= 11,
                TINYINT				= (-6),
                TYPE_DATE			= 91,
                TYPE_TIME			= 92,
                TYPE_TIMESTAMP			= 93,
                VARBINARY                       = (-3),
                VARCHAR                         = 12,
                WCHAR				= (-8),
                WLONGVARCHAR                    = (-10),
                WVARCHAR                        = (-9),
                UNASSIGNED                      = Int16.MaxValue
        }

        internal enum SQL_C_TYPE : short
        {
                BINARY				= (-2),
                BIT				= (-7),
                BOOKMARK			= (4 +(-22)),
                CHAR				= 1,
                DATE				= 9,
                DEFAULT				= 99,
                DOUBLE				= 8,
                FLOAT				= 7,
                GUID				= (-11),
                INTERVAL_DAY			= (100 + 3),
                INTERVAL_DAY_TO_HOUR		= (100 + 8),
                INTERVAL_DAY_TO_MINUTE		= (100 + 9),
                INTERVAL_DAY_TO_SECOND		= (100 + 10),
                INTERVAL_HOUR			= (100 + 4),
                INTERVAL_HOUR_TO_MINUTE	        = (100 + 11),
                INTERVAL_HOUR_TO_SECOND	        = (100 + 12),
                INTERVAL_MINUTE			= (100 + 5),
                INTERVAL_MINUTE_TO_SECOND	= (100 + 13),
                INTERVAL_MONTH			= (100 + 2),
                INTERVAL_SECOND			= (100 + 6),
                INTERVAL_YEAR			= (100 + 1),
                INTERVAL_YEAR_TO_MONTH		= (100 + 7),
                LONG				= 4,
                NUMERIC				= 2,
                SBIGINT				= ((-5)+(-20)),
                SHORT				= 5,
                SLONG				= (4 +(-20)),
                SSHORT				= (5 +(-20)),
                STINYINT			= ((-6)+(-20)),
                TCHAR				= 1,
                TIME				= 10,
                TIMESTAMP			= 11,
                TINYINT				= (-6),
                TYPE_DATE			= 91,
                TYPE_TIME			= 92,
                TYPE_TIMESTAMP			= 93,
                UBIGINT				= ((-5)+(-22)),
                ULONG				= (4 +(-22)),
                USHORT				= (5 +(-22)),
                UTINYINT			= ((-6)+(-22)),
                WCHAR				= (-8),
                UNASSIGNED                      = Int16.MaxValue
        }
}
