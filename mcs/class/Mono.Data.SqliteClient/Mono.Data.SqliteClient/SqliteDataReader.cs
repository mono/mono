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

namespace Mono.Data.SqliteClient
{
        public class SqliteDataReader : IDataReader
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

                public DataTable GetSchemaTable ()
                {

#if NOTDEF
                        // We sort of cheat here since sqlite treats all types as strings
                        // we -could- parse the table definition (since that's the only info
                        // that we can get out of sqlite about the table), but it's probably
                        // not worth it.
                        DataTable data_table = new DataTable ();

                        DataColumn col;
                        DataRow row;

                        // first create the columns
                        for (int i = 0; i < column_names.Count; i++) {
                                col = new DataColumn ();
                                col.DataType = System.Type.GetType("System.String");
                                col.ColumnName = columns[i];
                                data_table.Columns.Add (col);
                        }

                        // then loop through the rows
                        for (int i = 0; i < rows.Length; i++) {
                                row = data_table.NewRow ();
                                for (int j = 0; j < column_names.Count; j++) {
                                        row[columns[j]] = rows[i][j];
                                }
                                data_table.Rows.Add (row);
                        }

                        return data_table;
#else
                        return null;
#endif
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
                        return "System.String";
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
                        return System.Type.GetType ("System.String");
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
                        int num_to_fill = Math.Min (values.Length, columns.Count);
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
