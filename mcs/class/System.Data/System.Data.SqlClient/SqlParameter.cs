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

		[MonoTODO]
		public SqlParameter () 
		{
		}

		[MonoTODO]
		public SqlParameter (string parameterName, object value) 
		{
			this.parmName = parameterName;
			this.objValue = value;
		}
		
		[MonoTODO]
		public SqlParameter (string parameterName, SqlDbType dbType) 
		{
			this.parmName = parameterName;
			this.dbtype = dbType;
		}

		[MonoTODO]
		public SqlParameter (string parameterName, SqlDbType dbType, int size) 
		{

			this.parmName = parameterName;
			this.dbtype = dbType;
			this.size = size;
		}
		
		[MonoTODO]
		public SqlParameter(string parameterName, SqlDbType dbType, int size, string sourceColumn) 
		{

			this.parmName = parameterName;
			this.dbtype = dbType;
			this.size = size;
			this.sourceColumn = sourceColumn;
		}
			 
		[MonoTODO]
		public SqlParameter(string parameterName, SqlDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) 
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

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public DbType DbType {
			get { return theDbType; }
			set { theDbType = value; }
		}

		[MonoTODO]
		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		[MonoTODO]
		public bool IsNullable	{
			get { return isNullable; }
		}

		[MonoTODO]
		public int Offset {
			get { return offset; }
			set { offset = value; }
		}

		
		string IDataParameter.ParameterName {
			get { return parmName; }
			set { parmName = value; }
		}
		
		public string ParameterName {
			get { return parmName; }
			set { parmName = value; }
		}

		[MonoTODO]
		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		[MonoTODO]
		public DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}
		
		[MonoTODO]
		public SqlDbType SqlDbType {
			get { return dbtype; }
			set { dbtype = value; }
		}

		[MonoTODO]
		public object Value {
			get { return objValue; }
			set { objValue = value; }
		}

		[MonoTODO]
		public byte Precision {
			get { return precision; }
			set { precision = value; }
		}

		[MonoTODO]
                public byte Scale {
			get { return scale; }
			set { scale = value; }
		}

		[MonoTODO]
                public int Size {
			get { return size; }
			set { 
				sizeSet = true;
				size = value; 
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		internal string Prepare ()
		{
			StringBuilder result = new StringBuilder ();
			result.Append (parmName);
			result.Append (" ");
			result.Append (dbtype.ToString ());

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

		public override string ToString() 
		{
			return parmName;
		}

		#endregion // Methods
	}
}
