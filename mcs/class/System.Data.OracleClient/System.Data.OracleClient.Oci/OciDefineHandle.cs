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
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2004, 2009
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

		//IntPtr handle;
		IntPtr value;
		short indicator;
		//OracleType type;
		OciDataType ociType;
		OciDataType definedType;
		int definedSize;
		short rlenp = 0;
		//short precision;
		short scale;
		Type fieldType;
		//string name;

		// Oracle defines the LONG VARCHAR and LONG VARRAW to have a size of 2 to the 31 power - 5
		// see DefineLongVarChar and DefineLongVarRaw
		// TODO: see OCI Programmers Guide on how to do a piece-wise operations
		//       instead of using the below.  Or better yet, convert
		//       your LONG/LONG VARCHAR to CLOB and LONG RAW/LONG VARRAW to BLOB.
		internal static int LongVarCharMaxValue = (int) Int16.MaxValue - 5;
		internal static int LongVarRawMaxValue = (int) Int16.MaxValue - 5;
		
		OciErrorHandle errorHandle;

		OciLobLocator lobLocator;
		OciDateTimeDescriptor dateTimeDesc;
		OciIntervalDescriptor intervalDesc;

		#endregion // Fields

		#region Constructors

		internal OciDefineHandle (OciHandle parent, IntPtr newHandle)
			: base (OciHandleType.Define, parent, newHandle)
		{
		}

		internal void DefineByPosition (int position, OracleConnection connection)
		{
			OciParameterDescriptor parameter = ((OciStatementHandle) Parent).GetParameter (position);

			//name = parameter.GetName ();
			definedType = parameter.GetDataType ();
			definedSize = parameter.GetDataSize ();
			//precision = parameter.GetPrecision ();
			scale = parameter.GetScale ();

			Define (position, connection);

			parameter.Dispose ();
		}

		#endregion // Constructors

		#region Properties

		internal OciDataType DataType {
			get { return definedType; }
		}

		internal Type FieldType {
			get { return fieldType; }
		}

		internal int DefinedSize {
			get { return definedSize; }
		}

		internal OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		internal bool IsNull {
			get { return (indicator == -1); }
		}

		internal short Scale {
			get { return scale; }
		}

		internal short Size {
			get { return rlenp; }
		}

		internal IntPtr Value {
			get { return value; }
		}

		#endregion

		#region Methods

		void Define (int position, OracleConnection connection)
		{
			switch (definedType) {
			case OciDataType.Date:
				DefineDate (position, connection);
				return;
			case OciDataType.TimeStamp:
				DefineTimeStamp (position, connection);
				return;
			case OciDataType.Clob:
			case OciDataType.Blob:
				DefineLob (position, definedType, connection);
				return;
			case OciDataType.Raw:
			case OciDataType.VarRaw:
				DefineRaw( position, connection);
				return;
			case OciDataType.LongRaw:
			case OciDataType.LongVarRaw:
				DefineLongVarRaw (position, connection);
				return;
			case OciDataType.RowIdDescriptor:
				definedSize = 10;
				DefineChar (position, connection);
				return;
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
			case OciDataType.VarNum:
			case OciDataType.UnsignedInt:
				DefineNumber (position, connection);
				return;
			case OciDataType.Long:
			case OciDataType.LongVarChar:
				DefineLongVarChar (position, connection);
				return;
			case OciDataType.IntervalDayToSecond:
			case OciDataType.IntervalYearToMonth:
				DefineInterval (position, definedType, connection);
				return;
			default:
				DefineChar (position, connection); // HANDLE ALL OTHERS AS CHAR FOR NOW
				return;
			}
		}

		void DefineTimeStamp (int position, OracleConnection connection)
		{
			definedSize = -1;
			ociType = OciDataType.TimeStamp;
			fieldType = typeof(System.DateTime);

			dateTimeDesc = (OciDateTimeDescriptor) connection.Environment.Allocate (OciHandleType.TimeStamp);
			if (dateTimeDesc == null) {
				OciErrorInfo info = connection.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			value = dateTimeDesc.Handle;
			dateTimeDesc.ErrorHandle = ErrorHandle;

			int status = 0;

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

			definedSize = 11;

			if (status != 0) {
				OciErrorInfo info = connection.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineDate (int position, OracleConnection connection)
		{
			definedSize = 7;
			ociType = OciDataType.Date;
			fieldType = typeof(System.DateTime);

			value = OciCalls.AllocateClear (definedSize);

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

		void DefineLongVarChar (int position, OracleConnection connection)
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
			
			// TODO: get via piece-wise - a chunk at a time
			definedSize = LongVarCharMaxValue;

			value = OciCalls.AllocateClear (definedSize);
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

		void DefineChar (int position, OracleConnection connection)
		{
			fieldType = typeof (System.String);

			int maxByteCount = Encoding.UTF8.GetMaxByteCount (definedSize);
			value = OciCalls.AllocateClear (maxByteCount);

			ociType = OciDataType.Char;

			int status = 0;

			status = OciCalls.OCIDefineByPos (Parent,
						out handle,
						ErrorHandle,
						position + 1,
						value,
						maxByteCount,
						ociType,
						ref indicator,
						ref rlenp,
						IntPtr.Zero,
						0);
			OciErrorHandle.ThrowExceptionIfError (ErrorHandle, status);
		}

		void DefineNumber (int position, OracleConnection connection)
		{
			fieldType = typeof (System.Decimal);
			value = OciCalls.AllocateClear (definedSize);

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

		void DefineLob (int position, OciDataType type, OracleConnection connection)
		{
			ociType = type;

			if (ociType == OciDataType.Clob)
				fieldType = typeof(System.String);
			else if (ociType == OciDataType.Blob)
				fieldType = typeof(byte[]);

			int status = 0;

			definedSize = -1;

			lobLocator = (OciLobLocator) connection.Environment.Allocate (OciHandleType.LobLocator);

			if (lobLocator == null) {
				OciErrorInfo info = connection.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			value = lobLocator.Handle;
			lobLocator.ErrorHandle = connection.ErrorHandle;
			lobLocator.Service = connection.ServiceContext;
			lobLocator.Environment = connection.Environment;

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
				OciErrorInfo info = connection.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineRaw (int position, OracleConnection connection)
		{
			ociType = OciDataType.Raw;
			fieldType = typeof (byte[]);

			value = OciCalls.AllocateClear (definedSize);

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

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineLongVarRaw (int position, OracleConnection connection)
		{
			ociType = OciDataType.LongVarRaw;
			fieldType = typeof (byte[]);

			// TODO: get via piece-wise - a chunk at a time
			definedSize = LongVarRawMaxValue;

			value = OciCalls.AllocateClear (definedSize);

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

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		void DefineInterval (int position, OciDataType type, OracleConnection connection)
		{
			ociType = type;
			fieldType = typeof(string);
			definedSize = -1;
			
			switch (type) {
				case OciDataType.IntervalDayToSecond:
					definedSize = 11;
					intervalDesc = (OciIntervalDescriptor) connection.Environment.Allocate (OciHandleType.IntervalDayToSecond);
					break;
				case OciDataType.IntervalYearToMonth:
					intervalDesc = (OciIntervalDescriptor) connection.Environment.Allocate (OciHandleType.IntervalYearToMonth);
					definedSize = 5;
					break;
			}
			
			if (intervalDesc == null) {
				OciErrorInfo info = connection.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			value = intervalDesc.Handle;
			intervalDesc.ErrorHandle = ErrorHandle;

			int status = 0;

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
				OciErrorInfo info = connection.ErrorHandle.HandleError ();
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
					case OciDataType.TimeStamp:
					case OciDataType.IntervalDayToSecond:
					case OciDataType.IntervalYearToMonth:
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

		internal OracleLob GetOracleLob ()
		{
			return new OracleLob (lobLocator, ociType);
		}

                internal object GetValue (IFormatProvider formatProvider, OracleConnection conn)
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
				//IntPtr	env = Parent.Parent;	// Parent is statement, grandparent is environment
				IntPtr env = conn.Environment;
				int status = OciCalls.OCICharSetToUnicode (env, null, buffer, out rsize);
				OciErrorHandle.ThrowExceptionIfError (ErrorHandle, status);

				// Get string
				StringBuilder ret = new StringBuilder(rsize);
				status = OciCalls.OCICharSetToUnicode (env, ret, buffer, out rsize);
				OciErrorHandle.ThrowExceptionIfError (ErrorHandle, status);

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
			case OciDataType.VarNum:
			case OciDataType.UnsignedInt:
				tmp = Marshal.PtrToStringAnsi (Value, Size);
				if (tmp != null)
					return Decimal.Parse (String.Copy ((string) tmp), formatProvider);
				break;
			case OciDataType.TimeStamp:
				return dateTimeDesc.GetDateTime (conn.Environment, dateTimeDesc.ErrorHandle);
			case OciDataType.Date:
				return UnpackDate ();
			case OciDataType.Raw:
			case OciDataType.VarRaw:
				byte [] raw_buffer = new byte [Size];
				Marshal.Copy (Value, raw_buffer, 0, Size);
				return raw_buffer;
			case OciDataType.LongRaw:
			case OciDataType.LongVarRaw:
				buffer = new byte [LongVarRawMaxValue];
				Marshal.Copy (Value, buffer, 0, buffer.Length);

				int longrawSize = 0;
				if (BitConverter.IsLittleEndian)
					longrawSize = BitConverter.ToInt32 (new byte[]{buffer[0], buffer[1], buffer[2], buffer[3]}, 0);
				else
					longrawSize = BitConverter.ToInt32 (new byte[]{buffer[3], buffer[2], buffer[1], buffer[0]}, 0);

				byte[] longraw_buffer = new byte [longrawSize];
				Array.ConstrainedCopy (buffer, 4, longraw_buffer, 0, longrawSize);
				return longraw_buffer;
			case OciDataType.Blob:
			case OciDataType.Clob:
				return GetOracleLob ();
			case OciDataType.IntervalDayToSecond:
				return new OracleTimeSpan (intervalDesc.GetDayToSecond (conn.Environment, intervalDesc.ErrorHandle));
			case OciDataType.IntervalYearToMonth:
				return new OracleMonthSpan (intervalDesc.GetYearToMonth (conn.Environment, intervalDesc.ErrorHandle));
			default:
				throw new Exception("OciDataType not implemented: " + DataType.ToString ());
			}

			return DBNull.Value;
		}

                internal object GetOracleValue (IFormatProvider formatProvider, OracleConnection conn)
		{
                        object ovalue = GetValue (formatProvider, conn);

			switch (DataType) {
			case OciDataType.Raw:
			case OciDataType.VarRaw:
			case OciDataType.LongRaw:
			case OciDataType.LongVarRaw:
				return new OracleBinary ((byte[]) ovalue);
			case OciDataType.Date:
			case OciDataType.TimeStamp:
				return new OracleDateTime ((DateTime) ovalue);
			case OciDataType.Blob:
			case OciDataType.Clob:
				OracleLob lob = (OracleLob) ovalue;
				return lob;
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
			case OciDataType.VarNum:
			case OciDataType.UnsignedInt:
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
			case OciDataType.IntervalDayToSecond:
				return new OracleTimeSpan ((OracleTimeSpan) ovalue);
			case OciDataType.IntervalYearToMonth:
				return new OracleMonthSpan ((OracleMonthSpan) ovalue);
			default:
				// TODO: do other types
				throw new NotImplementedException ();
			}
		}

		[MonoTODO ("Be able to handle negative dates... i.e. BCE.")]
		internal DateTime UnpackDate ()
		{
			byte century = Marshal.ReadByte (value, 0);
			byte year = Marshal.ReadByte (value, 1);
			byte month = Marshal.ReadByte (value, 2);
			byte day = Marshal.ReadByte (value, 3);
			byte hour = Marshal.ReadByte (value, 4);
			byte minute = Marshal.ReadByte (value, 5);
			byte second = Marshal.ReadByte (value, 6);

			if (hour == 0)
				hour ++;
			if (minute == 0)
				minute ++;
			if (second == 0)
				second ++;

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
