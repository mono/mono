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
//     Daniel Morgan <danielmorgan@verizon.net>
//         
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2005
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
		OracleCommand command;
	
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

		public OracleCommand Command {
			get { return command; }
			set { command = value; }
		}

		#endregion // Properties

		#region Methods
		
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				
				if (disposing) {
					if (values != null) {
						foreach (OciDefineHandle h in values)
							h.Dispose ();
						values = null;
					}
				}
				
				base.Dispose (disposing);
			}
		}

		public OciParameterDescriptor GetParameter (int position)
		{
			IntPtr handle = IntPtr.Zero;
			int status = 0;

			status = OciCalls.OCIParamGet (this,
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

		public OciDefineHandle GetDefineHandle (int position, OracleConnection connection)
		{
			OciDefineHandle defineHandle = new OciDefineHandle (this, IntPtr.Zero);
			defineHandle.ErrorHandle = ErrorHandle;
			defineHandle.DefineByPosition (position, connection);

			return defineHandle;
		}

		void Define ()
		{
			Define (command.Connection);
		}

		void Define (OracleConnection connection) 
		{
			values = new ArrayList ();
			for (int i = 0; i < columnCount; i += 1)  
				values.Add (GetDefineHandle (i, connection));
		}

		public bool ExecuteQuery (bool schemaOnly)
		{
			return Execute (false, false, schemaOnly);
		}

		public bool ExecuteNonQuery (bool useAutoCommit)
		{
			return Execute (true, useAutoCommit, false);
		}

		public bool Execute (bool nonQuery, bool useAutoCommit, bool schemaOnly)
		{
			int status = 0;
			columnCount = 0;
			moreResults = false;
			int executeMode;

			if( useAutoCommit)
				executeMode = (int)OciExecuteMode.CommitOnSuccess;
			else {
				if (schemaOnly)
					executeMode = (int)OciExecuteMode.DescribeOnly;
				else
					executeMode = (int)OciExecuteMode.Default;
			}

			if (this.disposed) 
			{
				throw new InvalidOperationException ("StatementHandle is already disposed.");
			}

			status = OciCalls.OCIStmtExecute (Service,
				Handle,
				ErrorHandle,
				nonQuery,
				0,
				IntPtr.Zero,
				IntPtr.Zero,
				(OciExecuteMode)executeMode);

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
			case OciGlue.OCI_INVALID_HANDLE:
				throw new OracleException (0, "Invalid handle.");
			default:
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			return true;
		}
		
		internal void SetupRefCursorResult (OracleConnection connection) 
		{
			GetColumnCount ();
			Define (connection);
			moreResults = true;
		}

		void GetColumnCount ()
		{
			columnCount = GetAttributeInt32 (OciAttributeType.ParameterCount, ErrorHandle);
		}

		public OciStatementType GetStatementType ()
		{
			return (OciStatementType) GetAttributeUInt16 (OciAttributeType.StatementType, ErrorHandle);
		}

		public bool Fetch ()
		{
			int status = 0;

			if (this.disposed) 
			{
				throw new InvalidOperationException ("StatementHandle is already disposed.");
			}

			status = OciCalls.OCIStmtFetch (Handle,
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
			case OciGlue.OCI_SUCCESS_WITH_INFO:
				//OciErrorInfo ei = ErrorHandle.HandleError ();
				//command.Connection.CreateInfoMessage (ei);
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

			if (this.disposed) {
				throw new InvalidOperationException ("StatementHandle is already disposed.");
			}

			int rsize = 0;
			byte [] buffer;
			
			// Get size of buffer
			OciCalls.OCIUnicodeToCharSet (Parent, null, commandText, out rsize);
			
			// Fill buffer
			buffer = new byte[rsize];
			OciCalls.OCIUnicodeToCharSet (Parent, buffer, commandText, out rsize);

			// Execute statement
			status = OciCalls.OCIStmtPrepare (this,
				ErrorHandle,
				buffer,
				buffer.Length,
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
