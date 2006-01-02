//
// Mono.Data.SqliteClient.SqliteDataReader.cs
//
// Provides a means of reading a forward-only stream of rows from a Sqlite 
// database file.
//
// Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//            Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//			  Joshua Tauberer <tauberer@for.net>
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
	public class SqliteDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{

		#region Fields
		
		private SqliteCommand command;
		private ArrayList rows;
		private string[] columns;
		private Hashtable column_names;
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
			column_names = new Hashtable ();
			closed = false;
			current_row = -1;
			reading = true;
			ReadpVm (pVm, version, cmd);
			ReadingDone ();
		}
		
		#endregion

		#region Properties
		
		public int Depth {
			get { return 0; }
		}
		
		public int FieldCount {
			get { return columns.Length; }
		}
		
		public object this[string name] {
			get { return ((object[]) rows[current_row])[(int) column_names[name]]; }
		}
		
		public object this[int i] {
			get { return ((object[]) rows[current_row])[i]; }
		}
		
		public bool IsClosed {
			get { return closed; }
		}
		
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
							IntPtr decl = Sqlite.sqlite3_column_decltype (pVm, i);
							if (decl != IntPtr.Zero) {
								decltypes[i] = Marshal.PtrToStringAnsi (decl).ToLower();
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
							IntPtr fieldPtr = (IntPtr)Marshal.ReadInt32 (pazColName, i*IntPtr.Size);
							colName = Marshal.PtrToStringAnsi (fieldPtr);
						} else {
							colName = Marshal.PtrToStringAnsi (Sqlite.sqlite3_column_name (pVm, i));
						}
						columns[i] = colName;
						column_names [colName] = i;
					}
				}

				if (!hasdata) break;
				
				object[] data_row = new object [pN];
				for (int i = 0; i < pN; i++) {
					if (version == 2) {
						IntPtr fieldPtr = (IntPtr)Marshal.ReadInt32 (pazValue, i*IntPtr.Size);
						data_row[i] = Marshal.PtrToStringAnsi (fieldPtr);
					} else {
						switch (Sqlite.sqlite3_column_type (pVm, i)) {
							case 1:
								// If the column was declared as an 'int' or 'integer', let's play
								// nice and return an int (version 3 only).
								if (declmode[i] == 1)
									data_row[i] = (int)Sqlite.sqlite3_column_int64 (pVm, i);
								else
									data_row[i] = Sqlite.sqlite3_column_int64 (pVm, i);
								break;
							case 2:
								data_row[i] = Sqlite.sqlite3_column_double (pVm, i);
								break;
							case 3:
								data_row[i] = Marshal.PtrToStringAnsi (Sqlite.sqlite3_column_text (pVm, i));
								
								// If the column was declared as a 'date' or 'datetime', let's play
								// nice and return a DateTime (version 3 only).
								if (declmode[i] == 2)
									data_row[i] = DateTime.Parse((string)data_row[i]);
								break;
							case 4:
								int blobbytes = Sqlite.sqlite3_column_bytes (pVm, i);
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
		
		public void Close ()
		{
			closed = true;
		}
		
		public void Dispose ()
		{
			Close ();
		}
		
		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new DbEnumerator (this);
		}
		
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
		
		public bool NextResult ()
		{
			current_row++;
			
			return (current_row < rows.Count);
		}
		
		public bool Read ()
		{
			return NextResult ();
		}

		#endregion
		
		#region IDataRecord getters
		
		public bool GetBoolean (int i)
		{
			return Convert.ToBoolean (((object[]) rows[current_row])[i]);
		}
		
		public byte GetByte (int i)
		{
			return Convert.ToByte (((object[]) rows[current_row])[i]);
		}
		
		public long GetBytes (int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException ();
		}
		
		public char GetChar (int i)
		{
			return Convert.ToChar (((object[]) rows[current_row])[i]);
		}
		
		public long GetChars (int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException ();
		}
		
		public IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}
		
		public string GetDataTypeName (int i)
		{
			if (decltypes != null && decltypes[i] != null)
				return decltypes[i];
			return "text"; // SQL Lite data type
		}
		
		public DateTime GetDateTime (int i)
		{
			return Convert.ToDateTime (((object[]) rows[current_row])[i]);
		}
		
		public decimal GetDecimal (int i)
		{
			return Convert.ToDecimal (((object[]) rows[current_row])[i]);
		}
		
		public double GetDouble (int i)
		{
			return Convert.ToDouble (((object[]) rows[current_row])[i]);
		}
		
		public Type GetFieldType (int i)
		{
			return ((object[]) rows[current_row])[i].GetType();
		}
		
		public float GetFloat (int i)
		{
			return Convert.ToSingle (((object[]) rows[current_row])[i]);
		}
		
		public Guid GetGuid (int i)
		{
			throw new NotImplementedException ();
		}
		
		public short GetInt16 (int i)
		{
			return Convert.ToInt16 (((object[]) rows[current_row])[i]);
		}
		
		public int GetInt32 (int i)
		{
			return Convert.ToInt32 (((object[]) rows[current_row])[i]);
		}
		
		public long GetInt64 (int i)
		{
			return Convert.ToInt64 (((object[]) rows[current_row])[i]);
		}
		
		public string GetName (int i)
		{
			return columns[i];
		}
		
		public int GetOrdinal (string name)
		{
			return (int) column_names[name];
		}
		
		public string GetString (int i)
		{
			return (((object[]) rows[current_row])[i]).ToString();
		}
		
		public object GetValue (int i)
		{
			return ((object[]) rows[current_row])[i];
		}
		
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
		
		public bool IsDBNull (int i)
		{
			return (((object[]) rows[current_row])[i] == null);
		}
		        
		#endregion
	}
}
