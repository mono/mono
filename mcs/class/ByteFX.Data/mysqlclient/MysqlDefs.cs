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

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for ClientParam.
	/// </summary>
	[Flags()]
	internal enum ClientParam
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
		CLIENT_PROTOCOL_41		= 16384,
		CLIENT_SECURE_CONNECTION = 32768

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

	/// <summary>
	/// Specifies MySQL specific data type of a field, property, for use in a <see cref="MySqlParameter"/>.
	/// </summary>
	public enum MySqlDbType
	{
		/// <summary> <see cref="Decimal"/><para>A fixed precision and scale numeric value between -1038 -1 and 10 38 -1.</para></summary>
		Decimal		=   0,  
		/// <summary> <see cref="Byte"/><para>The signed range is -128 to 127. The unsigned range is 0 to 255.</para></summary>
 		Byte		=   1,  
		/// <summary><see cref="Int16"/><para>A 16-bit signed or unsigned integer. The signed range is -32768 to 32767. The unsigned range is 0 to 65535</para></summary>
		Short       =   2,
		/// <summary><see cref="Int32"/><para>A 32-bit signed or unsigned integer</para></summary>
		Int         =   3,
		/// <summary><b>Obsolete</b>  Please use Int for 32 bit values</summary>
		[Obsolete("Long is no longer the correct way of specifying a 32 bit value.  Use Int")]
		Long        =   3,
		/// <summary><see cref="Single"/><para>A small (single-precision) floating-point number. Allowable values are -3.402823466E+38 to -1.175494351E-38, 0, and 1.175494351E-38 to 3.402823466E+38.</para></summary>
		Float       =   4,
		/// <summary><see cref="Double"/><para>A normal-size (double-precision) floating-point number. Allowable values are -1.7976931348623157E+308 to -2.2250738585072014E-308, 0, and 2.2250738585072014E-308 to 1.7976931348623157E+308.</para></summary>
		Double      =   5,
		/// <summary>Specifies a null value</summary>
		Null        =   6,
		/// <summary>A timestamp. The range is '1970-01-01 00:00:00' to sometime in the year 2037</summary>
		Timestamp   =   7,
		/// <summary><see cref="Int64"/><para>A 64-bit signed or unsigned integer.</para></summary>
		BigInt      =   8,
		/// <summary><b>Obsolete</b>  Please use BigInt for 64 bit values</summary>
		[Obsolete("LongLong is no longer the correct way of specifying a 64 bit value.  Use BigInt")]
		LongLong    =   8,
		/// <summary>Specifies a 24 (3 byte) signed or unsigned value.</summary>
		Int24       =   9,
		///<summary>Date The supported range is '1000-01-01' to '9999-12-31'.</summary>
		Date        =  10,
		/// <summary> Time <para>The range is '-838:59:59' to '838:59:59'.</para></summary>
		Time        =  11,
		///<summary>DateTime The supported range is '1000-01-01 00:00:00' to '9999-12-31 23:59:59'.</summary>
		Datetime    =  12,  
		/// <summary>A year in 2- or 4-digit format (default is 4-digit). The allowable values are 1901 to 2155, 0000 in the 4-digit year format, and 1970-2069 if you use the 2-digit format (70-69)</summary>
		Year        =  13,
		/// <summary><b>Obsolete</b>  Use Datetime or Date type</summary>
		Newdate     =  14,
		/// <summary>An enumeration. A string object that can have only one value, chosen from the list of values 'value1', 'value2', ..., NULL or the special "" error value. An ENUM can have a maximum of 65535 distinct values</summary>
		Enum        = 247,
		/// <summary>A set. A string object that can have zero or more values, each of which must be chosen from the list of values 'value1', 'value2', ... A SET can have a maximum of 64 members.</summary>
		Set         = 248,
		/// <summary>A BLOB or TEXT column with a maximum length of 255 (2^8 - 1) characters</summary>
		TinyBlob    = 249,
		/// <summary>A BLOB or TEXT column with a maximum length of 16777215 (2^24 - 1) characters</summary>
		MediumBlob  = 250,
		/// <summary>A BLOB or TEXT column with a maximum length of 4294967295 or 4G (2^32 - 1) characters</summary>
		LongBlob    = 251,
		/// <summary>A BLOB or TEXT column with a maximum length of 65535 (2^16 - 1) characters</summary>
		Blob        = 252,
		/// <summary>A variable-length string containing 0 to 255 characters</summary>
		VarChar     = 253,
		/// <summary><b>Obsolete</b>  Use VarChar type</summary>
		String      = 254
	};


	enum Field_Type : byte
	{
		DECIMAL					=0,
		BYTE					=1,
		SHORT					=2,
		LONG					=3,
		FLOAT					=4,
		DOUBLE					=5,
		NULL					=6,
		TIMESTAMP				=7,
		LONGLONG				=8,
		INT24					=9,
		DATE					=10,
		TIME					=11,
		DATETIME				=12,
		YEAR					=13,
		NEWDATE					=14,
		ENUM					=247,
		SET						=248,
		TINY_BLOB				=249,
		MEDIUM_BLOB				=250,
		LONG_BLOB				=251,
		BLOB					=252,
		VAR_STRING				=253,
		STRING					=254,
	}
}
