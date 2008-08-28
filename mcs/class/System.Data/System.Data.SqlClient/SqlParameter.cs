//
// System.Data.SqlClient.SqlParameter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//   Diego Caravana (diego@toth.it)
//   Umadevi S (sumadevi@novell.com)
//   Amit Biswas (amit@amitbiswas.com)
//
// (C) Ximian, Inc. 2002
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

using Mono.Data.Tds;
using Mono.Data.Tds.Protocol;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.SqlClient {
#if NET_2_0
	[TypeConverterAttribute ("System.Data.SqlClient.SqlParameter+SqlParameterConverter, " + Consts.AssemblySystem_Data)]
	public sealed class SqlParameter : DbParameter, IDbDataParameter, IDataParameter, ICloneable
#else
	[TypeConverterAttribute (typeof (SqlParameterConverter))]
	public sealed class SqlParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
#endif // NET_2_0
	{
		#region Fields

		TdsMetaParameter metaParameter;

		SqlParameterCollection container = null;
		DbType dbType;
		ParameterDirection direction = ParameterDirection.Input;
		bool isTypeSet;
		int offset;
		SqlDbType sqlDbType;
		string sourceColumn;
		DataRowVersion sourceVersion;
		SqlCompareOptions compareInfo;
		int localeId;
		Object sqlValue;
#if NET_2_0
		bool sourceColumnNullMapping;
		string xmlSchemaCollectionDatabase = String.Empty;
		string xmlSchemaCollectionOwningSchema = String.Empty;
		string xmlSchemaCollectionName = String.Empty;
#endif

		#endregion // Fields

		#region Constructors

		public SqlParameter () 
			: this (String.Empty, SqlDbType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
			isTypeSet = false;
		}

		public SqlParameter (string parameterName, object value) 
		{
 			metaParameter = new TdsMetaParameter (parameterName, value);
 			InferSqlType (value);
 			metaParameter.Value =  SqlTypeToFrameworkType (value);
			sourceVersion = DataRowVersion.Current;
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
			metaParameter = new TdsMetaParameter (parameterName, size, 
							      isNullable, precision, 
							      scale, 
							      value);
			if (dbType != SqlDbType.Variant) 
				SqlDbType = dbType;
			metaParameter.Value = SqlTypeToFrameworkType (value);
			Direction = direction;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
		}

#if NET_2_0
		public SqlParameter (string parameterName, SqlDbType dbType, int size, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, Object value, string xmlSchemaCollectionDatabase, string xmlSchemaCollectionOwningSchema, string xmlSchemaCollectionName)
			: this (parameterName, dbType, size, direction, false, precision, scale, sourceColumn, sourceVersion, value)
		{
			XmlSchemaCollectionDatabase = xmlSchemaCollectionDatabase;
			XmlSchemaCollectionOwningSchema = xmlSchemaCollectionOwningSchema;
			XmlSchemaCollectionName = xmlSchemaCollectionName;
			SourceColumnNullMapping = sourceColumnNullMapping;
		}
#endif

		// This constructor is used internally to construct a
		// SqlParameter.  The value array comes from sp_procedure_params_rowset.
		// This is in SqlCommand.DeriveParameters.
		internal SqlParameter (object[] dbValues) 
			: this (dbValues [3].ToString (), String.Empty)
		{
			Precision = 0;
			Scale = 0;
			Direction = ParameterDirection.Input;

			ParameterName = (string) dbValues[3];

			switch ((short) dbValues[5]) {
			case 1:
				Direction = ParameterDirection.Input;
				break;
			case 2:
				Direction = ParameterDirection.Output;
				break;
			case 3:
				Direction = ParameterDirection.InputOutput;
				break;
			case 4:
				Direction = ParameterDirection.ReturnValue;
				break;
			default:
				Direction = ParameterDirection.Input;
				break;
			}
			IsNullable = (dbValues [8] != null && 
				dbValues [8] != DBNull.Value) ? (bool) dbValues [8] : false;

			if (dbValues [12] != null && dbValues [12] != DBNull.Value)
				Precision = (byte) ((short) dbValues [12]);

			if (dbValues [13] != null && dbValues [13] != DBNull.Value)
				Scale = (byte) ( (short) dbValues [13]);

			SetDbTypeName ((string) dbValues [16]);
		}

		#endregion // Constructors

		#region Properties

		// Used to ensure that only one collection can contain this
		// parameter
		internal SqlParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

		internal void CheckIfInitialized ()
		{
			if (!isTypeSet)
				throw new Exception ("all parameters to have an explicity set type");

			if (MetaParameter.IsVariableSizeType) {
				if (SqlDbType == SqlDbType.Decimal && Precision == 0)
					throw new Exception ("Parameter of type 'Decimal' have an explicitly set Precision and Scale");
				else if (Size == 0)
					throw new Exception ("all variable length parameters to have an explicitly set non-zero Size");
			}
		}
	
#if ONLY_1_0 || ONLY_1_1
		[Browsable (false)]
		[DataSysDescription ("The parameter generic type.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[RefreshProperties (RefreshProperties.All)]
		[DataCategory ("Data")]
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
	 	DbType DbType {
			get { return dbType; }
			set { 
				SetDbType (value); 
				isTypeSet = true;
			}
		}

#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("Input, output, or bidirectional parameter.")]
		[DefaultValue (ParameterDirection.Input)]
#endif
#if NET_2_0
		[RefreshProperties (RefreshProperties.All)]
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
	 ParameterDirection Direction {
			get { return direction; }
			set { 
				direction = value; 
				switch( direction ) {
					case ParameterDirection.Output:
					MetaParameter.Direction = TdsParameterDirection.Output;
						break;
					case ParameterDirection.InputOutput:
						MetaParameter.Direction = TdsParameterDirection.InputOutput;
						break;
					case ParameterDirection.ReturnValue:
						MetaParameter.Direction = TdsParameterDirection.ReturnValue;
						break;
				}
			}
		}

		internal TdsMetaParameter MetaParameter {
			get { return metaParameter; }
		}

#if ONLY_1_0 || ONLY_1_1
		[Browsable (false)]
		[DataSysDescription ("a design-time property used for strongly typed code-generation.")]
		[DefaultValue (false)]
		[DesignOnly (true)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]	 
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
		bool IsNullable {
			get { return metaParameter.IsNullable; }
			set { metaParameter.IsNullable = value; }
		}

		[Browsable (false)]
#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("Offset in variable length data types.")]
		[DefaultValue (0)]
#endif
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		public int Offset {
			get { return offset; }
			set { offset = value; }
		}
	
#if ONLY_1_0 || ONLY_1_1
		[DataSysDescription ("Name of the parameter, like '@p1'")]
		[DefaultValue ("")]
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
		string ParameterName {
			get { return metaParameter.ParameterName; }
			set { metaParameter.ParameterName = value; }
		}

		[DefaultValue (0)]
#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("For decimal, numeric, varnumeric DBTypes.")]
#endif
		public byte Precision {
			get { return metaParameter.Precision; }
			set { metaParameter.Precision = value; }
		}

		[DefaultValue (0)]
#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("For decimal, numeric, varnumeric DBTypes.")]
#endif
		public byte Scale {
			get { return metaParameter.Scale; }
			set { metaParameter.Scale = value; }
		}

#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("Size of variable length data types (string & arrays).")]
		[DefaultValue (0)]
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
		int Size {
			get { return metaParameter.Size; }
			set { metaParameter.Size = value; }
		}

#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("When used by a DataAdapter.Update, the source column name that is used to find the DataSetColumn name in the ColumnMappings. This is to copy a value between the parameter and a datarow.")]
		[DefaultValue ("")]
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
		string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("When used by a DataAdapter.Update (UpdateCommand only), the version of the DataRow value that is used to update the data source.")]
		[DefaultValue (DataRowVersion.Current)]
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
		DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}
		
#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("The parameter native type.")]
		[DefaultValue (SqlDbType.NVarChar)]
#endif
		[RefreshProperties (RefreshProperties.All)]
#if NET_2_0
		[DbProviderSpecificTypeProperty(true)]
#endif
		public SqlDbType SqlDbType {
			get { return sqlDbType; }
			set { 
				SetSqlDbType (value); 
				isTypeSet = true;
			}
		}

		[TypeConverterAttribute (typeof (StringConverter))]
#if ONLY_1_0 || ONLY_1_1
		[DataCategory ("Data")]
		[DataSysDescription ("Value of the parameter.")]
		[DefaultValue (null)]
#else
		[RefreshProperties (RefreshProperties.All)]
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
		object Value {
			get { return metaParameter.Value; }
			set {
				if (!isTypeSet)
					InferSqlType (value);
				metaParameter.Value = SqlTypeToFrameworkType (value);
			}
		}

#if NET_2_0
		[Browsable (false)]
		public SqlCompareOptions CompareInfo{
			get{ return compareInfo; }
			set{ compareInfo = value; }
		}

		[Browsable (false)]
		public int LocaleId { 
			get { return localeId; }
			set { localeId = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Object SqlValue { 
			get { return sqlValue; }
			set { sqlValue = value; }
		}
	
		public override bool SourceColumnNullMapping {
			get { return sourceColumnNullMapping; }
			set { sourceColumnNullMapping = value; }
		}

		public string XmlSchemaCollectionDatabase {
			get { return xmlSchemaCollectionDatabase; }
			set { xmlSchemaCollectionDatabase = (value == null ? String.Empty : value); }
		}

		public string XmlSchemaCollectionName {
			get { return xmlSchemaCollectionName; }
			set {
				xmlSchemaCollectionName = (value == null ? String.Empty : value);
			}
		}

		public string XmlSchemaCollectionOwningSchema {
			get { return xmlSchemaCollectionOwningSchema; } 
			set {
				xmlSchemaCollectionOwningSchema = (value == null ? String.Empty : value);
			}
		}
#endif
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
			if (value == null || value == DBNull.Value) {
				SetSqlDbType (SqlDbType.NVarChar);
				return;
			}

			Type type = value.GetType ();
			string exception = String.Format ("The parameter data type of {0} is invalid.", type.Name);

			switch (type.FullName) {
			case "System.Int64":
			case "System.Data.SqlTypes.SqlInt64":
				SetSqlDbType (SqlDbType.BigInt);
				break;
			case "System.Boolean":
			case "System.Data.SqlTypes.SqlBoolean":
				SetSqlDbType (SqlDbType.Bit);
				break;
			case "System.String":
			case "System.Data.SqlTypes.SqlString":
				SetSqlDbType (SqlDbType.NVarChar);
				break;
			case "System.DateTime":
			case "System.Data.SqlTypes.SqlDateTime":
				SetSqlDbType (SqlDbType.DateTime);
				break;
			case "System.Decimal":
			case "System.Data.SqlTypes.SqlDecimal":
				SetSqlDbType (SqlDbType.Decimal);
				break;
			case "System.Double":
			case "System.Data.SqlTypes.SqlDouble":
				SetSqlDbType (SqlDbType.Float);
				break;
			case "System.Byte[]":
			case "System.Data.SqlTypes.SqlBinary":
				SetSqlDbType (SqlDbType.VarBinary);
				break;
			case "System.Byte":
			case "System.Data.SqlTypes.SqlByte":
				SetSqlDbType (SqlDbType.TinyInt);
				break;
			case "System.Int32":
			case "System.Data.SqlTypes.SqlInt32":
				SetSqlDbType (SqlDbType.Int);
				break;
			case "System.Single":
			case "System.Data.SqlTypes.Single":
				SetSqlDbType (SqlDbType.Real);
				break;
			case "System.Int16":
			case "System.Data.SqlTypes.SqlInt16":
				SetSqlDbType (SqlDbType.SmallInt);
				break;
			case "System.Guid":
			case "System.Data.SqlTypes.SqlGuid":
				SetSqlDbType (SqlDbType.UniqueIdentifier);
				break;
			case "System.Money":
			case "System.SmallMoney":
			case "System.Data.SqlTypes.SqlMoney":
				SetSqlDbType (SqlDbType.Money);
				break;
			case "System.Object":
				SetSqlDbType (SqlDbType.Variant); 
				break;
			default:
				throw new ArgumentException (exception);
			}
		}

		// When the DbType is set, we also set the SqlDbType, as well as the SQL Server
		// string representation of the type name.  If the DbType is not convertible
		// to an SqlDbType, throw an exception.
		private void SetDbType (DbType type)
		{
			string exception = String.Format ("No mapping exists from DbType {0} to a known SqlDbType.", type);

			switch (type) {
			case DbType.AnsiString:
				MetaParameter.TypeName = "varchar";
				sqlDbType = SqlDbType.VarChar;
				MetaParameter.IsVariableSizeType = true;
				break;
			case DbType.AnsiStringFixedLength:
				MetaParameter.TypeName = "char";
				sqlDbType = SqlDbType.Char;
				MetaParameter.IsVariableSizeType = true;
				break;
			case DbType.Binary:
				MetaParameter.TypeName = "varbinary";
				sqlDbType = SqlDbType.VarBinary;
				MetaParameter.IsVariableSizeType = true;
				break;
			case DbType.Boolean:
				MetaParameter.TypeName = "bit";
				sqlDbType = SqlDbType.Bit;
				break;
			case DbType.Byte:
				MetaParameter.TypeName = "tinyint";
				sqlDbType = SqlDbType.TinyInt;
				break;
			case DbType.Currency:
				sqlDbType = SqlDbType.Money;
				MetaParameter.TypeName = "money";
				break;
			case DbType.Date:
			case DbType.DateTime:
				MetaParameter.TypeName = "datetime";
				sqlDbType = SqlDbType.DateTime;
				break;
			case DbType.Decimal:
				MetaParameter.TypeName = "decimal";
				sqlDbType = SqlDbType.Decimal;
				break;
			case DbType.Double:
				MetaParameter.TypeName = "float";
				sqlDbType = SqlDbType.Float;
				break;
			case DbType.Guid:
				MetaParameter.TypeName = "uniqueidentifier";
				sqlDbType = SqlDbType.UniqueIdentifier;
				break;
			case DbType.Int16:
				MetaParameter.TypeName = "smallint";
				sqlDbType = SqlDbType.SmallInt;
				break;
			case DbType.Int32:
				MetaParameter.TypeName = "int";
				sqlDbType = SqlDbType.Int;
				break;
			case DbType.Int64:
				MetaParameter.TypeName = "bigint";
				sqlDbType = SqlDbType.BigInt;
				break;
			case DbType.Object:
				MetaParameter.TypeName = "sql_variant";
				sqlDbType = SqlDbType.Variant;
				break;
			case DbType.Single:
				MetaParameter.TypeName = "real";
				sqlDbType = SqlDbType.Real;
				break;
			case DbType.String:
				MetaParameter.TypeName = "nvarchar";
				sqlDbType = SqlDbType.NVarChar;
				MetaParameter.IsVariableSizeType = true;
				break;
			case DbType.StringFixedLength:
				MetaParameter.TypeName = "nchar";
				sqlDbType = SqlDbType.NChar;
				MetaParameter.IsVariableSizeType = true;
				break;
			case DbType.Time:
				MetaParameter.TypeName = "datetime";
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
			case "sql_variant":
				SqlDbType = SqlDbType.Variant;
				break;
			default:
				SqlDbType = SqlDbType.Variant;
				break;
			}
		}

		// When the SqlDbType is set, we also set the DbType, as well as the SQL Server
		// string representation of the type name.  If the SqlDbType is not convertible
		// to a DbType, throw an exception.
		internal void SetSqlDbType (SqlDbType type)
		{
			string exception = String.Format ("No mapping exists from SqlDbType {0} to a known DbType.", type);

			switch (type) {
			case SqlDbType.BigInt:
				MetaParameter.TypeName = "bigint";
				dbType = DbType.Int64;
				break;
			case SqlDbType.Binary:
				MetaParameter.TypeName = "binary";
				dbType = DbType.Binary;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.Timestamp:
				MetaParameter.TypeName = "timestamp";
				dbType = DbType.Binary;
				break;
			case SqlDbType.VarBinary:
				MetaParameter.TypeName = "varbinary";
				dbType = DbType.Binary;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.Bit:
				MetaParameter.TypeName = "bit";
				dbType = DbType.Boolean;
				break;
			case SqlDbType.Char:
				MetaParameter.TypeName = "char";
				dbType = DbType.AnsiStringFixedLength;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.DateTime:
				MetaParameter.TypeName = "datetime";
				dbType = DbType.DateTime;
				break;
			case SqlDbType.SmallDateTime:
				MetaParameter.TypeName = "smalldatetime";
				dbType = DbType.DateTime;
				break;
			case SqlDbType.Decimal:
				MetaParameter.TypeName = "decimal";
				dbType = DbType.Decimal;
				break;
			case SqlDbType.Float:
				MetaParameter.TypeName = "float";
				dbType = DbType.Double;
				break;
			case SqlDbType.Image:
				MetaParameter.TypeName = "image";
				dbType = DbType.Binary;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.Int:
				MetaParameter.TypeName = "int";
				dbType = DbType.Int32;
				break;
			case SqlDbType.Money:
				MetaParameter.TypeName = "money";
				dbType = DbType.Currency;
				break;
			case SqlDbType.SmallMoney:
				MetaParameter.TypeName = "smallmoney";
				dbType = DbType.Currency;
				break;
			case SqlDbType.NChar:
				MetaParameter.TypeName = "nchar";
				dbType = DbType.StringFixedLength;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.NText:
				MetaParameter.TypeName = "ntext";
				dbType = DbType.String;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.NVarChar:
				MetaParameter.TypeName = "nvarchar";
				dbType = DbType.String;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.Real:
				MetaParameter.TypeName = "real";
				dbType = DbType.Single;
				break;
			case SqlDbType.SmallInt:
				MetaParameter.TypeName = "smallint";
				dbType = DbType.Int16;
				break;
			case SqlDbType.Text:
				MetaParameter.TypeName = "text";
				dbType = DbType.AnsiString;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.VarChar:
				MetaParameter.TypeName = "varchar";
				dbType = DbType.AnsiString;
				MetaParameter.IsVariableSizeType = true;
				break;
			case SqlDbType.TinyInt:
				MetaParameter.TypeName = "tinyint";
				dbType = DbType.Byte;
				break;
			case SqlDbType.UniqueIdentifier:
				MetaParameter.TypeName = "uniqueidentifier";
				dbType = DbType.Guid;
				break;
			case SqlDbType.Variant:
				MetaParameter.TypeName = "sql_variant";
				dbType = DbType.Object;
				break;
			default:
				throw new ArgumentException (exception);
			}
			sqlDbType = type;
		}

		public override string ToString() 
		{
			return ParameterName;
		}

		private object SqlTypeToFrameworkType (object value)
		{
			if (! (value is INullable)) // if the value is not SqlType
				return ConvertToFrameworkType (value);

			// Map to .net type, as Mono TDS respects only types from .net
			switch (value.GetType ().FullName) {
			case "System.Data.SqlTypes.SqlBinary":
				return ( (SqlBinary) value).Value;
			case "System.Data.SqlTypes.SqlBoolean":
				return ( (SqlBoolean) value).Value;
			case "System.Data.SqlTypes.SqlByte":
				return ( (SqlByte) value).Value;
			case "System.Data.SqlTypes.SqlDateTime":
				return ( (SqlDateTime) value).Value;
			case "System.Data.SqlTypes.SqlDecimal":
				return ( (SqlDecimal) value).Value;
			case "System.Data.SqlTypes.SqlDouble":
				return ( (SqlDouble) value).Value;
			case "System.Data.SqlTypes.SqlGuid":
				return ( (SqlGuid) value).Value;
			case "System.Data.SqlTypes.SqlInt16":
				return ( (SqlInt16) value).Value;
			case "System.Data.SqlTypes.SqlInt32 ":
				return ( (SqlInt32 ) value).Value;
			case "System.Data.SqlTypes.SqlInt64":
				return ( (SqlInt64) value).Value;
			case "System.Data.SqlTypes.SqlMoney":
				return ( (SqlMoney) value).Value;
			case "System.Data.SqlTypes.SqlSingle":
				return ( (SqlSingle) value).Value;
			case "System.Data.SqlTypes.SqlString":
				return ( (SqlString) value).Value;
			}
			return value;
		}

		internal object ConvertToFrameworkType (object value)
		{
			if (value == null || value == DBNull.Value)
				return value;
			
			if (value is string && ((string)value).Length == 0)
				return DBNull.Value;
			
			switch (sqlDbType)  {
			case SqlDbType.BigInt :
				return Convert.ChangeType (value, typeof (Int64));
			case SqlDbType.Binary:
			case SqlDbType.Image:
			case SqlDbType.VarBinary:
				if (value is byte[])
					return value;
				break;
			case SqlDbType.Bit:
				return Convert.ChangeType (value, typeof (bool));
			case SqlDbType.Int:
				return Convert.ChangeType (value, typeof (Int32));
			case SqlDbType.SmallInt :
				return Convert.ChangeType (value, typeof (Int16));
			case SqlDbType.TinyInt :
				return Convert.ChangeType (value, typeof (byte));
			case SqlDbType.Float:
				return Convert.ChangeType (value, typeof (Double));
			case SqlDbType.Real:
				return Convert.ChangeType (value, typeof (Single));
			case SqlDbType.Decimal:
				return Convert.ChangeType (value, typeof (Decimal));
			case SqlDbType.Money:
			case SqlDbType.SmallMoney:
				{
					Decimal val = (Decimal)Convert.ChangeType (value, typeof (Decimal));
					return Decimal.Round(val, 4);
				}
			case SqlDbType.DateTime:
			case SqlDbType.SmallDateTime:
				return Convert.ChangeType (value, typeof (DateTime));
			case SqlDbType.VarChar:
			case SqlDbType.NVarChar:
			case SqlDbType.Char:
			case SqlDbType.NChar:
			case SqlDbType.Text:
			case SqlDbType.NText:
				return Convert.ChangeType (value,  typeof (string));
			case SqlDbType.UniqueIdentifier:
				return Convert.ChangeType (value,  typeof (Guid));
			case SqlDbType.Variant:
				return metaParameter.Value;
			}
			throw new  NotImplementedException ("Type Not Supported : " + sqlDbType.ToString());
		}

#if NET_2_0
		public override void ResetDbType ()
		{
			InferSqlType (metaParameter.Value);
		}

		public void ResetSqlDbType ()
		{
			InferSqlType (metaParameter.Value);
		}
#endif // NET_2_0

		#endregion // Methods
	}
}
