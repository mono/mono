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
		
		private OleDbCommand command;
		private bool open;
		private ArrayList gdaResults;
		private int currentResult;
		private int currentRow;

		#endregion

		#region Constructors

		internal OleDbDataReader (OleDbCommand command, ArrayList results) 
		{
			this.command = command;
			open = true;
			if (results != null)
				gdaResults = results;
			else
				gdaResults = new ArrayList ();
			currentResult = -1;
			currentRow = -1;
		}

		#endregion

		#region Properties

		public int Depth {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public int FieldCount {
			get {
				if (currentResult < 0 ||
				    currentResult >= gdaResults.Count)
					return 0;

				return libgda.gda_data_model_get_n_columns (
					(IntPtr) gdaResults[currentResult]);
			}
		}

		public bool IsClosed {
			get {
				return !open;
			}
		}

		public object this[string name] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public object this[int index] {
			get {
				if (currentResult < 0 ||
				    currentResult >= gdaResults.Count)
					return null;
				
				return libgda.gda_data_model_get_value_at (
						(IntPtr) gdaResults[currentResult],
						index,
						currentRow);
			}
		}

		public int RecordsAffected {
			get {
				int total_rows;
				
				if (currentResult < 0 ||
				    currentResult >= gdaResults.Count)
					return 0;

				total_rows = libgda.gda_data_model_get_n_rows (
					(IntPtr) gdaResults[currentResult]);
				if (total_rows > 0) {
					if (FieldCount > 0) {
						// It's a SELECT statement
						return -1;
					}
				}

				return FieldCount > 0 ? -1 : total_rows;
			}
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

		public bool GetBoolean (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_vtype (value) != GdaValueType.Boolean)
				throw new InvalidCastException ();
			return libgda.gda_value_get_boolean (value);
		}

		public byte GetByte (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_vtype (value) != GdaValueType.Tinyint)
				throw new InvalidCastException ();
			return libgda.gda_value_get_tinyint (value);
		}

		[MonoTODO]
		public long GetBytes (int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}
		
		public char GetChar (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_vtype (value) != GdaValueType.Tinyint)
				throw new InvalidCastException ();
			return (char) libgda.gda_value_get_tinyint (value);
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

		public string GetDataTypeName (int index)
		{
			IntPtr value;

			if (currentResult == -1)
				return "unknown";

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    index, currentRow);
			if (value == IntPtr.Zero)
				return "unknown";

			return libgda.gda_type_to_string (libgda.gda_value_get_vtype (value));
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

		public object GetValue (int ordinal)
		{
			IntPtr value;
			GdaValueType type;

			if (currentResult == -1)
				throw new IndexOutOfRangeException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new IndexOutOfRangeException ();

			type = libgda.gda_value_get_vtype (value);
			// FIXME: return correct type

			return (object) libgda.gda_value_stringify (value);
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

		public bool NextResult ()
		{
			int i = currentResult + 1;
			if (i >= 0 && i < gdaResults.Count) {
				currentResult++;
				return true;
			}

			return false;
		}

		public bool Read ()
		{
			if (currentResult < 0 ||
			    currentResult >= gdaResults.Count)
				return false;

			currentRow++;
			if (currentRow <
			    libgda.gda_data_model_get_n_rows ((IntPtr) gdaResults[currentResult]))
				return true;

			return false;
		}

		#endregion
	}
}
