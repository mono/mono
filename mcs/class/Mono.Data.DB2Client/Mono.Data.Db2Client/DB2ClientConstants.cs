#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;

namespace DB2ClientCS
{
	/// <summary>
	/// Summary description for DB2Constants.
	/// </summary>
	public class DB2ClientConstants
	{
		public DB2ClientConstants()
		{
			//
			// TODO: Add constructor logic here
			//

		}
		public const short  SQL_HANDLE_ENV = 1;
		public const short  SQL_HANDLE_DBC = 2;
        public const short  SQL_HANDLE_STMT = 3;
        public const short  SQL_HANDLE_DESC = 4;

		/* RETCODE values             */
        public const long  SQL_SUCCESS			 = 0;
        public const long  SQL_SUCCESS_WITH_INFO = 1;
        public const long  SQL_NEED_DATA         = 99;
        public const long  SQL_NO_DATA           = 100;
        public const long  SQL_STILL_EXECUTING   = 2;
        public const long  SQL_ERROR             = -1;
        public const long  SQL_INVALID_HANDLE    = -2;
		public const int   SQL_NTS				 = -3;
		public const long  SQL_NULL_HANDLE		 = 0L;
		public const short SQL_COMMIT			 = 0;
		public const short SQL_ROLLBACK			 = 1;
		public const short SQL_NO_DATA_FOUND	 = 100;

		/* Isolation levels */
		public const long  SQL_TXN_READ_UNCOMMITTED		= 0x00000001L;
		public const long  SQL_TXN_READ_COMMITTED		= 0x00000002L;
		public const long  SQL_TXN_REPEATABLE_READ		= 0x00000004L;
		public const long  SQL_TXN_SERIALIZABLE_READ	= 0x00000008L;
		public const long  SQL_TXN_NOCOMMIT				= 0x00000020L;

		/* Connect options */
		public const long SQL_TXN_ISOLATION		= 108;
		public const long SQL_AUTOCOMMIT		= 102;

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
		public const int  SQL_WCHAR				= (-8);
		public const int  SQL_WVARCHAR			= (-9);
		public const int  SQL_WLONGVARCHAR		= (-10);
		public const int  SQL_TYPE_DATE			= 91;
		public const int  SQL_TYPE_TIME			= 92;
		public const int  SQL_TYPE_TIMESTAMP	= 93;

		public const int  SQL_C_CHAR			= SQL_CHAR;
		public const int  SQL_C_DEFAULT			= 99;

		public const int  SQL_BLOB_LOCATOR		= 31;
		public const int  SQL_CLOB_LOCATOR		= 41;
		public const int  SQL_DBCLOB_LOCATOR	= -351;

		public const int  SQL_C_BLOB_LOCATOR = SQL_BLOB_LOCATOR;
		public const int  SQL_C_CLOB_LOCATOR = SQL_CLOB_LOCATOR;
		public const int  SQL_C_DBCLOB_LOCATOR = SQL_DBCLOB_LOCATOR;

		public const int  SQL_USER_DEFINED_TYPE = (-450);

		/* SQLDriverConnect Options */
        public const int  SQL_DRIVER_NOPROMPT   = 0;
        public const int  SQL_DRIVER_COMPLETE   = 1;
        public const int  SQL_DRIVER_PROMPT     = 2;
		public const int  SQL_DRIVER_COMPLETE_REQUIRED = 3;

		/* Null settings */
		public const int  SQL_NO_NULLS			= 0;
		public const int  SQL_NULLABLE			= 1;
		public const int  SQL_NULLABLE_UNKNOWN	= 2;

		/* Defines for SQLBindParameter and SQLProcedureColumns */
		public const int SQL_PARAM_TYPE_UNKNOWN = 0;
		public const int SQL_PARAM_INPUT		= 1;
		public const int SQL_PARAM_INPUT_OUTPUT = 2;
		public const int SQL_RESULT_COL         = 3;
		public const int SQL_PARAM_OUTPUT       = 4;
		public const int SQL_RETURN_VALUE       = 5;
		}
}
