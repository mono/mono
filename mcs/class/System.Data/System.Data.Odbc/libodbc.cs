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
	internal enum OdbcHandleType : ushort {
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

	[StructLayout(LayoutKind.Sequential)]
	internal struct OdbcTimestamp
	{
		public short year;
		public ushort month;
		public ushort day;
		public ushort hour;
		public ushort minute;
		public ushort second;
		public ulong fraction;
	}

	
//	sealed internal class libodbc
	internal class libodbc
	{
		[DllImport("odbc32")]
		public static extern OdbcReturn SQLAllocHandle (OdbcHandleType HandleType, IntPtr InputHandle, ref IntPtr OutputHandlePtr);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLSetEnvAttr (IntPtr EnvHandle, OdbcEnv Attribute, IntPtr Value, int StringLength);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLConnect (IntPtr ConnectionHandle, string ServerName, short NameLength1, string UserName, short NameLength2, string Authentication, short NameLength3);

		[DllImport("odbc32")]
		public static extern OdbcReturn  SQLDriverConnect(IntPtr ConnectionHandle, IntPtr WindowHandle, string InConnectionString, short StringLength1, string OutConnectionString, short BufferLength,	ref short StringLength2Ptr,	ushort DriverCompletion);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLExecDirect (IntPtr StatementHandle, string StatementText, int TextLength);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLRowCount (IntPtr StatementHandle, ref int RowCount);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLNumResultCols (IntPtr StatementHandle, ref short ColumnCount);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLFetch (IntPtr StatementHandle);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref bool TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref double TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref long TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref short TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref float TargetPtr, int BufferLen, ref int Len);
	
		[DllImport("odbc32")]
		public static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref OdbcTimestamp TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, ref int TargetPtr, int BufferLen, ref int Len);
	
		[DllImport("odbc32")]
		public static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcType TargetType, byte[] TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLDescribeCol(IntPtr StatementHandle, ushort ColumnNumber, byte[] ColumnName, short BufferLength, ref short NameLength, ref OdbcType DataType, ref short ColumnSize, ref short DecimalDigits, ref short Nullable);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLFreeHandle(ushort HandleType, IntPtr SqlHandle);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLDisconnect(IntPtr ConnectionHandle);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLPrepare(IntPtr StatementHandle, string Statement, int TextLength);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLExecute(IntPtr StatementHandle);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLSetConnectAttr(IntPtr ConnectionHandle, int Attribute, uint Value, int Length);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLEndTran(int HandleType, IntPtr Handle, short CompletionType);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLBindParam(IntPtr StatementHandle, short ParamNum, short ValueType,
				short ParamType, int LenPrecision, short ParamScale, ref int ParamValue, int StrLen);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLBindParam(IntPtr StatementHandle, short ParamNum, short ValueType,
				short ParamType, int LenPrecision, short ParamScale, byte[] ParamValue, int StrLen);

		[DllImport("odbc32")]
		public static extern OdbcReturn SQLCancel(IntPtr StatementHandle);
		
		[DllImport("odbc32")]
		public static extern OdbcReturn SQLCloseCursor(IntPtr StatementHandle);
	}
}
