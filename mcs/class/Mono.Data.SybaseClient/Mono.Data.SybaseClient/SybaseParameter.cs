//
// Mono.Data.SybaseClient.SybaseParameter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.Tds.Protocol;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		SybaseParameterCollection container = null;
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
		SybaseType sqlDbType;
		string sourceColumn;
		DataRowVersion sourceVersion;
		string typeName;

		#endregion // Fields

		#region Constructors

		public SybaseParameter () 
			: this (String.Empty, SybaseType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public SybaseParameter (string parameterName, object value) 
		{
			this.parameterName = parameterName;
			this.objValue = value;
			this.sourceVersion = DataRowVersion.Current;
			InferSqlType (value);
		}
		
		public SybaseParameter (string parameterName, SybaseType dbType) 
			: this (parameterName, dbType, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public SybaseParameter (string parameterName, SybaseType dbType, int size) 
			: this (parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}
		
		public SybaseParameter (string parameterName, SybaseType dbType, int size, string sourceColumn) 
			: this (parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, sourceColumn, DataRowVersion.Current, null)
		{
		}
		
		public SybaseParameter (string parameterName, SybaseType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) 
		{
			SybaseType = dbType;
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
		// SybaseParameter.  The value array comes from sp_procedure_params_rowset.
		// This is in SybaseCommand.DeriveParameters.
		internal SybaseParameter (object[] dbValues)
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
		internal SybaseParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

		public DbType DbType {
			get { return dbType; }
			set { 
				SetDbType (value); 
				isTypeSet = true;
			}
		}

		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		string IDataParameter.ParameterName {
			get { return parameterName; }
			set { parameterName = value; }
		}

		public bool IsNullable	{
			get { return isNullable; }
			set { isNullable = value; }
		}

		public int Offset {
			get { return offset; }
			set { offset = value; }
		}
		
		public string ParameterName {
			get { return parameterName; }
			set { parameterName = value; }
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
			set { 
				size = value; 
				isSizeSet = true;
			}
		}

		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		public DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}
		
		public SybaseType SybaseType {
			get { return sqlDbType; }
			set { 
				SetSybaseType (value); 
				isTypeSet = true;
			}
		}

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
			return new SybaseParameter (ParameterName, SybaseType, Size, Direction, IsNullable, Precision, Scale, SourceColumn, SourceVersion, Value);
		}

		// If the value is set without the DbType/SybaseType being set, then we
		// infer type information.
		private void InferSqlType (object value)
		{
			Type type = value.GetType ();

			string exception = String.Format ("The parameter data type of {0} is invalid.", type.Name);

			switch (type.FullName) {
			case "System.Int64":
				SetSybaseType (SybaseType.BigInt);
				break;
			case "System.Boolean":
				SetSybaseType (SybaseType.Bit);
				break;
			case "System.String":
				SetSybaseType (SybaseType.NVarChar);
				break;
			case "System.DateTime":
				SetSybaseType (SybaseType.DateTime);
				break;
			case "System.Decimal":
				SetSybaseType (SybaseType.Decimal);
				break;
			case "System.Double":
				SetSybaseType (SybaseType.Float);
				break;
			case "System.Byte[]":
				SetSybaseType (SybaseType.VarBinary);
				break;
			case "System.Byte":
				SetSybaseType (SybaseType.TinyInt);
				break;
			case "System.Int32":
				SetSybaseType (SybaseType.Int);
				break;
			case "System.Single":
				SetSybaseType (SybaseType.Real);
				break;
			case "System.Int16":
				SetSybaseType (SybaseType.SmallInt);
				break;
			case "System.Guid":
				SetSybaseType (SybaseType.UniqueIdentifier);
				break;
			case "System.Object":
				SetSybaseType (SybaseType.Variant);
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
			case SybaseType.VarBinary :
			case SybaseType.NVarChar :
			case SybaseType.VarChar :
				if (!isSizeSet || size == 0)
					throw new InvalidOperationException ("All variable length parameters must have an explicitly set non-zero size.");
				result.Append (String.Format ("({0})", size));
				break;
			case SybaseType.NChar :
			case SybaseType.Char :
			case SybaseType.Binary :
				if (size > 0) 
					result.Append (String.Format ("({0})", size));
				break;
			case SybaseType.Decimal :
				result.Append (String.Format ("({0},{1})", precision, scale));
				break;
                        default:
                                break;
                        }

                        return result.ToString ();
		}

		// When the DbType is set, we also set the SybaseType, as well as the SQL Server
		// string representation of the type name.  If the DbType is not convertible
		// to an SybaseType, throw an exception.
		private void SetDbType (DbType type)
		{
			string exception = String.Format ("No mapping exists from DbType {0} to a known SybaseType.", type);

			switch (type) {
			case DbType.AnsiString:
				typeName = "varchar";
				sqlDbType = SybaseType.VarChar;
				break;
			case DbType.AnsiStringFixedLength:
				typeName = "char";
				sqlDbType = SybaseType.Char;
				break;
			case DbType.Binary:
				typeName = "varbinary";
				sqlDbType = SybaseType.VarBinary;
				break;
			case DbType.Boolean:
				typeName = "bit";
				sqlDbType = SybaseType.Bit;
				break;
			case DbType.Byte:
				typeName = "tinyint";
				sqlDbType = SybaseType.TinyInt;
				break;
			case DbType.Currency:
				sqlDbType = SybaseType.Money;
				typeName = "money";
				break;
			case DbType.Date:
			case DbType.DateTime:
				typeName = "datetime";
				sqlDbType = SybaseType.DateTime;
				break;
			case DbType.Decimal:
				typeName = "decimal";
				sqlDbType = SybaseType.Decimal;
				break;
			case DbType.Double:
				typeName = "float";
				sqlDbType = SybaseType.Float;
				break;
			case DbType.Guid:
				typeName = "uniqueidentifier";
				sqlDbType = SybaseType.UniqueIdentifier;
				break;
			case DbType.Int16:
				typeName = "smallint";
				sqlDbType = SybaseType.SmallInt;
				break;
			case DbType.Int32:
				typeName = "int";
				sqlDbType = SybaseType.Int;
				break;
			case DbType.Int64:
				typeName = "bigint";
				sqlDbType = SybaseType.BigInt;
				break;
			case DbType.Object:
				typeName = "sql_variant";
				sqlDbType = SybaseType.Variant;
				break;
			case DbType.Single:
				typeName = "real";
				sqlDbType = SybaseType.Real;
				break;
			case DbType.String:
				typeName = "nvarchar";
				sqlDbType = SybaseType.NVarChar;
				break;
			case DbType.StringFixedLength:
				typeName = "nchar";
				sqlDbType = SybaseType.NChar;
				break;
			case DbType.Time:
				typeName = "datetime";
				sqlDbType = SybaseType.DateTime;
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
				SybaseType = SybaseType.BigInt;
				break;
			case "binary":
				SybaseType = SybaseType.Binary;
				break;
			case "bit":
				SybaseType = SybaseType.Bit;
				break;
			case "char":
				SybaseType = SybaseType.Char;
				break;
			case "datetime":
				SybaseType = SybaseType.DateTime;
				break;
			case "decimal":
				SybaseType = SybaseType.Decimal;
				break;
			case "float":
				SybaseType = SybaseType.Float;
				break;
			case "image":
				SybaseType = SybaseType.Image;
				break;
			case "int":
				SybaseType = SybaseType.Int;
				break;
			case "money":
				SybaseType = SybaseType.Money;
				break;
			case "nchar":
				SybaseType = SybaseType.NChar;
				break;
			case "ntext":
				SybaseType = SybaseType.NText;
				break;
			case "nvarchar":
				SybaseType = SybaseType.NVarChar;
				break;
			case "real":
				SybaseType = SybaseType.Real;
				break;
			case "smalldatetime":
				SybaseType = SybaseType.SmallDateTime;
				break;
			case "smallint":
				SybaseType = SybaseType.SmallInt;
				break;
			case "smallmoney":
				SybaseType = SybaseType.SmallMoney;
				break;
			case "text":
				SybaseType = SybaseType.Text;
				break;
			case "timestamp":
				SybaseType = SybaseType.Timestamp;
				break;
			case "tinyint":
				SybaseType = SybaseType.TinyInt;
				break;
			case "uniqueidentifier":
				SybaseType = SybaseType.UniqueIdentifier;
				break;
			case "varbinary":
				SybaseType = SybaseType.VarBinary;
				break;
			case "varchar":
				SybaseType = SybaseType.VarChar;
				break;
			default:
				SybaseType = SybaseType.Variant;
				break;
			}
		}

		// When the SybaseType is set, we also set the DbType, as well as the SQL Server
		// string representation of the type name.  If the SybaseType is not convertible
		// to a DbType, throw an exception.
		private void SetSybaseType (SybaseType type)
		{
			string exception = String.Format ("No mapping exists from SybaseType {0} to a known DbType.", type);

			switch (type) {
			case SybaseType.BigInt:
				typeName = "bigint";
				dbType = DbType.Int64;
				break;
			case SybaseType.Binary:
				typeName = "binary";
				dbType = DbType.Binary;
				break;
			case SybaseType.Timestamp:
				typeName = "timestamp";
				dbType = DbType.Binary;
				break;
			case SybaseType.VarBinary:
				typeName = "varbinary";
				dbType = DbType.Binary;
				break;
			case SybaseType.Bit:
				typeName = "bit";
				dbType = DbType.Boolean;
				break;
			case SybaseType.Char:
				typeName = "char";
				dbType = DbType.AnsiStringFixedLength;
				break;
			case SybaseType.DateTime:
				typeName = "datetime";
				dbType = DbType.DateTime;
				break;
			case SybaseType.SmallDateTime:
				typeName = "smalldatetime";
				dbType = DbType.DateTime;
				break;
			case SybaseType.Decimal:
				typeName = "decimal";
				dbType = DbType.Decimal;
				break;
			case SybaseType.Float:
				typeName = "float";
				dbType = DbType.Double;
				break;
			case SybaseType.Image:
				typeName = "image";
				dbType = DbType.Binary;
				break;
			case SybaseType.Int:
				typeName = "int";
				dbType = DbType.Int32;
				break;
			case SybaseType.Money:
				typeName = "money";
				dbType = DbType.Currency;
				break;
			case SybaseType.SmallMoney:
				typeName = "smallmoney";
				dbType = DbType.Currency;
				break;
			case SybaseType.NChar:
				typeName = "nchar";
				dbType = DbType.StringFixedLength;
				break;
			case SybaseType.NText:
				typeName = "ntext";
				dbType = DbType.String;
				break;
			case SybaseType.NVarChar:
				typeName = "nvarchar";
				dbType = DbType.String;
				break;
			case SybaseType.Real:
				typeName = "real";
				dbType = DbType.Single;
				break;
			case SybaseType.SmallInt:
				typeName = "smallint";
				dbType = DbType.Int16;
				break;
			case SybaseType.Text:
				typeName = "text";
				dbType = DbType.AnsiString;
				break;
			case SybaseType.VarChar:
				typeName = "varchar";
				dbType = DbType.AnsiString;
				break;
			case SybaseType.TinyInt:
				typeName = "tinyint";
				dbType = DbType.Byte;
				break;
			case SybaseType.UniqueIdentifier:
				typeName = "uniqueidentifier";
				dbType = DbType.Guid;
				break;
			case SybaseType.Variant:
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
