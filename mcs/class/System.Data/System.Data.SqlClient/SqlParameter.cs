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
using System.Collections;
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

		SqlParameterCollection container;
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
		bool typeChanged;
#if NET_2_0
		bool sourceColumnNullMapping;
		string xmlSchemaCollectionDatabase = String.Empty;
		string xmlSchemaCollectionOwningSchema = String.Empty;
		string xmlSchemaCollectionName = String.Empty;
#endif

		static Hashtable type_mapping;

		#endregion // Fields

		#region Constructors

		static SqlParameter ()
		{
			type_mapping = new Hashtable ();
			type_mapping.Add (typeof (long), SqlDbType.BigInt);
			type_mapping.Add (typeof (SqlTypes.SqlInt64), SqlDbType.BigInt);

			type_mapping.Add (typeof (bool), SqlDbType.Bit);
			type_mapping.Add (typeof (SqlTypes.SqlBoolean), SqlDbType.Bit);

			type_mapping.Add (typeof (string), SqlDbType.NVarChar);
			type_mapping.Add (typeof (SqlTypes.SqlString), SqlDbType.NVarChar);

			type_mapping.Add (typeof (DateTime), SqlDbType.DateTime);
			type_mapping.Add (typeof (SqlTypes.SqlDateTime), SqlDbType.DateTime);

			type_mapping.Add (typeof (decimal), SqlDbType.Decimal);
			type_mapping.Add (typeof (SqlTypes.SqlDecimal), SqlDbType.Decimal);

			type_mapping.Add (typeof (double), SqlDbType.Float);
			type_mapping.Add (typeof (SqlTypes.SqlDouble), SqlDbType.Float);

			type_mapping.Add (typeof (byte []), SqlDbType.VarBinary);
			type_mapping.Add (typeof (SqlTypes.SqlBinary), SqlDbType.VarBinary);

			type_mapping.Add (typeof (byte), SqlDbType.TinyInt);
			type_mapping.Add (typeof (SqlTypes.SqlByte), SqlDbType.TinyInt);

			type_mapping.Add (typeof (int), SqlDbType.Int);
			type_mapping.Add (typeof (SqlTypes.SqlInt32), SqlDbType.Int);

			type_mapping.Add (typeof (float), SqlDbType.Real);
			type_mapping.Add (typeof (SqlTypes.SqlSingle), SqlDbType.Real);

			type_mapping.Add (typeof (short), SqlDbType.SmallInt);
			type_mapping.Add (typeof (SqlTypes.SqlInt16), SqlDbType.SmallInt);

			type_mapping.Add (typeof (Guid), SqlDbType.UniqueIdentifier);
			type_mapping.Add (typeof (SqlTypes.SqlGuid), SqlDbType.UniqueIdentifier);

			type_mapping.Add (typeof (SqlTypes.SqlMoney), SqlDbType.Money);

			type_mapping.Add (typeof (object), SqlDbType.Variant);
		}

		public SqlParameter () 
			: this (String.Empty, SqlDbType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
			isTypeSet = false;
		}

		public SqlParameter (string parameterName, object value)
		{
			if (parameterName == null)
				parameterName = string.Empty;
			metaParameter = new TdsMetaParameter (parameterName, GetFrameworkValue);
			metaParameter.RawValue = value;
			InferSqlType (value);
			sourceVersion = DataRowVersion.Current;
		}
		
		public SqlParameter (string parameterName, SqlDbType dbType) 
			: this (parameterName, dbType, 0, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Current, null)
		{
		}

		public SqlParameter (string parameterName, SqlDbType dbType, int size) 
			: this (parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Current, null)
		{
		}
		
		public SqlParameter (string parameterName, SqlDbType dbType, int size, string sourceColumn) 
			: this (parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, sourceColumn, DataRowVersion.Current, null)
		{
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public SqlParameter (string parameterName, SqlDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) 
		{
			if (parameterName == null)
				parameterName = string.Empty;

			metaParameter = new TdsMetaParameter (parameterName, size, 
							      isNullable, precision, 
							      scale,
							      GetFrameworkValue);
			metaParameter.RawValue = value;
			if (dbType != SqlDbType.Variant) 
				SqlDbType = dbType;
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
		//
		// http://social.msdn.microsoft.com/forums/en-US/transactsql/thread/900756fd-3980-48e3-ae59-a15d7fc15b4c/
		internal SqlParameter (object[] dbValues) 
			: this (dbValues [3].ToString (), (object) null)
		{
			ParameterName = (string) dbValues [3];

			switch ((short) dbValues [5]) {
			case 1:
				Direction = ParameterDirection.Input;
				break;
			case 2:
				Direction = ParameterDirection.InputOutput;
				break;
			case 3:
				Direction = ParameterDirection.Output;
				break;
			case 4:
				Direction = ParameterDirection.ReturnValue;
				break;
			default:
				Direction = ParameterDirection.Input;
				break;
			}

			SetDbTypeName ((string) dbValues [16]);

			if (MetaParameter.IsVariableSizeType) {
				if (dbValues [10] != DBNull.Value)
					Size = (int) dbValues [10];
			}

			if (SqlDbType == SqlDbType.Decimal) {
				if (dbValues [12] != null && dbValues [12] != DBNull.Value)
					Precision = (byte) ((short) dbValues [12]);
				if (dbValues [13] != null && dbValues [13] != DBNull.Value)
					Scale = (byte) ((short) dbValues [13]);
			}
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
				typeChanged = true;
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
			set {
				if (value == null)
					value = string.Empty;
				metaParameter.ParameterName = value;
			}
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
			get {
				if (sourceColumn == null)
					return string.Empty;
				return sourceColumn;
			}
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
				typeChanged = true;
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
			get { return metaParameter.RawValue; }
			set {
				if (!isTypeSet) {
#if NET_2_0
					InferSqlType (value);
#else
					if (value != null && value != DBNull.Value)
						InferSqlType (value);
#endif
				}
				metaParameter.RawValue = value;
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
			set {
				sqlValue = value;
				if (value is INullable)
					value = SqlTypeToFrameworkType (value);
				Value = value;
			}
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
		void InferSqlType (object value)
		{
			if (value == null || value == DBNull.Value) {
				SetSqlDbType (SqlDbType.NVarChar);
				return;
			}

			Type type = value.GetType ();
			if (type.IsEnum)
				type = Enum.GetUnderlyingType (type);
			object t = type_mapping [type];
			if (t == null)
				throw new ArgumentException (String.Format ("The parameter data type of {0} is invalid.", type.FullName));
			SetSqlDbType ((SqlDbType) t);
		}

		// When the DbType is set, we also set the SqlDbType, as well as the SQL Server
		// string representation of the type name.  If the DbType is not convertible
		// to an SqlDbType, throw an exception.
		private void SetDbType (DbType type)
		{
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
#if NET_2_0
				// Handle Xml type as string
			case DbType.Xml:
				MetaParameter.TypeName = "xml";
				sqlDbType = SqlDbType.Xml;
				MetaParameter.IsVariableSizeType = true;
				break;
#endif
			default:
				string exception = String.Format ("No mapping exists from DbType {0} to a known SqlDbType.", type);
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
#if NET_2_0				
			case "xml":
				SqlDbType = SqlDbType.Xml;
				break;
#endif
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
#if NET_2_0
			case SqlDbType.Xml:
				MetaParameter.TypeName = "xml";
				dbType = DbType.Xml;
				MetaParameter.IsVariableSizeType = true;
				break;
#endif
			default:
				string exception = String.Format ("No mapping exists from SqlDbType {0} to a known DbType.", type);
				throw new ArgumentOutOfRangeException ("SqlDbType", exception);
			}
			sqlDbType = type;
		}

		public override string ToString() 
		{
			return ParameterName;
		}

		object GetFrameworkValue (object rawValue, ref bool updated)
		{
			object tdsValue;

			updated = typeChanged || updated;
			if (updated) {
				tdsValue = SqlTypeToFrameworkType (rawValue);
				typeChanged = false;
			} else
				tdsValue = null;
			return tdsValue;
		}

		object SqlTypeToFrameworkType (object value)
		{
			if (!(value is INullable)) // if the value is not SqlType
				return ConvertToFrameworkType (value);

			Type type = value.GetType ();
			// Map to .net type, as Mono TDS respects only types from .net

			if (typeof (SqlString) == type) {
				return ((SqlString) value).Value;
			}

			if (typeof (SqlInt16) == type) {
				return ((SqlInt16) value).Value;
			}

			if (typeof (SqlInt32) == type) {
				return ((SqlInt32) value).Value;
			}

			if (typeof (SqlDateTime) == type) {
				return ((SqlDateTime) value).Value;
			}

			if (typeof (SqlInt64) == type) {
				return ((SqlInt64) value).Value;
			}

			if (typeof (SqlBinary) == type) {
				return ((SqlBinary) value).Value;
			}

			if (typeof (SqlBoolean) == type) {
				return ((SqlBoolean) value).Value;
			}

			if (typeof (SqlByte) == type) {
				return ((SqlByte) value).Value;
			}

			if (typeof (SqlDecimal) == type) {
				return ((SqlDecimal) value).Value;
			}

			if (typeof (SqlDouble) == type) {
				return ((SqlDouble) value).Value;
			}

			if (typeof (SqlGuid) == type) {
				return ((SqlGuid) value).Value;
			}

			if (typeof (SqlMoney) == type) {
				return ((SqlMoney) value).Value;
			}

			if (typeof (SqlMoney) == type) {
				return ((SqlMoney) value).Value;
			}

			if (typeof (SqlSingle) == type) {
				return ((SqlSingle) value).Value;
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
#if NET_2_0
			case SqlDbType.Xml:
#endif
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
			InferSqlType (Value);
		}

		public void ResetSqlDbType ()
		{
			InferSqlType (Value);
		}
#endif // NET_2_0

		#endregion // Methods
	}
}
