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
	internal enum OdbcHandleType {
		Env = 1,
		Dbc = 2,
		Stmt = 3,
		Desc = 4
	};

	internal enum OdbcReturn {
		Error = -1,
		InvalidHandle = -2,
		StillExecuting = 2,
		NeedData = 99,
		Success = 0,
		SuccessWithInfo = 1
	}

	internal enum OdbcEnv {
		OdbcVersion = 200,
		ConnectionPooling = 201,
		CPMatch = 202
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct OdbcTimestamp
	{
		public short year;
		public ushort month;
		public ushort day;
		public ushort hour;
		public ushort minute;
		public ushort second;
		public ulong fraction;
	}

	sealed internal class libodbc
	{
		public static void DisplayError(string Msg, OdbcReturn Ret)
		{
			if ((Ret!=OdbcReturn.Success) && (Ret!=OdbcReturn.SuccessWithInfo)) {
				Console.WriteLine("ERROR: {0}: <{1}>",Msg,Ret);

			}
		}



		[DllImport("libodbc")]
		public static extern OdbcReturn SQLAllocHandle (ushort HandleType, int 
InputHandle, ref int OutputHandlePtr);


		[DllImport("libodbc")]
		public static extern OdbcReturn SQLSetEnvAttr (int EnvHandle, ushort 
Attribute, IntPtr Value, int StringLength);


		[DllImport("libodbc")]
		public static extern OdbcReturn SQLConnect (int ConnectionHandle, string 
ServerName, short NameLength1, string UserName, short NameLength2, string 
Authentication, short NameLength3);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLExecDirect (int StatementHandle, string 
StatementText, int TextLength);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLRowCount (int StatementHandle, ref int 
RowCount);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLNumResultCols (int StatementHandle, ref 
short ColumnCount);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLFetch (int StatementHandle);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLGetData (int StatementHandle, ushort 
ColumnNumber, short TargetType, ref int TargetPtr, int BufferLen, ref int 
Len);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLGetData (int StatementHandle, ushort 
ColumnNumber, short TargetType, byte[] TargetPtr, int BufferLen, ref int 
Len);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLGetData (int StatementHandle, ushort 
ColumnNumber, short TargetType, ref float TargetPtr, int BufferLen, ref int 
Len);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLGetData (int StatementHandle, ushort 
ColumnNumber, short TargetType, ref OdbcTimestamp TargetPtr, int BufferLen, 
ref int Len);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLDescribeCol(int StatmentHandle, ushort 
ColumnNumber, byte[] ColumnName, short BufferLength, ref short NameLength, 
ref short DataType, ref short ColumnSize, ref short DecimalDigits, ref short 
Nullable);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLFreeHandle(ushort HandleType, int 
SqlHandle);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLDisconnect(int ConnectionHandle);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLPrepare(int StatementHandle, string 
Statement, int TextLength);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLExecute(int StatementHandle);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLSetConnectAttr(int ConnectionHandle, 
int Attribute, uint Value, int Length);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLEndTran(int HandleType, int Handle, 
short CompletionType);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLBindParam(int StatementHandle, short 
ParamNum, short ValueType,
				short ParamType, int LenPrecision, short ParamScale, ref int ParamValue, 
int StrLen);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLBindParam(int StatementHandle, short 
ParamNum, short ValueType,
				short ParamType, int LenPrecision, short ParamScale, byte[] ParamValue, 
int StrLen);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLCancel(int StatementHandle);

		[DllImport("libodbc")]
		public static extern OdbcReturn SQLCloseCursor(int StatementHandle);
	}
}

