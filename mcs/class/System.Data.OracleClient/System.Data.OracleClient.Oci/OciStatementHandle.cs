// 
// OciStatementHandle.cs 
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
// 
// Author: 
//     Tim Coleman <tim@timcoleman.com>
//         
// Copyright (C) Tim Coleman, 2003
// 

using System;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciStatementHandle : OciHandle, IOciHandle, IDisposable
	{
		#region Fields

		OciStatementLanguage language;
		OciStatementMode mode;
		OciServiceHandle serviceHandle;
		OciErrorHandle errorHandle;
	
		#endregion // Fields

		#region Constructors

		public OciStatementHandle (OciEnvironmentHandle environment, IntPtr handle)
			: base (OciHandleType.Statement, environment, handle)
		{
			language = OciStatementLanguage.NTV;
			mode = OciStatementMode.Default;
		}

		#endregion // Constructors

		#region Properties

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		public OciStatementLanguage Language {
			get { return language; }
			set { language = value; }
		}

		public OciServiceHandle Service {
			get { return serviceHandle; }
			set { serviceHandle = value; }
		}

		#endregion // Properties

		#region Methods

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
							[MarshalAs (UnmanagedType.U4)] int stmt_length,
							[MarshalAs (UnmanagedType.U4)] OciStatementLanguage language,
							[MarshalAs (UnmanagedType.U4)] OciStatementMode mode);

		public void Dispose ()
		{
			Environment.FreeHandle (this);
		}

		public bool ExecuteQuery ()
		{
			return Execute (false);
		}

		public bool ExecuteNonQuery ()
		{
			return Execute (true);
		}

		public bool Execute (bool nonQuery)
		{
			int status = 0;
			status = OCIStmtExecute (Service.Handle,
						Handle,
						ErrorHandle.Handle,
						nonQuery,
						0,
						IntPtr.Zero,
						IntPtr.Zero,
						OciExecuteMode.Default);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return true;
		}

		public OciStatementType GetStatementType ()
		{
			int status = 0;
			int size;
			int statementType;

			status = OciGlue.OCIAttrGetInt32 (Handle,
							OciHandleType.Statement,
							out statementType,
							out size,
							OciAttributeType.StatementType,
							errorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return (OciStatementType) statementType;
		}

		public void Prepare (string commandText)
		{
			int status = 0;
			status = OCIStmtPrepare (Handle,
						errorHandle.Handle,
						commandText,
						commandText.Length,
						language,
						mode);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		#endregion // Methods
	}
}
