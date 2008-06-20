//
// Mono.Data.SqliteClient.SqliteDataReader.cs
//
// Provides a means of reading a forward-only stream of rows from a Sqlite 
// database file.
//
// Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//            Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//	      Joshua Tauberer <tauberer@for.net>
//
// Copyright (C) 2002  Vladimir Vukicevic
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

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SqliteClient
{
	public class SqliteDataReader :
#if NET_2_0
		DbDataReader, IDataReader, IDisposable, IDataRecord
#else
		MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
#endif
	{

		#region Fields
		
		private SqliteCommand command;
		private ArrayList rows;
		private string[] columns;
		private Hashtable column_names_sens, column_names_insens;
		private int current_row;
		private bool closed;
		private bool reading;
		private int records_affected;
		private string[] decltypes;
		
		#endregion

		#region Constructors and destructors
		
		internal SqliteDataReader (SqliteCommand cmd, IntPtr pVm, int version)
		{
			command = cmd;
			rows = new ArrayList ();
			column_names_sens = new Hashtable ();
#if NET_2_0
			column_names_insens = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
#else
			column_names_insens = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
							     CaseInsensitiveComparer.DefaultInvariant);
#endif
			closed = false;
			current_row = -1;
			reading = true;
			ReadpVm (pVm, version, cmd);
			ReadingDone ();
		}
		
		#endregion

		#region Properties
		
#if NET_2_0
		override
#endif
		public int Depth {
			get { return 0; }
		}
		
#if NET_2_0
		override
#endif
		public int FieldCount {
			get { return columns.Length; }
		}
		
#if NET_2_0
		override
#endif
		public object this[string name] {
			get {
				return GetValue (GetOrdinal (name));
			}
		}
		
#if NET_2_0
		override
#endif
		public object this[int i] {
			get { return GetValue (i); }
		}
		
#if NET_2_0
		override
#endif
		public bool IsClosed {
			get { return closed; }
		}
		
#if NET_2_0
		override
#endif
		public int RecordsAffected {
			get { return records_affected; }
		}
		
		#endregion

		#region Internal Methods
		
		internal void ReadpVm (IntPtr pVm, int version, SqliteCommand cmd)
		{
			int pN;
			IntPtr pazValue;
			IntPtr pazColName;
			bool first = true;
			
			int[] declmode = null;

			while (true) {
				bool hasdata = cmd.ExecuteStatement(pVm, out pN, out pazValue, out pazColName);
			
				// For the first row, get the column information
				if (first) {
					first = false;
					
					if (version == 3) {
						// A decltype might be null if the type is unknown to sqlite.
						decltypes = new string[pN];
						declmode = new int[pN]; // 1 == integer, 2 == datetime
						for (int i = 0; i < pN; i++) {
							IntPtr decl = Sqlite.sqlite3_column_decltype16 (pVm, i);
							if (decl != IntPtr.Zero) {
								decltypes[i] = Marshal.PtrToStringUni (decl).ToLower(System.Globalization.CultureInfo.InvariantCulture);
								if (decltypes[i] == "int" || decltypes[i] == "integer")
									declmode[i] = 1;
								else if (decltypes[i] == "date" || decltypes[i] == "datetime")
									declmode[i] = 2;
							}
						}
					}
					
					columns = new string[pN];	
					for (int i = 0; i < pN; i++) {
						string colName;
						if (version == 2) {
							IntPtr fieldPtr = Marshal.ReadIntPtr (pazColName, i*IntPtr.Size);
							colName = Sqlite.HeapToString (fieldPtr, ((SqliteConnection)cmd.Connection).Encoding);
						} else {
							colName = Marshal.PtrToStringUni (Sqlite.sqlite3_column_name16 (pVm, i));
						}
						columns[i] = colName;
						column_names_sens [colName] = i;
						column_names_insens [colName] = i;
					}
				}

				if (!hasdata) break;
				
				object[] data_row = new object [pN];
				for (int i = 0; i < pN; i++) {
					if (version == 2) {
						IntPtr fieldPtr = Marshal.ReadIntPtr (pazValue, i*IntPtr.Size);
						data_row[i] = Sqlite.HeapToString (fieldPtr, ((SqliteConnection)cmd.Connection).Encoding);
					} else {
						switch (Sqlite.sqlite3_column_type (pVm, i)) {
							case 1:
								long val = Sqlite.sqlite3_column_int64 (pVm, i);
							
								// If the column was declared as an 'int' or 'integer', let's play
								// nice and return an int (version 3 only).
								if (declmode[i] == 1 && val >= int.MinValue && val <= int.MaxValue)
									data_row[i] = (int)val;
								
								// Or if it was declared a date or datetime, do the reverse of what we
								// do for DateTime parameters.
								else if (declmode[i] == 2)
									data_row[i] = DateTime.FromFileTime(val);
								
								else
									data_row[i] = val;
									
								break;
							case 2:
								data_row[i] = Sqlite.sqlite3_column_double (pVm, i);
								break;
							case 3:
								data_row[i] = Marshal.PtrToStringUni (Sqlite.sqlite3_column_text16 (pVm, i));
								
								// If the column was declared as a 'date' or 'datetime', let's play
								// nice and return a DateTime (version 3 only).
								if (declmode[i] == 2)
									data_row[i] = DateTime.Parse((string)data_row[i]);
								break;
							case 4:
								int blobbytes = Sqlite.sqlite3_column_bytes16 (pVm, i);
								IntPtr blobptr = Sqlite.sqlite3_column_blob (pVm, i);
								byte[] blob = new byte[blobbytes];
								Marshal.Copy (blobptr, blob, 0, blobbytes);
								data_row[i] = blob;
								break;
							case 5:
								data_row[i] = null;
								break;
							default:
								throw new ApplicationException ("FATAL: Unknown sqlite3_column_type");
						}
					}
				}
				
				rows.Add (data_row);
			}
		}
		internal void ReadingDone ()
		{
			records_affected = command.NumChanges ();
			reading = false;
		}
		
		#endregion

		#region  Public Methods
		
#if NET_2_0
		override
#endif
		public void Close ()
		{
			closed = true;
		}
		
#if NET_2_0
		protected override void Dispose (bool disposing)
		{
			if (disposing)
				Close ();
		}
#else
		public void Dispose ()
		{
			Close ();
		}
#endif
		
#if NET_2_0
		public override IEnumerator GetEnumerator ()
#else
		IEnumerator IEnumerable.GetEnumerator () 
#endif
		{
			return new DbEnumerator (this);
		}
		
#if NET_2_0
		override
#endif
		public DataTable GetSchemaTable () 
		{
			DataTable dataTableSchema = new DataTable ();
			
			dataTableSchema.Columns.Add ("ColumnName", typeof (String));
			dataTableSchema.Columns.Add ("ColumnOrdinal", typeof (Int32));
			dataTableSchema.Columns.Add ("ColumnSize", typeof (Int32));
			dataTableSchema.Columns.Add ("NumericPrecision", typeof (Int32));
			dataTableSchema.Columns.Add ("NumericScale", typeof (Int32));
			dataTableSchema.Columns.Add ("IsUnique", typeof (Boolean));
			dataTableSchema.Columns.Add ("IsKey", typeof (Boolean));
			dataTableSchema.Columns.Add ("BaseCatalogName", typeof (String));
			dataTableSchema.Columns.Add ("BaseColumnName", typeof (String));
			dataTableSchema.Columns.Add ("BaseSchemaName", typeof (String));
			dataTableSchema.Columns.Add ("BaseTableName", typeof (String));
			dataTableSchema.Columns.Add ("DataType", typeof(Type));
			dataTableSchema.Columns.Add ("AllowDBNull", typeof (Boolean));
			dataTableSchema.Columns.Add ("ProviderType", typeof (Int32));
			dataTableSchema.Columns.Add ("IsAliased", typeof (Boolean));
			dataTableSchema.Columns.Add ("IsExpression", typeof (Boolean));
			dataTableSchema.Columns.Add ("IsIdentity", typeof (Boolean));
			dataTableSchema.Columns.Add ("IsAutoIncrement", typeof (Boolean));
			dataTableSchema.Columns.Add ("IsRowVersion", typeof (Boolean));
			dataTableSchema.Columns.Add ("IsHidden", typeof (Boolean));
			dataTableSchema.Columns.Add ("IsLong", typeof (Boolean));
			dataTableSchema.Columns.Add ("IsReadOnly", typeof (Boolean));
			
			dataTableSchema.BeginLoadData();
			for (int i = 0; i < this.FieldCount; i += 1 ) {
				
				DataRow schemaRow = dataTableSchema.NewRow ();
				
				schemaRow["ColumnName"] = columns[i];
				schemaRow["ColumnOrdinal"] = i;
				schemaRow["ColumnSize"] = 0;
				schemaRow["NumericPrecision"] = 0;
				schemaRow["NumericScale"] = 0;
				schemaRow["IsUnique"] = false;
				schemaRow["IsKey"] = false;
				schemaRow["BaseCatalogName"] = "";
				schemaRow["BaseColumnName"] = columns[i];
				schemaRow["BaseSchemaName"] = "";
				schemaRow["BaseTableName"] = "";
				schemaRow["DataType"] = typeof(string);
				schemaRow["AllowDBNull"] = true;
				schemaRow["ProviderType"] = 0;
				schemaRow["IsAliased"] = false;
				schemaRow["IsExpression"] = false;
				schemaRow["IsIdentity"] = false;
				schemaRow["IsAutoIncrement"] = false;
				schemaRow["IsRowVersion"] = false;
				schemaRow["IsHidden"] = false;
				schemaRow["IsLong"] = false;
				schemaRow["IsReadOnly"] = false;
				
				dataTableSchema.Rows.Add (schemaRow);
				schemaRow.AcceptChanges();
			}
			dataTableSchema.EndLoadData();
			
			return dataTableSchema;
		}
		
#if NET_2_0
		override
#endif
		public bool NextResult ()
		{
			current_row++;
			
			return (current_row < rows.Count);
		}
		
#if NET_2_0
		override
#endif
		public bool Read ()
		{
			return NextResult ();
		}

		#endregion
		
		#region IDataRecord getters
		
#if NET_2_0
		override
#endif
		public bool GetBoolean (int i)
		{
			return Convert.ToBoolean (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public byte GetByte (int i)
		{
			return Convert.ToByte (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public long GetBytes (int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			byte[] data = (byte[])(((object[]) rows[current_row])[i]);
			if (buffer != null)
				Array.Copy (data, fieldOffset, buffer, bufferOffset, length);
			return data.LongLength - fieldOffset;
		}
		
#if NET_2_0
		override
#endif
		public char GetChar (int i)
		{
			return Convert.ToChar (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public long GetChars (int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
		{
			char[] data = (char[])(((object[]) rows[current_row])[i]);
			if (buffer != null)
				Array.Copy (data, fieldOffset, buffer, bufferOffset, length);
			return data.LongLength - fieldOffset;
		}
		
#if !NET_2_0
		public IDataReader GetData (int i)
		{
			return ((IDataReader) this [i]);
		}
#endif
		
#if NET_2_0
		override
#endif
		public string GetDataTypeName (int i)
		{
			if (decltypes != null && decltypes[i] != null)
				return decltypes[i];
			return "text"; // SQL Lite data type
		}
		
#if NET_2_0
		override
#endif
		public DateTime GetDateTime (int i)
		{
			return Convert.ToDateTime (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public decimal GetDecimal (int i)
		{
			return Convert.ToDecimal (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public double GetDouble (int i)
		{
			return Convert.ToDouble (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public Type GetFieldType (int i)
		{
			int row = current_row;
			if (row == -1 && rows.Count == 0) return typeof(string);
			if (row == -1) row = 0;
			object element = ((object[]) rows[row])[i];
			if (element != null)
				return element.GetType();
			else
				return typeof (string);

			// Note that the return value isn't guaranteed to
			// be the same as the rows are read if different
			// types of information are stored in the column.
		}
		
#if NET_2_0
		override
#endif
		public float GetFloat (int i)
		{
			return Convert.ToSingle (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public Guid GetGuid (int i)
		{
			object value = GetValue (i);
			if (!(value is Guid)) {
				if (value is DBNull)
					throw new SqliteExecutionException ("Column value must not be null");
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return ((Guid) value);
		}
		
#if NET_2_0
		override
#endif
		public short GetInt16 (int i)
		{
			return Convert.ToInt16 (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public int GetInt32 (int i)
		{
			return Convert.ToInt32 (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public long GetInt64 (int i)
		{
			return Convert.ToInt64 (((object[]) rows[current_row])[i]);
		}
		
#if NET_2_0
		override
#endif
		public string GetName (int i)
		{
			return columns[i];
		}
		
#if NET_2_0
		override
#endif
		public int GetOrdinal (string name)
		{
			object v = column_names_sens[name];
			if (v == null)
				v = column_names_insens[name];
			if (v == null)
				throw new ArgumentException("Column does not exist.");
			return (int) v;
		}
		
#if NET_2_0
		override
#endif
		public string GetString (int i)
		{
			return (((object[]) rows[current_row])[i]).ToString();
		}
		
#if NET_2_0
		override
#endif
		public object GetValue (int i)
		{
			return ((object[]) rows[current_row])[i];
		}
		
#if NET_2_0
		override
#endif
		public int GetValues (object[] values)
		{
			int num_to_fill = System.Math.Min (values.Length, columns.Length);
			for (int i = 0; i < num_to_fill; i++) {
				if (((object[]) rows[current_row])[i] != null) {
					values[i] = ((object[]) rows[current_row])[i];
				} else {
					values[i] = DBNull.Value;
				}
			}
			return num_to_fill;
		}
		
#if NET_2_0
		override
#endif
		public bool IsDBNull (int i)
		{
			return (((object[]) rows[current_row])[i] == null);
		}

#if NET_2_0
		public override bool HasRows {
			get { return rows.Count > 0; }
		}

		// [MonoTODO]
		public override int VisibleFieldCount {
			get { return FieldCount; }
		}
#endif
		#endregion
	}
}
