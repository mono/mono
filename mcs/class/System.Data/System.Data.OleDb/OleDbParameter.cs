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

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
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
		ParameterDirection direction;
		OleDbType oleDbType;
		DbType dbType;

		#endregion

		#region Constructors
		
		public OleDbParameter ()
		{
			name = String.Empty;
			value = null;
			size = 0;
			isNullable = true;
			precision = 0;
			scale = 0;
			sourceColumn = String.Empty;
		}

		public OleDbParameter (string name, object value) 
			: this ()
		{
			this.name = name;
			this.value = value;
		}

		public OleDbParameter (string name, OleDbType dataType) 
			: this ()
		{
			this.name = name;
			this.oleDbType = dataType;
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

		public OleDbParameter(string name, OleDbType dataType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
			: this (name, dataType, size, srcColumn)
		{
			this.direction = direction;
			this.isNullable = isNullable;
			this.precision = precision;
			this.scale = scale;
			this.sourceVersion = srcVersion;
			this.value = value;
		}

		#endregion

		#region Properties

		public DbType DbType {
			get { return dbType; }
			set { 
				dbType = value;
				oleDbType = DbTypeToOleDbType (value);
			}
		}
		
		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}
		
		public bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

		public OleDbType OleDbType {
			get { return oleDbType; }
			set {
				oleDbType = value;
				dbType = OleDbTypeToDbType (value);
			}
		}
		
		public string ParameterName {
			get { return name; }
			set { name = value; }
		}

		public byte Precision {
			get { return precision; }
			set { precision = value; }
		}
		
		public byte Scale {
			get { return scale; }
			set { scale = value; }
		}
		
		public int Size {
			get { return size; }
			set { size = value; }
		}

		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}
		
		public DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}
		
		public object Value {
			get { return value; }
			set { value = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

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

		#endregion
	}
}
