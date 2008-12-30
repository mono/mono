//
// System.Data.Odbc.libodbc
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//   Sureshkumar T (tsureshkumar@novell.com)
//
// Copyright (C) Brian Ritchie, 2002
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
using System.Runtime.InteropServices;

namespace System.Data.Odbc
{
	internal enum OdbcHandleType : short
	{
		Env = 1,
		Dbc = 2,
		Stmt = 3,
		Desc = 4
	}

	internal enum OdbcReturn : short
	{
		Error = -1,
		InvalidHandle = -2,
		StillExecuting = 2,
		NeedData = 99,
		Success = 0,
		SuccessWithInfo = 1,
		NoData = 100
	}

	internal enum OdbcEnv : ushort
	{
		OdbcVersion = 200,
		ConnectionPooling = 201,
		CPMatch = 202
	}

	internal enum OdbcConnectionAttribute : int 
	{
		AutoCommit = 102,
		TransactionIsolation = 108,
		CurrentCatalog = 109,
#if NET_2_0
		CoptTransactionIsolation = 1227		/* SQL_COPT_SS_TXN_ISOLATION */
#endif
	}

	internal enum OdbcInfo : ushort
	{
		DataSourceName = 2,
		DriverName = 6,
		DriverVersion = 7,
		DatabaseName = 16,
		DbmsVersion = 18,
		IdentifierQuoteChar = 29
	}

	internal enum OdbcInputOutputDirection : short
	{
		Input = 1,
		InputOutput = 2,
		ResultCol = 3,
		Output = 4,
		ReturnValue = 5
	}

	internal enum OdbcIsolationLevel
	{
		ReadUncommitted = 1,
		ReadCommitted = 2,
		RepeatableRead = 4,
		Serializable = 8,
		Snapshot = 32		/* SQL_TXN_SS_SNAPSHOT */
	}

	internal enum OdbcLengthIndicator : short
	{
		NoTotal = -4,
		NullData = -1
	}

	// Keep this sorted.
	internal enum FieldIdentifier : short
	{
		AutoUniqueValue = 11,	/* SQL_DESC_AUTO_UNIQUE_VALUE */
		BaseColumnName = 22,	/* SQL_DESC_BASE_COLUMN_NAME */
		BaseTableName = 23,	/* SQL_DESC_BASE_TABLE_NAME */
		CaseSensitive = 12,	/* SQL_DESC_CASE_SENSITIVE */
		CatelogName = 17,	/* SQL_DESC_CATALOG_NAME */
		ConsiseType = 2,	/* SQL_DESC_CONCISE_TYPE */
		Count = 1001,		/* SQL_DESC_COUNT */
		DisplaySize = 6,	/* SQL_DESC_DISPLAY_SIZE */
		FixedPrecScale = 9,	/* SQL_DESC_FIXED_PREC_SCALE */
		Label = 18,		/* SQL_DESC_LABEL */
		Length = 1003,		/* SQL_DESC_LENGTH */
		LiteralPrefix = 27,	/* SQL_DESC_LITERAL_PREFIX */
		LiteralSuffix = 28,	/* SQL_DESC_LITERAL_SUFFIX */
		LocalTypeName = 29,	/* SQL_DESC_LOCAL_TYPE_NAME */
		Name = 1011,		/* SQL_DESC_NAME */
		Nullable = 1008,	/* SQL_DESC_NULLABLE */
		NumPrecRadix = 32,	/* SQL_DESC_NUM_PREC_RADIX */
		OctetLength = 1013,	/* SQL_DESC_OCTET_LENGTH */
		Precision = 1005,	/* SQL_DESC_PRECISION */
		Scale = 1006,		/* SQL_DESC_SCALE */
		SchemaName = 16,	/* SQL_DESC_SCHEMA_NAME */
		Searchable = 13,	/* SQL_DESC_SEARCHABLE */
		TableName = 15,		/* SQL_DESC_TABLE_NAME */
		Type = 1002,		/* SQL_DESC_TYPE */
		TypeName = 14,		/* SQL_DESC_TYPE_NAME */
		Unnamed = 1012,		/* SQL_DESC_UNNAMED */
		Unsigned = 8,		/* SQL_DESC_UNSIGNED */
		Updatable = 10		/* SQL_DESC_UPDATABLE */
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct OdbcTimestamp
	{
		internal short year;
		internal ushort month;
		internal ushort day;
		internal ushort hour;
		internal ushort minute;
		internal ushort second;
		internal ulong fraction;
	}

