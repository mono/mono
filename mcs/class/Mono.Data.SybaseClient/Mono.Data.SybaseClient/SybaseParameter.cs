//
// Mono.Data.SybaseClient.SybaseParameter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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

		string parmName;
		SybaseType dbtype;
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
		public SybaseParameter () 
		{
		}

		[MonoTODO]
		public SybaseParameter (string parameterName, object value) 
		{
			this.parmName = parameterName;
			this.objValue = value;
		}
		
		[MonoTODO]
		public SybaseParameter (string parameterName, SybaseType dbType) 
		{
			this.parmName = parameterName;
			this.dbtype = dbType;
		}

		[MonoTODO]
		public SybaseParameter (string parameterName, SybaseType dbType, int size) 
		{

			this.parmName = parameterName;
			this.dbtype = dbType;
			this.size = size;
		}
		
		[MonoTODO]
		public SybaseParameter(string parameterName, SybaseType dbType, int size, string sourceColumn) 
		{

			this.parmName = parameterName;
			this.dbtype = dbType;
			this.size = size;
			this.sourceColumn = sourceColumn;
		}
			 
		[MonoTODO]
		public SybaseParameter(string parameterName, SybaseType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) 
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
		public SybaseType SybaseType {
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
			case SybaseType.Image :
			case SybaseType.NVarChar :
			case SybaseType.VarBinary :
			case SybaseType.VarChar :
				if (!sizeSet || size == 0)
					throw new InvalidOperationException ("All variable length parameters must have an explicitly set non-zero size.");
				result.Append ("(");
				result.Append (size.ToString ());
				result.Append (")");
				break;
			case SybaseType.Decimal :
			case SybaseType.Money :
			case SybaseType.SmallMoney :
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
