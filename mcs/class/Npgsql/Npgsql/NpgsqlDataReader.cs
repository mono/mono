
// Npgsql.NpgsqlDataReader.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Collections;

using NpgsqlTypes;

namespace Npgsql
{
    /// <summary>
    /// Provides a means of reading a forward-only stream of rows from a PostgreSQL backend.  This class cannot be inherited.
    /// </summary>
    public sealed class NpgsqlDataReader : IDataReader, IEnumerable
    {
        private NpgsqlConnection 	_connection;
        private ArrayList 			_resultsets;
        private ArrayList			_responses;
        private Int32 				_rowIndex;
        private Int32				_resultsetIndex;
        private	NpgsqlResultSet		_currentResultset;
        private DataTable			_currentResultsetSchema;
        private CommandBehavior     _behavior;
        private Boolean             _isClosed;


        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlDataReader";

        internal NpgsqlDataReader( ArrayList resultsets, ArrayList responses, NpgsqlConnection connection, CommandBehavior behavior)
        {
            _resultsets = resultsets;
            _responses = responses;
            _connection = connection;
            _rowIndex = -1;
            _resultsetIndex = 0;

            if (_resultsets.Count > 0)
                _currentResultset = (NpgsqlResultSet)_resultsets[_resultsetIndex];

            _behavior = behavior;
            _isClosed = false;
        }

        private Boolean HaveResultSet()
        {
            return (_currentResultset != null);
        }

        private Boolean HaveRow()
        {
            return (HaveResultSet() && _rowIndex >= 0 && _rowIndex < _currentResultset.Count);
        }

        private void CheckHaveResultSet()
        {
            if (! HaveResultSet())
            {
                throw new InvalidOperationException("Cannot read data. No result set.");
            }
        }

        private void CheckHaveRow()
        {
            CheckHaveResultSet();

            if (_rowIndex < 0)
            {
                throw new InvalidOperationException("DataReader positioned before beginning of result set. Did you call Read()?");
            }
            else if (_rowIndex >= _currentResultset.Count)
            {
                throw new InvalidOperationException("DataReader positioned beyond end of result set.");
            }
        }


