//
// System.Data.Odbc.OdbcParameter
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	public sealed class OdbcParameter : MarshalByRefObject, IDbDataParameter, 
IDataParameter, ICloneable
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
		OdbcType odbcType;
		DbType dbType;

		int IntValue;

		#endregion

		#region Constructors

		public OdbcParameter ()
		{
			name = String.Empty;
			value = null;
			size = 0;
			isNullable = true;
			precision = 0;
			scale = 0;
			sourceColumn = String.Empty;
		}

		public OdbcParameter (string name, object value)
			: this ()
		{
			this.name = name;
			this.value = value;
		}

		public OdbcParameter (string name, OdbcType dataType)
			: this ()
		{
			this.name = name;
			OdbcType = dataType;
		}

		public OdbcParameter (string name, OdbcType dataType, int size)
			: this (name, dataType)
		{
			this.size = size;
		}

		public OdbcParameter (string name, OdbcType dataType, int size, string 
srcColumn)
			: this (name, dataType, size)
		{
			this.sourceColumn = srcColumn;
		}

		public OdbcParameter(string name, OdbcType dataType, int size, 
ParameterDirection direction, bool isNullable, byte precision, byte scale, 
string srcColumn, DataRowVersion srcVersion, object value)
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
			}
		}

		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		public bool IsNullable {
			get { return isNullable; }
		}

		public OdbcType OdbcType {
			get { return odbcType; }
			set {
				odbcType = value;
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
			get {
				return IntValue;
			}
			set { this.IntValue =(int) value; }
		}

		#endregion // Properties

		#region Internal Properties

		internal void Bind(int hstmt,int ParamNum)
		{
			if (OdbcType==OdbcType.Integer)
			{
				OdbcReturn ret=libodbc.SQLBindParam(hstmt, Convert.ToInt16(ParamNum), 4, 
4, 0,0,ref IntValue, 0);
				libodbc.DisplayError("SQLBindParam",ret);

			}
			else Console.WriteLine("Unknown Paramter Type");

		}

		#endregion // Internal Properties

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
		#endregion
	}
}

