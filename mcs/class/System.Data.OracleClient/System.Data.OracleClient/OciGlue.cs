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
// Namespace: System.Data.OracleClient
// 
// Author: 
//     Daniel Morgan <danmorg@sc.rr.com>
//         
// Copyright (C) Daniel Morgan, 2002
// 

using System;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.OCI {
	internal sealed class OciGlue {

		// TODO: need to clean up, dispose, close, etc...
		
		// connection parameters
		string database = "";
		string username = "";
		string password = "";

		public const Int32 OCI_SUCCESS = 0;
		public const Int32 OCI_SUCCESS_WITH_INFO = 1;
		public const Int32 OCI_RESERVED_FOR_INT_USE = 200;
		public const Int32 OCI_NO_DATA = 100;
		public const Int32 OCI_ERROR = -1;
		public const Int32 OCI_INVALID_HANDLE = -2;
		public const Int32 OCI_NEED_DATA = 99;
		public const Int32 OCI_STILL_EXECUTING = -3123;
		public const Int32 OCI_CONTINUE = -24200;

		private UInt32 ociGlueConnectionHandle = 0;

		// http://download-west.oracle.com/docs/cd/A87861_01/NT817EE/index.htm
		// from oracle/ora81/oci/include/oci.h

		[DllImport ("ociglue")]
		public static extern Int32 OciGlue_BeginTransaction (UInt32 connection_handle);

		[DllImport ("ociglue")]
		public static extern Int32 OciGlue_CommitTransaction (UInt32 connection_handle);

		[DllImport ("ociglue")]
		public static extern Int32 OciGlue_RollbackTransaction (UInt32 connection_handle);

		[DllImport ("ociglue")]
		public static extern IntPtr OciGlue_Connect (out Int32 status,
			out UInt32 ociGlueConnectionHandle, out uint errcode, 
			string database, string username, string password);

		[DllImport ("ociglue")]
		public static extern Int32 OciGlue_Disconnect (UInt32 connection_handle);

		[DllImport ("ociglue")]
		public static extern Int32 OciGlue_PrepareAndExecuteNonQuerySimple (
			UInt32 ociGlueConnectionHandle,
			string sqlstmt, out int found);

		[DllImport ("ociglue")]
		public static extern UInt32 OciGlue_ConnectionCount();

		[DllImport ("ociglue")]
		public static extern IntPtr OciGlue_CheckError (Int32 status, UInt32 connection_handle);

		[DllImport ("ociglue")]
		public static extern void OciGlue_Free (IntPtr obj);

		public string CheckError (Int32 status) {
			IntPtr intptrMsg = IntPtr.Zero;
			string strMsg = "";
			string msg = "";
			
			intptrMsg = OciGlue_CheckError(status, ociGlueConnectionHandle);
			if(intptrMsg != IntPtr.Zero)
				strMsg = Marshal.PtrToStringAnsi(intptrMsg);
				if(strMsg != null) {
					msg = String.Copy(strMsg);
					OciGlue_Free(intptrMsg);
				}

			return msg;
		}

		public Int32 Connect(OracleConnectionInfo conInfo) {

			Int32 status = 0;
			string error = "";
			uint errcode = 0;
			string errmsg = "";
			string strMsg = "";
			IntPtr ipErrMsg = IntPtr.Zero;
			
			database = conInfo.Database;
			username = conInfo.Username;
			password = conInfo.Password;

			Console.WriteLine("OciGlue_Connect");
			ipErrMsg = OciGlue_Connect (out status,
				out ociGlueConnectionHandle, out errcode, 
				database, username, password);
			Console.WriteLine("  Handle: " + ociGlueConnectionHandle);

			if(status != 0) {
				if(status == -1) {
					if(ipErrMsg != IntPtr.Zero) {
						strMsg = "";
						strMsg = Marshal.PtrToStringAnsi(ipErrMsg);
						if(strMsg != null) {
							errmsg = String.Copy(strMsg);
							OciGlue_Free(ipErrMsg);
						}
					}
					error = "OCI Error - errcode: " + errcode.ToString() + " errmsg: " + errmsg;
					
				}
				else
					error = CheckStatus(status);

				throw new Exception("Error: Unable to connect: " + error);
			}		
						
			return status;
		}

		public Int32 BeginTransaction () 
		{
			Int32 status = 0;
			string msg = "";

			Console.WriteLine ("OciGlue_BeginTransaction");
			Console.WriteLine ("  Handle: " + ociGlueConnectionHandle);
			status = OciGlue.OciGlue_BeginTransaction (ociGlueConnectionHandle);

			if (status != 0) {
				msg = CheckStatus (status);
				throw new Exception (msg);
			}

			return status;
		}

		public Int32 CommitTransaction () 
		{
			Int32 status = 0;
			string msg = "";

			Console.WriteLine ("OciGlue_CommitTransaction");
			Console.WriteLine ("  Handle: " + ociGlueConnectionHandle);
			status = OciGlue.OciGlue_CommitTransaction (ociGlueConnectionHandle);

			if (status != 0) {
				msg = CheckStatus (status);
				throw new Exception (msg);
			}

			return status;
		}

		public Int32 RollbackTransaction () 
		{
			Int32 status = 0;
			string msg = "";

			Console.WriteLine ("OciGlue_RollbackTransaction");
			Console.WriteLine ("  Handle: " + ociGlueConnectionHandle);
			status = OciGlue.OciGlue_RollbackTransaction (ociGlueConnectionHandle);

			if (status != 0) {
				msg = CheckStatus (status);
				throw new Exception (msg);
			}

			return status;
		}

		public Int32 Disconnect() {

			Int32 status = 0;
			string msg = "";
			
			Console.WriteLine("OciGlue_Disconnect");
			Console.WriteLine("  Handle: " + ociGlueConnectionHandle);
			status = OciGlue.OciGlue_Disconnect (ociGlueConnectionHandle);
			ociGlueConnectionHandle = 0;
			
			if(status != 0) {
				msg = CheckStatus(status);
				throw new Exception(msg);
			}
						
			return status;
		}


		// Helper methods
		public Int32 PrepareAndExecuteNonQuerySimple(string sql) 
		{
			Int32 status = 0;
			int found = 0;

			Console.WriteLine("PrepareAndExecuteNonQuerySimple");
			status = OciGlue_PrepareAndExecuteNonQuerySimple (
				ociGlueConnectionHandle, sql, out found);

			Console.WriteLine("  Handle: " + ociGlueConnectionHandle +
				" Found: " + found.ToString());

			CheckStatus(status);
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
