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
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.SqlClient {
	/// <summary>
	/// Represents a parameter to a Command object, and optionally, 
	/// its mapping to DataSet columns; and is implemented by .NET 
	/// data providers that access data sources.
	/// </summary>
	public sealed class SqlParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		string parmName;
		SqlDbType dbtype;
		DbType theDbType;
		object objValue;
		int size;
		string sourceColumn;
		ParameterDirection direction = ParameterDirection.Input;
		bool isNullable;
		byte precision;
		byte scale;
		DataRowVersion sourceVersion;
		int offset;

		bool sizeSet = false;

		#endregion // Fields

		#region Constructors

		public SqlParameter () 
			: this (String.Empty, SqlDbType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public SqlParameter (string parameterName, object value) 
			: this (parameterName, SqlDbType.NVarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, value)
		{
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
			this.parmName = parameterName;
			this.dbtype = dbType;
			this.size = size;
			this.sourceColumn = sourceColumn;
			this.direction = direction;
			this.isNullable = isNullable;
			this.precision = precision;
			this.scale = scale;
			this.sourceVersion = sourceVersion;
			this.objValue = value;
		}

		internal SqlParameter (object[] dbValues)
		{
			precision = 0;
			scale = 0;
			direction = ParameterDirection.Input;

			parmName = (string) dbValues[3];

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

			dbtype = TypeNameToSqlDbType ((string) dbValues[16]);
		}

		#endregion // Constructors

		#region Properties

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The parameter generic type.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[RefreshProperties (RefreshProperties.All)]
		public DbType DbType {
			get { return theDbType; }
			set { theDbType = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Input, output, or bidirectional parameter.")]
		[DefaultValue (ParameterDirection.Input)]
		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		string IDataParameter.ParameterName {
			get { return parmName; }
			set { parmName = value; }
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
			get { return parmName; }
			set { parmName = value; }
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
				sizeSet = true;
				size = value; 
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
			get { return dbtype; }
			set { dbtype = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Value of the parameter.")]
		[DefaultValue (null)]
		public object Value {
			get { return objValue; }
			set { objValue = value; }
		}

		#endregion // Properties

		#region Methods

		object ICloneable.Clone ()
		{
			return new SqlParameter (ParameterName, SqlDbType, Size, Direction, IsNullable, Precision, Scale, SourceColumn, SourceVersion, Value);
		}

		internal string Prepare (string name)
		{
			StringBuilder result = new StringBuilder ();
			result.Append (name);
			result.Append (" ");
			result.Append (dbtype.ToString ().ToLower ());

			switch (dbtype) {
			case SqlDbType.Image :
			case SqlDbType.NVarChar :
			case SqlDbType.VarBinary :
			case SqlDbType.VarChar :
				if (!sizeSet || size == 0)
					throw new InvalidOperationException ("All variable length parameters must have an explicitly set non-zero size.");
				result.Append ("(");
				result.Append (size.ToString ());
				result.Append (")");
				break;
			case SqlDbType.Decimal :
			case SqlDbType.Money :
			case SqlDbType.SmallMoney :
				result.Append ("(");
				result.Append (precision.ToString ());
				result.Append (",");
				result.Append (scale.ToString ());
				result.Append (")");
				break;
                        default:
                                break;
                        }

                        return result.ToString ();
		}

		internal static SqlDbType TypeNameToSqlDbType (string typeName)
		{
			switch (typeName) {
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
			}
			return SqlDbType.Variant;
		}

		public override string ToString() 
		{
			return parmName;
		}

		#endregion // Methods
	}
}
