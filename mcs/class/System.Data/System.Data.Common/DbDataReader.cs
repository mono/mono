//
// System.Data.Common.DbDataReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data;

namespace System.Data.Common {
	public abstract class DbDataReader : MarshalByRefObject, IDataReader, IDataReader2, IDataRecord, IDataRecord2, IDisposable, IEnumerable
	{
		#region Constructors

		protected DbDataReader ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract int Depth { get; }
		public abstract int FieldCount { get; }
		public abstract bool HasRows { get; }
		public abstract bool IsClosed { get; }
		public abstract object this [int index] { get; }
		public abstract object this [string name] { get; }
		public abstract int RecordsAffected { get; }
		public abstract int VisibleFieldCount { get; }

		#endregion // Properties

		#region Methods

		public abstract void Close ();
		public abstract void Dispose ();
		public abstract bool GetBoolean (int i);
		public abstract byte GetByte (int i);
		public abstract long GetBytes (int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);
		public abstract char GetChar (int i);
		public abstract long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length);

		[MonoTODO]
		public DbDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		public abstract string GetDataTypeName (int i);
		public abstract DateTime GetDateTime (int i);
		public abstract decimal GetDecimal (int i);
		public abstract double GetDouble (int i);
		public abstract IEnumerator GetEnumerator ();
		public abstract Type GetFieldProviderSpecificType (int i);
		public abstract Type GetFieldType (int i);
		public abstract float GetFloat (int i);
		public abstract Guid GetGuid (int i);
		public abstract short GetInt16 (int i);
		public abstract int GetInt32 (int i);
		public abstract long GetInt64 (int i);
		public abstract string GetName (int i);
		public abstract int GetOrdinal (string name);
		public abstract object GetProviderSpecificValue (int i);
		public abstract int GetProviderSpecificValues (object[] values);
		public abstract DataTable GetSchemaTable ();
		public abstract string GetString (int i);
		public abstract object GetValue (int i);
		public abstract int GetValues (object[] values);

		IDataReader IDataRecord.GetData (int i)
		{
			return ((IDataReader) this).GetData (i);
		}

		public abstract bool IsDBNull (int i);
		public abstract bool NextResult ();
		public abstract bool Read ();

		#endregion // Methods
	}
}

#endif // NET_1_2