        /// <summary>
        /// Releases the resources used by the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases the resources used by the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        protected void Dispose (bool disposing)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Dispose");
            if (disposing)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.  Always returns zero.
        /// </summary>
        public Int32 Depth
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Depth");
                return 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        public Boolean IsClosed
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsClosed");
                return _isClosed;
            }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        public Int32 RecordsAffected
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "RecordsAffected");

                if (HaveResultSet())
                {
                    return -1;
                }

                String[] _returnStringTokens = ((String)_responses[_resultsetIndex]).Split(null);	// whitespace separator.

                try
                {
                    return Int32.Parse(_returnStringTokens[_returnStringTokens.Length - 1]);
                }
                catch (FormatException)
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Indicates if NpgsqlDatareader has rows to be read.
        /// </summary>

        public Boolean HasRows
        {
            get
            {
                return _currentResultset.Count > 0;
            }

        }

        /// <summary>
        /// Closes the data reader object.
        /// </summary>
        public void Close()
        {
            if ((_behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
            {
                _connection.Close();

            }

            _isClosed = true;
        }

        /// <summary>
        /// Advances the data reader to the next result, when multiple result sets were returned by the PostgreSQL backend.
        /// </summary>
        /// <returns>True if the reader was advanced, otherwise false.</returns>
        public Boolean NextResult()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "NextResult");

            if((_resultsetIndex + 1) < _resultsets.Count)
            {
                _resultsetIndex++;
                _rowIndex = -1;
                _currentResultset = (NpgsqlResultSet)_resultsets[_resultsetIndex];
                return true;
            }
            else
                return false;

        }

        /// <summary>
        /// Advances the data reader to the next row.
        /// </summary>
        /// <returns>True if the reader was advanced, otherwise false.</returns>
        public Boolean Read()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Read");

            CheckHaveResultSet();

            if (_rowIndex < _currentResultset.Count)
            {
                _rowIndex++;
                return (_rowIndex < _currentResultset.Count);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a System.Data.DataTable that describes the column metadata of the DataReader.
        /// </summary>
        public DataTable GetSchemaTable()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetSchemaTable");

            if(_currentResultsetSchema == null)
                _currentResultsetSchema = GetResultsetSchema();

            return _currentResultsetSchema;
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public Int32 FieldCount
        {
            get
            {

                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "FieldCount");

                if (! HaveResultSet()) //Executed a non return rows query.
                    return -1;
                else
                    return _currentResultset.RowDescription.NumFields;


            }

        }

        /// <summary>
        /// Return the column name of the column at index <param name="Index"></param>.
        /// </summary>
        public String GetName(Int32 Index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetName");

            CheckHaveResultSet();

            return _currentResultset.RowDescription[Index].name;
        }

        /// <summary>
        /// Return the data type OID of the column at index <param name="Index"></param>.
        /// </summary>
        public String GetDataTypeOID(Int32 Index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDataTypeName");

            CheckHaveResultSet();

            NpgsqlBackendTypeInfo  TI = GetTypeInfo(Index);

            return _currentResultset.RowDescription[Index].type_oid.ToString();
        }

        /// <summary>
        /// Return the data type name of the column at index <param name="Index"></param>.
        /// </summary>
        public String GetDataTypeName(Int32 Index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDataTypeName");

            CheckHaveResultSet();

            NpgsqlBackendTypeInfo  TI = GetTypeInfo(Index);

            if (TI == null)
            {
                return _currentResultset.RowDescription[Index].type_oid.ToString();
            }
            else
            {
                return TI.Name;
            }
        }

        /// <summary>
        /// Return the data type of the column at index <param name="Index"></param>.
        /// </summary>
        public Type GetFieldType(Int32 Index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetFieldType");

            CheckHaveResultSet();

            NpgsqlBackendTypeInfo  TI = GetTypeInfo(Index);

            if (TI == null)
            {
                return typeof(String);  //Default type is string.
            }
            else
            {
                return TI.Type;
            }
        }

        /// <summary>
        /// Return the data DbType of the column at index <param name="Index"></param>.
        /// </summary>
        public DbType GetFieldDbType(Int32 Index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetFieldType");

            CheckHaveResultSet();

            NpgsqlBackendTypeInfo  TI = GetTypeInfo(Index);

            if (TI == null)
            {
                return DbType.String;
            }
            else
            {
                //return TI.DBType;
                return DbType.String;
            }
        }

        /// <summary>
        /// Return the data NpgsqlDbType of the column at index <param name="Index"></param>.
        /// </summary>
        public NpgsqlDbType GetFieldNpgsqlDbType(Int32 Index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetFieldType");

            CheckHaveResultSet();

            NpgsqlBackendTypeInfo  TI = GetTypeInfo(Index);

            if (TI == null)
            {
                return NpgsqlDbType.Text;
            }
            else
            {
                return TI.NpgsqlDbType;

            }
        }


        /// <summary>
        /// Return the value of the column at index <param name="Index"></param>.
        /// </summary>
        public Object GetValue(Int32 Index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetValue");

            if (Index < 0 || Index >= _currentResultset.RowDescription.NumFields)
            {
                throw new IndexOutOfRangeException("Column index out of range");
            }

            CheckHaveRow();

            return ((NpgsqlAsciiRow)_currentResultset[_rowIndex])[Index];
        }

        /// <summary>
        /// Copy values from each column in the current row into <param name="Values"></param>.
        /// </summary>
        /// <returns>The number of column values copied.</returns>
        public Int32 GetValues(Object[] Values)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetValues");

            CheckHaveRow();

            // Only the number of elements in the array are filled.
            // It's also possible to pass an array with more that FieldCount elements.
            Int32 maxColumnIndex = (Values.Length < FieldCount) ? Values.Length : FieldCount;

            for (Int32 i = 0; i < maxColumnIndex; i++)
            {
                Values[i] = GetValue(i);
            }

            return maxColumnIndex;

        }

        /// <summary>
        /// Return the column name of the column named <param name="Name"></param>.
        /// </summary>
        public Int32 GetOrdinal(String Name)
        {
            CheckHaveResultSet();
            return _currentResultset.RowDescription.FieldIndex(Name);
        }

        /// <summary>
        /// Gets the value of a column in its native format.
        /// </summary>
        public Object this [ Int32 i ]
        {
            get
            {
                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, i);
                return GetValue(i);
            }
        }

        /// <summary>
        /// Gets the value of a column in its native format.
        /// </summary>
        public Object this [ String name ]
        {
            get
            {
                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, name);
                return GetValue(_currentResultset.RowDescription.FieldIndex(name));
            }
        }

        /// <summary>
        /// Gets the value of a column converted to a Boolean.
        /// </summary>
        public Boolean GetBoolean(Int32 i)
        {
            // Should this be done using the GetValue directly and not by converting to String
            // and parsing from there?
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBoolean");

            return Convert.ToBoolean(GetValue(i));
        }

        /// <summary>
        /// Gets the value of a column converted to a Byte.  Not implemented.
        /// </summary>
        public Byte GetByte(Int32 i)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets raw data from a column.
        /// </summary>
        public Int64 GetBytes(Int32 i, Int64 fieldOffset, Byte[] buffer, Int32 bufferoffset, Int32 length)
        {

            Byte[] result;

            result = (Byte[]) GetValue(i);

            if (buffer == null)
                return result.Length;


            // We just support read all the field for while. So, any fieldOffset value other than 0 will not read
            // anything and return 0.

            if (fieldOffset != 0)
                return 0;

            // [TODO] Implement blob support.

            result.CopyTo(buffer, 0);


            return result.Length;

        }

        /// <summary>
        /// Gets the value of a column converted to a Char.  Not implemented.
        /// </summary>
        public Char GetChar(Int32 i)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets raw data from a column.
        /// </summary>
        public Int64 GetChars(Int32 i, Int64 fieldoffset, Char[] buffer, Int32 bufferoffset, Int32 length)
        {
            String		str;

            str = GetString(i);
            if (buffer == null)
                return str.Length;

            str.ToCharArray(bufferoffset, length).CopyTo(buffer, 0);
            return buffer.GetLength(0);
        }

        /// <summary>
        /// Gets the value of a column converted to a Guid.  Not implemented.
        /// </summary>
        public Guid GetGuid(Int32 i)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of a column converted to Int16.
        /// </summary>
        public Int16 GetInt16(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt16");

            return Convert.ToInt16(GetValue(i));
        }

        /// <summary>
        /// Gets the value of a column converted to Int32.
        /// </summary>
        public Int32 GetInt32(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt32");

            return Convert.ToInt32(GetValue(i));
        }

        /// <summary>
        /// Gets the value of a column converted to Int64.
        /// </summary>
        public Int64 GetInt64(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt64");

            return Convert.ToInt64(GetValue(i));
        }

        /// <summary>
        /// Gets the value of a column converted to Single.
        /// </summary>
        public Single GetFloat(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetFloat");

            return Convert.ToSingle(GetValue(i));
        }

        /// <summary>
        /// Gets the value of a column converted to Double.
        /// </summary>
        public Double GetDouble(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDouble");

            return Convert.ToDouble(GetValue(i));
        }

        /// <summary>
        /// Gets the value of a column converted to a String.
        /// </summary>
        public String GetString(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetString");

            return Convert.ToString(GetValue(i));
        }

        /// <summary>
        /// Gets the value of a column converted to Decimal.
        /// </summary>
        public Decimal GetDecimal(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDecimal");

            return Convert.ToDecimal(GetValue(i));
        }

        /// <summary>
        /// Gets the value of a column converted to a DateTime.
        /// </summary>
        public DateTime GetDateTime(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDateTime");

            return Convert.ToDateTime(GetValue(i));
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public IDataReader GetData(Int32 i)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Report whether the value in a column is DBNull.
        /// </summary>
        public Boolean IsDBNull(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IsDBNull");

            return (GetValue(i) == DBNull.Value);
        }

        internal NpgsqlBackendTypeInfo GetTypeInfo(Int32 FieldIndex)
        {
            return _currentResultset.RowDescription[FieldIndex].type_info;
        }

        private DataTable GetResultsetSchema()
        {

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetResultsetSchema");
            DataTable result = null;

            NpgsqlRowDescription rd = _currentResultset.RowDescription;
            Int16 numFields = rd.NumFields;
            if(numFields > 0)
            {
                result = new DataTable("SchemaTable");

                result.Columns.Add ("ColumnName", typeof (string));
                result.Columns.Add ("ColumnOrdinal", typeof (int));
                result.Columns.Add ("ColumnSize", typeof (int));
                result.Columns.Add ("NumericPrecision", typeof (int));
                result.Columns.Add ("NumericScale", typeof (int));
                result.Columns.Add ("IsUnique", typeof (bool));
                result.Columns.Add ("IsKey", typeof (bool));
                DataColumn dc = result.Columns["IsKey"];
                dc.AllowDBNull = true; // IsKey can have a DBNull
                result.Columns.Add ("BaseCatalogName", typeof (string));
                result.Columns.Add ("BaseColumnName", typeof (string));
                result.Columns.Add ("BaseSchemaName", typeof (string));
                result.Columns.Add ("BaseTableName", typeof (string));
                result.Columns.Add ("DataType", typeof(Type));
                result.Columns.Add ("AllowDBNull", typeof (bool));
                result.Columns.Add ("ProviderType", typeof (int));
                result.Columns.Add ("IsAliased", typeof (bool));
                result.Columns.Add ("IsExpression", typeof (bool));
                result.Columns.Add ("IsIdentity", typeof (bool));
                result.Columns.Add ("IsAutoIncrement", typeof (bool));
                result.Columns.Add ("IsRowVersion", typeof (bool));
                result.Columns.Add ("IsHidden", typeof (bool));
                result.Columns.Add ("IsLong", typeof (bool));
                result.Columns.Add ("IsReadOnly", typeof (bool));

                DataRow row;

                for (Int16 i = 0; i < numFields; i++)
                {
                    row = result.NewRow();

                    row["ColumnName"] = GetName(i);
                    row["ColumnOrdinal"] = i + 1;
                    row["ColumnSize"] = (int) rd[i].type_size;
                    row["NumericPrecision"] = 0;
                    row["NumericScale"] = 0;
                    row["IsUnique"] = false;
                    row["IsKey"] = DBNull.Value;
                    row["BaseCatalogName"] = "";
                    row["BaseColumnName"] = GetName(i);
                    row["BaseSchemaName"] = "";
                    row["BaseTableName"] = "";
                    row["DataType"] = GetFieldType(i);
                    row["AllowDBNull"] = false;
                    row["ProviderType"] = (int) rd[i].type_oid;
                    row["IsAliased"] = false;
                    row["IsExpression"] = false;
                    row["IsIdentity"] = false;
                    row["IsAutoIncrement"] = false;
                    row["IsRowVersion"] = false;
                    row["IsHidden"] = false;
                    row["IsLong"] = false;
                    row["IsReadOnly"] = false;

                    result.Rows.Add(row);
                }
            }

            return result;

        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return new System.Data.Common.DbEnumerator (this);
        }
    }
}
