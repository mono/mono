//
// Mono.Data.TdsClient.TdsParameter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
//

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
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Data.TdsClient {
	public sealed class TdsParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		TdsMetaParameter metaParameter;

		TdsParameterCollection container = null;
		DbType dbType;
		ParameterDirection direction = ParameterDirection.Input;
		bool isNullable;
		bool isSizeSet = false;
		bool isTypeSet = false;
		int offset;
		TdsType sybaseType;
		string sourceColumn;
		DataRowVersion sourceVersion;

		#endregion // Fields

		#region Constructors

		public TdsParameter () 
			: this (String.Empty, TdsType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public TdsParameter (string parameterName, object value) 
		{
			metaParameter = new TdsMetaParameter (parameterName, value);
			this.sourceVersion = DataRowVersion.Current;
			InferTdsType (value);
		}
		
		public TdsParameter (string parameterName, TdsType dbType) 
			: this (parameterName, dbType, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public TdsParameter (string parameterName, TdsType dbType, int size) 
			: this (parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}
		
		public TdsParameter (string parameterName, TdsType dbType, int size, string sourceColumn) 
			: this (parameterName, dbType, size, ParameterDirection.Input, false, 0, 0, sourceColumn, DataRowVersion.Current, null)
		{
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]	 
		public TdsParameter (string parameterName, TdsType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) 
		{
			metaParameter = new TdsMetaParameter (parameterName, size, isNullable, precision, scale, value);

			TdsType = dbType;
			Direction = direction;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
		}

		// This constructor is used internally to construct a
		// TdsParameter.  The value array comes from sp_procedure_params_rowset.
		// This is in TdsCommand.DeriveParameters.
		internal TdsParameter (object[] dbValues)
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
			}

			IsNullable = (bool) dbValues[8];

			if (dbValues[12] != null)
				Precision = (byte) ((short) dbValues[12]);
			if (dbValues[13] != null)
				Scale = (byte) ((short) dbValues[13]);

			SetDbTypeName ((string) dbValues[16]);
		}

		#endregion // Constructors

		#region Properties

		// Used to ensure that only one collection can contain this
		// parameter
		internal TdsParameterCollection Container {
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
			set { 
				direction = value; 
				if (direction == ParameterDirection.Output)
					MetaParameter.Direction = TdsParameterDirection.Output;
			}
		}

		internal TdsMetaParameter MetaParameter {
			get { return metaParameter; }
		}

		string IDataParameter.ParameterName {
			get { return metaParameter.ParameterName; }
			set { metaParameter.ParameterName = value; }
		}

		public bool IsNullable	{
			get { return metaParameter.IsNullable; }
			set { metaParameter.IsNullable = value; }
		}

		public int Offset {
			get { return offset; }
			set { offset = value; }
		}
		
		public string ParameterName {
			get { return metaParameter.ParameterName; }
			set { metaParameter.ParameterName = value; }
		}

		public byte Precision {
			get { return metaParameter.Precision; }
			set { metaParameter.Precision = value; }
		}

                public byte Scale {
			get { return metaParameter.Scale; }
			set { metaParameter.Scale = value; }
		}

                public int Size {
			get { return metaParameter.Size; }
			set { metaParameter.Size = value; }
		}

		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		public DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}
		
		public TdsType TdsType {
			get { return sybaseType; }
			set { 
				SetTdsType (value); 
				isTypeSet = true;
			}
		}

		public object Value {
			get { return metaParameter.Value; }
			set { 
				if (!isTypeSet)
					InferTdsType (value);
				metaParameter.Value = value; 
			}
		}

		#endregion // Properties

		#region Methods

		object ICloneable.Clone ()
		{
			return new TdsParameter (ParameterName, TdsType, Size, Direction, IsNullable, Precision, Scale, SourceColumn, SourceVersion, Value);
		}

		// If the value is set without the DbType/TdsType being set, then we
		// infer type information.
		private void InferTdsType (object value)
		{
			Type type = value.GetType ();

			string exception = String.Format ("The parameter data type of {0} is invalid.", type.Name);

			switch (type.FullName) {
			case "System.Int64":
				SetTdsType (TdsType.BigInt);
				break;
			case "System.Boolean":
				SetTdsType (TdsType.Bit);
				break;
			case "System.String":
				SetTdsType (TdsType.NVarChar);
				break;
			case "System.DateTime":
				SetTdsType (TdsType.DateTime);
				break;
			case "System.Decimal":
				SetTdsType (TdsType.Decimal);
				break;
			case "System.Double":
				SetTdsType (TdsType.Float);
				break;
			case "System.Byte[]":
				SetTdsType (TdsType.VarBinary);
				break;
			case "System.Byte":
				SetTdsType (TdsType.TinyInt);
				break;
			case "System.Int32":
				SetTdsType (TdsType.Int);
				break;
			case "System.Single":
				SetTdsType (TdsType.Real);
				break;
			case "System.Int16":
				SetTdsType (TdsType.SmallInt);
				break;
			case "System.Guid":
				SetTdsType (TdsType.UniqueIdentifier);
				break;
			case "System.Object":
				SetTdsType (TdsType.Variant);
				break;
			default:
				throw new ArgumentException (exception);				
			}
		}

		// When the DbType is set, we also set the TdsType, as well as the SQL Server
		// string representation of the type name.  If the DbType is not convertible
		// to an TdsType, throw an exception.
		private void SetDbType (DbType type)
		{
			string exception = String.Format ("No mapping exists from DbType {0} to a known TdsType.", type);

			switch (type) {
			case DbType.AnsiString:
				MetaParameter.TypeName = "varchar";
				sybaseType = TdsType.VarChar;
				break;
			case DbType.AnsiStringFixedLength:
				MetaParameter.TypeName = "char";
				sybaseType = TdsType.Char;
				break;
			case DbType.Binary:
				MetaParameter.TypeName = "varbinary";
				sybaseType = TdsType.VarBinary;
				break;
			case DbType.Boolean:
				MetaParameter.TypeName = "bit";
				sybaseType = TdsType.Bit;
				break;
			case DbType.Byte:
				MetaParameter.TypeName = "tinyint";
				sybaseType = TdsType.TinyInt;
				break;
			case DbType.Currency:
				sybaseType = TdsType.Money;
				MetaParameter.TypeName = "money";
				break;
			case DbType.Date:
			case DbType.DateTime:
				MetaParameter.TypeName = "datetime";
				sybaseType = TdsType.DateTime;
				break;
			case DbType.Decimal:
				MetaParameter.TypeName = "decimal";
				sybaseType = TdsType.Decimal;
				break;
			case DbType.Double:
				MetaParameter.TypeName = "float";
				sybaseType = TdsType.Float;
				break;
			case DbType.Guid:
				MetaParameter.TypeName = "uniqueidentifier";
				sybaseType = TdsType.UniqueIdentifier;
				break;
			case DbType.Int16:
				MetaParameter.TypeName = "smallint";
				sybaseType = TdsType.SmallInt;
				break;
			case DbType.Int32:
				MetaParameter.TypeName = "int";
				sybaseType = TdsType.Int;
				break;
			case DbType.Int64:
				MetaParameter.TypeName = "bigint";
				sybaseType = TdsType.BigInt;
				break;
			case DbType.Object:
				MetaParameter.TypeName = "sql_variant";
				sybaseType = TdsType.Variant;
				break;
			case DbType.Single:
				MetaParameter.TypeName = "real";
				sybaseType = TdsType.Real;
				break;
			case DbType.String:
				MetaParameter.TypeName = "nvarchar";
				sybaseType = TdsType.NVarChar;
				break;
			case DbType.StringFixedLength:
				MetaParameter.TypeName = "nchar";
				sybaseType = TdsType.NChar;
				break;
			case DbType.Time:
				MetaParameter.TypeName = "datetime";
				sybaseType = TdsType.DateTime;
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
				TdsType = TdsType.BigInt;
				break;
			case "binary":
				TdsType = TdsType.Binary;
				break;
			case "bit":
				TdsType = TdsType.Bit;
				break;
			case "char":
				TdsType = TdsType.Char;
				break;
			case "datetime":
				TdsType = TdsType.DateTime;
				break;
			case "decimal":
				TdsType = TdsType.Decimal;
				break;
			case "float":
				TdsType = TdsType.Float;
				break;
			case "image":
				TdsType = TdsType.Image;
				break;
			case "int":
				TdsType = TdsType.Int;
				break;
			case "money":
				TdsType = TdsType.Money;
				break;
			case "nchar":
				TdsType = TdsType.NChar;
				break;
			case "ntext":
				TdsType = TdsType.NText;
				break;
			case "nvarchar":
				TdsType = TdsType.NVarChar;
				break;
			case "real":
				TdsType = TdsType.Real;
				break;
			case "smalldatetime":
				TdsType = TdsType.SmallDateTime;
				break;
			case "smallint":
				TdsType = TdsType.SmallInt;
				break;
			case "smallmoney":
				TdsType = TdsType.SmallMoney;
				break;
			case "text":
				TdsType = TdsType.Text;
				break;
			case "timestamp":
				TdsType = TdsType.Timestamp;
				break;
			case "tinyint":
				TdsType = TdsType.TinyInt;
				break;
			case "uniqueidentifier":
				TdsType = TdsType.UniqueIdentifier;
				break;
			case "varbinary":
				TdsType = TdsType.VarBinary;
				break;
			case "varchar":
				TdsType = TdsType.VarChar;
				break;
			default:
				TdsType = TdsType.Variant;
				break;
			}
		}

		// When the TdsType is set, we also set the DbType, as well as the SQL Server
		// string representation of the type name.  If the TdsType is not convertible
		// to a DbType, throw an exception.
		private void SetTdsType (TdsType type)
		{
			string exception = String.Format ("No mapping exists from TdsType {0} to a known DbType.", type);

			switch (type) {
			case TdsType.BigInt:
				MetaParameter.TypeName = "bigint";
				dbType = DbType.Int64;
				break;
			case TdsType.Binary:
				MetaParameter.TypeName = "binary";
				dbType = DbType.Binary;
				break;
			case TdsType.Timestamp:
				MetaParameter.TypeName = "timestamp";
				dbType = DbType.Binary;
				break;
			case TdsType.VarBinary:
				MetaParameter.TypeName = "varbinary";
				dbType = DbType.Binary;
				break;
			case TdsType.Bit:
				MetaParameter.TypeName = "bit";
				dbType = DbType.Boolean;
				break;
			case TdsType.Char:
				MetaParameter.TypeName = "char";
				dbType = DbType.AnsiStringFixedLength;
				break;
			case TdsType.DateTime:
				MetaParameter.TypeName = "datetime";
				dbType = DbType.DateTime;
				break;
			case TdsType.SmallDateTime:
				MetaParameter.TypeName = "smalldatetime";
				dbType = DbType.DateTime;
				break;
			case TdsType.Decimal:
				MetaParameter.TypeName = "decimal";
				dbType = DbType.Decimal;
				break;
			case TdsType.Float:
				MetaParameter.TypeName = "float";
				dbType = DbType.Double;
				break;
			case TdsType.Image:
				MetaParameter.TypeName = "image";
				dbType = DbType.Binary;
				break;
			case TdsType.Int:
				MetaParameter.TypeName = "int";
				dbType = DbType.Int32;
				break;
			case TdsType.Money:
				MetaParameter.TypeName = "money";
				dbType = DbType.Currency;
				break;
			case TdsType.SmallMoney:
				MetaParameter.TypeName = "smallmoney";
				dbType = DbType.Currency;
				break;
			case TdsType.NChar:
				MetaParameter.TypeName = "nchar";
				dbType = DbType.StringFixedLength;
				break;
			case TdsType.NText:
				MetaParameter.TypeName = "ntext";
				dbType = DbType.String;
				break;
			case TdsType.NVarChar:
				MetaParameter.TypeName = "nvarchar";
				dbType = DbType.String;
				break;
			case TdsType.Real:
				MetaParameter.TypeName = "real";
				dbType = DbType.Single;
				break;
			case TdsType.SmallInt:
				MetaParameter.TypeName = "smallint";
				dbType = DbType.Int16;
				break;
			case TdsType.Text:
				MetaParameter.TypeName = "text";
				dbType = DbType.AnsiString;
				break;
			case TdsType.VarChar:
				MetaParameter.TypeName = "varchar";
				dbType = DbType.AnsiString;
				break;
			case TdsType.TinyInt:
				MetaParameter.TypeName = "tinyint";
				dbType = DbType.Byte;
				break;
			case TdsType.UniqueIdentifier:
				MetaParameter.TypeName = "uniqueidentifier";
				dbType = DbType.Guid;
				break;
			case TdsType.Variant:
				MetaParameter.TypeName = "sql_variant";
				dbType = DbType.Object;
				break;
			default:
				throw new ArgumentException (exception);
			}
			sybaseType = type;
		}

		public override string ToString() 
		{
			return ParameterName;
		}

		#endregion // Methods
	}
}
