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
		#region Fields

		bool connected;
		OciEnvironmentHandle environment;
		OciErrorHandle error;
		OciServerHandle server;
		OciServiceHandle service;
		OciSessionHandle session;

		// OCI Return Codes
		public const int OCI_DEFAULT = 0;
		public const int OCI_SUCCESS = 0;
		public const int OCI_SUCCESS_WITH_INFO = 1; // Diagnostic or Warning message - call OCIErrorGet 
		public const int OCI_RESERVED_FOR_INT_USE = 200;
		public const int OCI_NO_DATA = 100;
		public const int OCI_ERROR = -1; // use error handle to get error code and description - call OCIErrorGet
		public const int OCI_INVALID_HANDLE = -2;
		public const int OCI_NEED_DATA = 99;
		public const int OCI_STILL_EXECUTING = -3123;
		public const int OCI_CONTINUE = -24200;

		#endregion // Fields

		#region Properties

		public bool Connected {
			get { return connected; }
		}

		public OciEnvironmentHandle Environment {
			get { return environment; }
		}

		public OciErrorHandle ErrorHandle {
			get { return error; }
		}

		public OciServiceHandle ServiceContext {
			get { return service; }
		}

		public OciServerHandle ServerHandle {
			get { return server; }
		}

		public OciSessionHandle SessionHandle {
			get { return session; }
		}

		#endregion // Properties

		#region Methods

		public void CreateConnection (OracleConnectionInfo conInfo) 
		{
			environment = new OciEnvironmentHandle (OciEnvironmentMode.Threaded | OciEnvironmentMode.NoUserCallback);

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

			session = (OciSessionHandle) environment.Allocate (OciHandleType.Session);
			if (session == null) {
				OciErrorInfo info = environment.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			session.Username = conInfo.Username;
			session.Password = conInfo.Password;
			session.Service = service;
				
			if (!server.Attach (conInfo.Database, ErrorHandle)) {
				OciErrorInfo info = error.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (!service.SetServer (server)) {
				OciErrorInfo info = error.HandleError ();
				Disconnect ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (!session.BeginSession (conInfo.CredentialType, OciSessionMode.Default, ErrorHandle)) {
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
			if (session != null) {
				session.EndSession (error);
				session.Dispose ();
				session = null;
			}
			if (server != null) {
				server.Detach (error);
				server.Dispose ();
				server = null;
			}
			if (error != null) {
				error.Dispose ();
				error = null;
			}
			if (service != null) {
				service.Dispose ();
				service = null;
			}
			if (environment != null) {
				environment.Dispose ();
				environment = null;
			}
		}

		public static string ReturnCodeToString (int status) 
		{
			switch (status) {
			case OCI_DEFAULT:
				return "OCI_DEFAULT or OCI_SUCCESS"; // both are zero
			case OCI_SUCCESS_WITH_INFO:
				return "OCI_SUCCESS_WITH_INFO"; 
			case OCI_RESERVED_FOR_INT_USE:
				return "OCI_RESERVED_FOR_INT_USE";
			case OCI_NO_DATA:
				return "OCI_NO_DATA";
			case OCI_ERROR:
				return "OCI_ERROR";
			case OCI_INVALID_HANDLE:
				return "OCI_INVALID_HANDLE";
			case OCI_NEED_DATA:
				return "OCI_NEED_DATA";
			case OCI_STILL_EXECUTING:
				return "OCI_STILL_EXECUTING";
			case OCI_CONTINUE:
				return "OCI_CONTINUE";
			}
			return "Unknown Error";
		}

		#endregion // Methods
	}
}

