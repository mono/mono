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

		OciStatementLanguage language;
		OciStatementMode mode;
		OciServiceHandle serviceHandle;
		OciErrorHandle errorHandle;

		ArrayList values;
		ArrayList parameters;

		bool disposed = false;
		bool moreResults;
		int columnCount;
	
		#endregion // Fields

		#region Constructors

		public OciStatementHandle (OciHandle parent, IntPtr handle)
			: base (OciHandleType.Statement, parent, handle)
		{
			parameters = new ArrayList ();
			language = OciStatementLanguage.NTV;
			mode = OciStatementMode.Default;
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

		public OciStatementLanguage Language {
			get { return language; }
			set { language = value; }
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
		static extern int OCIBindByName (IntPtr stmtp,
						out IntPtr bindpp,
						IntPtr errhp,
						string placeholder,
						int placeh_len,
						IntPtr valuep,
						int value_sz,
						[MarshalAs (UnmanagedType.U4)] OciDataType dty,
						ref int indp,
						IntPtr alenp,
						ushort rcodep,
						uint maxarr_len,
						IntPtr curelp,
						uint mode);

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

			status = OCIParamGet (Handle,
						OciHandleType.Statement,
						ErrorHandle.Handle,
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

		public OciBindHandle GetBindHandle (string name, object val, OciDataType type)
		{
			IntPtr handle = IntPtr.Zero;
			IntPtr value = IntPtr.Zero;
			int indicator = 0;

			string stringValue = val.ToString ();
			int definedSize = 0;
			int status = 0;

			if (val == DBNull.Value)
				indicator = -1;
			else
				switch (type) {
				case OciDataType.Number:
				case OciDataType.Integer:
				case OciDataType.Float:
				case OciDataType.VarNum:
					type = OciDataType.Char;
					definedSize = stringValue.Length;
					value = Marshal.StringToHGlobalAnsi (stringValue);
					break;
				case OciDataType.Date:
					break;
				default:
					type = OciDataType.Char;
					definedSize = stringValue.Length;
					value = Marshal.StringToHGlobalAnsi (stringValue);
					break;
				}

			status = OCIBindByName (Handle,
						out handle,
						ErrorHandle,
						name,
						name.Length,
						value,
						definedSize,
						type,
						ref indicator,
						IntPtr.Zero,
						0,
						0,
						IntPtr.Zero,
						0);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			OciBindHandle output = new OciBindHandle (this, handle);
			output.Value = value;
			return output;
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

			status = OCIStmtExecute (Service.Handle,
						Handle,
						ErrorHandle.Handle,
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
