#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

namespace DB2ClientCS
{
	/// <summary>
	/// DB2CLIPrototypes class is a wrapper for the db2cli.lib, IBM's Call Level Interface to DB2
	/// </summary>
	internal class DB2ClientPrototypes
	{
		[DllImport("db2cli.dll", EntryPoint = "SQLAllocHandle")]
			internal static extern short SQLAllocHandle(short handleType, IntPtr inputHandle, ref IntPtr outputHandle);
		[DllImport("db2cli.Dll", EntryPoint = "SQLConnect")]
			internal static extern short SQLConnect(IntPtr sqlHdbc, string serverName, short serverNameLength, string userName, short userNameLength, string authentication, short authenticationLength);
		[DllImport("db2cli.Dll", CharSet = CharSet.Auto, EntryPoint = "SQLDisconnect")]
			internal static extern short SQLDisconnect(IntPtr sqlHdbc);
		[DllImport("db2cli.dll", EntryPoint = "SQLGetDiagRec")]
			internal static extern short SQLGetDiagRec( short handleType, IntPtr handle, short recNum, [Out] StringBuilder sqlState, ref IntPtr nativeErrorPtr, [Out] StringBuilder errorMessage, short bufferLength, ref IntPtr shortTextLengthPtr);
		[DllImport("db2cli.Dll", EntryPoint = "SQLSetConnectAttr")]
			internal static extern short SQLSetConnectAttr(IntPtr sqlHdbc, long sqlAttr, [In] IntPtr sqlValuePtr, long sqlValueLength);
		[DllImport("db2cli.Dll", EntryPoint = "SQLEndTran")]
			internal static extern short SQLEndTran (short handleType, IntPtr handle, short fType);
		[DllImport("db2cli.Dll", EntryPoint = "SQLCancel")]
			internal static extern short SQLCancel(IntPtr handle);
		[DllImport("db2cli.dll", EntryPoint = "SQLNumResultCols")]
			internal static extern short SQLNumResultCols(IntPtr handle, ref int numCols);
		[DllImport("db2cli.Dll", EntryPoint = "SQLFetch")]
			internal static extern short SQLFetch(IntPtr handle);
		[DllImport("db2cli.dll", EntryPoint = "SQLRowCount")]
			internal static extern short SQLRowCount(IntPtr stmtHandle, ref int numRows);
		[DllImport("db2cli.dll", EntryPoint = "SQLExecute")]
			internal static extern short SQLExecute(IntPtr handle);
		[DllImport ("db2cli.dll", EntryPoint = "SQLExecDirect")]
			internal static extern short SQLExecDirect(IntPtr stmtHandle, string stmt, int length);
		[DllImport("db2cli.Dll", EntryPoint = "SQLDescribeCol")]
			internal static extern short SQLDescribeCol(IntPtr stmtHandle, ushort colNum, [Out] StringBuilder colName, short colNameMaxLength, IntPtr colNameLength, ref IntPtr dataType, ref IntPtr colSizePtr, ref IntPtr scalePtr, ref IntPtr nullablePtr );
		[DllImport("db2cli.dll", EntryPoint = "SQLBindCol")]
			internal static extern short  SQLBindCol(IntPtr stmtHandle, ushort colNum, int dataType, [Out] byte[] dataBufferPtr, int dataBufferLength, int StrLen_or_IndPtr);
		[DllImport("db2cli.dll", EntryPoint = "SQLDriverConnect")]
			internal static extern short SQLDriverConnect(IntPtr hdbc, int centered, [In] string inConnectStr, [In] int inStrLength, [Out] StringBuilder outConnectStr, [Out] int outStrCapacity, [Out] IntPtr outStrLengthReturned, [In] int completion);
		[DllImport("db2cli.dll", EntryPoint = "SQLPrepare")]
			internal static extern short SQLPrepare(IntPtr stmtHandle, string stmt, int length);
		[DllImport("db2cli.dll", EntryPoint = "SQLDescribeParam")]
			internal static extern short SQLDescribeParam(IntPtr stmtHandle, short paramNumber,ref IntPtr dataType, ref IntPtr paramSize, ref IntPtr decimalDigits, ref IntPtr nullable);
		[DllImport("db2cli.dll", EntryPoint = "SQLNumParams")]
			internal static extern short SQLNumParams(IntPtr stmtHandle, ref IntPtr numParams);
		[DllImport("db2cli.dll")]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber, 
			short dataType, short valueType, short paramType, uint colSize, short decDigits, 
			byte[] dataBufferPtr, int dataBufferLength, int StrLen_or_IndPtr);
		[DllImport("db2cli.dll", EntryPoint = "SQLBindParameter")]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber, 
			short dataType, short valueType, short paramType, uint colSize, short decDigits,
			ref int dataBufferPtr, int dataBufferLength, int StrLen_or_IndPtr);
		[DllImport("db2cli.dll", EntryPoint = "SQLGetLength")]
			internal static extern short SQLGetLength( IntPtr stmtHandle, short locatorCType, int Locator,
			IntPtr stringLength, IntPtr indicatorValue);
		[DllImport("db2cli.dll", EntryPoint = "SQLGetPosition")]
			internal static extern short SQLGetPosition(IntPtr stmtHandle, short locatorCType, int sourceLocator, int searchLocator, 
			string searchLiteral, int searchLiteralLength, uint fromPosition, IntPtr locatedAt, IntPtr indicatorValue);
		[DllImport("db2cli.dll", EntryPoint = "SQLGetPosition")]
		    internal static extern short SQLBindFileToCol (IntPtr stmtHandle, ushort colNum, string fileName, IntPtr fileNameLength, 
			IntPtr fileOptions, short maxFileNameLength, IntPtr stringLength, IntPtr indicatorValue);
		[DllImport("db2cli.dll", EntryPoint = "SQLGetPosition")]
		    internal static extern short SQLBindFileToParam (IntPtr stmtHandle, ushort targetType, short dataType, string fileName,
			IntPtr fileNameLength, short maxFileNameLength, IntPtr indicatorValue);
	}
}
