//
// System.Data.OleDb.OleDbDataReader
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbDataReader : MarshalByRefObject, IDataReader, IDisposable, IDataRecord, IEnumerable
	{
		#region Fields
		
		OleDbCommand command;
		bool open;

		#endregion

		#region Constructors

		internal OleDbDataReader (OleDbCommand command) 
		{
			this.command = command;
			open = true;
			command.OpenReader(this);
		}

		#endregion

		#region Properties

		public int Depth {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public int FieldCount {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public bool IsClosed {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public object this[string name] {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public object this[int index] {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public int RecordsAffected {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		~OleDbDataReader ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool GetBoolean (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte GetByte (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetBytes (int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public char GetChar (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetChars (int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OleDbDataReader GetData (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetDataTypeName (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DateTime GetDateTime (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public double GetDouble (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetFieldType (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public float GetFloat (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Guid GetGuid (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public short GetInt16 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetInt32 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetInt64 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetName (int index)
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
		public string GetString (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public TimeSpan GetTimeSpan (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetValue (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IDataReader IDataRecord.GetData (int ordinal)
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
		public bool IsDBNull (int ordinal)
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

		#endregion
	}
}
