// 
// OracleParameter.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman , 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;

namespace System.Data.OracleClient {
	public sealed class OracleParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		DbType dbType = DbType.AnsiString;
		ParameterDirection direction = ParameterDirection.Input;
		bool isNullable = false;
		int offset = 0;
		OracleType oracleType = OracleType.VarChar;
		string name;
		byte precision = 0x0;
		byte scale = 0x0;
		int size = 0;
		bool sizeSet = false;
		string srcColumn = String.Empty;
		DataRowVersion srcVersion = DataRowVersion.Current;
		object value = null;

		OracleParameterCollection container = null;

		#endregion // Fields

		#region Constructors

		public OracleParameter ()
		{
		}

		public OracleParameter (string name, object value)
		{
		}

		public OracleParameter (string name, OracleType dataType)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size, string srcColumn)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
		{
		}

		#endregion // Constructors

		#region Properties

		internal OracleParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

		public DbType DbType {
			get { return dbType; }
			set { SetDbType (value); }
		}

		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		public bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

		public int Offset {
			get { return offset; }
			set { offset = value; }
		}

		public OracleType OracleType {
			get { return oracleType; }
			set { SetOracleType (value); }
		}
		
		public string ParameterName {
			get { return name; }
			set { name = value; }
		}

		public byte Precision {
			get { return precision; }
			set { /* NO EFFECT*/ }
		}

		public byte Scale {
			get { return precision; }
			set { /* NO EFFECT*/ }
		}

		public int Size {
			get { return size; }
			set { 
				sizeSet = true;
				size = value; 
			}
		}

		public string SourceColumn {
			get { return srcColumn; }
			set { srcColumn = value; }
		}

		public DataRowVersion SourceVersion {
			get { return srcVersion; }
			set { srcVersion = value; }
		}

		public object Value {
			get { return this.value; }
			set { this.value = value; }
		}

		#endregion // Properties

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

		void SetDbType (DbType dbType)
		{
			this.dbType = dbType;
		}

		void SetOracleType (OracleType oracleType)
		{
			this.oracleType = oracleType;
		}

		#endregion // Methods
	}
}
