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
	public sealed class OdbcParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		string name;
		object ParamValue;
		int size;
		bool isNullable;
		byte precision;
		byte scale;
		DataRowVersion sourceVersion;
		string sourceColumn;
		ParameterDirection direction;
		OdbcType odbcType;
		DbType dbType;

		// Buffers for parameter value based on type. Currently I've only optimized 
		// for int parameters and everything else is just converted to a string.
		int intbuf;
		byte[] buffer;

		#endregion

		#region Constructors
		
		public OdbcParameter ()
		{
			name = String.Empty;
			ParamValue = null;
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
			this.ParamValue = value;
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

		public OdbcParameter (string name, OdbcType dataType, int size, string srcColumn)
			: this (name, dataType, size)
		{
			this.sourceColumn = srcColumn;
		}

		public OdbcParameter(string name, OdbcType dataType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
			: this (name, dataType, size, srcColumn)
		{
			this.direction = direction;
			this.isNullable = isNullable;
			this.precision = precision;
			this.scale = scale;
			this.sourceVersion = srcVersion;
			this.ParamValue = value;
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
				return ParamValue;
			}
			set { 
				this.ParamValue = value; 
				// Load buffer with new value
				if (odbcType==OdbcType.Int)
					intbuf=(int) value;
				else
				{
					// Treat everything else as a string
					// Init string buffer
					if (buffer==null || buffer.Length< ((size>20)?size:20) )
						buffer=new byte[(size>20)?size:20];
					else
						buffer.Initialize();
					// Convert value into string and store into buffer
					byte[] strValueBuffer=System.Text.Encoding.ASCII.GetBytes(ParamValue.ToString());
					strValueBuffer.CopyTo(buffer,0);
				}
			}
		}

		#endregion // Properties

		#region public Properties

		public void Bind(IntPtr hstmt,int ParamNum)
		{
			OdbcReturn ret;
			// Convert System.Data.ParameterDirection into odbc enum
			OdbcInputOutputDirection paramdir=libodbc.ConvertParameterDirection(this.direction);
			// Bind parameter based on type
			if (odbcType==OdbcType.Int)
				ret=libodbc.SQLBindParameter(hstmt, (ushort) ParamNum, (short) paramdir, 
					(short) odbcType, (short) odbcType, Convert.ToUInt32(size), 
					0, ref intbuf, 0, 0);
			else
				ret=libodbc.SQLBindParameter(hstmt, (ushort) ParamNum, 	(short) paramdir,
					(short) OdbcType.Char, (short) odbcType, Convert.ToUInt32(size), 
					0, 	buffer, 0, 0);
			// Check for error condition
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLBindParam",OdbcHandleType.Stmt,hstmt));
		}
		
		#endregion // public Properties

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
