using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.Db2Client
{
	/// <summary>
	/// db2Prototypes class is a wrapper for the db2.lib, IBM's Call Level Interface to Db2
	/// </summary>
	internal class Db2CLIWrapper
	{
		private const string libname = "db2cli";
		
		[DllImport(libname, EntryPoint = "SQLAllocHandle")]
			internal static extern short SQLAllocHandle(short handleType, IntPtr inputHandle,  ref IntPtr outputHandle);
		[DllImport(libname, EntryPoint = "SQLConnect")]
			internal static extern short SQLConnect(IntPtr sqlHdbc, string serverName, short serverNameLength, string userName, short userNameLength, string authentication, short authenticationLength);
		[DllImport(libname, EntryPoint = "SQLColAttribute")]
			internal static extern short SQLColAttribute(IntPtr StatementHandle, short ColumnNumber, short FieldIdentifier, IntPtr CharacterAttribute, short BufferLength,  ref short StringLength,  ref int NumericAttribute);
		[DllImport(libname, EntryPoint = "SQLGetData")]
			internal static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetPtr, IntPtr BufferLength, ref IntPtr StrLen_or_Ind);
		[DllImport(libname, EntryPoint="SQLMoreResults")]
			internal static extern short SQLMoreResults(IntPtr StatementHandle);
		
		[DllImport(libname, EntryPoint = "SQLGetData")]
		internal static extern short SQLGetData(IntPtr StatementHandle, short ColumnNumber, short TargetType, byte[] TargetPtr, IntPtr BufferLength, ref IntPtr StrLen_or_Ind);
			
		[DllImport(libname, CharSet = CharSet.Auto, EntryPoint = "SQLDisconnect")]
			internal static extern short SQLDisconnect(IntPtr sqlHdbc);
		[DllImport(libname, EntryPoint = "SQLGetDiagRec")]
			internal static extern short SQLGetDiagRec( short handleType, IntPtr handle, short recNum, [Out] StringBuilder sqlState, ref IntPtr nativeErrorPtr, [Out] StringBuilder errorMessage, short bufferLength, ref IntPtr shortTextLengthPtr);
		[DllImport(libname, EntryPoint = "SQLSetConnectAttr")]
			internal static extern short SQLSetConnectAttr(IntPtr sqlHdbc, long sqlAttr, [In] IntPtr sqlValuePtr, long sqlValueLength);
		[DllImport(libname, EntryPoint = "SQLEndTran")]
			internal static extern short SQLEndTran (short handleType, IntPtr handle, short fType);
		[DllImport(libname, EntryPoint = "SQLCancel")]
			internal static extern short SQLCancel(IntPtr handle);
		[DllImport(libname, EntryPoint = "SQLNumResultCols")]
			internal static extern short SQLNumResultCols(IntPtr handle, ref int numCols);
		[DllImport(libname, EntryPoint = "SQLFetch")]
			internal static extern short SQLFetch(IntPtr handle);
		[DllImport(libname, EntryPoint = "SQLRowCount")]
			internal static extern short SQLRowCount(IntPtr stmtHandle, ref int numRows);
		[DllImport(libname, EntryPoint = "SQLExecute")]
			internal static extern short SQLExecute(IntPtr handle);
		[DllImport (libname, EntryPoint = "SQLExecDirect")]
			internal static extern short SQLExecDirect(IntPtr stmtHandle, string stmt, int length);
		[DllImport(libname, EntryPoint = "SQLDescribeCol")]
			internal static extern short SQLDescribeCol(IntPtr stmtHandle, ushort colNum, StringBuilder colName, short colNameMaxLength, IntPtr colNameLength, ref IntPtr dataType, ref IntPtr colSizePtr, ref IntPtr scalePtr, ref IntPtr nullablePtr );
		[DllImport(libname, EntryPoint = "SQLBindCol")]
			internal static extern short SQLBindCol(IntPtr StatementHandle, short ColumnNumber, short TargetType, IntPtr TargetValue, IntPtr BufferLength, ref IntPtr StrLen_or_Ind); 
		[DllImport(libname, EntryPoint = "SQLDriverConnect")]
			internal static extern short SQLDriverConnect(IntPtr hdbc, int centered, [In] string inConnectStr, [In] int inStrLength, [Out] StringBuilder outConnectStr, [Out] int outStrCapacity, [Out] IntPtr outStrLengthReturned, [In] int completion);
		[DllImport(libname, EntryPoint = "SQLPrepare")]
			internal static extern short SQLPrepare(IntPtr stmtHandle, string stmt, int length);
		[DllImport(libname, EntryPoint = "SQLDescribeParam")]
			internal static extern short SQLDescribeParam(IntPtr stmtHandle, short paramNumber,ref IntPtr dataType, ref IntPtr paramSize, ref IntPtr decimalDigits, ref IntPtr nullable);
		[DllImport(libname, EntryPoint = "SQLNumParams")]
			internal static extern short SQLNumParams(IntPtr stmtHandle, ref IntPtr numParams);
		[DllImport(libname)]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber, 
			short dataType, short valueType, short paramType, uint colSize, short decDigits, 
			IntPtr dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);
			
		[DllImport(libname)]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber, 
			short dataType, short valueType, short paramType, uint colSize, short decDigits, 
			byte[] dataBufferPtr, int dataBufferLength, IntPtr StrLen_or_IndPtr);
						
		[DllImport(libname, EntryPoint = "SQLBindParameter")]
			internal static extern short SQLBindParameter(IntPtr stmtHandle, ushort paramNumber, 
			short dataType, short valueType, short paramType, uint colSize, short decDigits,
			ref int dataBufferPtr, int dataBufferLength, int StrLen_or_IndPtr);
		
		[DllImport(libname, EntryPoint = "SQLDescribeParam")]
			internal static extern short SQLDescribeParam(IntPtr stmtHandle, short ParameterNumber, IntPtr DataTypePtr, IntPtr ParameterSizePtr, IntPtr DecimalDigitsPtr, IntPtr NullablePtr); 
		
		[DllImport(libname, EntryPoint = "SQLGetLength")]
			internal static extern short SQLGetLength( IntPtr stmtHandle, short locatorCType, int Locator,
			IntPtr stringLength, IntPtr indicatorValue);
		[DllImport(libname, EntryPoint = "SQLGetPosition")]
			internal static extern short SQLGetPosition(IntPtr stmtHandle, short locatorCType, int sourceLocator, int searchLocator, 
			string searchLiteral, int searchLiteralLength, uint fromPosition, IntPtr locatedAt, IntPtr indicatorValue);
		[DllImport(libname, EntryPoint = "SQLGetPosition")]
		    internal static extern short SQLBindFileToCol (IntPtr stmtHandle, ushort colNum, string fileName, IntPtr fileNameLength, 
			IntPtr fileOptions, short maxFileNameLength, IntPtr stringLength, IntPtr indicatorValue);
		[DllImport(libname, EntryPoint = "SQLGetPosition")]
		    internal static extern short SQLBindFileToParam (IntPtr stmtHandle, ushort targetType, short dataType, string fileName,
			IntPtr fileNameLength, short maxFileNameLength, IntPtr indicatorValue);	
	}
}
