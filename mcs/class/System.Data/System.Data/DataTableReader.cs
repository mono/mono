//
// System.Data.DataTableReader.cs
//
// Author:
//   Sureshkumar T      <tsureshkumar@novell.com>
//   Tim Coleman        (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;
using System.Data.Common;

namespace System.Data {
        public sealed class DataTableReader : DbDataReader
        {
                bool            _closed;
                DataTable []    _tables;
                IEnumerator     _enumerator;
                int             _current = -1;
                int             _index;
                DataTable       _schemaTable;

                #region Constructors

                
                public DataTableReader (DataTable dt)
                        : this (new DataTable[] {dt})
                {
                }
                
                public DataTableReader (DataTable[] dataTables)
                {
                        if (dataTables == null 
                            || dataTables.Length <= 0)
                                throw new ArgumentException ("Cannot Create DataTable. Argument Empty!");

                        this._tables = new DataTable [dataTables.Length];

                        for (int i = 0; i < dataTables.Length; i++)
                                this._tables [i] = dataTables [i];
                        
                        _closed = false;
                        _index = 0;
                        _current = -1;
                }

                #endregion // Constructors

                #region Properties

                public override int Depth {
                        get { return 0; }
                }

                public override int FieldCount {
                        get { return CurrentTable.Columns.Count; }
                }

                public override bool HasRows {
                        get { return CurrentTable.Rows.Count > 0; }
                }

                public override bool IsClosed {
                        get { return _closed; }
                }
                
                public override object this [int index] {
                        get { 
                                Validate ();
                                if (index < 0 || index >= FieldCount)
                                        throw new ArgumentOutOfRangeException (String.Format ("index {0} is not" +
                                                                                           "in the range",
                                                                                           index)
                                                                            );
                                DataRow row = CurrentRow;
                                if (row.RowState == DataRowState.Deleted)
                                        throw new InvalidOperationException ("Deleted Row's information cannot be accessed!");
                                return row [index];
                        }
                }

                private DataTable CurrentTable
                {
                        get { return _tables [_index]; }
                }
                
                private DataRow CurrentRow
                {
                        get { return (DataRow) CurrentTable.Rows [_current]; }
                }

                
                public override object this [string name] {
                        get {
                                Validate ();
                                DataRow row = CurrentRow;
                                if (row.RowState == DataRowState.Deleted)
                                        throw new InvalidOperationException ("Deleted Row's information cannot be accessed!");
                                return row [name];
                        }
                }
                
                public override int RecordsAffected {
                        get { return 0; }
                }
                
                public override int VisibleFieldCount {
                        get { return CurrentTable.Columns.Count; }
                }

                #endregion // Properties

                #region Methods
                
                public override void Close ()
                {
                        _closed = true;
                }
                
                public override void Dispose ()
                {
                        Close ();
                }
                
                public override bool GetBoolean (int i)
                {
                        return (bool) GetValue (i);
                }
                
                public override byte GetByte (int i)
                {
                        return (byte) GetValue (i);
                }

                [MonoTODO]
                public override long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
                {
                        throw new NotImplementedException ();
                }
                
                public override char GetChar (int i)
                {
                        return (char) GetValue (i);
                }

                [MonoTODO]
                public override long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
                {
                        throw new NotImplementedException ();
                }
                
                public override string GetDataTypeName (int i)
                {
                        return GetFieldType (i).ToString ();
                }
                
                public override DateTime GetDateTime (int i)
                {
                        return (DateTime) GetValue (i);
                }
                
                public override decimal GetDecimal (int i)
                {
                        return (decimal) GetValue (i);
                }
                
                public override double GetDouble (int i)
                {
                        return (double) GetValue (i);
                }

                [MonoTODO]
                public override IEnumerator GetEnumerator ()
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                public override Type GetFieldProviderSpecificType (int i)
                {
                        throw new NotImplementedException ();
                }
                
