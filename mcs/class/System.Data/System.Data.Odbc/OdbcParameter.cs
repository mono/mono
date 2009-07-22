//
// System.Data.Odbc.OdbcParameter
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//   Sureshkumar T <tsureshkumar@novell.com>  2004.
//
// Copyright (C) Brian Ritchie, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Globalization;
using System.ComponentModel;

namespace System.Data.Odbc
{
#if NET_2_0
	[TypeConverterAttribute ("System.Data.Odbc.OdbcParameter+OdbcParameterConverter, " + Consts.AssemblySystem_Data)]
#else
	[TypeConverterAttribute (typeof (OdbcParameterConverter))]
#endif
	public sealed class OdbcParameter :
#if NET_2_0
	DbParameter,
#else
	MarshalByRefObject,
#endif // NET_2_0
	ICloneable, IDbDataParameter, IDataParameter
	{
		#region Fields

		string name;
		ParameterDirection direction;
		bool isNullable;
		int size;
		DataRowVersion sourceVersion;
		string sourceColumn;
		byte _precision;
		byte _scale;
		object _value;

		private OdbcTypeMap _typeMap;
		private NativeBuffer _nativeBuffer = new NativeBuffer ();
		private NativeBuffer _cbLengthInd;
		private OdbcParameterCollection container;
		
		#endregion

		#region Constructors
		
		public OdbcParameter ()
		{
			_cbLengthInd = new NativeBuffer ();
			ParameterName = String.Empty;
			IsNullable = false;
			SourceColumn = String.Empty;
			Direction = ParameterDirection.Input;
			_typeMap = OdbcTypeConverter.GetTypeMap (OdbcType.NVarChar);
		}

		public OdbcParameter (string name, object value)
			: this ()
		{
			this.ParameterName = name;
			Value = value;
			//FIXME: MS.net does not infer OdbcType from value unless a type is provided
			_typeMap = OdbcTypeConverter.InferFromValue (value);
			if (value != null && !value.GetType ().IsValueType) {
				Type type = value.GetType ();
				if (type.IsArray)
					Size = type.GetElementType () == typeof (byte) ?
						((Array) value).Length : 0;
				else
					Size = value.ToString ().Length;
			}
		}

		public OdbcParameter (string name, OdbcType type)
			: this ()
		{
			this.ParameterName = name;
			_typeMap = (OdbcTypeMap) OdbcTypeConverter.GetTypeMap (type);
		}

		public OdbcParameter (string name, OdbcType type, int size)
			: this (name, type)
		{
			this.Size = size;
		}

		public OdbcParameter (string name, OdbcType type, int size, string sourcecolumn)
			: this (name, type, size)
		{
			this.SourceColumn = sourcecolumn;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public OdbcParameter (string parameterName, OdbcType odbcType, int size,
				     ParameterDirection parameterDirection, bool isNullable, 
				     byte precision, byte scale, string srcColumn, 
				     DataRowVersion srcVersion, object value)
			: this (parameterName, odbcType, size, srcColumn)
		{
			this.Direction = parameterDirection;
			this.IsNullable = isNullable;
			this.SourceVersion = srcVersion;
		}

		#endregion

		#region Properties

		// Used to ensure that only one collection can contain this
		// parameter
		internal OdbcParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

#if ONLY_1_1
		[BrowsableAttribute (false)]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
#endif
		[OdbcCategory ("Data")]
		[OdbcDescriptionAttribute ("The parameter generic type")]
		public
#if NET_2_0
		override
#endif
		DbType DbType {
			get { return _typeMap.DbType; }
			set { 
				if (value == _typeMap.DbType)
					return;
				
				_typeMap = OdbcTypeConverter.GetTypeMap (value);
			}
		}

		[OdbcCategory ("Data")]
		[OdbcDescriptionAttribute ("Input, output, or bidirectional parameter")]  
#if NET_2_0
		[RefreshPropertiesAttribute (RefreshProperties.All)]
#else
		[DefaultValue (ParameterDirection.Input)]
#endif
		public
#if NET_2_0
		override
#endif
		ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

#if ONLY_1_1
		[BrowsableAttribute (false)]
		[DesignOnlyAttribute (true)]
		[EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
		[DefaultValue (false)]
#endif
		[OdbcDescriptionAttribute ("A design-time property used for strongly typed code generation")]
		public
#if NET_2_0
		override
#endif
		bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

		[DefaultValue (OdbcType.NChar)]
		[OdbcDescriptionAttribute ("The parameter native type")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		[OdbcCategory ("Data")]
#if NET_2_0
		[DbProviderSpecificTypeProperty (true)]
#endif
		public OdbcType OdbcType {
			get { return _typeMap.OdbcType; }
			set {
				if (value == _typeMap.OdbcType)
					return;

				_typeMap = OdbcTypeConverter.GetTypeMap (value);
			}
		}

		[OdbcDescription ("DataParameter_ParameterName")]
#if ONLY_1_1
		[DefaultValue ("")]
#endif
		public 
#if NET_2_0
		override
#endif
		string ParameterName {
			get { return name; }
			set { name = value; }
		}

		[OdbcDescription ("DbDataParameter_Precision")]
		[OdbcCategory ("DataCategory_Data")]
		[DefaultValue (0)]
		public byte Precision {
			get { return _precision; }
			set { _precision = value; }
		}

		[OdbcDescription ("DbDataParameter_Scale")]
		[OdbcCategory ("DataCategory_Data")]
		[DefaultValue (0)]
		public byte Scale {
			get { return _scale; }
			set { _scale = value; }
		}
		
		[OdbcDescription ("DbDataParameter_Size")]
		[OdbcCategory ("DataCategory_Data")]
#if ONLY_1_1
		[DefaultValue (0)]
#endif
		public
#if NET_2_0
		override
#endif
		int Size {
			get { return size; }
			set { size = value; }
		}

		[OdbcDescription ("DataParameter_SourceColumn")]
		[OdbcCategory ("DataCategory_Data")]
#if ONLY_1_1
		[DefaultValue ("")]
#endif
		public
#if NET_2_0
		override
#endif
		string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}
		
		[OdbcDescription ("DataParameter_SourceVersion")]
		[OdbcCategory ("DataCategory_Data")]
#if ONLY_1_1
		[DefaultValue ("Current")]
#endif
		public
#if NET_2_0
		override
#endif
		DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

		[TypeConverter (typeof(StringConverter))]
		[OdbcDescription ("DataParameter_Value")]
		[OdbcCategory ("DataCategory_Data")]
#if ONLY_1_1
		[DefaultValue (null)]
#else
		[RefreshPropertiesAttribute (RefreshProperties.All)]
#endif
		public
#if NET_2_0
		override
#endif
		object Value {
			get { return _value; }
			set { _value = value; }
		}

		#endregion // Properties

		#region Methods

		internal void Bind (OdbcCommand command, IntPtr hstmt, int ParamNum)
		{
			OdbcReturn ret;
			int len;
			
			// Convert System.Data.ParameterDirection into odbc enum
			OdbcInputOutputDirection paramdir = libodbc.ConvertParameterDirection (this.Direction);

			_cbLengthInd.EnsureAlloc (Marshal.SizeOf (typeof (int)));
			if (Value is DBNull)
				len = (int)OdbcLengthIndicator.NullData;
			else {
				len = GetNativeSize ();
				AllocateBuffer ();
			}
			
			Marshal.WriteInt32 (_cbLengthInd, len);
			ret = libodbc.SQLBindParameter (hstmt, (ushort) ParamNum, (short) paramdir,
				_typeMap.NativeType, _typeMap.SqlType, Convert.ToUInt32 (Size),
				0, (IntPtr) _nativeBuffer, 0, _cbLengthInd);

			// Check for error condition
			if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
				throw command.Connection.CreateOdbcException (OdbcHandleType.Stmt, hstmt);
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return ParameterName;
		}
		
		private int GetNativeSize ()
		{
			TextInfo ti = CultureInfo.InvariantCulture.TextInfo;
			Encoding enc = Encoding.GetEncoding (ti.ANSICodePage);

			switch (_typeMap.OdbcType) {
			case OdbcType.Binary:
				if (Value.GetType ().IsArray &&
				    Value.GetType ().GetElementType () == typeof (byte))
					return ( (Array) Value).Length;
				else
					return Value.ToString ().Length;
			case OdbcType.Bit:
				return Marshal.SizeOf (typeof (byte));
			case OdbcType.Double:
				return Marshal.SizeOf (typeof (double));
			case OdbcType.Real:
				return Marshal.SizeOf (typeof (float));
			case OdbcType.Int:
				return Marshal.SizeOf (typeof (int));
			case OdbcType.BigInt:
				return Marshal.SizeOf (typeof (long));
			case OdbcType.Decimal:
			case OdbcType.Numeric:
				return 19;
			case OdbcType.SmallInt:
				return Marshal.SizeOf (typeof (Int16));
			case OdbcType.TinyInt:
				return Marshal.SizeOf (typeof (byte));
			case OdbcType.Char:
			case OdbcType.Text:
			case OdbcType.VarChar:
				return enc.GetByteCount (Convert.ToString (Value)) + 1;
			case OdbcType.NChar:
			case OdbcType.NText:
			case OdbcType.NVarChar:
				// FIXME: Change to unicode
				return enc.GetByteCount (Convert.ToString (Value)) + 1;
			case OdbcType.VarBinary:
			case OdbcType.Image:
				if (Value.GetType ().IsArray &&
				    Value.GetType ().GetElementType () == typeof (byte))
					return ((Array) Value).Length;
				throw new ArgumentException ("Unsupported Native Type!");
			case OdbcType.Date:
			case OdbcType.DateTime:
			case OdbcType.SmallDateTime:
			case OdbcType.Time:
			case OdbcType.Timestamp:
				return 18;
			case OdbcType.UniqueIdentifier:
				return Marshal.SizeOf (typeof (Guid));
			}

			if (Value.GetType ().IsArray &&
			    Value.GetType ().GetElementType () == typeof (byte))
				return ((Array) Value).Length;
			
			return Value.ToString ().Length;
		}

		private void AllocateBuffer ()
		{
			int size = GetNativeSize ();

			if (_nativeBuffer.Size == size)
				return;

			_nativeBuffer.AllocBuffer (size);
		}

		internal void CopyValue ()
		{
			if (_nativeBuffer.Handle == IntPtr.Zero)
				return;

			if (Value is DBNull)
				return;
			
			DateTime dt;
			TextInfo ti = CultureInfo.InvariantCulture.TextInfo;
			Encoding enc = Encoding.GetEncoding (ti.ANSICodePage);
			byte [] nativeBytes, buffer;

			switch (_typeMap.OdbcType) {
			case OdbcType.Bit:
				Marshal.WriteByte (_nativeBuffer, Convert.ToByte (Value));
				return;
			case OdbcType.Double:
				Marshal.StructureToPtr (Convert.ToDouble (Value), _nativeBuffer, false);
				return;
			case OdbcType.Real:
				Marshal.StructureToPtr (Convert.ToSingle (Value), _nativeBuffer, false);
				return;
			case OdbcType.Int:
				Marshal.WriteInt32 (_nativeBuffer, Convert.ToInt32 (Value));
				return;
			case OdbcType.BigInt:
				Marshal.WriteInt64 (_nativeBuffer, Convert.ToInt64 (Value));
				return;
			case OdbcType.Decimal:
			case OdbcType.Numeric:
				// for numeric, the buffer is a packed decimal struct.
				// ref http://www.it-faq.pl/mskb/181/254.HTM
				int [] bits = Decimal.GetBits (Convert.ToDecimal (Value));
				buffer = new byte [19]; // ref sqltypes.h
				buffer [0] = Precision;
				buffer [1] = (byte) ((bits [3] & 0x00FF0000) >> 16); // scale
				buffer [2] = (byte) ((bits [3] & 0x80000000) > 0 ? 2 : 1); //sign
				Buffer.BlockCopy (bits, 0, buffer, 3, 12); // copy data
				for (int j = 16; j < 19; j++) // pad with 0
					buffer [j] = 0;
				Marshal.Copy (buffer, 0, _nativeBuffer, 19); 
				return; 
			case OdbcType.SmallInt:
				Marshal.WriteInt16 (_nativeBuffer, Convert.ToInt16 (Value));
				return;
			case OdbcType.TinyInt:
				Marshal.WriteByte (_nativeBuffer, Convert.ToByte (Value));
				return;
			case OdbcType.Char:
			case OdbcType.Text:
			case OdbcType.VarChar:
				buffer = new byte [GetNativeSize ()];
				nativeBytes = enc.GetBytes (Convert.ToString (Value));
				Array.Copy (nativeBytes, 0, buffer, 0, nativeBytes.Length);
				buffer [buffer.Length-1] = (byte) 0;
				Marshal.Copy (buffer, 0, _nativeBuffer, buffer.Length);
				Marshal.WriteInt32 (_cbLengthInd, -3);
				return;
			case OdbcType.NChar:
			case OdbcType.NText:
			case OdbcType.NVarChar:
				// FIXME : change to unicode
				buffer = new byte [GetNativeSize ()];
				nativeBytes = enc.GetBytes (Convert.ToString (Value));
				Array.Copy (nativeBytes, 0, buffer, 0, nativeBytes.Length);
				buffer [buffer.Length-1] = (byte) 0;
				Marshal.Copy (buffer, 0, _nativeBuffer, buffer.Length);
				Marshal.WriteInt32 (_cbLengthInd, -3);
				return;
			case OdbcType.VarBinary:
			case OdbcType.Image:
			case OdbcType.Binary:
				if (Value.GetType ().IsArray &&
				    Value.GetType ().GetElementType () == typeof (byte)) {
					Marshal.Copy ( (byte []) Value, 0, _nativeBuffer, ((byte []) Value).Length);
				}else
					throw new ArgumentException ("Unsupported Native Type!");
				return;
			case OdbcType.Date:
				dt = (DateTime) Value;
				Marshal.WriteInt16 (_nativeBuffer, 0, (short) dt.Year);
				Marshal.WriteInt16 (_nativeBuffer, 2, (short) dt.Month);
				Marshal.WriteInt16 (_nativeBuffer, 4, (short) dt.Day);
				return;
			case OdbcType.Time:
				dt = (DateTime) Value;
				Marshal.WriteInt16 (_nativeBuffer, 0, (short) dt.Hour);
				Marshal.WriteInt16 (_nativeBuffer, 2, (short) dt.Minute);
				Marshal.WriteInt16 (_nativeBuffer, 4, (short) dt.Second);
				return;
			case OdbcType.SmallDateTime:
			case OdbcType.Timestamp:
			case OdbcType.DateTime:
				dt = (DateTime) Value;
				Marshal.WriteInt16 (_nativeBuffer, 0, (short) dt.Year);
				Marshal.WriteInt16 (_nativeBuffer, 2, (short) dt.Month);
				Marshal.WriteInt16 (_nativeBuffer, 4, (short) dt.Day);
				Marshal.WriteInt16 (_nativeBuffer, 6, (short) dt.Hour);
				Marshal.WriteInt16 (_nativeBuffer, 8, (short) dt.Minute);
				Marshal.WriteInt16 (_nativeBuffer, 10, (short) dt.Second);
				Marshal.WriteInt32 (_nativeBuffer, 12, (int) (dt.Ticks % 10000000) * 100);
				return;
			case OdbcType.UniqueIdentifier:
				throw new NotImplementedException ();
			}

			if (Value.GetType ().IsArray &&
			    Value.GetType ().GetElementType () == typeof (byte)) {
				Marshal.Copy ( (byte []) Value, 0, _nativeBuffer, ((byte []) Value).Length);
			}else
				throw new ArgumentException ("Unsupported Native Type!");
		}

#if NET_2_0
		public override bool SourceColumnNullMapping {
			get { return false; }
			set { }
		}

		public override void ResetDbType ()
		{
			_typeMap = OdbcTypeConverter.GetTypeMap (OdbcType.NVarChar);
		}

		public void ResetOdbcType ()
		{
			_typeMap = OdbcTypeConverter.GetTypeMap (OdbcType.NVarChar);
		}
#endif

		#endregion
	}
}
