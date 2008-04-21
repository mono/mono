//
// System.Data.OleDb.OleDbParameter
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
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
using System.Data;
using System.Data.Common;
using System.ComponentModel;

namespace System.Data.OleDb
{
#if NET_2_0
	[TypeConverterAttribute ("System.Data.OleDb.OleDbParameter+OleDbParameterConverter, " + Consts.AssemblySystem_Data)]
	public sealed class OleDbParameter : DbParameter, IDbDataParameter, ICloneable
#else
	[TypeConverterAttribute (typeof (OleDbParameterConverter))]
	public sealed class OleDbParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
#endif
	{
		#region Fields

		string name;
		object value;
		int size;
		bool isNullable;
		byte precision;
		byte scale;
		DataRowVersion sourceVersion;
		string sourceColumn;
#if NET_2_0
		bool sourceColumnNullMapping;
#endif
		ParameterDirection direction;
		OleDbType oleDbType;
		DbType dbType;
		OleDbParameterCollection container;
		IntPtr gdaParameter;

		#endregion

		#region Constructors
		
		public OleDbParameter ()
		{
			name = string.Empty;
			isNullable = true;
			sourceColumn = string.Empty;
			gdaParameter = IntPtr.Zero;
		}

		public OleDbParameter (string name, object value) 
			: this ()
		{
			this.name = name;
			this.value = value;
			OleDbType = GetOleDbType (value);
		}

		public OleDbParameter (string name, OleDbType dataType) 
			: this ()
		{
			this.name = name;
			OleDbType = dataType;
		}

		public OleDbParameter (string name, OleDbType dataType, int size)
			: this (name, dataType)
		{
			this.size = size;
		}

		public OleDbParameter (string name, OleDbType dataType, int size, string srcColumn)
			: this (name, dataType, size)
		{
			this.sourceColumn = srcColumn;
		}
		
		[EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
		public OleDbParameter (string parameterName, OleDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
			: this (parameterName, dbType, size, srcColumn)
		{
			this.direction = direction;
			this.isNullable = isNullable;
			this.precision = precision;
			this.scale = scale;
			this.sourceVersion = srcVersion;
			this.value = value;
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public OleDbParameter (string parameterName, OleDbType dbType, int size, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value)
			: this (parameterName, dbType, size, sourceColumn)
		{
			this.direction = direction;
			this.precision = precision;
			this.scale = scale;
			this.sourceVersion = sourceVersion;
			this.sourceColumnNullMapping = sourceColumnNullMapping;
			this.value = value;
		}
#endif

		#endregion

		#region Properties

#if !NET_2_0
		[BrowsableAttribute (false)]
		[DataSysDescriptionAttribute ("The parameter generic type.")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
#endif
		[DataCategory ("DataCategory_Data")]
		public
#if NET_2_0
		override
#endif
		DbType DbType {
			get { return dbType; }
			set {
				dbType = value;
				oleDbType = DbTypeToOleDbType (value);
			}
		}

#if NET_2_0
		[RefreshProperties (RefreshProperties.All)]
#else
		[DataSysDescriptionAttribute ("Input, output, or bidirectional parameter.")]
		[DefaultValue (ParameterDirection.Input)]
#endif
		[DataCategory ("DataCategory_Data")]
		public
#if NET_2_0
		override
#endif
		ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

#if !NET_2_0
		[BrowsableAttribute (false)]
		[DataSysDescriptionAttribute ("a design-time property used for strongly typed code-generation.")]
		[DesignOnlyAttribute (true)]
		[EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
		[DefaultValue (false)]
#endif
		public
#if NET_2_0
		override
#endif
		bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

#if NET_2_0
		[DbProviderSpecificTypeProperty (true)]
#else
		[DefaultValue (OleDbType.VarWChar)]
		[DataSysDescriptionAttribute ("The parameter native type.")]
#endif
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		[DataCategory ("DataCategory_Data")]
		public OleDbType OleDbType {
			get { return oleDbType; }
			set {
				oleDbType = value;
				dbType = OleDbTypeToDbType (value);
			}
		}

#if !NET_2_0
		[DefaultValue ("")]
		[DataSysDescriptionAttribute ("Name of the parameter.")]
#endif
		public
#if NET_2_0
		override
#endif
		string ParameterName {
			get { return name; }
			set { name = value; }
		}

		[DefaultValue (0)]
#if !NET_2_0
		[DataSysDescriptionAttribute ("For decimal, numeric, varnumeric DBTypes.")]
#endif
		[DataCategory ("DataCategory_Data")]
		public byte Precision {
			get { return precision; }
			set { precision = value; }
		}

		[DefaultValue (0)]
#if !NET_2_0
		[DataSysDescriptionAttribute ("For decimal, numeric, varnumeric DBTypes.")]
#endif
		[DataCategory ("DataCategory_Data")]
		public byte Scale {
			get { return scale; }
			set { scale = value; }
		}

#if !NET_2_0
		[DefaultValue (0)]
		[DataSysDescriptionAttribute ("Size of variable length data types (string & arrays).")]
#endif
		[DataCategory ("DataCategory_Data")]
		public
#if NET_2_0
		override
#endif
		int Size {
			get { return size; }
			set { size = value; }
		}

#if !NET_2_0
		[DefaultValue ("")]
		[DataSysDescriptionAttribute ("When used by a DataAdapter.Update, the source column name that is used to find the DataSetColumn name in the ColumnMappings. This is to copy a value between the parameter and a datarow.")]
#endif
		[DataCategory ("DataCategory_Data")]
		public
#if NET_2_0
		override
#endif
		string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

#if NET_2_0
		public override bool SourceColumnNullMapping {
			get {
				return sourceColumnNullMapping;
			} set {
				sourceColumnNullMapping = value;
			}
		}
#endif

#if !NET_2_0
		[DefaultValue (DataRowVersion.Current)]
		[DataSysDescriptionAttribute ("When used by a DataAdapter.Update (UpdateCommand only), the version of the DataRow value that is used to update the data source.")]
#endif
		[DataCategory ("DataCategory_Data")]
		public
#if NET_2_0
		override
#endif
		DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

#if NET_2_0
		[RefreshPropertiesAttribute (RefreshProperties.All)]
#else
		[DefaultValue (null)]
		[DataSysDescriptionAttribute ("Value of the parameter.")]
#endif
		[TypeConverter (typeof (StringConverter))]
		[DataCategory ("DataCategory_Data")]
		public
#if NET_2_0
		override
#endif
		object Value {
			get { return value; }
			set { this.value = value; }
		}

		// Used to ensure that only one collection can contain this
		// parameter
		internal OleDbParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

		#endregion // Properties

		#region Internal Properties

		internal IntPtr GdaParameter {
			get { return gdaParameter; }
		}

		#endregion // Internal Properties

		#region Methods

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		public override void ResetDbType ()
		{
			ResetOleDbType ();
		}

		public void ResetOleDbType ()
		{
			oleDbType = GetOleDbType (Value);
			dbType = OleDbTypeToDbType (oleDbType);
		}
#endif

		public override string ToString ()
		{
			return ParameterName;
		}

		private OleDbType DbTypeToOleDbType (DbType dbType)
		{
			switch (dbType) {
			case DbType.AnsiString :
				return OleDbType.VarChar;
			case DbType.AnsiStringFixedLength :
				return OleDbType.Char;
			case DbType.Binary :
				return OleDbType.Binary;
			case DbType.Boolean :
				return OleDbType.Boolean;
			case DbType.Byte :
				return OleDbType.UnsignedTinyInt;
			case DbType.Currency :
				return OleDbType.Currency;
			case DbType.Date :
				return OleDbType.Date;
			case DbType.DateTime :
				throw new NotImplementedException ();
			case DbType.Decimal :
				return OleDbType.Decimal;
			case DbType.Double :
				return OleDbType.Double;
			case DbType.Guid :
				return OleDbType.Guid;
			case DbType.Int16 :
				return OleDbType.SmallInt;
			case DbType.Int32 :
				return OleDbType.Integer;
			case DbType.Int64 :
				return OleDbType.BigInt;
			case DbType.Object :
				return OleDbType.Variant;
			case DbType.SByte :
				return OleDbType.TinyInt;
			case DbType.Single :
				return OleDbType.Single;
			case DbType.String :
				return OleDbType.WChar;
			case DbType.StringFixedLength :
				return OleDbType.VarWChar;
			case DbType.Time :
				throw new NotImplementedException ();
			case DbType.UInt16 :
				return OleDbType.UnsignedSmallInt;
			case DbType.UInt32 :
				return OleDbType.UnsignedInt;
			case DbType.UInt64 :
				return OleDbType.UnsignedBigInt;
			case DbType.VarNumeric :
				return OleDbType.VarNumeric;
			}
			return OleDbType.Variant;
		}

		private DbType OleDbTypeToDbType (OleDbType oleDbType)
		{
			switch (oleDbType) {
			case OleDbType.BigInt :
				return DbType.Int64;
			case OleDbType.Binary :
				return DbType.Binary;
			case OleDbType.Boolean :
				return DbType.Boolean;
			case OleDbType.BSTR :
				return DbType.AnsiString;
			case OleDbType.Char :
				return DbType.AnsiStringFixedLength;
			case OleDbType.Currency :
				return DbType.Currency;
			case OleDbType.Date :
				return DbType.DateTime;
			case OleDbType.DBDate :
				return DbType.DateTime;
			case OleDbType.DBTime :
				throw new NotImplementedException ();
			case OleDbType.DBTimeStamp :
				return DbType.DateTime;
			case OleDbType.Decimal :
				return DbType.Decimal;
			case OleDbType.Double :
				return DbType.Double;
			case OleDbType.Empty :
				throw new NotImplementedException ();
			case OleDbType.Error :
				throw new NotImplementedException ();
			case OleDbType.Filetime :
				return DbType.DateTime;
			case OleDbType.Guid :
				return DbType.Guid;
			case OleDbType.IDispatch :
				return DbType.Object;
			case OleDbType.Integer :
				return DbType.Int32;
			case OleDbType.IUnknown :
				return DbType.Object;
			case OleDbType.LongVarBinary :
				return DbType.Binary;
			case OleDbType.LongVarChar :
				return DbType.AnsiString;
			case OleDbType.LongVarWChar :
				return DbType.String;
			case OleDbType.Numeric :
				return DbType.Decimal;
			case OleDbType.PropVariant :
				return DbType.Object;
			case OleDbType.Single :
				return DbType.Single;
			case OleDbType.SmallInt :
				return DbType.Int16;
			case OleDbType.TinyInt :
				return DbType.SByte;
			case OleDbType.UnsignedBigInt :
				return DbType.UInt64;
			case OleDbType.UnsignedInt :
				return DbType.UInt32;
			case OleDbType.UnsignedSmallInt :
				return DbType.UInt16;
			case OleDbType.UnsignedTinyInt :
				return DbType.Byte;
			case OleDbType.VarBinary :
				return DbType.Binary;
			case OleDbType.VarChar :
				return DbType.AnsiString;
			case OleDbType.Variant :
				return DbType.Object;
			case OleDbType.VarNumeric :
				return DbType.VarNumeric;
			case OleDbType.VarWChar :
				return DbType.StringFixedLength;
			case OleDbType.WChar :
				return DbType.String;
			}
			return DbType.Object;
		}

		private OleDbType GetOleDbType (object value)
		{
			if (value is Guid) return OleDbType.Guid;
			if (value is TimeSpan) return OleDbType.DBTime;

			switch (Type.GetTypeCode (value.GetType ())) {
			case TypeCode.Boolean :
				return OleDbType.Boolean;
			case TypeCode.Byte :
				if (value.GetType().IsArray) 
					return OleDbType.Binary;
				else 
					return OleDbType.UnsignedTinyInt;
			case TypeCode.Char :
				return OleDbType.Char;
			case TypeCode.DateTime :
				return OleDbType.Date;
			case TypeCode.DBNull :
				return OleDbType.Empty;
			case TypeCode.Decimal :
				return OleDbType.Decimal;
			case TypeCode.Double :
				return OleDbType.Double;
			case TypeCode.Empty :
				return OleDbType.Empty;
			case TypeCode.Int16 :
				return OleDbType.SmallInt;
			case TypeCode.Int32 :
				return OleDbType.Integer;
			case TypeCode.Int64 :
				return OleDbType.BigInt;
			case TypeCode.SByte :
				return OleDbType.TinyInt;
			case TypeCode.String :
				return OleDbType.VarChar;
			case TypeCode.Single :
				return OleDbType.Single;
			case TypeCode.UInt64 :
				return OleDbType.UnsignedBigInt;
			case TypeCode.UInt32 :
				return OleDbType.UnsignedInt;
			case TypeCode.UInt16 :
				return OleDbType.UnsignedSmallInt;
			case TypeCode.Object :
				return OleDbType.Variant;
			}
			return OleDbType.IUnknown;
		}

		#endregion
	}
}
