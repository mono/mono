//
// Mono.Data.SybaseClient.SybaseParameter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (monodanmorg@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Daniel Morgan, 2008
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

namespace Mono.Data.SybaseClient {
#if NET_2_0
	public sealed class SybaseParameter : DbParameter, IDbDataParameter, IDataParameter, ICloneable
#else
	public sealed class SybaseParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
#endif // NET_2_0
	{
		#region Fields

		TdsMetaParameter metaParameter;

		SybaseParameterCollection container = null;
		DbType dbType;
		ParameterDirection direction = ParameterDirection.Input;
		bool isNullable;
		bool isSizeSet = false;
		bool isTypeSet = false;
		int offset;
		SybaseType sybaseType;
		string sourceColumn;
		DataRowVersion sourceVersion;
#if NET_2_0
		bool sourceColumnNullMapping;
#endif

		#endregion // Fields

		#region Constructors

		public SybaseParameter () 
			: this (String.Empty, SybaseType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public SybaseParameter (string parameterName, object value) 
		{
			metaParameter = new TdsMetaParameter (parameterName, value);
			this.sourceVersion = DataRowVersion.Current;
			InferSybaseType (value);
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
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]	 
		public SybaseParameter (string parameterName, SybaseType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) 
		{
			metaParameter = new TdsMetaParameter (parameterName, size, isNullable, precision, scale, value);

			SybaseType = dbType;
			Direction = direction;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
		}

		// This constructor is used internally to construct a
		// SybaseParameter.  The value array comes from sp_procedure_params_rowset.
		// This is in SybaseCommand.DeriveParameters.
		internal SybaseParameter (object[] dbValues)
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
		internal SybaseParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

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

		public 
#if NET_2_0
		override
#endif // NET_2_0
		ParameterDirection Direction {
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

		public 
#if NET_2_0
		override
#endif // NET_2_0
		bool IsNullable	{
			get { return metaParameter.IsNullable; }
			set { metaParameter.IsNullable = value; }
		}

		public int Offset {
			get { return offset; }
			set { offset = value; }
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0		
		string ParameterName {
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

		public 
#if NET_2_0
		override
#endif // NET_2_0
                int Size {
			get { return metaParameter.Size; }
			set { metaParameter.Size = value; }
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

#if NET_2_0
		[DbProviderSpecificTypeProperty(true)]
#endif		
		public SybaseType SybaseType {
			get { return sybaseType; }
			set { 
				SetSybaseType (value); 
				isTypeSet = true;
			}
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		object Value {
			get { return metaParameter.Value; }
			set { 
				if (!isTypeSet)
					InferSybaseType (value);
				metaParameter.Value = value; 
			}
		}

#if NET_2_0
		public override bool SourceColumnNullMapping {
			get { return sourceColumnNullMapping; }
			set { sourceColumnNullMapping = value; }
		}
#endif

		#endregion // Properties

		#region Methods

		object ICloneable.Clone ()
		{
			return new SybaseParameter (ParameterName, SybaseType, Size, Direction, IsNullable, Precision, Scale, SourceColumn, SourceVersion, Value);
		}

		// If the value is set without the DbType/SybaseType being set, then we
		// infer type information.
		private void InferSybaseType (object value)
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

		// When the DbType is set, we also set the SybaseType, as well as the SQL Server
		// string representation of the type name.  If the DbType is not convertible
		// to an SybaseType, throw an exception.
		private void SetDbType (DbType type)
		{
			string exception = String.Format ("No mapping exists from DbType {0} to a known SybaseType.", type);

			switch (type) {
			case DbType.AnsiString:
				MetaParameter.TypeName = "varchar";
				sybaseType = SybaseType.VarChar;
				break;
			case DbType.AnsiStringFixedLength:
				MetaParameter.TypeName = "char";
				sybaseType = SybaseType.Char;
				break;
			case DbType.Binary:
				MetaParameter.TypeName = "varbinary";
				sybaseType = SybaseType.VarBinary;
				break;
			case DbType.Boolean:
				MetaParameter.TypeName = "bit";
				sybaseType = SybaseType.Bit;
				break;
			case DbType.Byte:
				MetaParameter.TypeName = "tinyint";
				sybaseType = SybaseType.TinyInt;
				break;
			case DbType.Currency:
				sybaseType = SybaseType.Money;
				MetaParameter.TypeName = "money";
				break;
			case DbType.Date:
			case DbType.DateTime:
				MetaParameter.TypeName = "datetime";
				sybaseType = SybaseType.DateTime;
				break;
			case DbType.Decimal:
				MetaParameter.TypeName = "decimal";
				sybaseType = SybaseType.Decimal;
				break;
			case DbType.Double:
				MetaParameter.TypeName = "float";
				sybaseType = SybaseType.Float;
				break;
			case DbType.Guid:
				MetaParameter.TypeName = "uniqueidentifier";
				sybaseType = SybaseType.UniqueIdentifier;
				break;
			case DbType.Int16:
				MetaParameter.TypeName = "smallint";
				sybaseType = SybaseType.SmallInt;
				break;
			case DbType.Int32:
				MetaParameter.TypeName = "int";
				sybaseType = SybaseType.Int;
				break;
			case DbType.Int64:
				MetaParameter.TypeName = "bigint";
				sybaseType = SybaseType.BigInt;
				break;
			case DbType.Object:
				MetaParameter.TypeName = "sql_variant";
				sybaseType = SybaseType.Variant;
				break;
			case DbType.Single:
				MetaParameter.TypeName = "real";
				sybaseType = SybaseType.Real;
				break;
			case DbType.String:
				MetaParameter.TypeName = "nvarchar";
				sybaseType = SybaseType.NVarChar;
				break;
			case DbType.StringFixedLength:
				MetaParameter.TypeName = "nchar";
				sybaseType = SybaseType.NChar;
				break;
			case DbType.Time:
				MetaParameter.TypeName = "datetime";
				sybaseType = SybaseType.DateTime;
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
				MetaParameter.TypeName = "bigint";
				dbType = DbType.Int64;
				break;
			case SybaseType.Binary:
				MetaParameter.TypeName = "binary";
				dbType = DbType.Binary;
				break;
			case SybaseType.Timestamp:
				MetaParameter.TypeName = "timestamp";
				dbType = DbType.Binary;
				break;
			case SybaseType.VarBinary:
				MetaParameter.TypeName = "varbinary";
				dbType = DbType.Binary;
				break;
			case SybaseType.Bit:
				MetaParameter.TypeName = "bit";
				dbType = DbType.Boolean;
				break;
			case SybaseType.Char:
				MetaParameter.TypeName = "char";
				dbType = DbType.AnsiStringFixedLength;
				break;
			case SybaseType.DateTime:
				MetaParameter.TypeName = "datetime";
				dbType = DbType.DateTime;
				break;
			case SybaseType.SmallDateTime:
				MetaParameter.TypeName = "smalldatetime";
				dbType = DbType.DateTime;
				break;
			case SybaseType.Decimal:
				MetaParameter.TypeName = "decimal";
				dbType = DbType.Decimal;
				break;
			case SybaseType.Float:
				MetaParameter.TypeName = "float";
				dbType = DbType.Double;
				break;
			case SybaseType.Image:
				MetaParameter.TypeName = "image";
				dbType = DbType.Binary;
				break;
			case SybaseType.Int:
				MetaParameter.TypeName = "int";
				dbType = DbType.Int32;
				break;
			case SybaseType.Money:
				MetaParameter.TypeName = "money";
				dbType = DbType.Currency;
				break;
			case SybaseType.SmallMoney:
				MetaParameter.TypeName = "smallmoney";
				dbType = DbType.Currency;
				break;
			case SybaseType.NChar:
				MetaParameter.TypeName = "nchar";
				dbType = DbType.StringFixedLength;
				break;
			case SybaseType.NText:
				MetaParameter.TypeName = "ntext";
				dbType = DbType.String;
				break;
			case SybaseType.NVarChar:
				MetaParameter.TypeName = "nvarchar";
				dbType = DbType.String;
				break;
			case SybaseType.Real:
				MetaParameter.TypeName = "real";
				dbType = DbType.Single;
				break;
			case SybaseType.SmallInt:
				MetaParameter.TypeName = "smallint";
				dbType = DbType.Int16;
				break;
			case SybaseType.Text:
				MetaParameter.TypeName = "text";
				dbType = DbType.AnsiString;
				break;
			case SybaseType.VarChar:
				MetaParameter.TypeName = "varchar";
				dbType = DbType.AnsiString;
				break;
			case SybaseType.TinyInt:
				MetaParameter.TypeName = "tinyint";
				dbType = DbType.Byte;
				break;
			case SybaseType.UniqueIdentifier:
				MetaParameter.TypeName = "uniqueidentifier";
				dbType = DbType.Guid;
				break;
			case SybaseType.Variant:
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

#if NET_2_0
		public override void ResetDbType ()
		{
			InferSybaseType (metaParameter.Value);
		}

		public void ResetSybaseDbType ()
		{
			InferSybaseType (metaParameter.Value);
		}
#endif // NET_2_0

		#endregion // Methods
	}
}
