//
// System.Data.Sql.SqlMetaData
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Data.SqlTypes;

namespace System.Data.Sql {
	public sealed class SqlMetaData
	{
		#region Fields

		public const long x_lMax = -1;

		SqlCompareOptions compareOptions = SqlCompareOptions.None;
		string databaseName = null;
		bool isPartialLength = false;
		long localeId = 0L;
		long maxLength = 4L;
		string name;
		byte precision = 10;
		byte scale = 0;
		string schemaName = null;
		SqlDbType sqlDbType = SqlDbType.Int;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType type)
		{
			this.name = name;
			this.sqlDbType = type;
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType type, long maxLength)
		{
			this.maxLength = maxLength;
			this.name = name;
			this.sqlDbType = type;
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType type, SqlMetaData[] columnMetaData)
		{
			this.sqlDbType = type;
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType type, byte precision, byte scale)
		{
			this.name = name;
			this.precision = precision;
			this.scale = scale;
			this.sqlDbType = type;
		}

		[MonoTODO]
		public SqlMetaData (string strName, long maxLength, long localeId, SqlCompareOptions compareOptions, string udtTypeName)
		{
			this.compareOptions = compareOptions;
			this.localeId = localeId;
			this.maxLength = maxLength;
			this.name = strName;
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType type, long maxLength, long locale, SqlCompareOptions compareOptions)
		{
			this.compareOptions = compareOptions;
			this.localeId = locale;
			this.maxLength = maxLength;
			this.name = name;
			this.sqlDbType = type;
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType type, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, string DatabaseName, string SchemaName, bool PartialLength, string udtTypeName)
		{
			this.compareOptions = compareOptions;
			this.databaseName = DatabaseName;
			this.isPartialLength = PartialLength;
			this.localeId = localeId;
			this.maxLength = maxLength;
			this.name = name;
			this.precision = precision;
			this.scale = scale;
			this.schemaName = SchemaName;
			this.sqlDbType = type;
		}

		#endregion // Constructors

		#region Properties

		public SqlCompareOptions CompareOptions {
			get { return compareOptions; }
		}

		public string DatabaseName {
			get { return databaseName; }
		}

		[MonoTODO]
		public DbType DbType {
			get { throw new NotImplementedException (); }
		}

		public bool IsPartialLength {
			get { return isPartialLength; }
		}

		public long LocaleId {
			get { return localeId; }
		}

		public static long MAX {
			get { return x_lMax; }
		}

		public long MaxLength {
			get { return maxLength; }
		}

		public string Name {
			get { return name; }
		}

		public byte Precision { 
			get { return precision; }
		}

		public byte Scale { 
			get { return scale; }
		}

		public string SchemaName {
			get { return schemaName; }
		}

		public SqlDbType SqlDbType {
			get { return sqlDbType; }
		}

		[MonoTODO]
		public string TypeName {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public bool Adjust (bool value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public byte Adjust (byte value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public byte[] Adjust (byte[] value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public char Adjust (char value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public char[] Adjust (char[] value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public DateTime Adjust (DateTime value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public decimal Adjust (decimal value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public double Adjust (double value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public Guid Adjust (Guid value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public short Adjust (short value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public int Adjust (int value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public long Adjust (long value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public object Adjust (object value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public float Adjust (float value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlBinary Adjust (SqlBinary value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlBoolean Adjust (SqlBoolean value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlByte Adjust (SqlByte value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlBytes Adjust (SqlBytes value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlChars Adjust (SqlChars value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlDateTime Adjust (SqlDateTime value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlDecimal Adjust (SqlDecimal value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlDouble Adjust (SqlDouble value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlGuid Adjust (SqlGuid value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlInt16 Adjust (SqlInt16 value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlInt32 Adjust (SqlInt32 value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlInt64 Adjust (SqlInt64 value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlMoney Adjust (SqlMoney value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlSingle Adjust (SqlSingle value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlString Adjust (SqlString value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public string Adjust (string value)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public SqlMetaData GetMetaData (int i)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public static SqlMetaData InferFromValue (object value, string name)
		{
			throw new NotImplementedException (); 
		}

		#endregion // Methods
	}
}

#endif
