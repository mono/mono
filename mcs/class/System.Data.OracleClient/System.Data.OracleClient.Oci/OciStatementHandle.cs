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
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciStatementHandle : OciHandle, IDisposable
	{
		#region Fields

		int columnCount;
		bool disposed = false;
		OciErrorHandle errorHandle;
		bool moreResults;
		OciServiceHandle serviceHandle;
		ArrayList values;
	
		#endregion // Fields

		#region Constructors

		public OciStatementHandle (OciHandle parent, IntPtr handle)
			: base (OciHandleType.Statement, parent, handle)
		{
			moreResults = false;
		}

		#endregion // Constructors

		#region Properties

		public int ColumnCount {
			get { return columnCount; }
		}

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		public OciServiceHandle Service {
			get { return serviceHandle; }
			set { serviceHandle = value; }
		}

		public ArrayList Values {
			get { return values; }
		}

		#endregion // Properties

		#region Methods

		[DllImport ("oci")]
		static extern int OCIDescriptorFree (IntPtr descp,
						[MarshalAs (UnmanagedType.U4)] OciHandleType type);

		[DllImport ("oci")]
		static extern int OCIParamGet (IntPtr hndlp,
						[MarshalAs (UnmanagedType.U4)] OciHandleType htype,
						IntPtr errhp,
						out IntPtr parmdpp,
						[MarshalAs (UnmanagedType.U4)] int pos);

		[DllImport ("oci")]
		static extern int OCIStmtExecute (IntPtr svchp,
						IntPtr stmthp,
						IntPtr errhp,
						[MarshalAs (UnmanagedType.U4)] bool iters,
						uint rowoff,
						IntPtr snap_in,
						IntPtr snap_out,
						[MarshalAs (UnmanagedType.U4)] OciExecuteMode mode);

		[DllImport ("oci")]
		public static extern int OCIStmtFetch (IntPtr stmtp,
							IntPtr errhp,
							uint nrows,
							ushort orientation,
							uint mode);
							

		[DllImport ("oci")]
		public static extern int OCIStmtPrepare (IntPtr stmthp,
							IntPtr errhp,
							string stmt,
							[MarshalAs (UnmanagedType.U4)] int stmt_length,
							[MarshalAs (UnmanagedType.U4)] OciStatementLanguage language,
							[MarshalAs (UnmanagedType.U4)] OciStatementMode mode);

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				base.Dispose (disposing);
			}
		}

		public OciParameterDescriptor GetParameter (int position)
		{
			IntPtr handle = IntPtr.Zero;
			int status = 0;

			status = OCIParamGet (this,
						OciHandleType.Statement,
						ErrorHandle,
						out handle,
						position + 1);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			OciParameterDescriptor output = new OciParameterDescriptor (this, handle);
			output.ErrorHandle = ErrorHandle;
			return output;
		}

		public OciDefineHandle GetDefineHandle (int position)
		{
			OciDefineHandle defineHandle = new OciDefineHandle (this, IntPtr.Zero);
			defineHandle.ErrorHandle = ErrorHandle;
			defineHandle.DefineByPosition (position);

			return defineHandle;
		}

		void Define ()
		{
			values = new ArrayList ();
			for (int i = 0; i < columnCount; i += 1)  
				values.Add (GetDefineHandle (i));
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
			columnCount = 0;
			moreResults = false;

			status = OCIStmtExecute (Service,
						Handle,
						ErrorHandle,
						nonQuery,
						0,
						IntPtr.Zero,
						IntPtr.Zero,
						OciExecuteMode.Default);

			switch (status) {
			case OciGlue.OCI_DEFAULT:
				if (!nonQuery) {
					GetColumnCount ();
					Define ();
					moreResults = true;
				}
				break;
			case OciGlue.OCI_NO_DATA:
				break;
			default:
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			return true;
		}

		void GetColumnCount ()
		{
			columnCount = GetAttributeInt32 (OciAttributeType.ParameterCount, ErrorHandle);
		}

		public OciStatementType GetStatementType ()
		{
			return (OciStatementType) GetAttributeInt32 (OciAttributeType.StatementType, ErrorHandle);
		}

		public bool Fetch ()
		{
			int status = 0;
			status = OCIStmtFetch (Handle,
						ErrorHandle.Handle,
						1,
						2,
						0);

			switch (status) {
			case OciGlue.OCI_NO_DATA:
				moreResults = false;
				break;
			case OciGlue.OCI_DEFAULT:
				moreResults = true;
				break;
			default:
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return moreResults;
		}

		public void Prepare (string commandText)
		{
			int status = 0;

			status = OCIStmtPrepare (this,
						ErrorHandle,
						commandText,
						commandText.Length,
						OciStatementLanguage.NTV,
						OciStatementMode.Default);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		#endregion // Methods
	}
}
