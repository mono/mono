//
// System.Data.Odbc.libodbc
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//  
//
// Copyright (C) Brian Ritchie, 2002
// 
//

using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace System.Data.Odbc
{
	internal enum OdbcHandleType : short {
		Env = 1,
		Dbc = 2,
		Stmt = 3,
		Desc = 4
	};

	internal enum OdbcReturn : short {
		Error = -1,
		InvalidHandle = -2,
		StillExecuting = 2,
		NeedData = 99,
		Success = 0,
		SuccessWithInfo = 1,
		NoData=100
	}

	internal enum OdbcEnv : ushort {
		OdbcVersion = 200,
		ConnectionPooling = 201,
		CPMatch = 202
	}

	internal enum OdbcConnectionAttribute : int 
	{
		AutoCommit=102,
		TransactionIsolation=108
	}

	internal enum OdbcInputOutputDirection : short
	{
		Input=1,
		InputOutput=2,
		ResultCol=3,
		Output=4,
		ReturnValue=5
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

	
//	sealed internal class libodbc
	internal class libodbc
	{
		internal static OdbcInputOutputDirection ConvertParameterDirection(
			ParameterDirection dir)
		{
			switch (dir)
			{
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

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLAllocHandle (OdbcHandleType HandleType, IntPtr InputHandle, ref IntPtr OutputHandlePtr);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLSetEnvAttr (IntPtr EnvHandle, OdbcEnv Attribute, IntPtr Value, int StringLength);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLConnect (IntPtr ConnectionHandle, string ServerName, short NameLength1, string UserName, short NameLength2, string Authentication, short NameLength3);

		[DllImport("odbc32")]
		internal static extern OdbcReturn  SQLDriverConnect(IntPtr ConnectionHandle, IntPtr WindowHandle, string InConnectionString, short StringLength1, string OutConnectionString, short BufferLength,	ref short StringLength2Ptr,	ushort DriverCompletion);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLExecDirect (IntPtr StatementHandle, string StatementText, int TextLength);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLRowCount (IntPtr StatementHandle, ref int RowCount);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLNumResultCols (IntPtr StatementHandle, ref short ColumnCount);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLFetch (IntPtr StatementHandle);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref bool TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref double TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref long TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref short TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref float TargetPtr, int BufferLen, ref int Len);
	
		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref OdbcTimestamp TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref int TargetPtr, int BufferLen, ref int Len);
	
		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, byte[] TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLDescribeCol(IntPtr StatementHandle, ushort ColumnNumber, byte[] ColumnName, short BufferLength, ref short NameLength, ref short DataType, ref short ColumnSize, ref short DecimalDigits, ref short Nullable);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLFreeHandle(ushort HandleType, IntPtr SqlHandle);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLDisconnect(IntPtr ConnectionHandle);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLPrepare(IntPtr StatementHandle, string Statement, int TextLength);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLExecute(IntPtr StatementHandle);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLSetConnectAttr(IntPtr ConnectionHandle, OdbcConnectionAttribute Attribute, IntPtr Value, int Length);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLEndTran(int HandleType, IntPtr Handle, short CompletionType);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLBindParameter(IntPtr StatementHandle, ushort ParamNum, 
				short InputOutputType, short ValueType, short ParamType, uint ColSize, 
				short DecimalDigits, byte[] ParamValue, int BufLen, int StrLen);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLBindParameter(IntPtr StatementHandle, ushort ParamNum, 
				short InputOutputType, short ValueType, short ParamType, uint ColSize, 
				short DecimalDigits, ref int ParamValue, int BufLen, int StrLen);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLCancel(IntPtr StatementHandle);
		
		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLCloseCursor(IntPtr StatementHandle);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLError(IntPtr EnvironmentHandle,
							   IntPtr ConnectionHandle, IntPtr StatementHandle,
							   byte[] Sqlstate, ref int NativeError,
							   byte[] MessageText, short BufferLength,
							   ref short TextLength);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLGetStmtAttr(IntPtr StatementHandle,
								int Attribute, ref IntPtr Value, int BufLen, int StrLen);

		[DllImport("odbc32")]
		internal static extern OdbcReturn SQLSetDescField(IntPtr DescriptorHandle,
			short RecNumber, short FieldIdentifier, byte[] Value, int BufLen);
		

	}
}
