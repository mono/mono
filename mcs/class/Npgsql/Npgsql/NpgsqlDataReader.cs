
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
using System.Text;

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
        private Int32               _recordsAffected;
        private	NpgsqlResultSet		_currentResultset;
        private DataTable			_currentResultsetSchema;
        private CommandBehavior     _behavior;
        private Boolean             _isClosed;
        private NpgsqlCommand       _command;


        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlDataReader";

        internal NpgsqlDataReader( ArrayList resultsets, ArrayList responses, CommandBehavior behavior, NpgsqlCommand command)
        {
            _resultsets = resultsets;
            _responses = responses;
            _connection = command.Connection;
            _rowIndex = -1;
            _resultsetIndex = -1;
            _recordsAffected = -1;

            // positioned before the first results.
            // move to the first results
            NextResult();

            _behavior = behavior;
            _isClosed = false;
            _command = command;
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
                return _recordsAffected;
            }
        }

        /// <summary>
        /// Indicates if NpgsqlDatareader has rows to be read.
        /// </summary>

        public Boolean HasRows
        {
            get
            {
	    	return (HaveResultSet() ? _currentResultset.Count > 0 : false);
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
            if (this.ReaderClosed != null)
	            this.ReaderClosed(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Is raised whenever Close() is called.
        /// </summary>
        public event EventHandler ReaderClosed;

        /// <summary>
        /// Advances the data reader to the next result, when multiple result sets were returned by the PostgreSQL backend.
        /// </summary>
        /// <returns>True if the reader was advanced, otherwise false.</returns>
        public Boolean NextResult()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "NextResult");

            _currentResultset = null;
            while((_resultsetIndex + 1) < _resultsets.Count && !HaveResultSet())
            {
                _resultsetIndex++;
                _rowIndex = -1;
                _currentResultset = (NpgsqlResultSet)_resultsets[_resultsetIndex];

                if (!HaveResultSet())
                {
                    String[] _returnStringTokens = ((String)_responses[_resultsetIndex]).Split(null);	// whitespace separator.
                    int responseAffectedRows = 0;

                    try
                    {
                        responseAffectedRows = Int32.Parse(_returnStringTokens[_returnStringTokens.Length - 1]);
                    }
                    catch (FormatException)
                    {
                        responseAffectedRows = -1;
                    }

                    if (responseAffectedRows != -1)
                    {
                        if (_recordsAffected == -1)
                        {
                            _recordsAffected = responseAffectedRows;
                        }
                        else
                        {
                            _recordsAffected += responseAffectedRows;
                        }
                    }
                }
            }
            return HaveResultSet();

        }

        /// <summary>
        /// Advances the data reader to the next row.
        /// </summary>
        /// <returns>True if the reader was advanced, otherwise false.</returns>
        public Boolean Read()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Read");
	    
            if (!HaveResultSet())
	    	return false;
		
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
        /// FIXME: Why this method returns String?
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
                Int32 fieldIndex = _currentResultset.RowDescription.FieldIndex(name);
                if (fieldIndex == -1)
                    throw new IndexOutOfRangeException("Field not found");
                return GetValue(fieldIndex);
            }
        }

        /// <summary>
        /// Gets the value of a column as Boolean.
        /// </summary>
        public Boolean GetBoolean(Int32 i)
        {
            // Should this be done using the GetValue directly and not by converting to String
            // and parsing from there?
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBoolean");

            return (Boolean) GetValue(i);
        }

        /// <summary>
        /// Gets the value of a column as Byte.  Not implemented.
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
        /// Gets the value of a column as Char.  Not implemented.
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
        /// Gets the value of a column as Int16.
        /// </summary>
        public Int16 GetInt16(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt16");

            return (Int16) GetValue(i);
        }

        /// <summary>
        /// Gets the value of a column as Int32.
        /// </summary>
        public Int32 GetInt32(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt32");

            return (Int32) GetValue(i);
        }

        /// <summary>
        /// Gets the value of a column as Int64.
        /// </summary>
        public Int64 GetInt64(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt64");

            return (Int64) GetValue(i);
        }

        /// <summary>
        /// Gets the value of a column as Single.
        /// </summary>
        public Single GetFloat(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetFloat");

            return (Single) GetValue(i);
        }

        /// <summary>
        /// Gets the value of a column as Double.
        /// </summary>
        public Double GetDouble(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDouble");

            return (Double) GetValue(i);
        }

        /// <summary>
        /// Gets the value of a column as String.
        /// </summary>
        public String GetString(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetString");

            return (String) GetValue(i);
        }

        /// <summary>
        /// Gets the value of a column as Decimal.
        /// </summary>
        public Decimal GetDecimal(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDecimal");

            return (Decimal) GetValue(i);
        }

        /// <summary>
        /// Gets the value of a column as DateTime.
        /// </summary>
        public DateTime GetDateTime(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDateTime");

            return (DateTime) GetValue(i);
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
                result.Columns.Add ("BaseCatalogName", typeof (string));
                result.Columns.Add ("BaseColumnName", typeof (string));
                result.Columns.Add ("BaseSchemaName", typeof (string));
                result.Columns.Add ("BaseTableName", typeof (string));
                result.Columns.Add ("DataType", typeof(Type));
                result.Columns.Add ("AllowDBNull", typeof (bool));
                result.Columns.Add ("ProviderType", typeof (string));
                result.Columns.Add ("IsAliased", typeof (bool));
                result.Columns.Add ("IsExpression", typeof (bool));
                result.Columns.Add ("IsIdentity", typeof (bool));
                result.Columns.Add ("IsAutoIncrement", typeof (bool));
                result.Columns.Add ("IsRowVersion", typeof (bool));
                result.Columns.Add ("IsHidden", typeof (bool));
                result.Columns.Add ("IsLong", typeof (bool));
                result.Columns.Add ("IsReadOnly", typeof (bool));

                if (_connection.Connector.BackendProtocolVersion == ProtocolVersion.Version2)
                {
                    FillSchemaTable_v2(result);
                }
                else if (_connection.Connector.BackendProtocolVersion == ProtocolVersion.Version3)
                {
                    FillSchemaTable_v3(result);
                }
            }

            return result;

        }

        private void FillSchemaTable_v2(DataTable schema)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "FillSchemaTable_v2");
            NpgsqlRowDescription rd = _currentResultset.RowDescription;
            ArrayList keyList = null;
			
			if ((_behavior & CommandBehavior.KeyInfo) == CommandBehavior.KeyInfo)
			{
				keyList = GetPrimaryKeys(GetTableNameFromQuery());
			}

            DataRow row;

            for (Int16 i = 0; i < rd.NumFields; i++)
            {
                row = schema.NewRow();

                row["ColumnName"] = GetName(i);
                row["ColumnOrdinal"] = i + 1;
                if (rd[i].type_modifier != -1 && rd[i].type_info != null && (rd[i].type_info.Name == "varchar" || rd[i].type_info.Name == "bpchar"))
                    row["ColumnSize"] = rd[i].type_modifier - 4;
                else if (rd[i].type_modifier != -1 && rd[i].type_info != null && (rd[i].type_info.Name == "bit" || rd[i].type_info.Name == "varbit"))
                    row["ColumnSize"] = rd[i].type_modifier;
                else
                    row["ColumnSize"] = (int) rd[i].type_size;
                if (rd[i].type_modifier != -1 && rd[i].type_info != null && rd[i].type_info.Name == "numeric")
                {
                    row["NumericPrecision"] = ((rd[i].type_modifier-4)>>16)&ushort.MaxValue;
                    row["NumericScale"] = (rd[i].type_modifier-4)&ushort.MaxValue;
                }
                else
                {
                    row["NumericPrecision"] = 0;
                    row["NumericScale"] = 0;
                }
                row["IsUnique"] = false;
                row["IsKey"] = IsKey(GetName(i), keyList);
                row["BaseCatalogName"] = "";
                row["BaseSchemaName"] = "";
                row["BaseTableName"] = "";
                row["BaseColumnName"] = GetName(i);
                row["DataType"] = GetFieldType(i);
                row["AllowDBNull"] = IsNullable(null, i);
                if (rd[i].type_info != null)
                {
                    row["ProviderType"] = rd[i].type_info.Name;
                }
                row["IsAliased"] = false;
                row["IsExpression"] = false;
                row["IsIdentity"] = false;
                row["IsAutoIncrement"] = false;
                row["IsRowVersion"] = false;
                row["IsHidden"] = false;
                row["IsLong"] = false;
                row["IsReadOnly"] = false;

                schema.Rows.Add(row);
            }
        }

        private void FillSchemaTable_v3(DataTable schema)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "FillSchemaTable_v3");
            NpgsqlRowDescription rd = _currentResultset.RowDescription;

			Hashtable oidTableLookup = null;
			KeyLookup keyLookup = new KeyLookup();
			Hashtable columnLookup = null;

			if ((_behavior & CommandBehavior.KeyInfo) == CommandBehavior.KeyInfo)
			{
				ArrayList tableOids = new ArrayList();
				for(short i=0; i<rd.NumFields; ++i)
				{
					if (rd[i].table_oid != 0 && !tableOids.Contains(rd[i].table_oid))
						tableOids.Add(rd[i].table_oid);
				}
				oidTableLookup = GetTablesFromOids(tableOids);

				if (oidTableLookup != null && oidTableLookup.Count == 1)
				{
					// only 1, but we can't index into the Hashtable
					foreach(DictionaryEntry entry in oidTableLookup)
					{
						keyLookup = GetKeys((Int32)entry.Key);
					}
				}

				columnLookup = GetColumns();
			}

            DataRow row;
            for (Int16 i = 0; i < rd.NumFields; i++)
            {
                row = schema.NewRow();

				string baseColumnName = GetBaseColumnName(columnLookup, i);

                row["ColumnName"] = GetName(i);
                row["ColumnOrdinal"] = i + 1;
                if (rd[i].type_modifier != -1 && rd[i].type_info != null && (rd[i].type_info.Name == "varchar" || rd[i].type_info.Name == "bpchar"))
                    row["ColumnSize"] = rd[i].type_modifier - 4;
                else if (rd[i].type_modifier != -1 && rd[i].type_info != null && (rd[i].type_info.Name == "bit" || rd[i].type_info.Name == "varbit"))
                    row["ColumnSize"] = rd[i].type_modifier;
                else
                    row["ColumnSize"] = (int) rd[i].type_size;
                if (rd[i].type_modifier != -1 && rd[i].type_info != null && rd[i].type_info.Name == "numeric")
                {
                    row["NumericPrecision"] = ((rd[i].type_modifier-4)>>16)&ushort.MaxValue;
                    row["NumericScale"] = (rd[i].type_modifier-4)&ushort.MaxValue;
                }
                else
                {
                    row["NumericPrecision"] = 0;
                    row["NumericScale"] = 0;
                }
                row["IsUnique"] = IsUnique(keyLookup, baseColumnName);
                row["IsKey"] = IsKey(keyLookup, baseColumnName);
                if (rd[i].table_oid != 0 && oidTableLookup != null)
                {
                    row["BaseCatalogName"] = ((object[])oidTableLookup[rd[i].table_oid])[Tables.table_catalog];
                    row["BaseSchemaName"] = ((object[])oidTableLookup[rd[i].table_oid])[Tables.table_schema];
                    row["BaseTableName"] = ((object[])oidTableLookup[rd[i].table_oid])[Tables.table_name];
                }
                else
                {
                    row["BaseCatalogName"] = "";
                    row["BaseSchemaName"] = "";
                    row["BaseTableName"] = "";
                }
                row["BaseColumnName"] = baseColumnName;
                row["DataType"] = GetFieldType(i);
                row["AllowDBNull"] = IsNullable(columnLookup, i);
                if (rd[i].type_info != null)
                {
                    row["ProviderType"] = rd[i].type_info.Name;
                }
                row["IsAliased"] = string.CompareOrdinal((string)row["ColumnName"], baseColumnName) != 0;
                row["IsExpression"] = false;
                row["IsIdentity"] = false;
                row["IsAutoIncrement"] = IsAutoIncrement(columnLookup, i);
                row["IsRowVersion"] = false;
                row["IsHidden"] = false;
                row["IsLong"] = false;
                row["IsReadOnly"] = false;

                schema.Rows.Add(row);
            }
        }


        private Boolean IsKey(String ColumnName, ArrayList ListOfKeys)
        {
            if (ListOfKeys == null || ListOfKeys.Count == 0)
                return false;

            foreach(String s in ListOfKeys)
            {

                if (s == ColumnName)
                    return true;
            }

            return false;
        }

        private ArrayList GetPrimaryKeys(String tablename)
        {

            if (tablename == String.Empty)
                return null;

            String getPKColumns = "select a.attname from pg_catalog.pg_class ct, pg_catalog.pg_class ci, pg_catalog.pg_attribute a, pg_catalog.pg_index i  WHERE ct.oid=i.indrelid AND ci.oid=i.indexrelid  AND a.attrelid=ci.oid AND i.indisprimary AND ct.relname = :tablename";

            ArrayList result = new ArrayList();
            NpgsqlConnection metadataConn = _connection.Clone();

            NpgsqlCommand c = new NpgsqlCommand(getPKColumns, metadataConn);
            c.Parameters.Add(new NpgsqlParameter("tablename", NpgsqlDbType.Text));
            c.Parameters["tablename"].Value = tablename;


            NpgsqlDataReader dr = c.ExecuteReader();


            while (dr.Read())
                result.Add(dr[0]);


            metadataConn.Close();

            return result;
        }

		private bool IsKey(KeyLookup keyLookup, string fieldName)
		{
			if (keyLookup.primaryKey == null || keyLookup.primaryKey.Count == 0)
				return false;

			for (int i=0; i<keyLookup.primaryKey.Count; ++i)
			{
                if (fieldName == (String)keyLookup.primaryKey[i])
					return true;
			}

			return false;
		}

		private bool IsUnique(KeyLookup keyLookup, string fieldName)
		{
			if (keyLookup.uniqueColumns == null || keyLookup.uniqueColumns.Count == 0)
				return false;

			for (int i=0; i<keyLookup.uniqueColumns.Count; ++i)
			{
                if (fieldName == (String)keyLookup.uniqueColumns[i])
					return true;
			}

			return false;
		}

		private struct KeyLookup
		{
			/// <summary>
			/// Contains the column names as the keys
			/// </summary>
			public ArrayList primaryKey;
			/// <summary>
			/// Contains all unique columns
			/// </summary>
			public ArrayList uniqueColumns;
		}

		private KeyLookup GetKeys(Int32 tableOid)
		{
      
			string getKeys = "select a.attname, ci.relname, i.indisprimary from pg_catalog.pg_class ct, pg_catalog.pg_class ci, pg_catalog.pg_attribute a, pg_catalog.pg_index i WHERE ct.oid=i.indrelid AND ci.oid=i.indexrelid AND a.attrelid=ci.oid AND i.indisunique AND ct.oid = :tableOid order by ci.relname";

			KeyLookup lookup = new KeyLookup();
			lookup.primaryKey = new ArrayList();
			lookup.uniqueColumns = new ArrayList();

			using (NpgsqlConnection metadataConn = _connection.Clone())
			{
				NpgsqlCommand c = new NpgsqlCommand(getKeys, metadataConn);
				c.Parameters.Add(new NpgsqlParameter("tableOid", NpgsqlDbType.Integer)).Value = tableOid;

				using (NpgsqlDataReader dr = c.ExecuteReader())
				{
					string previousKeyName = null;
					string possiblyUniqueColumn = null;
					string columnName;
					string currentKeyName;
					// loop through adding any column that is primary to the primary key list
					// add any column that is the only column for that key to the unique list
					// unique here doesn't mean general unique constraint (with possibly multiple columns)
					// it means all values in this single column must be unique
					while (dr.Read())
					{
         
						columnName = dr.GetString(0);
						currentKeyName = dr.GetString(1);
						// if i.indisprimary
						if (dr.GetBoolean(2))
						{
							// add column name as part of the primary key
							lookup.primaryKey.Add(columnName);
						}
						if (currentKeyName != previousKeyName)
						{
							if (possiblyUniqueColumn != null)
							{
								lookup.uniqueColumns.Add(possiblyUniqueColumn);
							}
							possiblyUniqueColumn = columnName;
						}
						else
						{
							possiblyUniqueColumn = null;
						}
						previousKeyName = currentKeyName;
					}
					// if finished reading and have a possiblyUniqueColumn name that is
					// not null, then it is the name of a unique column
					if (possiblyUniqueColumn != null)
						lookup.uniqueColumns.Add(possiblyUniqueColumn);
				}
			}

			return lookup;
		}

        private Boolean IsNullable(Hashtable columnLookup, Int32 FieldIndex)
        {
            if (columnLookup == null || _currentResultset.RowDescription[FieldIndex].table_oid == 0)
                return true;

            string lookupKey = _currentResultset.RowDescription[FieldIndex].table_oid.ToString() + "," + _currentResultset.RowDescription[FieldIndex].column_attribute_number;
            object[] row = (object[])columnLookup[lookupKey];
            if (row != null)
                return !(bool)row[Columns.column_notnull];
            else
                return true;
        }

        private string GetBaseColumnName(Hashtable columnLookup, Int32 FieldIndex)
        {
            if (columnLookup == null || _currentResultset.RowDescription[FieldIndex].table_oid == 0)
                return GetName(FieldIndex);
            
            string lookupKey = _currentResultset.RowDescription[FieldIndex].table_oid.ToString() + "," + _currentResultset.RowDescription[FieldIndex].column_attribute_number;
            object[] row = (object[])columnLookup[lookupKey];
            if (row != null)
                return (string)row[Columns.column_name];
            else
                return GetName(FieldIndex);
        }

		private bool IsAutoIncrement(Hashtable columnLookup, Int32 FieldIndex)
		{
			if (columnLookup == null || _currentResultset.RowDescription[FieldIndex].table_oid == 0)
				return false;

			string lookupKey = _currentResultset.RowDescription[FieldIndex].table_oid.ToString() + "," + _currentResultset.RowDescription[FieldIndex].column_attribute_number;
			object[] row = (object[])columnLookup[lookupKey];
			if (row != null)
				return row[Columns.column_default].ToString().StartsWith("nextval(");
			else
				return true;
		}


        ///<summary>
        /// This methods parses the command text and tries to get the tablename
        /// from it.
        ///</summary>
        private String GetTableNameFromQuery()
        {
            Int32 fromClauseIndex = _command.CommandText.ToLower().IndexOf("from");

            String tableName = _command.CommandText.Substring(fromClauseIndex + 4).Trim();

            if (tableName == String.Empty)
                return String.Empty;

            /*if (tableName.EndsWith("."));
                return String.Empty;
              */
            foreach (Char c in tableName.Substring (0, tableName.Length - 1))
            if (!Char.IsLetterOrDigit (c) && c != '_' && c != '.')
                return String.Empty;


            return tableName;

        }

        private struct Tables
        {
            public const int table_catalog = 0;
            public const int table_schema = 1;
            public const int table_name = 2;
            public const int table_id = 3;
        }

        private Hashtable GetTablesFromOids(ArrayList oids)
        {
            if (oids.Count == 0)
                return null;

            StringBuilder sb = new StringBuilder();

            // the column index is used to find data.
            // any changes to the order of the columns needs to be reflected in struct Tables
            sb.Append("SELECT current_database() AS table_catalog, nc.nspname AS table_schema, c.relname AS table_name, c.oid as table_id");
            sb.Append(" FROM pg_namespace nc, pg_class c WHERE c.relnamespace = nc.oid AND (c.relkind = 'r' OR c.relkind = 'v') AND c.oid IN (");
            bool first = true;
            foreach(int oid in oids)
            {
                if (!first)
                    sb.Append(',');
                sb.Append(oid);
                first = false;
            }
            sb.Append(')');

            using (NpgsqlConnection connection = _connection.Clone())
            using (NpgsqlCommand command = new NpgsqlCommand(sb.ToString(), connection))
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                Hashtable oidLookup = new Hashtable();
                int columnCount = reader.FieldCount;
                while (reader.Read())
                {
                    object[] values = new object[columnCount];
                    reader.GetValues(values);
                    oidLookup[Convert.ToInt32(reader[Tables.table_id])] = values;
                }
                return oidLookup;
            }
        }

        private struct Columns
        {
            public const int column_name = 0;
            public const int column_notnull = 1;
            public const int table_id = 2;
            public const int column_num = 3;
			public const int column_default = 4;
        }

        private Hashtable GetColumns()
        {
            StringBuilder sb = new StringBuilder();

            // the column index is used to find data.
            // any changes to the order of the columns needs to be reflected in struct Columns
            sb.Append("SELECT a.attname AS column_name, a.attnotnull AS column_notnull, a.attrelid AS table_id, a.attnum AS column_num, d.adsrc as column_default");
            sb.Append(" FROM pg_attribute a LEFT OUTER JOIN pg_attrdef d ON a.attrelid = d.adrelid AND a.attnum = d.adnum WHERE a.attnum > 0 AND (");
            bool first = true;
            for(int i=0; i<_currentResultset.RowDescription.NumFields; ++i)
            {
                if (_currentResultset.RowDescription[i].table_oid != 0)
                {
                    if (!first)
                        sb.Append(" OR ");
                    sb.AppendFormat("(a.attrelid={0} AND a.attnum={1})", _currentResultset.RowDescription[i].table_oid, _currentResultset.RowDescription[i].column_attribute_number);
                    first = false;
                }
            }
            sb.Append(')');

            // if the loop ended without setting first to false, then there will be no results from the query
            if (first)
                return null;

            using (NpgsqlConnection connection = _connection.Clone())
            using (NpgsqlCommand command = new NpgsqlCommand(sb.ToString(), connection))
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                Hashtable columnLookup = new Hashtable();
                int columnCount = reader.FieldCount;
                while(reader.Read())
                {
                    object[] values = new object[columnCount];
                    reader.GetValues(values);
                    columnLookup[reader[Columns.table_id].ToString() + "," + reader[Columns.column_num].ToString()] = values;
                }
                return columnLookup;
            }
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return new System.Data.Common.DbEnumerator (this);
        }
    }
}
