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

namespace ByteFX.Data.MySQLClient
{
	/// <summary>
	/// Summary description for ClientParam.
	/// </summary>
	internal enum ClientParam : short
	{
		CLIENT_LONG_PASSWORD	= 1,			// new more secure passwords
		CLIENT_FOUND_ROWS		= 2,			// found instead of affected rows
		CLIENT_LONG_FLAG		= 4,			// Get all column flags
		CLIENT_CONNECT_WITH_DB	= 8,			// One can specify db on connect
		CLIENT_NO_SCHEMA		= 16,			// Don't allow db.table.column
		CLIENT_COMPRESS			= 32,			// Client can use compression protocol
		CLIENT_ODBC				= 64,			// ODBC client
		CLIENT_LOCAL_FILES		= 128,			// Can use LOAD DATA LOCAL
		CLIENT_IGNORE_SPACE		= 256,			// Ignore spaces before '('
		CLIENT_CHANGE_USER		= 512,			// Support the mysql_change_user()
		CLIENT_INTERACTIVE		= 1024,			// This is an interactive client
		CLIENT_SSL				= 2048,			// Switch to SSL after handshake
		CLIENT_IGNORE_SIGPIPE	= 4096,			// IGNORE sigpipes
		CLIENT_TRANSACTIONS		= 8192,			// Client knows about transactions
	}
	
	/// <summary>
	/// DB Operations Code
	/// </summary>
	internal enum DBCmd : byte
	{
		SLEEP        =  0,
		QUIT         =  1,
		INIT_DB      =  2,
		QUERY        =  3,
		FIELD_LIST   =  4,
		CREATE_DB    =  5,
		DROP_DB      =  6,
		RELOAD       =  7,
		SHUTDOWN     =  8,
		STATISTICS   =  9,
		PROCESS_INFO = 10,
		CONNECT      = 11,
		PROCESS_KILL = 12,
		DEBUG        = 13,
		PING         = 14,
		TIME         = 15,
		DELAYED_INSERT = 16,
		CHANGE_USER    = 17,
	}

	public enum MySQLDbType
	{
		Decimal		=   0,
		Tiny        =   1,
		Byte		=   1,
		Short       =   2,
		Long        =   3,
		Float       =   4,
		Double      =   5,
		Null        =   6,
		Timestamp   =   7,
		LongLong    =   8,
		Int24       =   9,
		Date        =  10,
		Time        =  11,
		Datetime    =  12,
		Year        =  13,
		Newdate     =  14,
		Enum        = 247,
		Set         = 248,
		TinyBlob    = 249,
		MediumBlob  = 250,
		LongBlob    = 251,
		Blob        = 252,
		VarChar     = 253,
		String      = 254
	};


	enum Field_Type : byte
	{
		DECIMAL					=0,
		BYTE						=1,
		SHORT					=2,
		LONG						=3,
		FLOAT					=4,
		DOUBLE					=5,
		NULL						=6,
		TIMESTAMP				=7,
		LONGLONG				=8,
		INT24						=9,
		DATE						=10,
		TIME						=11,
		DATETIME				=12,
		YEAR						=13,
		NEWDATE				=14,
		ENUM						=247,
		SET						=248,
		TINY_BLOB				=249,
		MEDIUM_BLOB			=250,
		LONG_BLOB				=251,
		BLOB						=252,
		VAR_STRING			=253,
		STRING					=254,
	}
}
