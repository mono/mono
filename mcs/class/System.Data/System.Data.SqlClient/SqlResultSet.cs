//
// System.Data.SqlClient.SqlResultSet
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data.Sql;
using System.Data.SqlTypes;

namespace System.Data.SqlClient {
	public sealed class SqlResultSet : MarshalByRefObject, IEnumerable, ISqlResultSet, ISqlReader, ISqlRecord, ISqlGetTypedData, IGetTypedData, ISqlUpdatableRecord, ISqlSetTypedData, IDataReader, IDisposable, IDataUpdatableRecord, IDataRecord, ISetTypedData
	{
		#region Fields

		bool disposed;

		#endregion // Fields

		#region Properties

		public int Depth {
			get { throw new NotImplementedException (); }
		}

		public int FieldCount {
			get { throw new NotImplementedException (); }
		}
		
		public bool HasRows {
			get { throw new NotImplementedException (); }
		}

		public int HiddenFieldCount {
			get { throw new NotImplementedException (); }
		}

		object IDataRecord.this [int x] {
			get { return this [x]; }
		}

		object IDataRecord.this [string x] {
			get { return this [x]; }
		}

		public bool IsClosed {
			get { throw new NotImplementedException (); }
		}

		public object this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public object this [string name] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int RecordsAffected {
			get { throw new NotImplementedException (); }
		}

		public bool Scrollable {
			get { throw new NotImplementedException (); }
		}

		public ResultSetSensitivity Sensitivity {
			get { throw new NotImplementedException (); }
		}

		public bool Updatable {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ISqlUpdatableRecord CreateRecord ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					Close ();
				}
				disposed = true;
			}
		}

		[MonoTODO]
		public bool GetBoolean (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte GetByte (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public char GetChar (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetDataTypeName (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DateTime GetDateTime (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public decimal GetDecimal (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public double GetDouble (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetFieldType (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public float GetFloat (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Guid GetGuid (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public short GetInt16 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetInt32 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetInt64 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetName (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetObjectRef (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetOrdinal (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable GetSchemaTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBinary GetSqlBinary (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBoolean GetSqlBoolean (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlByte GetSqlByte (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBytes GetSqlBytes (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBytes GetSqlBytesRef (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlChars GetSqlChars (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlChars GetSqlCharsRef (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDate GetSqlDate (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDateTime GetSqlDateTime (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDecimal GetSqlDecimal (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDouble GetSqlDouble (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlGuid GetSqlGuid (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlInt16 GetSqlInt16 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlInt32 GetSqlInt32 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlInt64 GetSqlInt64 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMetaData GetSqlMetaData (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMoney GetSqlMoney (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlSingle GetSqlSingle (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlString GetSqlString (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlTime GetSqlTime (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlUtcDateTime GetSqlUtcDateTime (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetSqlValue (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetSqlValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlXmlReader GetSqlXmlReader (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetString (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetValue (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (ISqlRecord record)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsDBNull (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsSetAsDefault (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool NextResult ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Read ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ReadAbsolute (int position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ReadFirst ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ReadLast ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ReadPrevious ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool ReadRelative (int position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetBoolean (int i, bool value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetByte (int i, byte value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetChar (int i, char value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetDateTime (int i, DateTime value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetDecimal (int i, decimal value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetDefault (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetDouble (int i, double value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetFloat (int i, float value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetGuid (int i, Guid value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetInt16 (int i, short value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetInt32 (int i, int value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetInt64 (int i, long value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetObjectRef (int i, object buffer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlBinary (int i, SqlBinary value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlBoolean (int i, SqlBoolean value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlByte (int i, SqlByte value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlBytes (int i, SqlBytes value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlBytesRef (int i, SqlBytes value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlChars (int i, SqlChars value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlCharsRef (int i, SqlChars value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlDate (int i, SqlDate value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlDateTime (int i, SqlDateTime value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlDecimal (int i, SqlDecimal value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlDouble (int i, SqlDouble value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlGuid (int i, SqlGuid value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlInt16 (int i, SqlInt16 value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlInt32 (int i, SqlInt32 value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlInt64 (int i, SqlInt64 value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlMoney (int i, SqlMoney value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlSingle (int i, SqlSingle value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlString (int i, SqlString value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlTime (int i, SqlTime value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlUtcDateTime (int i, SqlUtcDateTime value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSqlXmlReader (int i, SqlXmlReader value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetString (int i, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetValue (int i, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int SetValues (object[] value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Update ()
		{
			throw new NotImplementedException ();
		}


		#endregion // Methods
	}
}

#endif
