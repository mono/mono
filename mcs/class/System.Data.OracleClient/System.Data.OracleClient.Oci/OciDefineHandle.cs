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
	internal sealed class OciDefineHandle : OciHandle, IDisposable
	{
		#region Fields

		bool disposed = false;

		IntPtr handle;
		IntPtr value;
		short indicator;
		OracleType type;
		OciDataType ociType;
		OciDataType definedType;
		int definedSize;
		int rlenp;
		short scale;

		OciErrorHandle errorHandle;

		OciLobLocator lobLocator;
		byte[] date;
	
		#endregion // Fields

		#region Constructors

		public OciDefineHandle (OciHandle parent, IntPtr newHandle)
			: base (OciHandleType.Define, parent, newHandle)
		{
		}

		public void DefineByPosition (int position)
		{
			OciParameterDescriptor parameter = ((OciStatementHandle) Parent).GetParameter (position);
			definedSize = parameter.GetDataSize ();
			scale = parameter.GetScale ();
			definedType = parameter.GetDataType ();
			Define (position);
			parameter.Dispose ();
		}

		#endregion // Constructors

		#region Properties

		public OciDataType DataType {
			get { return definedType; }
		}

		public int DefinedSize {
			get { return definedSize; }
		}

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		public bool IsNull {
			get { return (indicator == -1); }
		}

		public short Scale {
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

		void Define (int position)
		{
			switch (definedType) {
			case OciDataType.Date:
				definedSize = 7;
				DefineDate (position); 
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

		void DefineDate (int position)
		{
			ociType = OciDataType.Date;
			value = Marshal.AllocHGlobal (definedSize);

			int status = 0;

			status = OciCalls.OCIDefineByPos (Parent,
						out handle,
						ErrorHandle,
						position + 1,
						value,
						definedSize,
						ociType,
						ref indicator,
						ref rlenp,
						IntPtr.Zero,
						0);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineChar (int position)
		{
			ociType = OciDataType.Char;
			
			// The buffer is able to contain twice the defined size
			// to allow usage of multibyte characters
			value = Marshal.AllocHGlobal (definedSize * 2);

			int status = 0;

			status = OciCalls.OCIDefineByPos (Parent,
						out handle,
						ErrorHandle,
						position + 1,
						value,
						definedSize * 2,
						ociType,
						ref indicator,
						ref rlenp,
						IntPtr.Zero,
						0);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineLob (int position, OciDataType type)
		{
			ociType = type;
			int status = 0;

			definedSize = -1;

			lobLocator = (OciLobLocator) Parent.Parent.Allocate (OciHandleType.LobLocator);

			if (lobLocator == null) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			value = lobLocator.Handle;
			lobLocator.ErrorHandle = ErrorHandle;
			lobLocator.Service = ((OciStatementHandle) Parent).Service;

			status = OciCalls.OCIDefineByPosPtr (Parent,
							out handle,
							ErrorHandle,
							position + 1,
							ref value,
							definedSize,
							ociType,
							ref indicator,
							ref rlenp,
							IntPtr.Zero,
							0);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		protected override void Dispose (bool disposing) 
		{
			if (!disposed) {
				try {
					Marshal.FreeHGlobal (value);
					disposed = true;
				} finally {
					base.Dispose (disposing);
				}
			}
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
				byte [] buffer = new byte [Size];
				Marshal.Copy (Value, buffer, 0, Size);

				return Encoding.UTF8.GetString (buffer);

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
				return UnpackDate ();
			}

			return DBNull.Value;
		}

		[MonoTODO ("Be able to handle negative dates... i.e. BCE.")]
		public DateTime UnpackDate ()
		{
			byte century = Marshal.ReadByte (value, 0);
			byte year = Marshal.ReadByte (value, 1);
			byte month = Marshal.ReadByte (value, 2);
			byte day = Marshal.ReadByte (value, 3);
			byte hour = Marshal.ReadByte (value, 4);
			byte minute = Marshal.ReadByte (value, 5);
			byte second = Marshal.ReadByte (value, 6);

			return new DateTime ((century - 100) * 100 + (year - 100),
						month,
						day,
						hour - 1,
						minute - 1,
						second - 1);

		}

		#endregion // Methods
	}
}
