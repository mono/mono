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
using System.Runtime.InteropServices;

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
			get {
				return 0; // no nested selects supported
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
			get {
				int pos;

				if (currentResult == -1)
					throw new InvalidOperationException ();

				pos = libgda.gda_data_model_get_column_position (
					(IntPtr) gdaResults[currentResult],
					name);
				if (pos == -1)
					throw new IndexOutOfRangeException ();

				return this[pos];
			}
		}

		public object this[int index] {
			get {
				return (object) GetValue (index);
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

		public void Close ()
		{
			for (int i = 0; i < gdaResults.Count; i++) {
				IntPtr obj = (IntPtr) gdaResults[i];
				libgda.FreeObject (obj);
			}

			gdaResults.Clear ();
			gdaResults = null;
			
			open = false;
			currentResult = -1;
			currentRow = -1;
		}

		~OleDbDataReader ()
		{
			if (open)
				Close ();
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
			
			if (libgda.gda_value_get_type (value) != GdaValueType.Boolean)
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
			
			if (libgda.gda_value_get_type (value) != GdaValueType.Tinyint)
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
			
			if (libgda.gda_value_get_type (value) != GdaValueType.Tinyint)
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
			IntPtr attrs;
			GdaValueType type;

			if (currentResult == -1)
				return "unknown";

			
			attrs = libgda.gda_data_model_describe_column ((IntPtr) gdaResults[currentResult],
								       index);
			if (attrs == IntPtr.Zero)
				return "unknown";

			type = libgda.gda_field_attributes_get_gdatype (attrs);
			libgda.gda_field_attributes_free (attrs);
			
			return libgda.gda_type_to_string (type);
		}

		public DateTime GetDateTime (int ordinal)
		{
			IntPtr value;
			DateTime dt;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_type (value) == GdaValueType.Date) {
				GdaDate gdt;

				gdt = (GdaDate) Marshal.PtrToStructure (libgda.gda_value_get_date (value),
									typeof (GdaDate));
				return new DateTime ((int) gdt.year, (int) gdt.month, (int) gdt.day);
			} else if (libgda.gda_value_get_type (value) == GdaValueType.Time) {
				GdaTime gdt;

				gdt = (GdaTime) Marshal.PtrToStructure (libgda.gda_value_get_time (value),
									typeof (GdaTime));
				return new DateTime (0, 0, 0, (int) gdt.hour, (int) gdt.minute, (int) gdt.second, 0);
			} else if (libgda.gda_value_get_type (value) == GdaValueType.Timestamp) {
				GdaTimestamp gdt;
				
				gdt = (GdaTimestamp) Marshal.PtrToStructure (libgda.gda_value_get_timestamp (value),
									     typeof (GdaTimestamp));

				return new DateTime ((int) gdt.year, (int) gdt.month, (int) gdt.day,
						     (int) gdt.hour, (int) gdt.minute, (int) gdt.second,
						     (int) gdt.fraction);
			}

			throw new InvalidCastException ();
		}

		[MonoTODO]
		public decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public double GetDouble (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_type (value) != GdaValueType.Double)
				throw new InvalidCastException ();
			return libgda.gda_value_get_double (value);
		}

		[MonoTODO]
		public Type GetFieldType (int index)
		{
			IntPtr value;
			GdaValueType type;

			if (currentResult == -1)
				throw new IndexOutOfRangeException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
				index, currentRow);
			if (value == IntPtr.Zero)
				throw new IndexOutOfRangeException ();

			type = libgda.gda_value_get_type (value);
			switch (type) {
			case GdaValueType.Bigint : return typeof (long);
			case GdaValueType.Boolean : return typeof (bool);
			case GdaValueType.Date : return typeof (DateTime);
			case GdaValueType.Double : return typeof (double);
			case GdaValueType.Integer : return typeof (int);
			case GdaValueType.Single : return typeof (float);
			case GdaValueType.Smallint : return typeof (byte);
			case GdaValueType.String : return typeof (string);
			case GdaValueType.Time : return typeof (DateTime);
			case GdaValueType.Timestamp : return typeof (DateTime);
			case GdaValueType.Tinyint : return typeof (byte);
			}

			return typeof(string); // default
		}

		public float GetFloat (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_type (value) != GdaValueType.Single)
				throw new InvalidCastException ();
			return libgda.gda_value_get_single (value);
		}

		[MonoTODO]
		public Guid GetGuid (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public short GetInt16 (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_type (value) != GdaValueType.Smallint)
				throw new InvalidCastException ();
			return (short) libgda.gda_value_get_smallint (value);
		}

		public int GetInt32 (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_type (value) != GdaValueType.Integer)
				throw new InvalidCastException ();
			return libgda.gda_value_get_integer (value);
		}

		public long GetInt64 (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_type (value) != GdaValueType.Bigint)
				throw new InvalidCastException ();
			return libgda.gda_value_get_bigint (value);
		}

		public string GetName (int index)
		{
			if (currentResult == -1)
				return null;

			return libgda.gda_data_model_get_column_title (
				(IntPtr) gdaResults[currentResult], index);
		}

		public int GetOrdinal (string name)
		{
			if (currentResult == -1)
				throw new IndexOutOfRangeException ();

			for (int i = 0; i < FieldCount; i++) {
				if (GetName (i) == name)
					return i;
			}

			throw new IndexOutOfRangeException ();
		}

		public DataTable GetSchemaTable ()
		{
			DataTable dataTableSchema = null;
			// Only Results from SQL SELECT Queries 
			// get a DataTable for schema of the result
			// otherwise, DataTable is null reference
			if(this.FieldCount > 0) {

				IntPtr attrs;
				GdaValueType gdaType;
				long columnSize = 0;

				if (currentResult == -1) {
					// FIXME: throw an exception?
					Console.WriteLine("Error: current result -1");
					return null;
				}
						
				dataTableSchema = new DataTable ();
				
				dataTableSchema.Columns.Add ("ColumnName", typeof (string));
				dataTableSchema.Columns.Add ("ColumnOrdinal", typeof (int));
				dataTableSchema.Columns.Add ("ColumnSize", typeof (int));
				dataTableSchema.Columns.Add ("NumericPrecision", typeof (int));
				dataTableSchema.Columns.Add ("NumericScale", typeof (int));
				dataTableSchema.Columns.Add ("IsUnique", typeof (bool));
				dataTableSchema.Columns.Add ("IsKey", typeof (bool));
				DataColumn dc = dataTableSchema.Columns["IsKey"];
				dc.AllowDBNull = true; // IsKey can have a DBNull
				dataTableSchema.Columns.Add ("BaseCatalogName", typeof (string));
				dataTableSchema.Columns.Add ("BaseColumnName", typeof (string));
				dataTableSchema.Columns.Add ("BaseSchemaName", typeof (string));
				dataTableSchema.Columns.Add ("BaseTableName", typeof (string));
				dataTableSchema.Columns.Add ("DataType", typeof(Type));
				dataTableSchema.Columns.Add ("AllowDBNull", typeof (bool));
				dataTableSchema.Columns.Add ("ProviderType", typeof (int));
				dataTableSchema.Columns.Add ("IsAliased", typeof (bool));
				dataTableSchema.Columns.Add ("IsExpression", typeof (bool));
				dataTableSchema.Columns.Add ("IsIdentity", typeof (bool));
				dataTableSchema.Columns.Add ("IsAutoIncrement", typeof (bool));
				dataTableSchema.Columns.Add ("IsRowVersion", typeof (bool));
				dataTableSchema.Columns.Add ("IsHidden", typeof (bool));
				dataTableSchema.Columns.Add ("IsLong", typeof (bool));
				dataTableSchema.Columns.Add ("IsReadOnly", typeof (bool));

				DataRow schemaRow;
				DbType dbType;
				Type typ;
								
				for (int i = 0; i < this.FieldCount; i += 1 ) {
					
					schemaRow = dataTableSchema.NewRow ();

					attrs = libgda.gda_data_model_describe_column ((IntPtr) gdaResults[currentResult],
						i);
					if (attrs == IntPtr.Zero){
						// FIXME: throw exception
						Console.WriteLine("Error: attrs null");
						return null;
					}

					gdaType = libgda.gda_field_attributes_get_gdatype (attrs);
					columnSize = libgda.gda_field_attributes_get_defined_size (attrs);
					libgda.gda_field_attributes_free (attrs);
										
					schemaRow["ColumnName"] = this.GetName(i);
					schemaRow["ColumnOrdinal"] = i + 1;
					
					schemaRow["ColumnSize"] = (int) columnSize;
					schemaRow["NumericPrecision"] = 0;
					schemaRow["NumericScale"] = 0;
					// TODO: need to get KeyInfo
					//if((cmdBehavior & CommandBehavior.KeyInfo) == CommandBehavior.KeyInfo) {
						// bool IsUnique, IsKey;
						// GetKeyInfo(field[i].Name, out IsUnique, out IsKey);
					//}
					//else {
						schemaRow["IsUnique"] = false;
						schemaRow["IsKey"] = DBNull.Value;
					//}
					schemaRow["BaseCatalogName"] = "";
					
					schemaRow["BaseColumnName"] = this.GetName(i);
					schemaRow["BaseSchemaName"] = "";
					schemaRow["BaseTableName"] = "";

					schemaRow["DataType"] = this.GetFieldType(i);

					schemaRow["AllowDBNull"] = false;
					
					schemaRow["ProviderType"] = (int) gdaType;
					schemaRow["IsAliased"] = false;
					schemaRow["IsExpression"] = false;
					schemaRow["IsIdentity"] = false;
					schemaRow["IsAutoIncrement"] = false;
					schemaRow["IsRowVersion"] = false;
					schemaRow["IsHidden"] = false;
					schemaRow["IsLong"] = false;
					schemaRow["IsReadOnly"] = false;
					
					schemaRow.AcceptChanges();
					
					dataTableSchema.Rows.Add (schemaRow);
				}
				
#if DEBUG_OleDbDataReader
				Console.WriteLine("********** DEBUG Table Schema BEGIN ************");
				foreach (DataRow myRow in dataTableSchema.Rows) {
					foreach (DataColumn myCol in dataTableSchema.Columns)
						Console.WriteLine(myCol.ColumnName + " = " + myRow[myCol]);
					Console.WriteLine();
				}
				Console.WriteLine("********** DEBUG Table Schema END ************");
#endif // DEBUG_OleDbDataReader

			}
			
			return dataTableSchema;
		}

		public string GetString (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new InvalidCastException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new InvalidCastException ();
			
			if (libgda.gda_value_get_type (value) != GdaValueType.String)
				throw new InvalidCastException ();
			return libgda.gda_value_get_string (value);
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

			type = libgda.gda_value_get_type (value);
			switch (type) {
			case GdaValueType.Bigint : return GetInt64 (ordinal);
			case GdaValueType.Boolean : return GetBoolean (ordinal);
			case GdaValueType.Date : return GetDateTime (ordinal);
			case GdaValueType.Double : return GetDouble (ordinal);
			case GdaValueType.Integer : return GetInt32 (ordinal);
			case GdaValueType.Single : return GetFloat (ordinal);
			case GdaValueType.Smallint : return GetByte (ordinal);
			case GdaValueType.String : return GetString (ordinal);
			case GdaValueType.Time : return GetDateTime (ordinal);
			case GdaValueType.Timestamp : return GetDateTime (ordinal);
			case GdaValueType.Tinyint : return GetByte (ordinal);
			}

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

		public bool IsDBNull (int ordinal)
		{
			IntPtr value;

			if (currentResult == -1)
				throw new IndexOutOfRangeException ();

			value = libgda.gda_data_model_get_value_at ((IntPtr) gdaResults[currentResult],
								    ordinal, currentRow);
			if (value == IntPtr.Zero)
				throw new IndexOutOfRangeException ();

			return libgda.gda_value_is_null (value);
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
