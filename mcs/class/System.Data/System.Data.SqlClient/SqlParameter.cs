//
// System.Data.SqlClient.SqlParameter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient.Internal;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.SqlClient {
	public sealed class SqlParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		SqlParameterCollection container = null;
		DbType dbType;
		ParameterDirection direction = ParameterDirection.Input;
		bool isNullable;
		bool isSizeSet = false;
		bool isTypeSet = false;
		object objValue;
		int offset;
		string parameterName;
		byte precision;
		byte scale;
		int size;
		SqlDbType sqlDbType;
		string sourceColumn;
		DataRowVersion sourceVersion;
		string typeName;

		#endregion // Fields

		#region Constructors

		public SqlParameter () 
			: this (String.Empty, SqlDbType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public SqlParameter (string parameterName, object value) 
		{
			this.parameterName = parameterName;
			this.objValue = value;
			this.sourceVersion = DataRowVersion.Current;
			InferSqlType (value);
		}
		
		public SqlParameter (string parameterName, SqlDbType dbType) 
			: this (parameterName, dbType, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public SqlParameter (string parameterName, SqlDbType dbType, int size) 
			: this (parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}
		
		public SqlParameter (string parameterName, SqlDbType dbType, int size, string sourceColumn) 
			: this (parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, sourceColumn, DataRowVersion.Current, null)
		{
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]	 
		public SqlParameter (string parameterName, SqlDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) 
		{
			SqlDbType = dbType;
			Size = size;
			Value = value;

			ParameterName = parameterName;
			Direction = direction;
			IsNullable = isNullable;
			Precision = precision;
			Scale = scale;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
		}

		// This constructor is used internally to construct a
		// SqlParameter.  The value array comes from sp_procedure_params_rowset.
		// This is in SqlCommand.DeriveParameters.
		internal SqlParameter (object[] dbValues)
		{
			precision = 0;
			scale = 0;
			direction = ParameterDirection.Input;

			parameterName = (string) dbValues[3];

			switch ((short) dbValues[5]) {
			case 1:
				direction = ParameterDirection.Input;
				break;
			case 2:
				direction = ParameterDirection.Output;
				break;
			case 3:
				direction = ParameterDirection.InputOutput;
				break;
			case 4:
				direction = ParameterDirection.ReturnValue;
				break;
			}

			isNullable = (bool) dbValues[8];

			if (dbValues[12] != null)
				precision = (byte) ((short) dbValues[12]);
			if (dbValues[13] != null)
				scale = (byte) ((short) dbValues[13]);

			SetDbTypeName ((string) dbValues[16]);
		}

		#endregion // Constructors

		#region Properties

		// Used to ensure that only one collection can contain this
		// parameter
		internal SqlParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The parameter generic type.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[RefreshProperties (RefreshProperties.All)]
		public DbType DbType {
			get { return dbType; }
			set { 
				SetDbType (value); 
				isTypeSet = true;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Input, output, or bidirectional parameter.")]
		[DefaultValue (ParameterDirection.Input)]
		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		string IDataParameter.ParameterName {
			get { return parameterName; }
			set { parameterName = value; }
		}

		[Browsable (false)]
		[DataSysDescription ("a design-time property used for strongly typed code-generation.")]
		[DefaultValue (false)]
		[DesignOnly (true)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]	 
		public bool IsNullable	{
			get { return isNullable; }
			set { isNullable = value; }
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("Offset in variable length data types.")]
		[DefaultValue (0)]
		public int Offset {
			get { return offset; }
			set { offset = value; }
		}
		
		[DataSysDescription ("Name of the parameter, like '@p1'")]
		[DefaultValue ("")]
		public string ParameterName {
			get { return parameterName; }
			set { parameterName = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("For decimal, numeric, varnumeric DBTypes.")]
		[DefaultValue (0)]
		public byte Precision {
			get { return precision; }
			set { precision = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("For decimal, numeric, varnumeric DBTypes.")]
		[DefaultValue (0)]
                public byte Scale {
			get { return scale; }
			set { scale = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Size of variable length datatypes (strings & arrays).")]
		[DefaultValue (0)]
                public int Size {
			get { return size; }
			set { 
				size = value; 
				isSizeSet = true;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("When used by a DataAdapter.Update, the source column name that is used to find the DataSetColumn name in the ColumnMappings. This is to copy a value between the parameter and a datarow.")]
		[DefaultValue ("")]
		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("When used by a DataAdapter.Update (UpdateCommand only), the version of the DataRow value that is used to update the data source.")]
		[DefaultValue (DataRowVersion.Current)]
		public DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}
		
		[DataCategory ("Data")]
		[DataSysDescription ("The parameter native type.")]
		[DefaultValue (SqlDbType.NVarChar)]
		[RefreshProperties (RefreshProperties.All)]
		public SqlDbType SqlDbType {
			get { return sqlDbType; }
			set { 
				SetSqlDbType (value); 
				isTypeSet = true;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Value of the parameter.")]
		[DefaultValue (null)]
		public object Value {
			get { return objValue; }
			set { 
				if (!isTypeSet)
					InferSqlType (value);
				objValue = value; 
			}
		}

		#endregion // Properties

		#region Methods

		object ICloneable.Clone ()
		{
			return new SqlParameter (ParameterName, SqlDbType, Size, Direction, IsNullable, Precision, Scale, SourceColumn, SourceVersion, Value);
		}

		// If the value is set without the DbType/SqlDbType being set, then we
		// infer type information.
		private void InferSqlType (object value)
		{
			Type type = value.GetType ();

			string exception = String.Format ("The parameter data type of {0} is invalid.", type.Name);

			switch (type.FullName) {
			case "System.Int64":
				SetSqlDbType (SqlDbType.BigInt);
				break;
			case "System.Boolean":
				SetSqlDbType (SqlDbType.Bit);
				break;
			case "System.String":
				SetSqlDbType (SqlDbType.NVarChar);
				break;
			case "System.DateTime":
				SetSqlDbType (SqlDbType.DateTime);
				break;
			case "System.Decimal":
				SetSqlDbType (SqlDbType.Decimal);
				break;
			case "System.Double":
				SetSqlDbType (SqlDbType.Float);
				break;
			case "System.Byte[]":
				SetSqlDbType (SqlDbType.VarBinary);
				break;
			case "System.Byte":
				SetSqlDbType (SqlDbType.TinyInt);
				break;
			case "System.Int32":
				SetSqlDbType (SqlDbType.Int);
				break;
			case "System.Single":
				SetSqlDbType (SqlDbType.Real);
				break;
			case "System.Int16":
				SetSqlDbType (SqlDbType.SmallInt);
				break;
			case "System.Guid":
				SetSqlDbType (SqlDbType.UniqueIdentifier);
				break;
			case "System.Object":
				SetSqlDbType (SqlDbType.Variant);
				break;
			default:
				throw new ArgumentException (exception);				
			}
		}

		internal string Prepare (string name)
		{
			StringBuilder result = new StringBuilder ();
			result.Append (name);
			result.Append (" ");
			result.Append (typeName);

			switch (sqlDbType) {
			case SqlDbType.VarBinary :
			case SqlDbType.NVarChar :
			case SqlDbType.VarChar :
				if (!isSizeSet || size == 0)
					throw new InvalidOperationException ("All variable length parameters must have an explicitly set non-zero size.");
				result.Append (String.Format ("({0})", size));
				break;
			case SqlDbType.NChar :
			case SqlDbType.Char :
			case SqlDbType.Binary :
				if (size > 0) 
					result.Append (String.Format ("({0})", size));
				break;
			case SqlDbType.Decimal :
				result.Append (String.Format ("({0},{1})", precision, scale));
				break;
                        default:
                                break;
                        }

                        return result.ToString ();
		}

		// When the DbType is set, we also set the SqlDbType, as well as the SQL Server
		// string representation of the type name.  If the DbType is not convertible
		// to an SqlDbType, throw an exception.
		private void SetDbType (DbType type)
		{
			string exception = String.Format ("No mapping exists from DbType {0} to a known SqlDbType.", type);

			switch (type) {
			case DbType.AnsiString:
				typeName = "varchar";
				sqlDbType = SqlDbType.VarChar;
				break;
			case DbType.AnsiStringFixedLength:
				typeName = "char";
				sqlDbType = SqlDbType.Char;
				break;
			case DbType.Binary:
				typeName = "varbinary";
				sqlDbType = SqlDbType.VarBinary;
				break;
			case DbType.Boolean:
				typeName = "bit";
				sqlDbType = SqlDbType.Bit;
				break;
			case DbType.Byte:
				typeName = "tinyint";
				sqlDbType = SqlDbType.TinyInt;
				break;
			case DbType.Currency:
				sqlDbType = SqlDbType.Money;
				typeName = "money";
				break;
			case DbType.Date:
			case DbType.DateTime:
				typeName = "datetime";
				sqlDbType = SqlDbType.DateTime;
				break;
			case DbType.Decimal:
				typeName = "decimal";
				sqlDbType = SqlDbType.Decimal;
				break;
			case DbType.Double:
				typeName = "float";
				sqlDbType = SqlDbType.Float;
				break;
			case DbType.Guid:
				typeName = "uniqueidentifier";
				sqlDbType = SqlDbType.UniqueIdentifier;
				break;
			case DbType.Int16:
				typeName = "smallint";
				sqlDbType = SqlDbType.SmallInt;
				break;
			case DbType.Int32:
				typeName = "int";
				sqlDbType = SqlDbType.Int;
				break;
			case DbType.Int64:
				typeName = "bigint";
				sqlDbType = SqlDbType.BigInt;
				break;
			case DbType.Object:
				typeName = "sql_variant";
				sqlDbType = SqlDbType.Variant;
				break;
			case DbType.Single:
				typeName = "real";
				sqlDbType = SqlDbType.Real;
				break;
			case DbType.String:
				typeName = "nvarchar";
				sqlDbType = SqlDbType.NVarChar;
				break;
			case DbType.StringFixedLength:
				typeName = "nchar";
				sqlDbType = SqlDbType.NChar;
				break;
			case DbType.Time:
				typeName = "datetime";
				sqlDbType = SqlDbType.DateTime;
				break;
			default:
				throw new ArgumentException (exception);
			}
			dbType = type;
		}

		// Used by internal constructor which has a SQL Server typename
		private void SetDbTypeName (string dbTypeName)
		{
			switch (dbTypeName.ToLower ()) {	
			case "bigint":
				SqlDbType = SqlDbType.BigInt;
				break;
			case "binary":
				SqlDbType = SqlDbType.Binary;
				break;
			case "bit":
				SqlDbType = SqlDbType.Bit;
				break;
			case "char":
				SqlDbType = SqlDbType.Char;
				break;
			case "datetime":
				SqlDbType = SqlDbType.DateTime;
				break;
			case "decimal":
				SqlDbType = SqlDbType.Decimal;
				break;
			case "float":
				SqlDbType = SqlDbType.Float;
				break;
			case "image":
				SqlDbType = SqlDbType.Image;
				break;
			case "int":
				SqlDbType = SqlDbType.Int;
				break;
			case "money":
				SqlDbType = SqlDbType.Money;
				break;
			case "nchar":
				SqlDbType = SqlDbType.NChar;
				break;
			case "ntext":
				SqlDbType = SqlDbType.NText;
				break;
			case "nvarchar":
				SqlDbType = SqlDbType.NVarChar;
				break;
			case "real":
				SqlDbType = SqlDbType.Real;
				break;
			case "smalldatetime":
				SqlDbType = SqlDbType.SmallDateTime;
				break;
			case "smallint":
				SqlDbType = SqlDbType.SmallInt;
				break;
			case "smallmoney":
				SqlDbType = SqlDbType.SmallMoney;
				break;
			case "text":
				SqlDbType = SqlDbType.Text;
				break;
			case "timestamp":
				SqlDbType = SqlDbType.Timestamp;
				break;
			case "tinyint":
				SqlDbType = SqlDbType.TinyInt;
				break;
			case "uniqueidentifier":
				SqlDbType = SqlDbType.UniqueIdentifier;
				break;
			case "varbinary":
				SqlDbType = SqlDbType.VarBinary;
				break;
			case "varchar":
				SqlDbType = SqlDbType.VarChar;
				break;
			default:
				SqlDbType = SqlDbType.Variant;
				break;
			}
		}

		// When the SqlDbType is set, we also set the DbType, as well as the SQL Server
		// string representation of the type name.  If the SqlDbType is not convertible
		// to a DbType, throw an exception.
		private void SetSqlDbType (SqlDbType type)
		{
			string exception = String.Format ("No mapping exists from SqlDbType {0} to a known DbType.", type);

			switch (type) {
			case SqlDbType.BigInt:
				typeName = "bigint";
				dbType = DbType.Int64;
				break;
			case SqlDbType.Binary:
				typeName = "binary";
				dbType = DbType.Binary;
				break;
			case SqlDbType.Timestamp:
				typeName = "timestamp";
				dbType = DbType.Binary;
				break;
			case SqlDbType.VarBinary:
				typeName = "varbinary";
				dbType = DbType.Binary;
				break;
			case SqlDbType.Bit:
				typeName = "bit";
				dbType = DbType.Boolean;
				break;
			case SqlDbType.Char:
				typeName = "char";
				dbType = DbType.AnsiStringFixedLength;
				break;
			case SqlDbType.DateTime:
				typeName = "datetime";
				dbType = DbType.DateTime;
				break;
			case SqlDbType.SmallDateTime:
				typeName = "smalldatetime";
				dbType = DbType.DateTime;
				break;
			case SqlDbType.Decimal:
				typeName = "decimal";
				dbType = DbType.Decimal;
				break;
			case SqlDbType.Float:
				typeName = "float";
				dbType = DbType.Double;
				break;
			case SqlDbType.Image:
				typeName = "image";
				dbType = DbType.Binary;
				break;
			case SqlDbType.Int:
				typeName = "int";
				dbType = DbType.Int32;
				break;
			case SqlDbType.Money:
				typeName = "money";
				dbType = DbType.Currency;
				break;
			case SqlDbType.SmallMoney:
				typeName = "smallmoney";
				dbType = DbType.Currency;
				break;
			case SqlDbType.NChar:
				typeName = "nchar";
				dbType = DbType.StringFixedLength;
				break;
			case SqlDbType.NText:
				typeName = "ntext";
				dbType = DbType.String;
				break;
			case SqlDbType.NVarChar:
				typeName = "nvarchar";
				dbType = DbType.String;
				break;
			case SqlDbType.Real:
				typeName = "real";
				dbType = DbType.Single;
				break;
			case SqlDbType.SmallInt:
				typeName = "smallint";
				dbType = DbType.Int16;
				break;
			case SqlDbType.Text:
				typeName = "text";
				dbType = DbType.AnsiString;
				break;
			case SqlDbType.VarChar:
				typeName = "varchar";
				dbType = DbType.AnsiString;
				break;
			case SqlDbType.TinyInt:
				typeName = "tinyint";
				dbType = DbType.Byte;
				break;
			case SqlDbType.UniqueIdentifier:
				typeName = "uniqueidentifier";
				dbType = DbType.Guid;
				break;
			case SqlDbType.Variant:
				typeName = "sql_variant";
				dbType = DbType.Object;
				break;
			default:
				throw new ArgumentException (exception);
			}
			sqlDbType = type;
		}

		public override string ToString() 
		{
			return parameterName;
		}

		#endregion // Methods
	}
}
