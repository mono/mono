// 
// OciDefineHandle.cs 
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
using System.Data.OracleClient;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciDefineHandle : IOciHandle, IDisposable
	{
		#region Fields

		IntPtr handle;
		IntPtr value;
		short indicator;
		OracleType type;
		OciDataType ociType;
		OciDataType definedType;
		int definedSize;
		int rlenp;
		sbyte scale;

		OciStatementHandle statement;
		OciLobLocator lobLocator;
	
		#endregion // Fields

		#region Constructors

		public OciDefineHandle (OciStatementHandle statement, int position)
		{
			int ociTypeInt;
			int status = 0;

			this.statement = statement;

			IntPtr parameterHandle = statement.CreateParameterHandle (position);

			status = OciGlue.OCIAttrGetInt32 (parameterHandle,
							(uint) OciDescriptorType.Parameter,
							out definedSize,
							IntPtr.Zero,
							OciAttributeType.DataSize,
							statement.ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			status = OciGlue.OCIAttrGetSByte (parameterHandle,
							(uint) OciDescriptorType.Parameter,
							out scale,
							IntPtr.Zero,
							OciAttributeType.DataSize,
							statement.ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			status = OciGlue.OCIAttrGetInt32 (parameterHandle,
							(uint) OciDescriptorType.Parameter,
							out ociTypeInt,
							IntPtr.Zero,
							OciAttributeType.DataType,
							statement.ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			definedType = (OciDataType) ociTypeInt;

			Define (position);

			statement.FreeParameterHandle (parameterHandle);
		}

		#endregion // Constructors

		#region Properties

		public OciDataType DataType {
			get { return definedType; }
		}

		public int DefinedSize {
			get { return definedSize; }
		}

		public IntPtr Handle {
			get { return handle; }
			set { handle = value; }
		}

		public OciHandleType HandleType {
			get { return OciHandleType.Define; }
		}

		public bool IsNull {
			get { return (indicator == -1); }
		}

		public sbyte Scale {
			get { return scale; }
		}

		public int Size {
			get { return rlenp; }
		}

		public IntPtr Value {
			get { return value; }
		}

		#endregion

		#region Methods

		[DllImport ("oci")]
		public static extern int OCIDefineByPos (IntPtr stmtp,
							out IntPtr defnpp,
							IntPtr errhp,
							[MarshalAs (UnmanagedType.U4)] int position,
							IntPtr valuep,
							int value_sz,
							[MarshalAs (UnmanagedType.U2)] OciDataType dty,
							ref short indp,
							ref int rlenp,
							IntPtr rcodep,
							uint mode);

		[DllImport ("oci", EntryPoint="OCIDefineByPos")]
		public static extern int OCIDefineByPosPtr (IntPtr stmtp,
							out IntPtr defnpp,
							IntPtr errhp,
							[MarshalAs (UnmanagedType.U4)] int position,
							ref IntPtr valuep,
							int value_sz,
							[MarshalAs (UnmanagedType.U2)] OciDataType dty,
							ref short indp,
							ref int rlenp,
							IntPtr rcodep,
							uint mode);

		void Define (int position)
		{
			switch (definedType) {
			case OciDataType.Date:
				definedSize = 20;
				DefineChar (position); // HANDLE AS CHAR FOR NOW
				return;
			case OciDataType.Clob:
			case OciDataType.Blob:
				definedSize = -1;
				DefineLob (position, definedType);
				return;
			default:
				DefineChar (position); // HANDLE ALL OTHERS AS CHAR FOR NOW
				return;
			}
		}

		void DefineChar (int position)
		{
			ociType = OciDataType.Char;
			value = Marshal.AllocHGlobal (definedSize);

			int status = 0;

			status = OCIDefineByPos (statement.Handle,
							out handle,
							statement.ErrorHandle.Handle,
							position,
							value,
							definedSize,
							ociType,
							ref indicator,
							ref rlenp,
							IntPtr.Zero,
							0);

			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineLob (int position, OciDataType type)
		{
			ociType = type;
			int status = 0;

			definedSize = -1;

			lobLocator = (OciLobLocator) statement.Environment.AllocateDescriptor (OciDescriptorType.LobLocator);
			if (lobLocator == null) {
				OciErrorInfo info = statement.Environment.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
			value = lobLocator.Handle;
			lobLocator.ErrorHandle = statement.ErrorHandle;
			lobLocator.Service = statement.Service;

			status = OCIDefineByPosPtr (statement.Handle,
							out handle,
							statement.ErrorHandle.Handle,
							position,
							ref value,
							definedSize,
							ociType,
							ref indicator,
							ref rlenp,
							IntPtr.Zero,
							0);

			if (status != 0) {
				OciErrorInfo info = statement.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public void Dispose ()
		{
			Marshal.FreeHGlobal (value);
		}

		public OracleLob GetOracleLob ()
		{
			return new OracleLob (lobLocator, ociType);
		}

		public object GetValue ()
		{
			object tmp;

			switch (DataType) {
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
				tmp = Marshal.PtrToStringAnsi (Value, Size);
				if (tmp != null)
					return String.Copy ((string) tmp);
				break;
			case OciDataType.Integer:
				tmp = Marshal.PtrToStringAnsi (Value, Size);
				if (tmp != null)
					return Int32.Parse (String.Copy ((string) tmp));
				break;
			case OciDataType.Number:
				tmp = Marshal.PtrToStringAnsi (Value, Size);
				if (tmp != null) {
					if (Scale == 0)
						return Int32.Parse (String.Copy ((string) tmp));
					else
						return Decimal.Parse (String.Copy ((string) tmp));
				}
				break;
			case OciDataType.Float:
				tmp = Marshal.PtrToStringAnsi (Value, Size);
				if (tmp != null)
					return Double.Parse (String.Copy ((string) tmp));
				break;
			case OciDataType.Date:
				tmp = Marshal.PtrToStringAnsi (Value, Size);
				if (tmp != null)
					return DateTime.Parse ((string) tmp);
				break;
			}

			return DBNull.Value;
		}

		#endregion // Methods
	}
}