	internal class libodbc
	{
		#region global constants
		internal const int		SQL_OV_ODBC2		= 2;
		internal const int		SQL_OV_ODBC3		= 3;

		internal const string		SQLSTATE_RIGHT_TRUNC	= "01004";
		internal const char		C_NULL			= '\0';
		internal const int		SQL_NTS			= -3;

		internal const short		SQL_TRUE		= 1;
		internal const short		SQL_FALSE		= 0;

		// SQLStatistics
		internal const short		SQL_INDEX_UNIQUE	= 0;
		internal const short		SQL_INDEX_ALL		= 1;
		internal const short		SQL_QUICK		= 0;
		internal const short		SQL_ENSURE		= 1;

		// SQLColumnAttribute
		internal const short		SQL_NO_NULLS		= 0;
		internal const short		SQL_NULLABLE		= 1;
		internal const short		SQL_NULLABLE_UNKNOWN	= 2;
		internal const short		SQL_ATTR_READONLY	= 0;
		internal const short		SQL_ATTR_WRITE		= 1;
		internal const short		SQL_ATTR_READWRITE_UNKNOWN = 2;
		#endregion

		internal static OdbcInputOutputDirection ConvertParameterDirection(
			ParameterDirection dir)
		{
			switch (dir) {
			case ParameterDirection.Input:
				return OdbcInputOutputDirection.Input;
			case ParameterDirection.InputOutput:
				return OdbcInputOutputDirection.InputOutput;
			case ParameterDirection.Output:
				return OdbcInputOutputDirection.Output;
			case ParameterDirection.ReturnValue:
				return OdbcInputOutputDirection.ReturnValue;
			default:
				return OdbcInputOutputDirection.Input;
			}
		}

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLAllocHandle (
			OdbcHandleType HandleType,
			IntPtr InputHandle,
			ref IntPtr OutputHandlePtr);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLSetEnvAttr (
			IntPtr EnvHandle,
			OdbcEnv Attribute,
			IntPtr Value,
			int StringLength);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLConnect (
			IntPtr ConnectionHandle,
			string ServerName,
			short NameLength1,
			string UserName,
			short NameLength2,
			string Authentication,
			short NameLength3);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLDriverConnect (
			IntPtr ConnectionHandle,
			IntPtr WindowHandle,
			string InConnectionString,
			short StringLength1,
			string OutConnectionString,
			short BufferLength,
			ref short StringLength2Ptr,
			ushort DriverCompletion);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLExecDirect (
			IntPtr StatementHandle,
			string StatementText,
			int TextLength);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLRowCount (
			IntPtr StatementHandle,
			ref int RowCount);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLNumResultCols (
			IntPtr StatementHandle,
			ref short ColumnCount);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLFetch (
			IntPtr StatementHandle);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetData (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			SQL_C_TYPE TargetType,
			ref bool TargetPtr,
			int BufferLen,
			ref int Len);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetData (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			SQL_C_TYPE TargetType,
			ref double TargetPtr,
			int BufferLen,
			ref int Len);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetData (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			SQL_C_TYPE TargetType,
			ref long TargetPtr,
			int BufferLen,
			ref int Len);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetData (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			SQL_C_TYPE TargetType,
			ref short TargetPtr,
			int BufferLen,
			ref int Len);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetData (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			SQL_C_TYPE TargetType,
			ref float TargetPtr,
			int BufferLen,
			ref int Len);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetData (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			SQL_C_TYPE TargetType,
			ref OdbcTimestamp TargetPtr,
			int BufferLen,
			ref int Len);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetData (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			SQL_C_TYPE TargetType,
			ref int TargetPtr,
			int BufferLen,
			ref int Len);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetData (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			SQL_C_TYPE TargetType,
			byte[] TargetPtr,
			int BufferLen,
			ref int Len);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLDescribeCol (
			IntPtr StatementHandle,
			ushort ColumnNumber,
			byte[] ColumnName,
			short BufferLength,
			ref short NameLength,
			ref short DataType,
			ref uint ColumnSize,
			ref short DecimalDigits,
			ref short Nullable);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLFreeHandle (
			ushort HandleType,
			IntPtr SqlHandle);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLDisconnect (
			IntPtr ConnectionHandle);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLPrepare (
			IntPtr StatementHandle,
			string Statement,
			int TextLength);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLExecute (
			IntPtr StatementHandle);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetConnectAttr (
			IntPtr ConnectionHandle,
			OdbcConnectionAttribute Attribute,
			out int value,
			int BufferLength,
			out int StringLength);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLSetConnectAttr (
			IntPtr ConnectionHandle,
			OdbcConnectionAttribute Attribute,
			IntPtr Value,
			int Length);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLEndTran (
			int HandleType,
			IntPtr Handle,
			short CompletionType);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLBindParameter (
			IntPtr StatementHandle,
			ushort ParamNum,
			short InputOutputType,
			SQL_C_TYPE ValueType,
			SQL_TYPE ParamType,
			uint ColSize,
			short DecimalDigits,
			IntPtr ParamValue,
			int BufLen,
			IntPtr StrLen);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLCancel (
			IntPtr StatementHandle);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLCloseCursor (
			IntPtr StatementHandle);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLError (
			IntPtr EnvironmentHandle,
			IntPtr ConnectionHandle,
			IntPtr StatementHandle,
			byte[] Sqlstate,
			ref int NativeError,
			byte[] MessageText,
			short BufferLength,
			ref short TextLength);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetStmtAttr (
			IntPtr StatementHandle,
			int Attribute,
			ref IntPtr Value,
			int BufLen,
			int StrLen);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLSetDescField (
			IntPtr DescriptorHandle,
			short RecNumber,
			short FieldIdentifier,
			byte[] Value,
			int BufLen);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetDiagRec (
			OdbcHandleType HandleType,
			IntPtr Handle,
			ushort RecordNumber,
			byte [] Sqlstate,
			ref int NativeError,
			byte [] MessageText,
			short BufferLength,
			ref short TextLength);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLMoreResults (
			IntPtr Handle);

