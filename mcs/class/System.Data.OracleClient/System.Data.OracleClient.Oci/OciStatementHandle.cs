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
	internal sealed class OciStatementHandle : OciHandle, IOciHandle, IDisposable
	{
		#region Fields

		OciStatementLanguage language;
		OciStatementMode mode;
		OciServiceHandle serviceHandle;
		OciErrorHandle errorHandle;

		ArrayList values;
		ArrayList parameters;

		bool moreResults;
		int columnCount;
	
		#endregion // Fields

		#region Constructors

		public OciStatementHandle (OciEnvironmentHandle environment, IntPtr handle)
			: base (OciHandleType.Statement, environment, handle)
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
		public static extern int OCIDescriptorFree (IntPtr descp,
							[MarshalAs (UnmanagedType.U4)] OciDescriptorType type);

		[DllImport ("oci")]
		public static extern int OCIParamGet (IntPtr hndlp,
							[MarshalAs (UnmanagedType.U4)] OciHandleType htype,
							IntPtr errhp,
							out IntPtr parmdpp,
							[MarshalAs (UnmanagedType.U4)] int pos);

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

		public IntPtr CreateParameterHandle (int position)
		{
			IntPtr handle = IntPtr.Zero;
			int status = 0;

			status = OCIParamGet (Handle,
						OciHandleType.Statement,
						ErrorHandle.Handle,
						out handle,
						position);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return handle;
		}

		public void FreeBindHandle (IntPtr handle)
		{
			int status = 0;

			//status = OCIDescriptorFree (handle, OciDescriptorType.Parameter);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public void FreeParameterHandle (IntPtr handle)
		{
			int status = 0;

			status = OCIDescriptorFree (handle, OciDescriptorType.Parameter);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void Define ()
		{
			values = new ArrayList ();
			for (int i = 0; i < columnCount; i += 1)  
				values.Add (new OciDefineHandle (this, i + 1));
		}

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
			int status = 0;
			status = OciGlue.OCIAttrGetInt32 ( Handle,
						(uint) OciHandleType.Statement,
						out columnCount,
						IntPtr.Zero,
						OciAttributeType.ParameterCount,
						ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public bool GetAttributeBool (IntPtr handle, OciAttributeType type)
		{
			bool output;
			int status = 0;

			status = OciGlue.OCIAttrGetBool (handle,
						(uint) OciDescriptorType.Parameter,
						out output,
						IntPtr.Zero,
						type,
						ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public sbyte GetAttributeSByte (IntPtr handle, OciAttributeType type) {
			sbyte output;
			int status = 0;

			status = OciGlue.OCIAttrGetSByte (handle,
				(uint) OciDescriptorType.Parameter,
				out output,
				IntPtr.Zero,
				type,
				ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public byte GetAttributeByte (IntPtr handle, OciAttributeType type)
		{
			byte output;
			int status = 0;

			status = OciGlue.OCIAttrGetByte (handle,
						(uint) OciDescriptorType.Parameter,
						out output,
						IntPtr.Zero,
						type,
						ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public ushort GetAttributeUInt16 (IntPtr handle, OciAttributeType type) {
			int status = 0;
			ushort output;

			status = OciGlue.OCIAttrGetUInt16 (handle,
				(uint) OciDescriptorType.Parameter,	
				out output,
				IntPtr.Zero,
				type,
				ErrorHandle.Handle);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public int GetAttributeInt32 (IntPtr handle, OciAttributeType type)
		{
			int status = 0;
			int output;

			status = OciGlue.OCIAttrGetInt32 (handle,
						(uint) OciDescriptorType.Parameter,	
						out output,
						IntPtr.Zero,
						type,
						ErrorHandle.Handle);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		[MonoTODO]
		public string GetAttributeString (IntPtr handle, OciAttributeType type)
		{
			string output = String.Empty;
			IntPtr outputPtr = IntPtr.Zero;
			int outSize;
			int status = 0;

			status = OciGlue.OCIAttrGet (handle,
						(uint) OciDescriptorType.Parameter,
						out outputPtr,
						out outSize,
						type,
						ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (outputPtr != IntPtr.Zero && outSize > 0) {
				object name = Marshal.PtrToStringAnsi (outputPtr, outSize);
				if (name != null) {
					output = String.Copy ((string) name);
					/* TRWC 
					 * We shouldn't really free this, but how can we make Oracle
					 * do it?
					 */
					// Marshal.FreeHGlobal (outputPtr); -- this *may* leak memory
				}
			}

			return output;
		}

		[MonoTODO]
		public OciColumnInfo DescribeColumn (int ordinal)
		{
			int status = 0;
			OciColumnInfo columnInfo;

			string schemaName;	

			IntPtr parameterHandle = CreateParameterHandle (ordinal + 1);

			columnInfo.ColumnName = GetAttributeString (parameterHandle, OciAttributeType.Name);
			columnInfo.ColumnOrdinal = ordinal + 1;
			columnInfo.ColumnSize = GetAttributeUInt16 (parameterHandle, OciAttributeType.DataSize);
			columnInfo.Precision = GetAttributeByte (parameterHandle, OciAttributeType.Precision);
			columnInfo.Scale = GetAttributeSByte (parameterHandle, OciAttributeType.Scale);
			columnInfo.DataType = (OciDataType) GetAttributeInt32 (parameterHandle, OciAttributeType.DataType);
			columnInfo.AllowDBNull = GetAttributeBool (parameterHandle, OciAttributeType.IsNull);
			columnInfo.BaseColumnName = GetAttributeString (parameterHandle, OciAttributeType.Name);

			// TRWC not sure what to do with this yet.
			schemaName = GetAttributeString (parameterHandle, OciAttributeType.SchemaName);

			FreeParameterHandle (parameterHandle);

			return columnInfo;
		}

		public OciStatementType GetStatementType ()
		{
			int status = 0;
			int statementType;

			status = OciGlue.OCIAttrGetInt32 (Handle,
							(uint) OciHandleType.Statement,
							out statementType,
							IntPtr.Zero,
							OciAttributeType.StatementType,
							errorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return (OciStatementType) statementType;
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
