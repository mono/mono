// 
// OracleParameter.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman , 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient {
	public sealed class OracleParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		string name;
		OracleType oracleType = OracleType.VarChar;
		OciDataType ociType;
		int size;
		ParameterDirection direction = ParameterDirection.Input;
		bool isNullable;
		byte precision;
		byte scale;
		string srcColumn;
		DataRowVersion srcVersion;
		DbType dbType = DbType.AnsiString;
		int offset = 0;
		bool sizeSet = false;
		object value = null;

		OracleParameterCollection container = null;
		OciBindHandle bindHandle;

		#endregion // Fields

		#region Constructors

		public OracleParameter ()
			: this (String.Empty, OracleType.VarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, object value)
		{
			this.name = name;
			this.value = value;
			SourceVersion = DataRowVersion.Current;
			InferOracleType (value);
		}

		public OracleParameter (string name, OracleType dataType)
			: this (name, dataType, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size)
			: this (name, dataType, size, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size, string srcColumn)
			: this (name, dataType, size, ParameterDirection.Input, false, 0, 0, srcColumn, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
		{
			this.name = name;
			this.size = size;
			this.value = value;

			OracleType = dataType;
			Direction = direction;
			SourceColumn = srcColumn;
			SourceVersion = srcVersion;
		}

		#endregion // Constructors

		#region Properties

		internal OracleParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

		public DbType DbType {
			get { return dbType; }
			set { SetDbType (value); }
		}

		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		public bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

		public int Offset {
			get { return offset; }
			set { offset = value; }
		}

		public OracleType OracleType {
			get { return oracleType; }
			set { SetOracleType (value); }
		}
		
		public string ParameterName {
			get { return name; }
			set { name = value; }
		}

		public byte Precision {
			get { return precision; }
			set { /* NO EFFECT*/ }
		}

		public byte Scale {
			get { return scale; }
			set { /* NO EFFECT*/ }
		}

		public int Size {
			get { return size; }
			set { 
				sizeSet = true;
				size = value; 
			}
		}

		public string SourceColumn {
			get { return srcColumn; }
			set { srcColumn = value; }
		}

		public DataRowVersion SourceVersion {
			get { return srcVersion; }
			set { srcVersion = value; }
		}

		public object Value {
			get { return this.value; }
			set { this.value = value; }
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
						[MarshalAs (UnmanagedType.U2)] OciDataType dty,
						int indp,
						IntPtr alenp,
						IntPtr rcodep,
						uint maxarr_len,
						IntPtr curelp,
						uint mode);


		private void AssertSizeIsSet ()
		{
			if (!sizeSet)
				throw new Exception ("Size must be set.");
		}

		internal void Bind (OciStatementHandle statement, OracleConnection connection)
		{
			if (bindHandle == null)
				bindHandle = new OciBindHandle ((OciHandle) statement);

			IntPtr tmpHandle = bindHandle.Handle;

			if (Direction != ParameterDirection.Input)
				AssertSizeIsSet ();
			if (!sizeSet)
				size = InferSize ();

			int status = 0;
			int indicator = 0;
			OciDataType bindType = ociType;
			IntPtr bindValue = IntPtr.Zero;
			int bindSize = size;

			if (value == DBNull.Value)
				indicator = -1;
			else {
				bindType = OciDataType.VarChar2; // FIXME
				bindValue = Marshal.StringToHGlobalAnsi (value.ToString ());
				bindSize = value.ToString ().Length;
			}

			status = OCIBindByName (statement,
						out tmpHandle,
						connection.ErrorHandle,
						ParameterName,
						ParameterName.Length,
						bindValue,
						bindSize,
						bindType,
						indicator,
						IntPtr.Zero,
						IntPtr.Zero,
						0,
						IntPtr.Zero,
						0);

			if (status != 0) {
				OciErrorInfo info = connection.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			bindHandle.SetHandle (tmpHandle);
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		private void InferOracleType (object value)
		{
			Type type = value.GetType ();
			string exception = String.Format ("The parameter data type of {0} is invalid.", type.Name);
			switch (type.FullName) {
			case "System.Int64":
				SetOracleType (OracleType.Number);
				break;
			case "System.Boolean":
			case "System.Byte":
				SetOracleType (OracleType.Byte);
				break;
			case "System.String":
				SetOracleType (OracleType.VarChar);
				break;
			case "System.DataType":
				SetOracleType (OracleType.DateTime);
				break;
			case "System.Decimal":
				SetOracleType (OracleType.Number);
				//scale = ((decimal) value).Scale;
				break;
			case "System.Double":
				SetOracleType (OracleType.Double);
				break;
			case "System.Byte[]":
			case "System.Guid":
				SetOracleType (OracleType.Raw);
				break;
			case "System.Int32":
				SetOracleType (OracleType.Int32);
				break;
			case "System.Single":
				SetOracleType (OracleType.Float);
				break;
			case "System.Int16":
				SetOracleType (OracleType.Int16);
				break;
			default:
				throw new ArgumentException (exception);
			}
		}

		[MonoTODO ("different size depending on type.")]
		private int InferSize ()
		{
			return value.ToString ().Length;
		}

		public void SetDbType (DbType type)
		{
			string exception = String.Format ("No mapping exists from DbType {0} to a known OracleType.", type);
			switch (type) {
			case DbType.AnsiString:
				oracleType = OracleType.VarChar;
				ociType = OciDataType.VarChar;
				break;
			case DbType.AnsiStringFixedLength:
				oracleType = OracleType.Char;
				ociType = OciDataType.Char;
				break;
			case DbType.Binary:
			case DbType.Guid:
				oracleType = OracleType.Raw;
				ociType = OciDataType.Raw;
				break;
			case DbType.Boolean:
			case DbType.Byte:
				oracleType = OracleType.Byte;
				ociType = OciDataType.Integer;
				break;
			case DbType.Currency:
			case DbType.Decimal:
			case DbType.Int64:
				oracleType = OracleType.Number;
				ociType = OciDataType.Number;
				break;
			case DbType.Date:
			case DbType.DateTime:
			case DbType.Time:
				oracleType = OracleType.DateTime;
				ociType = OciDataType.Char;
				break;
			case DbType.Double:
				oracleType = OracleType.Double;
				ociType = OciDataType.Float;
				break;
			case DbType.Int16:
				oracleType = OracleType.Int16;
				ociType = OciDataType.Integer;
				break;
			case DbType.Int32:
				oracleType = OracleType.Int32;
				ociType = OciDataType.Integer;
				break;
			case DbType.Object:
				oracleType = OracleType.Blob;
				ociType = OciDataType.Blob;
				break;
			case DbType.Single:
				oracleType = OracleType.Float;
				ociType = OciDataType.Float;
				break;
			case DbType.String:
				oracleType = OracleType.NVarChar;
				ociType = OciDataType.VarChar;
				break;
			case DbType.StringFixedLength:
				oracleType = OracleType.NChar;
				ociType = OciDataType.Char;
				break;
			default:
				throw new ArgumentException (exception);
			}
			dbType = type;

		}

		public void SetOracleType (OracleType type)
		{
			string exception = String.Format ("No mapping exists from OracleType {0} to a known DbType.", type);
			switch (type) {
			case OracleType.BFile:
			case OracleType.Blob:
			case OracleType.LongRaw:
			case OracleType.Raw:
				dbType = DbType.Binary;
				ociType = OciDataType.Raw;
				break;
			case OracleType.Byte:
				dbType = DbType.Byte;
				ociType = OciDataType.Integer;
				break;
			case OracleType.Char:
				dbType = DbType.AnsiStringFixedLength;
				ociType = OciDataType.Char;
				break;
			case OracleType.Clob:
			case OracleType.LongVarChar:
			case OracleType.RowId:
			case OracleType.VarChar:
				dbType = DbType.AnsiString;
				ociType = OciDataType.VarChar;
				break;
			case OracleType.Cursor:
			case OracleType.IntervalDayToSecond:
				dbType = DbType.Object;
				ociType = OciDataType.Blob;
				break;
			case OracleType.DateTime:
			case OracleType.Timestamp:
			case OracleType.TimestampLocal:
			case OracleType.TimestampWithTZ:
				dbType = DbType.DateTime;
				ociType = OciDataType.Char;
				break;
			case OracleType.Double:
				dbType = DbType.Double;
				ociType = OciDataType.Float;
				break;
			case OracleType.Float:
				dbType = DbType.Single;
				ociType = OciDataType.Float;
				break;
			case OracleType.Int16:
				dbType = DbType.Int16;
				ociType = OciDataType.Integer;
				break;
			case OracleType.Int32:
			case OracleType.IntervalYearToMonth:
				dbType = DbType.Int32;
				ociType = OciDataType.Integer;
				break;
			case OracleType.NChar:
				dbType = DbType.StringFixedLength;
				ociType = OciDataType.Char;
				break;
			case OracleType.NClob:
			case OracleType.NVarChar:
				dbType = DbType.String;
				ociType = OciDataType.VarChar;
				break;
			case OracleType.Number:
				dbType = DbType.VarNumeric;
				ociType = OciDataType.Number;
				break;
			case OracleType.SByte:
				dbType = DbType.SByte;
				ociType = OciDataType.Integer;
				break;
			case OracleType.UInt16:
				dbType = DbType.UInt16;
				ociType = OciDataType.Integer;
				break;
			case OracleType.UInt32:
				dbType = DbType.UInt32;
				ociType = OciDataType.Integer;
				break;
			default:
				throw new ArgumentException (exception);
			}

			oracleType = type;
		}

		public override string ToString ()
		{
			return ParameterName;
		}

		#endregion // Methods
	}
}
