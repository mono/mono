// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteDataReader.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SqliteClient
{
	public class SqliteDataReader : MarshalByRefObject,
		IEnumerable, IDataReader, IDisposable, IDataRecord
        {
                SqliteCommand command;
                ArrayList rows;
                ArrayList columns;
                Hashtable column_names;
                int current_row;
                bool closed;
                bool reading;
                int records_affected;

                internal SqliteDataReader (SqliteCommand cmd)
                {
                        command = cmd;
                        rows = new ArrayList ();
                        columns = new ArrayList ();
                        column_names = new Hashtable ();
                        closed = false;
                        current_row = -1;
                        reading = true;
                }

                internal void ReadingDone ()
                {
                        records_affected = command.NumChanges ();
                        reading = false;
                }

                public void Close ()
                {
                        closed = true;
                }

                public void Dispose ()
                {
                        // nothing to do
                }

		IEnumerator IEnumerable.GetEnumerator () {
			return new DbEnumerator (this);
		}

		public DataTable GetSchemaTable () {
			
			// We sort of cheat here since sqlite treats all types as strings
			// we -could- parse the table definition (since that's the only info
			// that we can get out of sqlite about the table), but it's probably
			// not worth it.

			DataTable dataTableSchema  = null;

			DataColumn dc;
			DataRow schemaRow;

			// only create the schema DataTable if
			// there is fields in a result set due
			// to the result of a query; otherwise,
			// a null needs to be returned
			if(this.FieldCount > 0) {
				
				dataTableSchema = new DataTable ();
				
				dataTableSchema.Columns.Add ("ColumnName", typeof (string));
				dataTableSchema.Columns.Add ("ColumnOrdinal", typeof (int));
				dataTableSchema.Columns.Add ("ColumnSize", typeof (int));
				dataTableSchema.Columns.Add ("NumericPrecision", typeof (int));
				dataTableSchema.Columns.Add ("NumericScale", typeof (int));
				dataTableSchema.Columns.Add ("IsUnique", typeof (bool));
				dataTableSchema.Columns.Add ("IsKey", typeof (bool));
				dc = dataTableSchema.Columns["IsKey"];
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

				for (int i = 0; i < this.FieldCount; i += 1 ) {
					
					schemaRow = dataTableSchema.NewRow ();
										
					schemaRow["ColumnName"] = columns[i];
					schemaRow["ColumnOrdinal"] = i + 1;
					
					// FIXME: how do you determine the column size
					//        using SQL Lite?
					int columnSize = 8192; // pulled out of the air
					schemaRow["ColumnSize"] = columnSize;
					schemaRow["NumericPrecision"] = 0;
					schemaRow["NumericScale"] = 0;

					schemaRow["IsUnique"] = false;
					schemaRow["IsKey"] = DBNull.Value;
					
					schemaRow["BaseCatalogName"] = "";
					
					schemaRow["BaseColumnName"] = columns[i];
					schemaRow["BaseSchemaName"] = "";
					schemaRow["BaseTableName"] = "";

					// FIXME: don't know how to determine
					// the .NET type based on the
					// SQL Lite data type
					// Use string
					schemaRow["DataType"] = typeof(string);

					schemaRow["AllowDBNull"] = true;
					
					// FIXME: don't know how to get the
					//  SQL Lite data type
					int providerType = 0; // out of the air
					schemaRow["ProviderType"] = providerType;
					
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
			}
			return dataTableSchema;
		}

                public bool NextResult ()
                {
                        current_row++;
                        if (current_row < rows.Count)
                                return true;
                        return false;
                }

                public bool Read ()
                {
                        return NextResult ();
                }

                public int Depth {
                        get {
                                return 0;
                        }
                }

                public bool IsClosed {
                        get {
                                return closed;
                        }
                }

                public int RecordsAffected {
                        get {
                                return records_affected;
                        }
                }

                // sqlite callback
                internal unsafe int SqliteCallback (ref object o, int argc, sbyte **argv, sbyte **colnames)
                {
                        // cache names of columns if we need to
                        if (column_names.Count == 0) {
                                for (int i = 0; i < argc; i++) {
                                        string col = new String (colnames[i]);
                                        columns.Add (col);
                                        column_names[col.ToLower ()] = i++;
                                }
                        }

                        ArrayList data_row = new ArrayList (argc);
                        for (int i = 0; i < argc; i++) {
                                if (argv[i] != ((sbyte *)0)) {
                                        data_row.Add(new String (argv[i]));
                                } else {
                                        data_row.Add(null);
                                }
                        }
                        rows.Add (data_row);
                        return 0;
                }

                //
                // IDataRecord getters
                //

		public bool GetBoolean (int i)
                {
                        return Convert.ToBoolean ((string) ((ArrayList) rows[current_row])[i]);
                }

		public byte GetByte (int i)
                {
                        return Convert.ToByte ((string) ((ArrayList) rows[current_row])[i]);
                }

		public long GetBytes (int i, long fieldOffset, byte[] buffer, 
                               int bufferOffset, int length)
                {
                        throw new NotImplementedException ();
                }

		public char GetChar (int i)
                {
                        return Convert.ToChar ((string) ((ArrayList) rows[current_row])[i]);
                }

		public long GetChars (int i, long fieldOffset, char[] buffer, 
                               int bufferOffset, int length)
                {
                        throw new NotImplementedException ();
                }

		public IDataReader GetData (int i)
                {
                        // sigh.. in the MSDN docs, it says that "This member supports the
                        // .NET Framework infrastructure and is not nitended to be used
                        // directly from your code." -- so why the hell is it in the public
                        // interface?
                        throw new NotImplementedException ();
                }

		public string GetDataTypeName (int i)
                {
                        return "text"; // SQL Lite data type
                }

		public DateTime GetDateTime (int i)
                {
                        return Convert.ToDateTime ((string) ((ArrayList) rows[current_row])[i]);
                }

		public decimal GetDecimal (int i)
                {
                        return Convert.ToDecimal ((string) ((ArrayList) rows[current_row])[i]);
                }

		public double GetDouble (int i)
                {
                        return Convert.ToDouble ((string) ((ArrayList) rows[current_row])[i]);
                }

		public Type GetFieldType (int i)
                {
                        return System.Type.GetType ("System.String"); // .NET data type
                }

		public float GetFloat (int i)
                {
                        return Convert.ToSingle ((string) ((ArrayList) rows[current_row])[i]);
                }

		public Guid GetGuid (int i)
                {
                        throw new NotImplementedException ();
                }

		public short GetInt16 (int i)
                {
                        return Convert.ToInt16 ((string) ((ArrayList) rows[current_row])[i]);
                }

		public int GetInt32 (int i)
                {
                        return Convert.ToInt32 ((string) ((ArrayList) rows[current_row])[i]);
                }

		public long GetInt64 (int i)
                {
                        return Convert.ToInt64 ((string) ((ArrayList) rows[current_row])[i]);
                }

		public string GetName (int i)
                {
                        return (string) columns[i];
                }

		public int GetOrdinal (string name)
                {
                        return (int) column_names[name];
                }

		public string GetString (int i)
                {
                        return ((string) ((ArrayList) rows[current_row])[i]);
                }

		public object GetValue (int i)
                {
                        return ((ArrayList) rows[current_row])[i];
                }

		public int GetValues (object[] values)
                {
                        int num_to_fill = System.Math.Min (values.Length, columns.Count);
                        for (int i = 0; i < num_to_fill; i++) {
                                if (((ArrayList) rows[current_row])[i] != null) {
                                        values[i] = ((ArrayList) rows[current_row])[i];
                                } else {
                                        values[i] = DBNull.Value;
                                }
                        }
                        return num_to_fill;
                }

		public bool IsDBNull (int i)
                {
                        if (((ArrayList) rows[current_row])[i] == null)
                                return true;
                        return false;
                }

		public int FieldCount {
                        get {
                                if (current_row == -1 || current_row == rows.Count)
                                        return 0;
                                return columns.Count;
                        }
                }

		public object this[string name] {
                        get {
                                return ((ArrayList) rows[current_row])[(int) column_names[name]];
                        }
                }
		
		public object this[int i] {
                        get {
                                return ((ArrayList) rows[current_row])[i];
                        }
                }
        }
}
