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
// Authors: 
//     Tim Coleman <tim@timcoleman.com>
//     Daniel Morgan <danielmorgan@verizon.net>
//         
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2004
// 

using System;
using System.Data.OracleClient;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.OracleClient.Oci 
{
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
		short rlenp = 0;
		short precision;
		short scale;
		Type fieldType;
		string name;

		// Oracle defines the LONG VARCHAR have a size of 2 to the 31 power - 5
		// maybe this should settable via a config file for System.Data.OracleClient.dll
		// see DefineLong
		internal static int LongVarCharMaxValue = (int) Int16.MaxValue - 5;

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
			
			name = parameter.GetName ();
			definedType = parameter.GetDataType ();
			definedSize = parameter.GetDataSize ();
			precision = parameter.GetPrecision ();
			scale = parameter.GetScale ();

			Define (position);

			parameter.Dispose ();
		}

		#endregion // Constructors

		#region Properties

		public OciDataType DataType {
			get { return definedType; }
		}

		public Type FieldType {
			get { return fieldType; }
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

		public short Size {
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
				DefineDate (position); 
				return;
			case OciDataType.Clob:
			case OciDataType.Blob:
				DefineLob (position, definedType);
				return;
			case OciDataType.Raw:
				DefineRaw( position);
				return;
			case OciDataType.RowIdDescriptor:
				definedSize = 10;
				DefineChar (position);
				return;
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
				DefineNumber (position);
				return;
			case OciDataType.Long:
			case OciDataType.LongVarChar:
				DefineLong (position);
				return;
			default:
				DefineChar (position); // HANDLE ALL OTHERS AS CHAR FOR NOW
				return;
			}
		}

		void DefineDate (int position)
		{
			definedSize = 7;
			ociType = OciDataType.Date;
			fieldType = typeof(System.DateTime);

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

		void DefineLong (int position) 
		{
			fieldType = typeof (System.String);

			// LONG VARCHAR max length is 2 to the 31 power - 5
			// the first 4 bytes of a LONG VARCHAR value contains the length
			// Int32.MaxValue - 5 causes out of memory in mono on win32
			// because I do not have 2GB of memory available
			// so Int16.MaxValue - 5 is used instead.
			// LAMESPEC for Oracle OCI - you can not get the length of the LONG VARCHAR value
			// until after you get the value.  This could be why Oracle deprecated LONG VARCHAR.
			// If you specify a definedSize less then the length of the column value,
			// then you will get an OCI_ERROR ORA-01406: fetched column value was truncated
			definedSize = LongVarCharMaxValue;
			
			value = Marshal.AllocHGlobal (definedSize);
			ociType = OciDataType.LongVarChar;

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
				IntPtr.Zero, 0);

			rlenp = (short) definedSize;

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineChar (int position)
		{
			fieldType = typeof (System.String);

			// The buffer is able to contain twice the defined size
			// to allow usage of multibyte characters
			value = Marshal.AllocHGlobal (definedSize * 2);

			ociType = OciDataType.Char;

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

		void DefineNumber (int position) 
		{
			fieldType = typeof (System.Decimal);
			value = Marshal.AllocHGlobal (definedSize);

			ociType = OciDataType.Char;

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

			if (ociType == OciDataType.Clob)
				fieldType = typeof(System.String);
			else if (ociType == OciDataType.Blob)
				fieldType = Type.GetType("System.Byte[]");

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

			definedSize = Int32.MaxValue;

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineRaw (int position)
		{
			ociType = OciDataType.Raw;
			fieldType = Type.GetType("System.Byte[]");

			value = Marshal.AllocHGlobal (definedSize);

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
							IntPtr.Zero, 0);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		protected override void Dispose (bool disposing) 
		{
			if (!disposed) {
				try {
					switch (definedType) {
					case OciDataType.Clob:
					case OciDataType.Blob:
						break;
					default:
						Marshal.FreeHGlobal (value);
						break;
					}
					disposed = true;
				} finally {
					base.Dispose (disposing);
					value = IntPtr.Zero;
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

			byte [] buffer = null;

			switch (DataType) {
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
			case OciDataType.RowIdDescriptor:
				buffer = new byte [Size];
				Marshal.Copy (Value, buffer, 0, Size);
				
				// Get length of returned string
				int 	rsize = 0;
				IntPtr	env = Parent.Parent;	// Parent is statement, grandparent is environment
				OciCalls.OCICharSetToUnicode (env, null, buffer, out rsize);
			
				// Get string
				StringBuilder ret = new StringBuilder(rsize);
				OciCalls.OCICharSetToUnicode (env, ret, buffer, out rsize);
	
				return ret.ToString ();
			case OciDataType.LongVarChar:
			case OciDataType.Long:
				buffer = new byte [LongVarCharMaxValue];
				Marshal.Copy (Value, buffer, 0, buffer.Length);
				
				int longSize = 0;
				if (BitConverter.IsLittleEndian)
					longSize = BitConverter.ToInt32 (new byte[]{buffer[0], buffer[1], buffer[2], buffer[3]}, 0);
				else
					longSize = BitConverter.ToInt32 (new byte[]{buffer[3], buffer[2], buffer[1], buffer[0]}, 0);

				ASCIIEncoding encoding = new ASCIIEncoding ();
				string e = encoding.GetString (buffer, 4, longSize);
				return e;
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
				tmp = Marshal.PtrToStringAnsi (Value, Size);
				if (tmp != null)
					return Decimal.Parse (String.Copy ((string) tmp));
				break;
			case OciDataType.Date:
				return UnpackDate ();
			case OciDataType.Raw:
				byte [] raw_buffer = new byte [Size];
				Marshal.Copy (Value, raw_buffer, 0, Size);
				return raw_buffer;
			case OciDataType.Blob:
			case OciDataType.Clob:
				return GetOracleLob ();
			}

			return DBNull.Value;
		}

		internal object GetOracleValue () 
		{
			object ovalue = GetValue ();

			switch (DataType) {
			case OciDataType.Raw:
				return new OracleBinary ((byte[]) ovalue);
			case OciDataType.Date:
				return new OracleDateTime ((DateTime) ovalue);
			case OciDataType.Blob:
			case OciDataType.Clob:
				OracleLob lob = (OracleLob) ovalue;
				return lob;
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
				return new OracleNumber ((decimal) ovalue);
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
			case OciDataType.LongVarChar:
			case OciDataType.Long:
			case OciDataType.RowIdDescriptor:
				return new OracleString ((string) ovalue);
			default:
				// TODO: do other types
				throw new NotImplementedException ();
			}
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
