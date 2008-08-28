//
// System.Data.OleDb.OleDbDataReader
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace System.Data.OleDb
{
#if NET_2_0
	public sealed class OleDbDataReader : DbDataReader, IDisposable
#else
	public sealed class OleDbDataReader : MarshalByRefObject, IDataReader, IDisposable, IDataRecord, IEnumerable
#endif
	{
		#region Fields
		
		private OleDbCommand command;
		private bool open;
		private ArrayList gdaResults;
		private int currentResult;
		private int currentRow;
		private bool disposed;

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

		public
#if NET_2_0
		override
#endif
		int Depth {
			get {
				return 0; // no nested selects supported
			}
		}

		public
#if NET_2_0
		override
#endif
		int FieldCount {
			get {
				if (currentResult < 0 || currentResult >= gdaResults.Count)
					return 0;

				return libgda.gda_data_model_get_n_columns (
					(IntPtr) gdaResults[currentResult]);
			}
		}

		public
#if NET_2_0
		override
#endif
		bool IsClosed {
			get {
				return !open;
			}
		}

		public
#if NET_2_0
		override
#endif
		object this[string name] {
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

		public
#if NET_2_0
		override
#endif
		object this[int index] {
			get {
				return (object) GetValue (index);
			}
		}

		public
#if NET_2_0
		override
#endif
		int RecordsAffected {
			get {
				int total_rows;
				
				if (currentResult < 0 || currentResult >= gdaResults.Count)
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
		
		[MonoTODO]
		public
#if NET_2_0
		override
#endif
		bool HasRows {
			get {
				throw new NotImplementedException ();
			}
		}

#if NET_2_0
		[MonoTODO]
		public override int VisibleFieldCount {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		#endregion

		#region Methods

		public
#if NET_2_0
		override
#endif
		void Close ()
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

		public
#if NET_2_0
		override
#endif
		bool GetBoolean (int ordinal)
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

		public
#if NET_2_0
		override
#endif
		byte GetByte (int ordinal)
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
		public
#if NET_2_0
		override
#endif
		long GetBytes (int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}
		
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public
#if NET_2_0
		override
#endif
		char GetChar (int ordinal)
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
		public
#if NET_2_0
		override
#endif
		long GetChars (int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		public new OleDbDataReader GetData (int ordinal)
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		protected override DbDataReader GetDbDataReader (int ordinal)
		{
			return this.GetData (ordinal);
		}
#endif

		public
#if NET_2_0
		override
#endif
		string GetDataTypeName (int index)
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

		public
#if NET_2_0
		override
#endif
		DateTime GetDateTime (int ordinal)
		{
			IntPtr value;

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
		public
#if NET_2_0
		override
#endif
		decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public
#if NET_2_0
		override
#endif
		double GetDouble (int ordinal)
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

		public
#if NET_2_0
		override
#endif
		Type GetFieldType (int index)
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

		public
#if NET_2_0
		override
#endif
		float GetFloat (int ordinal)
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
		public
#if NET_2_0
		override
#endif
		Guid GetGuid (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public
#if NET_2_0
		override
#endif
		short GetInt16 (int ordinal)
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

		public
#if NET_2_0
		override
#endif
		int GetInt32 (int ordinal)
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

		public
#if NET_2_0
		override
#endif
		long GetInt64 (int ordinal)
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

		public
#if NET_2_0
		override
#endif
		string GetName (int index)
		{
			if (currentResult == -1)
				return null;

			return libgda.gda_data_model_get_column_title (
				(IntPtr) gdaResults[currentResult], index);
		}

		public
#if NET_2_0
		override
#endif
		int GetOrdinal (string name)
		{
			if (currentResult == -1)
				throw new IndexOutOfRangeException ();

			for (int i = 0; i < FieldCount; i++) {
				if (GetName (i) == name)
					return i;
			}

			throw new IndexOutOfRangeException ();
		}

		public
#if NET_2_0
		override
#endif
		DataTable GetSchemaTable ()
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
#if DEBUG_OleDbDataReader
					Console.WriteLine("Error: current result -1");
#endif
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

				for (int i = 0; i < this.FieldCount; i += 1 ) {
					
					schemaRow = dataTableSchema.NewRow ();

					attrs = libgda.gda_data_model_describe_column ((IntPtr) gdaResults[currentResult],
						i);
					if (attrs == IntPtr.Zero){
						// FIXME: throw exception
#if DEBUG_OleDbDataReader
						Console.WriteLine("Error: attrs null");
#endif
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

		public
#if NET_2_0
		override
#endif
		string GetString (int ordinal)
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

		public
#if NET_2_0
		override
#endif
		object GetValue (int ordinal)
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
		public
#if NET_2_0
		override
#endif
		int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

#if !NET_2_0
		[MonoTODO]
		IDataReader IDataRecord.GetData (int ordinal)
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_2_0
		public override IEnumerator GetEnumerator()
#else
		IEnumerator IEnumerable.GetEnumerator ()
#endif
		{
			return new DbEnumerator(this);
		}

		public
#if NET_2_0
		override
#endif
		bool IsDBNull (int ordinal)
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

		public
#if NET_2_0
		override
#endif
		bool NextResult ()
		{
			int i = currentResult + 1;
			if (i >= 0 && i < gdaResults.Count) {
				currentResult++;
				return true;
			}

			return false;
		}

		public
#if NET_2_0
		override
#endif
		bool Read ()
		{
			if (currentResult < 0 || currentResult >= gdaResults.Count)
				return false;

			currentRow++;
			if (currentRow < libgda.gda_data_model_get_n_rows ((IntPtr) gdaResults[currentResult]))
				return true;

			return false;
		}

		#endregion

		#region Destructors

		private new void Dispose (bool disposing)
		{
			if (!this.disposed) {
				if (disposing) {
					// release any managed resources
					command = null;
					GC.SuppressFinalize (this);
				}
				// release any unmanaged resources
				if (gdaResults != null) {
					gdaResults.Clear ();
					gdaResults = null;
				}

				// close any handles
				if (open)
					Close ();

				this.disposed = true;
			}
		}

		void IDisposable.Dispose() {
			Dispose (true);
		}

#if ONLY_1_1
		~OleDbDataReader () {
			Dispose (false);
		}
#endif
		#endregion // Destructors
	}
}
