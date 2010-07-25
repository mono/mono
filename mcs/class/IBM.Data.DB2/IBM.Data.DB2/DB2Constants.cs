
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

namespace IBM.Data.DB2
{
	/// <summary>
	/// Summary description for DB2Constants.
	/// </summary>
	internal sealed class DB2Constants
	{
		public const short  SQL_HANDLE_ENV = 1;
		public const short  SQL_HANDLE_DBC = 2;
		public const short  SQL_HANDLE_STMT = 3;
		public const short  SQL_HANDLE_DESC = 4;

		/* RETCODE values             */
		public const short  SQL_SUCCESS			 = 0;
		public const short  SQL_SUCCESS_WITH_INFO = 1;
		public const short  SQL_NEED_DATA         = 99;
		public const short  SQL_NO_DATA           = 100;
		public const short  SQL_STILL_EXECUTING   = 2;
		public const short  SQL_ERROR             = -1;
		public const short  SQL_INVALID_HANDLE    = -2;

		public const int   SQL_NTS				 = -3;
		public const long  SQL_NULL_HANDLE		 = 0L;
		public const short SQL_COMMIT			 = 0;
		public const short SQL_ROLLBACK			 = 1;
		public const short SQL_NO_DATA_FOUND	 = 100;

		/* SQLFreeStmt option values  */
		public const short SQL_CLOSE             = 0;
		public const short SQL_DROP              = 1;
		public const short SQL_UNBIND            = 2;
		public const short SQL_RESET_PARAMS      = 3;

		/* Isolation levels */
		public const long  SQL_TXN_READ_UNCOMMITTED		= 0x00000001L;
		public const long  SQL_TXN_READ_COMMITTED		= 0x00000002L;
		public const long  SQL_TXN_REPEATABLE_READ		= 0x00000004L;
		public const long  SQL_TXN_SERIALIZABLE_READ	= 0x00000008L;
		public const long  SQL_TXN_NOCOMMIT				= 0x00000020L;

		/* Connect options */
		public const int  SQL_ATTR_TXN_ISOLATION= 108;
		public const int  SQL_ATTR_AUTOCOMMIT	= 102;

		/* attribute */
		public const int  SQL_ATTR_ANSI_APP		= 115;
		public const int  SQL_AA_TRUE           = 1;      /* the application is an ANSI app */
		public const int  SQL_AA_FALSE          = 0;      /* the application is a Unicode app */

		public const int  SQL_ATTR_CONNECTION_DEAD = 1209;    /* GetConnectAttr only */
		public const int  SQL_CD_TRUE           = 1;      /* the connection is dead */
		public const int  SQL_CD_FALSE          = 0;      /* the connection is not dead */

		public const int  SQL_ATTR_QUERY_TIMEOUT = 0;
		public const int  SQL_ATTR_MAX_ROWS		= 1;
		public const int  SQL_ATTR_DEFERRED_PREPARE = 1277;

		/*Used for batch operations*/
		public const int  SQL_ATTR_PARAMSET_SIZE = 22;
		public const int  SQL_ATTR_PARAM_STATUS_PTR = 20;
		public const int  SQL_ATTR_PARAMS_PROCESSED_PTR = 21;
		public const int SQL_ATTR_PARAM_BIND_TYPE	    = 18;

		public const int  SQL_IS_POINTER        = -4;
		public const int  SQL_IS_UINTEGER       = -5;
		public const int  SQL_IS_INTEGER        = -6;
		public const int  SQL_IS_USMALLINT      = -7;
		public const int  SQL_IS_SMALLINT       = -8;



		public const long SQL_AUTOCOMMIT_OFF	= 0L;
		public const long SQL_AUTOCOMMIT_ON		= 1L;

		/* Data Types */
		public const int  SQL_UNKNOWN_TYPE		= 0;
		public const int  SQL_CHAR				= 1;
		public const int  SQL_NUMERIC			= 2;
		public const int  SQL_DECIMAL			= 3;
		public const int  SQL_INTEGER			= 4;
		public const int  SQL_SMALLINT			= 5;
		public const int  SQL_FLOAT				= 6;
		public const int  SQL_REAL				= 7;
		public const int  SQL_DOUBLE			= 8;
		public const int  SQL_DATETIME			= 9;
		public const int  SQL_VARCHAR			= 12;
		public const int  SQL_VARBINARY         = (-3);
		public const int  SQL_LONGVARBINARY     = (-4);
		public const int  SQL_BIGINT			= (-5);
		public const int  SQL_BIT  				= (-7);
		public const int  SQL_WCHAR				= (-8);
		public const int  SQL_WVARCHAR			= (-9);
		public const int  SQL_WLONGVARCHAR		= (-10);
		public const int  SQL_GUID				= (-11);
		public const int  SQL_UTINYINT		    = (-28);

