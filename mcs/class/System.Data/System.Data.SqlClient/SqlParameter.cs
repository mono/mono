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
//   Veerapuram Varadhan (vvaradhan@novell.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004, 2008, 2009 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient {
	[TypeConverterAttribute ("System.Data.SqlClient.SqlParameter+SqlParameterConverter, " + Consts.AssemblySystem_Data)]
	public sealed class SqlParameter : DbParameter, IDbDataParameter, IDataParameter, ICloneable
	{
#region Import from old DbParameter
		static Hashtable dbTypeMapping = new Hashtable ();
                internal static Hashtable DbTypeMapping {
                        get { return dbTypeMapping;}
                        set { dbTypeMapping = value;}
                }

                // LAMESPEC: Implementors should populate the dbTypeMapping accordingly
                internal Type SystemType {
                        get {
                                return (Type) dbTypeMapping [SqlDbType];
                        }
                }
#endregion

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
		Type sqlType;
		bool typeChanged;
		bool sourceColumnNullMapping;
		string xmlSchemaCollectionDatabase = String.Empty;
		string xmlSchemaCollectionOwningSchema = String.Empty;
		string xmlSchemaCollectionName = String.Empty;

		static Hashtable type_mapping;

		#endregion // Fields

		#region Constructors

		
		static SqlParameter ()
		{
			if (DbTypeMapping == null)
				DbTypeMapping = new Hashtable ();
			
			DbTypeMapping.Add (SqlDbType.BigInt, typeof (long));
			DbTypeMapping.Add (SqlDbType.Bit, typeof (bool));
			DbTypeMapping.Add (SqlDbType.Char, typeof (string));
			DbTypeMapping.Add (SqlDbType.NChar, typeof (string));
			DbTypeMapping.Add (SqlDbType.Text, typeof (string));
			DbTypeMapping.Add (SqlDbType.NText, typeof (string));
			DbTypeMapping.Add (SqlDbType.VarChar, typeof (string));
			DbTypeMapping.Add (SqlDbType.NVarChar, typeof (string));
			DbTypeMapping.Add (SqlDbType.SmallDateTime, typeof (DateTime));
			DbTypeMapping.Add (SqlDbType.DateTime, typeof (DateTime));
			DbTypeMapping.Add (SqlDbType.DateTime2, typeof (DateTime));
			DbTypeMapping.Add (SqlDbType.DateTimeOffset, typeof (DateTimeOffset));
			DbTypeMapping.Add (SqlDbType.Decimal, typeof (decimal));
			DbTypeMapping.Add (SqlDbType.Float, typeof (double));
			DbTypeMapping.Add (SqlDbType.Binary, typeof (byte []));
			DbTypeMapping.Add (SqlDbType.Image, typeof (byte []));
			DbTypeMapping.Add (SqlDbType.Money, typeof (decimal));
			DbTypeMapping.Add (SqlDbType.SmallMoney, typeof (decimal));
			DbTypeMapping.Add (SqlDbType.VarBinary, typeof (byte []));
			DbTypeMapping.Add (SqlDbType.TinyInt, typeof (byte));
			DbTypeMapping.Add (SqlDbType.Int, typeof (int));
			DbTypeMapping.Add (SqlDbType.Real, typeof (float));
			DbTypeMapping.Add (SqlDbType.SmallInt, typeof (short));
			DbTypeMapping.Add (SqlDbType.UniqueIdentifier, typeof (Guid));
			DbTypeMapping.Add (SqlDbType.Variant, typeof (object));
			DbTypeMapping.Add (SqlDbType.Xml, typeof (string));

			type_mapping = new Hashtable ();

			type_mapping.Add (typeof (long), SqlDbType.BigInt);
			type_mapping.Add (typeof (SqlTypes.SqlInt64), SqlDbType.BigInt);

			type_mapping.Add (typeof (bool), SqlDbType.Bit);
			type_mapping.Add (typeof (SqlTypes.SqlBoolean), SqlDbType.Bit);

			type_mapping.Add (typeof (char), SqlDbType.NVarChar);
			type_mapping.Add (typeof (char []), SqlDbType.NVarChar);
			type_mapping.Add (typeof (SqlTypes.SqlChars), SqlDbType.NVarChar);

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

			type_mapping.Add (typeof (SqlTypes.SqlBytes), SqlDbType.VarBinary);

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

			type_mapping.Add (typeof (XmlReader), SqlDbType.Xml);
			type_mapping.Add (typeof (SqlTypes.SqlXml), SqlDbType.Xml);

			type_mapping.Add (typeof (object), SqlDbType.Variant);
			type_mapping.Add (typeof (DateTimeOffset), SqlDbType.DateTimeOffset);
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
			metaParameter.RawValue =  value;
			SqlDbType = dbType;
			Direction = direction;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
		}

		public SqlParameter (string parameterName, SqlDbType dbType, int size, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, Object value, string xmlSchemaCollectionDatabase, string xmlSchemaCollectionOwningSchema, string xmlSchemaCollectionName)
			: this (parameterName, dbType, size, direction, false, precision, scale, sourceColumn, sourceVersion, value)
		{
			XmlSchemaCollectionDatabase = xmlSchemaCollectionDatabase;
			XmlSchemaCollectionOwningSchema = xmlSchemaCollectionOwningSchema;
			XmlSchemaCollectionName = xmlSchemaCollectionName;
			SourceColumnNullMapping = sourceColumnNullMapping;
		}

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

			SqlDbType = (SqlDbType) FrameworkDbTypeFromName ((string) dbValues [16]);

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
	
		public override DbType DbType {
			get { return dbType; }
			set {
				SetDbType (value);
				typeChanged = true;
				isTypeSet = true;
			}
		}

		[RefreshProperties (RefreshProperties.All)]
		public override
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

		public override bool IsNullable {
			get { return metaParameter.IsNullable; }
			set { metaParameter.IsNullable = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public int Offset {
			get { return offset; }
			set { offset = value; }
		}
	
		public override string ParameterName {
			get { return metaParameter.ParameterName; }
			set {
				if (value == null)
					value = string.Empty;
				metaParameter.ParameterName = value;
			}
		}

		[DefaultValue (0)]
		public byte Precision {
			get { return metaParameter.Precision; }
			set { metaParameter.Precision = value; }
		}

		[DefaultValue (0)]
		public byte Scale {
			get { return metaParameter.Scale; }
			set { metaParameter.Scale = value; }
		}

		public override int Size {
			get { return metaParameter.Size; }
			set { metaParameter.Size = value; }
		}

		public override string SourceColumn {
			get {
				if (sourceColumn == null)
					return string.Empty;
				return sourceColumn;
			}
			set { sourceColumn = value; }
		}

		public override DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}
		
		[RefreshProperties (RefreshProperties.All)]
		[DbProviderSpecificTypeProperty(true)]
		public SqlDbType SqlDbType {
			get { return sqlDbType; }
			set {
				SetSqlDbType (value);
				typeChanged = true;
				isTypeSet = true;
			}
		}

		[TypeConverterAttribute (typeof (StringConverter))]
		[RefreshProperties (RefreshProperties.All)]
		public override object Value {
			get {
				if (sqlType != null)
					return GetSqlValue (metaParameter.RawValue);
				return metaParameter.RawValue;
			}
			set {
				if (!isTypeSet) {
					InferSqlType (value);
				}

				if (value is INullable) {
					sqlType = value.GetType ();
					value = SqlTypeToFrameworkType (value);
				}
				metaParameter.RawValue = value;
			}
		}

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
			get {
				return GetSqlValue (metaParameter.RawValue);
			}
			set {
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

		[BrowsableAttribute(false)]
		public string UdtTypeName { get; set; }

		[BrowsableAttribute(false)]
		public string TypeName { get; set; }

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

/*
		// Returns System.Type corresponding to the underlying SqlDbType
		internal override Type SystemType {
			get {
				return (Type) DbTypeMapping [sqlDbType];
			}
		}

		internal override object FrameworkDbType {
			get {
				return sqlDbType;
			}
			
			set {
				object t;
				try {
					t = (DbType) DbTypeFromName ((string)value);
					SetDbType ((DbType)t);
				} catch (ArgumentException) {
					t = (SqlDbType)FrameworkDbTypeFromName ((string)value);
					SetSqlDbType ((SqlDbType) t);
				}
			}
		}
*/

		DbType DbTypeFromName (string name)
		{
			switch (name.ToLower ()) {
				case "ansistring":
					return DbType.AnsiString;
				case "ansistringfixedlength":
					return DbType.AnsiStringFixedLength;
				case "binary": 
					return DbType.Binary;
				case "boolean":
					return DbType.Boolean;
				case "byte":
					return DbType.Byte;
				case "currency": 
					return DbType.Currency;
				case "date":
					return DbType.Date;
				case "datetime": 
					return DbType.DateTime;
				case "decimal":
					return DbType.Decimal;
				case "double": 
					return DbType.Double;
				case "guid": 
					return DbType.Guid;
				case "int16": 
					return DbType.Int16;
				case "int32": 
					return DbType.Int32;
				case "int64": 
					return DbType.Int64;
				case "object": 
					return DbType.Object;
				case "single": 
					return DbType.Single;
				case "string": 
					return DbType.String;
				case "stringfixedlength": 
					return DbType.StringFixedLength;
				case "time": 
					return DbType.Time;
				case "xml": 
					return DbType.Xml;
				default:
					string exception = String.Format ("No mapping exists from {0} to a known DbType.", name);
					throw new ArgumentException (exception);
			}
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
			case DbType.DateTime2:
				MetaParameter.TypeName = "datetime2";
				sqlDbType = SqlDbType.DateTime2;
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
				// Handle Xml type as string
			case DbType.Xml:
				MetaParameter.TypeName = "xml";
				sqlDbType = SqlDbType.Xml;
				MetaParameter.IsVariableSizeType = true;
				break;
			default:
				string exception = String.Format ("No mapping exists from DbType {0} to a known SqlDbType.", type);
				throw new ArgumentException (exception);
			}
			dbType = type;
		}

		// Used by internal constructor which has a SQL Server typename
		private SqlDbType FrameworkDbTypeFromName (string dbTypeName)
		{
			switch (dbTypeName.ToLower ()) {	
			case "bigint":
				return SqlDbType.BigInt;
			case "binary":
				return SqlDbType.Binary;
			case "bit":
				return SqlDbType.Bit;
			case "char":
				return SqlDbType.Char;
			case "datetime":
				return SqlDbType.DateTime;
			case "decimal":
				return SqlDbType.Decimal;
			case "float":
				return SqlDbType.Float;
			case "image":
				return SqlDbType.Image;
			case "int":
				return SqlDbType.Int;
			case "money":
				return SqlDbType.Money;
			case "nchar":
				return SqlDbType.NChar;
			case "ntext":
				return SqlDbType.NText;
			case "nvarchar":
				return SqlDbType.NVarChar;
			case "real":
				return SqlDbType.Real;
			case "smalldatetime":
				return SqlDbType.SmallDateTime;
			case "smallint":
				return SqlDbType.SmallInt;
			case "smallmoney":
				return SqlDbType.SmallMoney;
			case "text":
				return SqlDbType.Text;
			case "timestamp":
				return SqlDbType.Timestamp;
			case "tinyint":
				return SqlDbType.TinyInt;
			case "uniqueidentifier":
				return SqlDbType.UniqueIdentifier;
			case "varbinary":
				return SqlDbType.VarBinary;
			case "varchar":
				return SqlDbType.VarChar;
			case "sql_variant":
				return SqlDbType.Variant;
			case "xml":
				return SqlDbType.Xml;
			default:
				return SqlDbType.Variant;
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
			case SqlDbType.DateTime2:
				MetaParameter.TypeName = "datetime2";
				dbType = DbType.DateTime2;
				break;
			case SqlDbType.DateTimeOffset:
				MetaParameter.TypeName = "datetimeoffset";
				dbType = DbType.DateTimeOffset;
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
			case SqlDbType.Xml:
				MetaParameter.TypeName = "xml";
				dbType = DbType.Xml;
				MetaParameter.IsVariableSizeType = true;
				break;
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
		
		// TODO: Code copied from SqlDataReader, need a better approach
		object GetSqlValue (object value)
		{		
			if (value == null)
				return value;
			switch (sqlDbType) {
			case SqlDbType.BigInt:
				if (value == DBNull.Value)
					return SqlInt64.Null;
				return (SqlInt64) ((long) value);
			case SqlDbType.Binary:
			case SqlDbType.Image:
			case SqlDbType.VarBinary:
			case SqlDbType.Timestamp:
				if (value == DBNull.Value)
					return SqlBinary.Null;
				return (SqlBinary) (byte[]) value;
			case SqlDbType.Bit:
				if (value == DBNull.Value)
					return SqlBoolean.Null;
				return (SqlBoolean) ((bool) value);
			case SqlDbType.Char:
			case SqlDbType.NChar:
			case SqlDbType.NText:
			case SqlDbType.NVarChar:
			case SqlDbType.Text:
			case SqlDbType.VarChar:
				if (value == DBNull.Value)
					return SqlString.Null;

				string str;
				Type type = value.GetType ();
				if (type == typeof (char))
					str = value.ToString ();
				else if (type == typeof (char[]))
					str = new String ((char[])value);
				else
					str = ((string)value);
					return (SqlString) str;
			case SqlDbType.DateTime:
			case SqlDbType.SmallDateTime:
				if (value == DBNull.Value)
					return SqlDateTime.Null;
				return (SqlDateTime) ((DateTime) value);
			case SqlDbType.Decimal:
				if (value == DBNull.Value)
					return SqlDecimal.Null;
				if (value is TdsBigDecimal)
					return SqlDecimalExtensions.FromTdsBigDecimal ((TdsBigDecimal) value);
				return (SqlDecimal) ((decimal) value);
			case SqlDbType.Float:
				if (value == DBNull.Value)
					return SqlDouble.Null;
				return (SqlDouble) ((double) value);
			case SqlDbType.Int:
				if (value == DBNull.Value)
					return SqlInt32.Null;
				return (SqlInt32) ((int) value);
			case SqlDbType.Money:
			case SqlDbType.SmallMoney:
				if (value == DBNull.Value)
					return SqlMoney.Null;
				return (SqlMoney) ((decimal) value);
			case SqlDbType.Real:
				if (value == DBNull.Value)
					return SqlSingle.Null;
				return (SqlSingle) ((float) value);
			case SqlDbType.UniqueIdentifier:
				if (value == DBNull.Value)
					return SqlGuid.Null;
				return (SqlGuid) ((Guid) value);
			case SqlDbType.SmallInt:
				if (value == DBNull.Value)
					return SqlInt16.Null;
				return (SqlInt16) ((short) value);
			case SqlDbType.TinyInt:
				if (value == DBNull.Value)
					return SqlByte.Null;
				return (SqlByte) ((byte) value);
			case SqlDbType.Xml:
				if (value == DBNull.Value)
					return SqlXml.Null;
				return (SqlXml) value;
			default:
				throw new NotImplementedException ("Type '" + sqlDbType + "' not implemented.");
			}
		}
		
		object SqlTypeToFrameworkType (object value)
		{
			INullable nullable = value as INullable;
			if (nullable == null)
				return ConvertToFrameworkType (value);

			if (nullable.IsNull)
				return DBNull.Value;

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
			
			if (typeof (SqlBytes) == type) {
				return ((SqlBytes) value).Value;
			}

			if (typeof (SqlChars) == type) {
				return ((SqlChars) value).Value;
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

			if (typeof (SqlSingle) == type) {
				return ((SqlSingle) value).Value;
			}

			return value;
		}

		internal object ConvertToFrameworkType (object value)
		{
			if (value == null || value == DBNull.Value)
				return value;
			if (sqlDbType == SqlDbType.Variant)
				return metaParameter.Value;

			Type frameworkType = SystemType;
			if (frameworkType == null)
				throw new NotImplementedException ("Type Not Supported : " + sqlDbType.ToString());

			Type valueType = value.GetType ();
			if (valueType == frameworkType)
				return value;

			object sqlvalue = null;

			try {
				sqlvalue = ConvertToFrameworkType (value, frameworkType);
			} catch (FormatException ex) {
				throw new FormatException (string.Format (CultureInfo.InvariantCulture,
					"Parameter value could not be converted from {0} to {1}.",
					valueType.Name, frameworkType.Name), ex);
			}

			return sqlvalue;
		}

		object ConvertToFrameworkType (object value, Type frameworkType)
		{
			if (frameworkType == typeof (string)) {
				if (value is DateTime)
					return ((DateTime) value).ToString ("yyyy-MM-dd'T'HH':'mm':'ss.fffffff");
				if (value is DateTimeOffset)
					return ((DateTimeOffset) value).ToString ("yyyy-MM-dd'T'HH':'mm':'ss.fffffffzzz");
			}

			object sqlvalue = Convert.ChangeType (value, frameworkType);
			switch (sqlDbType) {
			case SqlDbType.Money:
			case SqlDbType.SmallMoney:
				sqlvalue = Decimal.Round ((decimal) sqlvalue, 4);
				break;
			}
			return sqlvalue;
		}

		public override void ResetDbType ()
		{
			InferSqlType (Value);
		}

		public void ResetSqlDbType ()
		{
			InferSqlType (Value);
		}

		#endregion // Methods
	}
}
