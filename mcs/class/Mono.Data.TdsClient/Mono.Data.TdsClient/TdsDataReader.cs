//
// Mono.Data.TdsClient.TdsDataReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace Mono.Data.TdsClient {
        public class TdsDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		#region Fields

		int fieldCount;
		object[] fields;
		bool hasRows;
		bool isClosed;
		int recordsAffected;

		#endregion // Fields

		#region Constructors

		#endregion // Constructors

		#region Properties

		public int Depth {
			get { return 0; }
		}

		public int FieldCount {
			get { return fieldCount; }
		}

		public bool HasRows {
			get { return hasRows; }
		}

		public bool IsClosed {
			get { return isClosed; }
		}

		[MonoTODO]
		public object this [int i] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object this [string name] {
			get { throw new NotImplementedException (); }
		}
	
		public int RecordsAffected {
			get { return recordsAffected; }
		}

		#endregion // Properties

                #region Methods

		[MonoTODO]
		public void Close ()
		{
			throw new NotImplementedException (); 
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

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public bool IsDBNull (int i)
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

                #endregion // Methods
	}
}
