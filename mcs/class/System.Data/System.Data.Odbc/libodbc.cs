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

        internal enum OdbcLengthIndicator : short
        {
                NoTotal         = -4,
                NullData        = -1
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
                #region global constants
		internal static int		SQL_OV_ODBC2		= 2;
		internal static int		SQL_OV_ODBC3		= 3;

                internal static string          SQLSTATE_RIGHT_TRUNC    = "01004";
                internal static char            C_NULL                  = '\0';
                #endregion

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

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLAllocHandle (OdbcHandleType HandleType, IntPtr InputHandle, ref IntPtr OutputHandlePtr);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLSetEnvAttr (IntPtr EnvHandle, OdbcEnv Attribute, IntPtr Value, int StringLength);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLConnect (IntPtr ConnectionHandle, string ServerName, short NameLength1, string UserName, short NameLength2, string Authentication, short NameLength3);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn  SQLDriverConnect(IntPtr ConnectionHandle, IntPtr WindowHandle, string InConnectionString, short StringLength1, string OutConnectionString, short BufferLength,	ref short StringLength2Ptr,	ushort DriverCompletion);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLExecDirect (IntPtr StatementHandle, string StatementText, int TextLength);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLRowCount (IntPtr StatementHandle, ref int RowCount);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLNumResultCols (IntPtr StatementHandle, ref short ColumnCount);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLFetch (IntPtr StatementHandle);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcCType TargetType, ref bool TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcCType TargetType, ref double TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcCType TargetType, ref long TargetPtr, int BufferLen, ref int Len);

                [DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcCType TargetType, ref short TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcCType TargetType, ref float TargetPtr, int BufferLen, ref int Len);
	
		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcCType TargetType, ref OdbcTimestamp TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcCType TargetType, ref int TargetPtr, int BufferLen, ref int Len);
	
		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetData (IntPtr StatementHandle, ushort ColumnNumber, OdbcCType TargetType, byte[] TargetPtr, int BufferLen, ref int Len);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLDescribeCol(IntPtr StatementHandle, ushort ColumnNumber, byte[] ColumnName, short BufferLength, ref short NameLength, ref short DataType, ref uint ColumnSize, ref short DecimalDigits, ref short Nullable);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLFreeHandle(ushort HandleType, IntPtr SqlHandle);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLDisconnect(IntPtr ConnectionHandle);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLPrepare(IntPtr StatementHandle, string Statement, int TextLength);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLExecute(IntPtr StatementHandle);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLSetConnectAttr(IntPtr ConnectionHandle, OdbcConnectionAttribute Attribute, IntPtr Value, int Length);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLEndTran(int HandleType, IntPtr Handle, short CompletionType);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLBindParameter(IntPtr StatementHandle, ushort ParamNum, 
				short InputOutputType, short ValueType, short ParamType, uint ColSize, 
				short DecimalDigits, byte[] ParamValue, int BufLen, int StrLen);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLBindParameter(IntPtr StatementHandle, ushort ParamNum, 
				short InputOutputType, short ValueType, short ParamType, uint ColSize, 
				short DecimalDigits, ref int ParamValue, int BufLen, int StrLen);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLCancel(IntPtr StatementHandle);
		
		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLCloseCursor(IntPtr StatementHandle);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLError(IntPtr EnvironmentHandle,
							   IntPtr ConnectionHandle, IntPtr StatementHandle,
							   byte[] Sqlstate, ref int NativeError,
							   byte[] MessageText, short BufferLength,
							   ref short TextLength);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetStmtAttr(IntPtr StatementHandle,
								int Attribute, ref IntPtr Value, int BufLen, int StrLen);

		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLSetDescField(IntPtr DescriptorHandle,
			short RecNumber, short FieldIdentifier, byte[] Value, int BufLen);
                        
		[DllImport("odbc32.dll")]
		internal static extern OdbcReturn SQLGetDiagRec (OdbcHandleType HandleType,
							   IntPtr Handle, ushort RecordNumber,
							   byte [] Sqlstate, ref int NativeError,
							   byte [] MessageText, short BufferLength,
							   ref short TextLength);

		[DllImport ("odbc32.dll")]
		internal static extern OdbcReturn SQLMoreResults (IntPtr Handle);

		internal enum SQLFreeStmtOptions : short
		{
			Close = 0,
			Drop,
			Unbind,
			ResetParams
		}

		[DllImport ("odbc32.dll")]
		internal static extern OdbcReturn SQLFreeStmt (IntPtr Handle,  SQLFreeStmtOptions option);

                #region Type Utilities
                internal static OdbcType NativeToOdbcType (OdbcCType native)
                {
                        switch (native)
                        {
                        case OdbcCType.SignedBigInt:	// SQL_C_SBIGINT
                                return OdbcType.BigInt;
                        case OdbcCType.BigInt:	// SQL_BIGINT
                                return OdbcType.BigInt;
                        case OdbcCType.Binary:	// SQL_BINARY
                                return OdbcType.Binary;
                        case OdbcCType.Bit:	// SQL_BIT
                                return OdbcType.Bit;
                        case OdbcCType.Char:	// SQL_CHAR
                                return OdbcType.Char;
                        case OdbcCType.Date:	// SQL_TYPE_DATE
                                return OdbcType.Date;
                        case OdbcCType.DateTime:	// SQL_DATETIME
                                return OdbcType.DateTime;
                        case OdbcCType.Decimal:	// SQL_DECIMAL
                                return OdbcType.Decimal;
                        case OdbcCType.Double:	// SQL_DOUBLE
                                return OdbcType.Double;
                        case OdbcCType.Image:	// SQL_LONGVARBINARY
                                return OdbcType.Image;
                        case OdbcCType.Int:	// SQL_INTEGER
                                return OdbcType.Int;
                        case OdbcCType.NChar:	// SQL_UNICODE_CHAR
                                return OdbcType.NChar;
                        case OdbcCType.NText:	// SQL_UNICODE_LONGVARCHAR
                                return OdbcType.NText;
                        case OdbcCType.Numeric:	// SQL_NUMERIC
                                return OdbcType.Numeric;
                        case OdbcCType.NVarChar:	// SQL_UNICODE_VARCHAR
                                return OdbcType.NVarChar;
                        case OdbcCType.Real:	// SQL_REAL
                                return OdbcType.Real;
                        case OdbcCType.SmallDateTime:	// ??????????????????????????
                                return OdbcType.SmallDateTime;
                        case OdbcCType.SmallInt:	// SQL_SMALLINT
                                return OdbcType.SmallInt;
                        case OdbcCType.Time:	// SQL_TYPE_TIME
                                return OdbcType.Time;
                        case OdbcCType.Text:	// SQL_LONGVARCHAR
                                return OdbcType.Text;
                        case OdbcCType.Timestamp:	// SQL_TYPE_TIMESTAMP
                                return OdbcType.Timestamp;
                        case OdbcCType.TinyInt:	// SQL_TINYINT
                                return OdbcType.TinyInt;
                        case OdbcCType.UniqueIdentifier:	// SQL_GUID
                                return OdbcType.UniqueIdentifier;
                        case OdbcCType.VarBinary:	// SQL_VARBINARY
                                return OdbcType.VarBinary;
                        case OdbcCType.VarChar:	// SQL_VARCHAR
                                return OdbcType.VarChar;
                        }
                        throw new ArgumentException ("Invalid Native Type");
                }
                #endregion // Type Utilities

	}
}