                public override Type GetFieldType (int i)
                {
                        ValidateClosed ();
                        return CurrentTable.Columns [i].DataType;
                }
                
                public override float GetFloat (int i)
                {
                        return (float) GetValue (i);
                }
                
                public override Guid GetGuid (int i)
                {
                        return (Guid) GetValue (i);
                }
                
                public override short GetInt16 (int i)
                {
                        return (short) GetValue (i);
                }
                
                public override int GetInt32 (int i)
                {
                        return (int) GetValue (i);
                }
                
                public override long GetInt64 (int i)
                {
                        return (long) GetValue (i);
                }
                
                public override string GetName (int i)
                {
                        return (string) GetValue (i);
                }
                
                public override int GetOrdinal (string name)
                {
                        ValidateClosed ();
                        DataColumn column = CurrentTable.Columns [name];
                        if (column == null)
                                throw new ArgumentException (String.Format ("Column {0} is not found in the schema",
                                                                             name));
                        return column.Ordinal;
                }

                [MonoTODO]
                public override object GetProviderSpecificValue (int i)
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                public override int GetProviderSpecificValues (object[] values)
                {
                        throw new NotImplementedException ();
                }
                
                public override string GetString (int i)
                {
                        return (string) GetValue (i);
                }
                
                public override object GetValue (int i)
                {
                        return this [i];
                }

                [MonoTODO]
                public override int GetValues (object[] values)
                {
                        throw new NotImplementedException ();
                }
                
                public override bool IsDBNull (int i)
                {
                        return GetValue (i) is DBNull;
                }
                
                public override DataTable GetSchemaTable ()
                {
                        if (_schemaTable != null)
                                return _schemaTable;
                        
                        DataTable dt = DbDataReader.GetSchemaTableTemplate ();
                        foreach (DataColumn column in CurrentTable.Columns) {
                                DataRow row = dt.NewRow ();

                                row ["ColumnName"]      = column.ColumnName;
                                row ["ColumnOrdinal"]   = column.Ordinal;
                                row ["ColumnSize"]      = column.MaxLength;
                                row ["NumericPrecision"]= DBNull.Value;
                                row ["NumericScale"]    = DBNull.Value;
                                row ["IsUnique"]        = DBNull.Value;
                                row ["IsKey"]           = DBNull.Value;
                                row ["DataType"]        = column.DataType.ToString ();
                                row ["AllowDBNull"]     = column.AllowDBNull;
                                row ["IsAliased"]       = DBNull.Value;
                                row ["IsExpression"]    = DBNull.Value;
                                row ["IsIdentity"]      = DBNull.Value;
                                row ["IsAutoIncrement"] = DBNull.Value;
                                row ["IsRowVersion"]    = DBNull.Value;
                                row ["IsHidden"]        = DBNull.Value;
                                row ["IsLong"]          = DBNull.Value;
                                row ["IsReadOnly"]      = column.ReadOnly;

                                dt.Rows.Add (row);
                        }
                        return _schemaTable = dt;
                }

                private void Validate ()
                {
			ValidateClosed ();
			if (_current == -1)
                                throw new InvalidOperationException ("Invalid attempt to read before " + 
                                                                     "Read is called");
                }

                private void ValidateClosed ()
                {
                        if (IsClosed)
                                throw new InvalidOperationException ("Invalid attempt to read when " + 
                                                                     "the reader is closed");
                }
                
                
                private bool MoveNext ()
                {
                        do {
                                _current++;
                        } while (_current < CurrentTable.Rows.Count 
                                 && CurrentRow.RowState == DataRowState.Deleted);
                        return _current < CurrentTable.Rows.Count;
                }

                public override bool NextResult ()
                {
                        _current = -1;
                        _index++;
                        _schemaTable = null;            // force to create fresh
                        return _index < _tables.Length;
                }

                public override bool Read ()
                {       
                        ValidateClosed ();
                        return MoveNext ();
                }

                #endregion // Methods
        }
}

#endif // NET_2_0
