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
using System.Text;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciGlue 
	{
		bool connected;
		OciEnvironmentHandle environment;
		OciErrorHandle error;
		OciServerHandle server;
		OciServiceHandle service;
		OciSessionHandle session;

		public bool Connected {
			get { return connected; }
		}

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

		[DllImport ("oci")]
		public static extern int OCIErrorGet (IntPtr hndlp,
							uint recordno,
							IntPtr sqlstate,
							out int errcodep,
							IntPtr bufp,
							uint bufsize,
							[MarshalAs (UnmanagedType.U4)] OciHandleType type);

		public void CreateConnection (OracleConnectionInfo conInfo) 
		{
			environment = new OciEnvironmentHandle (OciEnvironmentMode.NoUserCallback);
			if (environment.Handle == IntPtr.Zero)
				throw new OracleException (0, "Could not allocate the Oracle environment.");

			service = (OciServiceHandle) environment.Allocate (OciHandleType.Service);
			if (service == null) {
				OciErrorInfo info = environment.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			error = (OciErrorHandle) environment.Allocate (OciHandleType.Error);
			if (error == null) {
				OciErrorInfo info = environment.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			service.ErrorHandle = error;

			server = (OciServerHandle) environment.Allocate (OciHandleType.Server);
			if (server == null) {
				OciErrorInfo info = environment.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			server.ErrorHandle = error;
			server.TNSName = conInfo.Database;

			session = (OciSessionHandle) environment.Allocate (OciHandleType.Session);
			if (session == null) {
				OciErrorInfo info = environment.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			session.ErrorHandle = error;
			session.Username = conInfo.Username;
			session.Password = conInfo.Password;
			session.Service = service;
				
			if (!server.Attach ()) {
				OciErrorInfo info = error.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (!service.SetServer (server)) {
				OciErrorInfo info = error.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (!session.Begin (OciCredentialType.RDBMS, OciSessionMode.Default)) {
				OciErrorInfo info = error.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}


			if (!service.SetSession (session)) {
				OciErrorInfo info = error.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			connected = true;
		}

		public OciStatementHandle CreateStatement ()
		{
			OciStatementHandle statement = (OciStatementHandle) environment.Allocate (OciHandleType.Statement);
			if (statement == null) {
				OciErrorInfo info = environment.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			statement.ErrorHandle = error;
			statement.Service = service;

			return statement;	
		}

		public OciTransactionHandle CreateTransaction ()
		{
			OciTransactionHandle transaction = (OciTransactionHandle) environment.Allocate (OciHandleType.Transaction);
			if (transaction == null) {
				OciErrorInfo info = environment.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			transaction.ErrorHandle = error;
			transaction.Service = service;

			return transaction;
		}

		public void Disconnect() 
		{
			if (session != null)
				session.Dispose ();
			if (server != null)
				server.Dispose ();
			if (error != null)
				error.Dispose ();
			if (service != null)
				service.Dispose ();
			if (environment != null)
				environment.Dispose ();
		}
	}
}
