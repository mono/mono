// 
// ociglue.cs - provides glue between 
//              managed C#/.NET System.Data.OracleClient.dll and 
//              unmanaged native c library oci.dll
//              to be used in Mono System.Data.OracleClient as
//              the Oracle 8i data provider.
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.OCI
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
// 
// Authors: 
//     Daniel Morgan <danmorg@sc.rr.com>
//     Tim Coleman <tim@timcoleman.com>
//         
// Copyright (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman, 2002
// 

using System;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciGlue 
	{
		IntPtr environmentHandle;
		IntPtr errorHandle;
		IntPtr serverHandle;
		IntPtr serviceHandle;
		IntPtr sessionHandle;

		// TODO: need to clean up, dispose, close, etc...

		
		// connection parameters
		string database = "";
		string username = "";
		string password = "";

		// other codes
		public const int OCI_DEFAULT = 0;
		public const int OCI_SUCCESS = 0;
		public const int OCI_SUCCESS_WITH_INFO = 1;
		public const int OCI_RESERVED_FOR_INT_USE = 200;
		public const int OCI_NO_DATA = 100;
		public const int OCI_ERROR = -1;
		public const int OCI_INVALID_HANDLE = -2;
		public const int OCI_NEED_DATA = 99;
		public const int OCI_STILL_EXECUTING = -3123;
		public const int OCI_CONTINUE = -24200;

		private UInt32 ociGlueConnectionHandle = 0;

		// http://download-west.oracle.com/docs/cd/A87861_01/NT817EE/index.htm
		// from oracle/ora81/oci/include/oci.h

		[DllImport ("oci", EntryPoint = "OCIAttrSet")]
		public static extern int OCIAttrSet (IntPtr trgthndlp,
							[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
							IntPtr attributep,
							uint size,
							[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
							IntPtr errhp);

		[DllImport ("oci", EntryPoint = "OCIAttrSet")]
		public static extern int OCIAttrSetString (IntPtr trgthndlp,
							[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
							string attributep,
							uint size,
							[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
							IntPtr errhp);

		[DllImport ("oci", EntryPoint = "OCIAttrGet")]
		public static extern int OCIAttrGetInt32 (IntPtr trgthndlp,
							[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
							out int attributep,
							out int sizep,
							[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
							IntPtr errhp);

		[DllImport ("oci")]
		public static extern int OCIEnvCreate (out IntPtr envhpp,
							[MarshalAs (UnmanagedType.U4)] OciEnvironmentMode mode,
							IntPtr ctxp,
							IntPtr malocfp,
							IntPtr ralocfp,
							IntPtr mfreefp,
							int xtramem_sz,
							IntPtr usrmempp);

		[DllImport ("oci")]
		public static extern int OCIErrorGet (IntPtr hndlp,
							uint recordno,
							out string sqlstate,
							out int errcodep,
							out string bufp,
							uint bufsize,
							[MarshalAs (UnmanagedType.U4)] OciHandleType type);

		[DllImport ("oci")]
		public static extern int OCIHandleAlloc (IntPtr parenth, 
							out IntPtr hndlpp, 
							[MarshalAs (UnmanagedType.U4)] OciHandleType type, 
							int xtramem_sz, 
							IntPtr usrmempp);

		[DllImport ("oci")]
		public static extern int OCIHandleFree (IntPtr hndlp,
							[MarshalAs (UnmanagedType.U4)] OciHandleType type);
		[DllImport ("oci")]
		public static extern int OCIServerAttach (IntPtr srvhp, 
							IntPtr errhp, 
							string dblink, 
							int dblink_len, 
							uint mode);

		[DllImport ("oci")]
		public static extern int OCIServerDetach (IntPtr srvhp,
							IntPtr errhp,
							uint mode);

		[DllImport ("oci")]
		public static extern int OCISessionBegin (IntPtr svchp,
							IntPtr errhp,
							IntPtr usrhp,
							[MarshalAs (UnmanagedType.U4)] OciCredentialType credt,
							[MarshalAs (UnmanagedType.U4)] OciSessionMode mode);

		[DllImport ("oci")]
		public static extern int OCIStmtExecute (IntPtr svchp,
							IntPtr stmthp,
							IntPtr errhp,
							[MarshalAs (UnmanagedType.U4)] bool iters,
							uint rowoff,
							IntPtr snap_in,
							IntPtr snap_out,
							[MarshalAs (UnmanagedType.U4)] OciExecuteMode mode);

		[DllImport ("oci")]
		public static extern int OCIStmtPrepare (IntPtr stmthp,
							IntPtr errhp,
							string stmt,
							uint stmt_length,
							[MarshalAs (UnmanagedType.U4)] OciStatementLanguage language,
							[MarshalAs (UnmanagedType.U4)] OciStatementMode mode);

		[DllImport ("oci")]
		public static extern int OCITransStart (IntPtr svchp,
							IntPtr errhp,
							uint timeout,
							[MarshalAs (UnmanagedType.U4)] OciTransactionFlags flags);

		public string CheckError (Int32 status) 
		{
			IntPtr intptrMsg = IntPtr.Zero;
			string strMsg = "";
			string msg = "";
		/*	
			intptrMsg = OciGlue_CheckError(status, ociGlueConnectionHandle);
			if(intptrMsg != IntPtr.Zero)
				strMsg = Marshal.PtrToStringAnsi(intptrMsg);
				if(strMsg != null) {
					msg = String.Copy(strMsg);
					OciGlue_Free(intptrMsg);
				}

		*/
			return msg;
		}

		public int Connect (OracleConnectionInfo conInfo) 
		{
			Int32 status = 0;
			string error = "";
			int errcode = 0;
			string errmsg = "";
			string strMsg = "";
			
			status = OCIEnvCreate (out environmentHandle, 
						OciEnvironmentMode.Default | OciEnvironmentMode.NoUserCallback,
						IntPtr.Zero,
						IntPtr.Zero,
						IntPtr.Zero,
						IntPtr.Zero,
						0, 
						IntPtr.Zero);
			if (status != 0) {
				Console.WriteLine ("ERR1 {0}", status);
				return status;
			}

			// Allocate the service handle
			status = OCIHandleAlloc (environmentHandle,
						out serviceHandle,
						OciHandleType.Service,
						0,
						IntPtr.Zero);

				
			if (status != 0) {
				Console.WriteLine ("ERR2 {0}", status);
				return status;
			}

			// Allocate the error handle
			status = OCIHandleAlloc (environmentHandle,
						out errorHandle,
						OciHandleType.Error,
						0,
						IntPtr.Zero);
			if (status != 0) {
				Console.WriteLine ("ERR3");
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}

			// Allocate the server handle
			status = OCIHandleAlloc (environmentHandle,
						out serverHandle,
						OciHandleType.Server,
						0,
						IntPtr.Zero);

			if (status != 0) {
				Console.WriteLine ("ERR4");
				OCIHandleFree (errorHandle, OciHandleType.Error);
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}

			// Allocate the session handle
			status = OCIHandleAlloc (environmentHandle,
						out sessionHandle,
						OciHandleType.Session,
						0,
						IntPtr.Zero);


			if (status != 0) {
				Console.WriteLine ("ERR5");
				OCIHandleFree (serverHandle, OciHandleType.Server);
				OCIHandleFree (errorHandle, OciHandleType.Error);
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}

			/* Attach to Oracle server */
			status = OCIServerAttach (serverHandle,
						errorHandle,
						conInfo.Database,
						conInfo.Database.Length,
						OCI_DEFAULT);

			if (status != 0) {
				Console.WriteLine ("ERR6 {0}", status);
				OCIHandleFree (sessionHandle, OciHandleType.Session);
				OCIHandleFree (serverHandle, OciHandleType.Server);
				OCIHandleFree (errorHandle, OciHandleType.Error);
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}

			/*Set the server attribute in the service context */
			status = OCIAttrSet (serviceHandle,
						OciHandleType.Service,
						serverHandle,
						0,
						OciAttributeType.Server,
						errorHandle);
						
			if (status != 0) {
				Console.WriteLine ("ERR7 {0}", status);
				OCIHandleFree (sessionHandle, OciHandleType.Session);
				OCIHandleFree (serverHandle, OciHandleType.Server);
				OCIHandleFree (errorHandle, OciHandleType.Error);
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}

			/* Set the username attribute */
			status = OCIAttrSetString (sessionHandle,
						OciHandleType.Session,
						conInfo.Username,
						(uint) conInfo.Username.Length,
						OciAttributeType.Username,
						errorHandle);

			if (status != 0) {
				Console.WriteLine ("ERR8 {0}", status);
				OCIHandleFree (sessionHandle, OciHandleType.Session);
				OCIHandleFree (serverHandle, OciHandleType.Server);
				OCIHandleFree (errorHandle, OciHandleType.Error);
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}

			/* Set the password attribute */
			status = OCIAttrSetString (sessionHandle,
						OciHandleType.Session,
						conInfo.Password,
						(uint) conInfo.Password.Length,
						OciAttributeType.Password,
						errorHandle);

			if (status != 0) {
				Console.WriteLine ("ERR9 {0}", status);
				OCIHandleFree (sessionHandle, OciHandleType.Session);
				OCIHandleFree (serverHandle, OciHandleType.Server);
				OCIHandleFree (errorHandle, OciHandleType.Error);
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}

			/* Begin the session */
			status = OCISessionBegin (serviceHandle,
						errorHandle,
						sessionHandle,
						OciCredentialType.RDBMS,
						OciSessionMode.Default);

			if (status != 0) {
				Console.WriteLine ("ERR10 {0}", status);
				OCIServerDetach (serverHandle, errorHandle, OCI_DEFAULT);
				OCIHandleFree (sessionHandle, OciHandleType.Session);
				OCIHandleFree (serverHandle, OciHandleType.Server);
				OCIHandleFree (errorHandle, OciHandleType.Error);
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}

			/* Set the session attribute in the service context */
			status = OCIAttrSet (serviceHandle, 
						OciHandleType.Service,
						sessionHandle,
						0,
						OciAttributeType.Session,
						errorHandle);

			if (status != 0) {
				Console.WriteLine ("ERR11 {0}", status);
				OCIServerDetach (serverHandle, errorHandle, OCI_DEFAULT);
				OCIHandleFree (sessionHandle, OciHandleType.Session);
				OCIHandleFree (serverHandle, OciHandleType.Server);
				OCIHandleFree (errorHandle, OciHandleType.Error);
				OCIHandleFree (serviceHandle, OciHandleType.Service);
				return status;
			}
						
			return status;
		}

		public IntPtr PrepareStatement (string commandText)
		{
			IntPtr statementHandle;
			int status;

			status = OCIHandleAlloc (environmentHandle,
						out statementHandle,
						OciHandleType.Statement,
						0,
						IntPtr.Zero);

			if (status != 0) {
				Console.WriteLine ("ERR12 {0}", status);
				return IntPtr.Zero;
			}

			status = OCIStmtPrepare (statementHandle,
						errorHandle,
						commandText,
						(uint) commandText.Length,
						OciStatementLanguage.NTV,
						OciStatementMode.Default);

			if (status != 0) {
				Console.WriteLine ("ERR13 {0}", status);
				return IntPtr.Zero;
			}
		
			return statementHandle;
		}

		public OciStatementType GetStatementType (IntPtr statementHandle)
		{
			int status = 0;
			int statementType;
			int size;

			status = OCIAttrGetInt32 (statementHandle,
						OciHandleType.Statement,
						out statementType,
						out size,
						OciAttributeType.StatementType,
						errorHandle);

			if (status != 0) {
				Console.WriteLine ("ERR14 {0}", status);
				return OciStatementType.Default;
			}

			return (OciStatementType) statementType;
		}

		public bool ExecuteStatement (IntPtr statementHandle, OciStatementType statementType)
		{
			int status = 0;
			status = OCIStmtExecute (serviceHandle,
						statementHandle,
						errorHandle,
						statementType != OciStatementType.Select,
						0,
						IntPtr.Zero,
						IntPtr.Zero,
						OciExecuteMode.Default);

			if (status != 0) {
				Console.WriteLine ("ERR15 {0}", status);
				return false;
			}

			return true;
		}

		public IntPtr BeginTransaction () 
		{
			int status = 0;
			IntPtr transactionHandle;

			// Allocate the transaction handle
			status = OCIHandleAlloc (environmentHandle,
						out transactionHandle,
						OciHandleType.Transaction,
						0,
						IntPtr.Zero);

			if (status != 0) {
				Console.WriteLine ("ERR16 {0}", status);
				return IntPtr.Zero;
			}


			// Attach the transaction to the service context 
			status = OCIAttrSet (serviceHandle,
						OciHandleType.Service,
						transactionHandle,
						0,
						OciAttributeType.Transaction,
						errorHandle);

			if (status != 0) {
				Console.WriteLine ("ERR17 {0}", status);
				return IntPtr.Zero;
			}

			status = OCITransStart (serviceHandle,
						errorHandle,
						60,
						OciTransactionFlags.New);

			if (status != 0) {
				Console.WriteLine ("ERR18 {0}", status);
				return IntPtr.Zero;
			}

			return transactionHandle;
		}

		public Int32 CommitTransaction () 
		{
			Int32 status = 0;
			string msg = "";
/*
			Console.WriteLine ("OciGlue_CommitTransaction");
			Console.WriteLine ("  Handle: " + ociGlueConnectionHandle);
			status = OciGlue.OciGlue_CommitTransaction (ociGlueConnectionHandle);

			if (status != 0) {
				msg = CheckStatus (status);
				throw new Exception (msg);
			}
			*/

			return status;
		}

		public Int32 RollbackTransaction () 
		{
			Int32 status = 0;
			string msg = "";

			/*
			Console.WriteLine ("OciGlue_RollbackTransaction");
			Console.WriteLine ("  Handle: " + ociGlueConnectionHandle);
			status = OciGlue.OciGlue_RollbackTransaction (ociGlueConnectionHandle);

			if (status != 0) {
				msg = CheckStatus (status);
				throw new Exception (msg);
			}

			*/

			return status;
		}

		public Int32 Disconnect() {

			Int32 status = 0;
			string msg = "";
		
		/*
			Console.WriteLine("OciGlue_Disconnect");
			Console.WriteLine("  Handle: " + ociGlueConnectionHandle);
			status = OciGlue.OciGlue_Disconnect (ociGlueConnectionHandle);
			ociGlueConnectionHandle = 0;
			
			if(status != 0) {
				msg = CheckStatus(status);
				throw new Exception(msg);
			}
			*/
						
			return status;
		}


		// Helper methods
		public Int32 PrepareAndExecuteNonQuerySimple(string sql) 
		{
			Int32 status = 0;
			int found = 0;

/*
			Console.WriteLine("PrepareAndExecuteNonQuerySimple");
			status = OciGlue_PrepareAndExecuteNonQuerySimple (
				ociGlueConnectionHandle, sql, out found);

			Console.WriteLine("  Handle: " + ociGlueConnectionHandle +
				" Found: " + found.ToString());

			CheckStatus(status);
			*/
			return status;
		}
		
		public string CheckStatus(Int32 status) {		
			string msg = "";
						
			switch (status) {
			case OCI_SUCCESS:
				msg = "Succsss";
				break;
			case OCI_SUCCESS_WITH_INFO:
				msg = "Error - OCI_SUCCESS_WITH_INFO";
				break;
			case OCI_NEED_DATA:
				msg = "Error - OCI_NEED_DATA";
				break;
			case OCI_NO_DATA:
				msg = "Error - OCI_NODATA";
				break;
			case OCI_ERROR:
				if(ociGlueConnectionHandle != 0)
					msg = CheckError(status);
				else
					msg = "OCI_ERROR";
				break;
			case OCI_INVALID_HANDLE:
				msg = "Error - OCI_INVALID_HANDLE";
				break;
			case OCI_STILL_EXECUTING:
				msg = "Error - OCI_STILL_EXECUTE";
				break;
			case OCI_CONTINUE:
				msg = "Error - OCI_CONTINUE";
				break;
			default:
				msg = "Default";
				break;
			}
			return msg;
		}
	}
}