		internal enum SQLFreeStmtOptions : short
		{
			Close = 0,
			Drop,
			Unbind,
			ResetParams
		}

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLFreeStmt (
			IntPtr Handle,
			SQLFreeStmtOptions option);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLGetInfo (
			IntPtr connHandle,
			OdbcInfo info,
			byte [] buffer,
			short buffLength,
			ref short remainingStrLen);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLColAttribute (
			IntPtr StmtHandle,
			short column,
			FieldIdentifier fieldId,
			byte [] charAttributePtr,
			short bufferLength,
			ref short strLengthPtr,
			ref int numericAttributePtr);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLPrimaryKeys (
			IntPtr StmtHandle,
			string catalog,
			short catalogLength,
			string schema,
			short schemaLength,
			string tableName,
			short tableLength);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLStatistics (
			IntPtr StmtHandle,
			string catalog,
			short catalogLength,
			string schema,
			short schemaLength,
			string tableName,
			short tableLength,
			short unique,
			short Reserved);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLBindCol (
			IntPtr StmtHandle,
			short column,
			SQL_C_TYPE targetType,
			byte [] buffer,
			int bufferLength,
			ref int indicator);

		[DllImport ("odbc32.dll", CharSet = CharSet.Unicode)]
		internal static extern OdbcReturn SQLBindCol (
			IntPtr StmtHandle,
			short column,
			SQL_C_TYPE targetType,
			ref short value,
			int bufferLength,
			ref int indicator);
	}
}