		public const int  SQL_TYPE_DATE			= 91;
		public const int  SQL_TYPE_TIME			= 92;
		public const int  SQL_TYPE_TIMESTAMP	= 93;
		public const int SQL_TYPE_BINARY		= -2;
		public const int SQL_GRAPHIC            = -95;
		public const int SQL_VARGRAPHIC         = -96;
		public const int SQL_LONGVARGRAPHIC     = -97;
		public const int SQL_TYPE_BLOB			= -98;
		public const int SQL_TYPE_CLOB			= -99;
		public const int SQL_DBCLOB				= 350;

		public const int  SQL_C_CHAR			= SQL_CHAR;
		public const int  SQL_C_WCHAR			= SQL_WCHAR;
		public const int SQL_C_SBIGINT          = -25;
		public const int SQL_C_SLONG			= -16;
		public const int SQL_C_SSHORT			= -15;
		public const int SQL_C_TYPE_BINARY		= -2;
		public const int SQL_C_DOUBLE			= 8;
		public const int SQL_C_DECIMAL_IBM		= 3;
		public const int SQL_C_DECIMAL_OLEDB		= 2514;
		public const int  SQL_C_DEFAULT			= 99;
		public const int SQL_C_TYPE_DATE		= 91;
		public const int SQL_C_TYPE_TIME		= 92;
		public const int SQL_C_TYPE_TIMESTAMP		= 93;
		public const int SQL_C_TYPE_NUMERIC 		= 2;
		public const int SQL_C_TYPE_REAL		= 7;

		public const int  SQL_BLOB_LOCATOR		= 31;
		public const int  SQL_CLOB_LOCATOR		= 41;
		public const int  SQL_DBCLOB_LOCATOR	= -351;

		public const int  SQL_C_BLOB_LOCATOR = SQL_BLOB_LOCATOR;
		public const int  SQL_C_CLOB_LOCATOR = SQL_CLOB_LOCATOR;
		public const int  SQL_C_DBCLOB_LOCATOR = SQL_DBCLOB_LOCATOR;

		public const int  SQL_USER_DEFINED_TYPE = (-450);

		/* Special length values  */
		public const int SQL_NULL_DATA			= -1;

		/* SQLDriverConnect Options */
		public const int  SQL_DRIVER_NOPROMPT   = 0;
		public const int  SQL_DRIVER_COMPLETE   = 1;
		public const int  SQL_DRIVER_PROMPT     = 2;
		public const int  SQL_DRIVER_COMPLETE_REQUIRED = 3;

		/* Null settings */
		public const int  SQL_NO_NULLS			= 0;
		public const int  SQL_NULLABLE			= 1;
		public const int  SQL_NULLABLE_UNKNOWN	= 2;

		public const int SQL_PARAM_BIND_BY_COLUMN    = 0;

		/* Defines for SQLBindParameter and SQLProcedureColumns */
		public const int SQL_PARAM_TYPE_UNKNOWN = 0;
		public const int SQL_PARAM_INPUT		= 1;
		public const int SQL_PARAM_INPUT_OUTPUT = 2;
		public const int SQL_RESULT_COL         = 3;
		public const int SQL_PARAM_OUTPUT       = 4;
		public const int SQL_RETURN_VALUE       = 5;
		
		/*Defines for SQLColAttributeW*/
		public const int SQL_DESC_ALLOC_TYPE = 1099;
		public const int SQL_DESC_AUTO_UNIQUE_VALUE = 11;
		public const int SQL_DESC_BASE_COLUMN_NAME = 22;
		public const int SQL_DESC_BASE_TABLE_NAME = 23;
		public const int SQL_DESC_COLUMN_CATALOG_NAME = 17;
		public const int SQL_DESC_COLUMN_NAME = 1;
		public const int SQL_DESC_SCHEMA_NAME = 16;
		public const int SQL_DESC_COLUMN_TABLE_NAME = 15;
		public const int SQL_DESC_CONCISE_TYPE = 2;
		public const int SQL_DESC_COUNT = 1001;
		public const int SQL_DESC_DATA_PTR = 1010;
		public const int SQL_DESC_DATETIME_INTERVAL_CODE = 1007;
		public const int SQL_DESC_INDICATOR_PTR = 1009;
		public const int SQL_DESC_LENGTH = 1003;
		public const int SQL_DESC_NAME = 1011;
		public const int SQL_DESC_NULLABLE = 1008;
		public const int SQL_DESC_OCTET_LENGTH = 1013;
		public const int SQL_DESC_OCTET_LENGTH_PTR = 1004;
		public const int SQL_DESC_PRECISION = 1005;
		public const int SQL_DESC_SCALE = 1006;
		public const int SQL_DESC_TYPE = 1002;
		public const int SQL_DESC_TYPE_NAME = 14;
		public const int SQL_DESC_UNNAMED = 1012;
		public const int SQL_DESC_UNSIGNED = 8;
		public const int SQL_DESC_UPDATABLE = 10; 
	}
}
