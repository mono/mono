//
// Mono.Data.Sqlite.SQLiteDataReader.cs
//
// Author(s):
//   Robert Simpson (robert@blackcastlesoft.com)
//
// Adapted and modified for the Mono Project by
//   Marek Habersack (grendello@gmail.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2007 Marek Habersack
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

/********************************************************
 * ADO.NET 2.0 Data Provider for Sqlite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/
#if NET_2_0
namespace Mono.Data.Sqlite
{
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Collections;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Reflection;

  /// <summary>
  /// Sqlite implementation of DbDataReader.
  /// </summary>
  public class SqliteDataReader : DbDataReader
  {
    /// <summary>
    /// Underlying command this reader is attached to
    /// </summary>
    private SqliteCommand   _command;
    /// <summary>
    /// Index of the current statement in the command being processed
    /// </summary>
    private int             _activeStatementIndex;
    /// <summary>
    /// Current statement being Read()
    /// </summary>
    private SqliteStatement _activeStatement;
    /// <summary>
    /// State of the current statement being processed.
    /// -1 = First Step() executed, so the first Read() will be ignored
    ///  0 = Actively reading
    ///  1 = Finished reading
    ///  2 = Non-row-returning statement, no records
    /// </summary>
    private int             _readingState;
    /// <summary>
    /// Number of records affected by the insert/update statements executed on the command
    /// </summary>
    private int             _rowsAffected;
    /// <summary>
    /// Count of fields (columns) in the row-returning statement currently being processed
    /// </summary>
    private int             _fieldCount;
    /// <summary>
    /// Datatypes of active fields (columns) in the current statement, used for type-restricting data
    /// </summary>
    private SqliteType[]    _fieldTypeArray;

    /// <summary>
    /// The behavior of the datareader
    /// </summary>
    private CommandBehavior _commandBehavior;

    /// <summary>
    /// If set, then dispose of the command object when the reader is finished
    /// </summary>
    internal bool           _disposeCommand;

#if MONO_SUPPORT_KEYREADER
    /// <summary>
    /// An array of rowid's for the active statement if CommandBehavior.KeyInfo is specified
    /// </summary>
    private SqliteKeyReader _keyInfo;
#endif
    
    /// <summary>
    /// Internal constructor, initializes the datareader and sets up to begin executing statements
    /// </summary>
    /// <param name="cmd">The SqliteCommand this data reader is for</param>
    /// <param name="behave">The expected behavior of the data reader</param>
    internal SqliteDataReader(SqliteCommand cmd, CommandBehavior behave)
    {
      _command = cmd;
      _commandBehavior = behave;
      _activeStatementIndex = -1;
      _activeStatement = null;
      _rowsAffected = -1;
      _fieldCount = -1;

      if (_command != null)
        NextResult();
    }

    /// <summary>
    /// Closes the datareader, potentially closing the connection as well if CommandBehavior.CloseConnection was specified.
    /// </summary>
    public override void Close()
    {
      if (_command != null)
      {
        while (NextResult())
        {
        }
        _command.ClearDataReader();

        // If the datareader's behavior includes closing the connection, then do so here.
        if ((_commandBehavior & CommandBehavior.CloseConnection) != 0 && _command.Connection != null)
          _command.Connection.Close();

        if (_disposeCommand)
          ((IDisposable)_command).Dispose();
      }

      _command = null;
      _activeStatement = null;
      _fieldTypeArray = null;

#if MONO_SUPPORT_KEYREADER
      if (_keyInfo != null)
      {
        _keyInfo.Dispose();
        _keyInfo = null;
      }
#endif
    }

    /// <summary>
    /// Disposes the datareader.  Calls Close() to ensure everything is cleaned up.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Throw an error if the datareader is closed
    /// </summary>
    private void CheckClosed()
    {
      if (_command == null)
        throw new InvalidOperationException("DataReader has been closed");
    }

    /// <summary>
    /// Throw an error if a row is not loaded
    /// </summary>
    private void CheckValidRow()
    {
      if (_readingState != 0)
        throw new InvalidOperationException("No current row");
    }

    /// <summary>
    /// Enumerator support
    /// </summary>
    /// <returns>Returns a DbEnumerator object.</returns>
    public override System.Collections.IEnumerator GetEnumerator()
    {
      return new DbEnumerator(this);
    }

    /// <summary>
    /// Not implemented.  Returns 0
    /// </summary>
    public override int Depth
    {
      get
      {
        CheckClosed();
        return 0;
      }
    }

    /// <summary>
    /// Returns the number of columns in the current resultset
    /// </summary>
    public override int FieldCount
    {
      get
      {
        CheckClosed();
#if MONO_SUPPORT_KEYREADER
        if (_keyInfo == null)
          return _fieldCount;

        return _fieldCount + _keyInfo.Count;
#else
	return _fieldCount;
#endif
      }
    }

    /// <summary>
    /// Returns the number of visible fielsd in the current resultset
    /// </summary>
    public override int VisibleFieldCount
    {
      get
      {
        CheckClosed();
        return _fieldCount;
      }
    }

    /// <summary>
    /// Sqlite is inherently un-typed.  All datatypes in Sqlite are natively strings.  The definition of the columns of a table
    /// and the affinity of returned types are all we have to go on to type-restrict data in the reader.
    /// 
    /// This function attempts to verify that the type of data being requested of a column matches the datatype of the column.  In
    /// the case of columns that are not backed into a table definition, we attempt to match up the affinity of a column (int, double, string or blob)
    /// to a set of known types that closely match that affinity.  It's not an exact science, but its the best we can do.
    /// </summary>
    /// <returns>
    /// This function throws an InvalidTypeCast() exception if the requested type doesn't match the column's definition or affinity.
    /// </returns>
    /// <param name="i">The index of the column to type-check</param>
    /// <param name="typ">The type we want to get out of the column</param>
    private TypeAffinity VerifyType(int i, DbType typ)
    {
      CheckClosed();
      CheckValidRow();
      TypeAffinity affinity;

      affinity = _activeStatement._sql.ColumnAffinity(_activeStatement, i);

      switch (affinity)
      {
        case TypeAffinity.Int64:
          if (typ == DbType.Int16) return affinity;
          if (typ == DbType.Int32) return affinity;
          if (typ == DbType.Int64) return affinity;
          if (typ == DbType.Boolean) return affinity;
          if (typ == DbType.Byte) return affinity;
          if (typ == DbType.DateTime && _command.Connection._sql._datetimeFormat == SqliteDateFormats.Ticks) return affinity;
          if (typ == DbType.Single) return affinity;
          if (typ == DbType.Double) return affinity;
          if (typ == DbType.Decimal) return affinity;
          break;
        case TypeAffinity.Double:
          if (typ == DbType.Single) return affinity;
          if (typ == DbType.Double) return affinity;
          if (typ == DbType.Decimal) return affinity;
          break;
        case TypeAffinity.Text:
          if (typ == DbType.SByte) return affinity;
          if (typ == DbType.String) return affinity;
          if (typ == DbType.SByte) return affinity;
          if (typ == DbType.Guid) return affinity;
          if (typ == DbType.DateTime) return affinity;
          break;
        case TypeAffinity.Blob:
          if (typ == DbType.Guid) return affinity;
          if (typ == DbType.String) return affinity;
          if (typ == DbType.Binary) return affinity;
          break;
      }

      throw new InvalidCastException();
    }

    /// <summary>
    /// Retrieves the column as a boolean value
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>bool</returns>
    public override bool GetBoolean(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetBoolean(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.Boolean);
      return Convert.ToBoolean(GetValue(i), CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Retrieves the column as a single byte value
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>byte</returns>
    public override byte GetByte(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetByte(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.Byte);
      return Convert.ToByte(_activeStatement._sql.GetInt32(_activeStatement, i));
    }

    /// <summary>
    /// Retrieves a column as an array of bytes (blob)
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <param name="fieldOffset">The zero-based index of where to begin reading the data</param>
    /// <param name="buffer">The buffer to write the bytes into</param>
    /// <param name="bufferoffset">The zero-based index of where to begin writing into the array</param>
    /// <param name="length">The number of bytes to retrieve</param>
    /// <returns>The actual number of bytes written into the array</returns>
    /// <remarks>
    /// To determine the number of bytes in the column, pass a null value for the buffer.  The total length will be returned.
    /// </remarks>
    public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetBytes(i - VisibleFieldCount, fieldOffset, buffer, bufferoffset, length);
#endif
      
      VerifyType(i, DbType.Binary);
      return _activeStatement._sql.GetBytes(_activeStatement, i, (int)fieldOffset, buffer, bufferoffset, length);
    }

    /// <summary>
    /// Returns the column as a single character
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>char</returns>
    public override char GetChar(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetChar(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.SByte);
      return Convert.ToChar(_activeStatement._sql.GetInt32(_activeStatement, i));
    }

    /// <summary>
    /// Retrieves a column as an array of chars (blob)
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <param name="fieldoffset">The zero-based index of where to begin reading the data</param>
    /// <param name="buffer">The buffer to write the characters into</param>
    /// <param name="bufferoffset">The zero-based index of where to begin writing into the array</param>
    /// <param name="length">The number of bytes to retrieve</param>
    /// <returns>The actual number of characters written into the array</returns>
    /// <remarks>
    /// To determine the number of characters in the column, pass a null value for the buffer.  The total length will be returned.
    /// </remarks>
    public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetChars(i - VisibleFieldCount, fieldoffset, buffer, bufferoffset, length);
#endif
      
      VerifyType(i, DbType.String);
      return _activeStatement._sql.GetChars(_activeStatement, i, (int)fieldoffset, buffer, bufferoffset, length);
    }

    /// <summary>
    /// Retrieves the name of the back-end datatype of the column
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>string</returns>
    public override string GetDataTypeName(int i)
    {
      CheckClosed();

#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetDataTypeName(i - VisibleFieldCount);
#endif
      
      SqliteType typ = GetSqliteType(i);
      if (typ.Type == DbType.Object) return SqliteConvert.SqliteTypeToType(typ).Name;
      return _activeStatement._sql.ColumnType(_activeStatement, i, out typ.Affinity);
    }

    /// <summary>
    /// Retrieve the column as a date/time value
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>DateTime</returns>
    public override DateTime GetDateTime(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetDateTime(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.DateTime);
      return _activeStatement._sql.GetDateTime(_activeStatement, i);
    }

    /// <summary>
    /// Retrieve the column as a decimal value
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>decimal</returns>
    public override decimal GetDecimal(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetDecimal(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.Decimal);
      return Convert.ToDecimal(_activeStatement._sql.GetDouble(_activeStatement, i));
    }

    /// <summary>
    /// Returns the column as a double
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>double</returns>
    public override double GetDouble(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetDouble(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.Double);
      return _activeStatement._sql.GetDouble(_activeStatement, i);
    }

    /// <summary>
    /// Returns the .NET type of a given column
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>Type</returns>
    public override Type GetFieldType(int i)
    {
      CheckClosed();

#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetFieldType(i - VisibleFieldCount);
#endif
      
      return SqliteConvert.SqliteTypeToType(GetSqliteType(i));
    }

    /// <summary>
    /// Returns a column as a float value
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>float</returns>
    public override float GetFloat(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetFloat(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.Single);
      return Convert.ToSingle(_activeStatement._sql.GetDouble(_activeStatement, i));
    }

    /// <summary>
    /// Returns the column as a Guid
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>Guid</returns>
    public override Guid GetGuid(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetGuid(i - VisibleFieldCount);
#endif
      
      TypeAffinity affinity = VerifyType(i, DbType.Guid);
      if (affinity == TypeAffinity.Blob)
      {
        byte[] buffer = new byte[16];
        _activeStatement._sql.GetBytes(_activeStatement, i, 0, buffer, 0, 16);
        return new Guid(buffer);
      }
      else
        return new Guid(_activeStatement._sql.GetText(_activeStatement, i));
    }

    /// <summary>
    /// Returns the column as a short
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>Int16</returns>
    public override Int16 GetInt16(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetInt16(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.Int16);
      return Convert.ToInt16(_activeStatement._sql.GetInt32(_activeStatement, i));
    }

    /// <summary>
    /// Retrieves the column as an int
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>Int32</returns>
    public override Int32 GetInt32(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetInt32(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.Int32);
      return _activeStatement._sql.GetInt32(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the column as a long
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>Int64</returns>
    public override Int64 GetInt64(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetInt64(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.Int64);
      return _activeStatement._sql.GetInt64(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the name of the column
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>string</returns>
    public override string GetName(int i)
    {
      CheckClosed();
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetName(i - VisibleFieldCount);
#endif
      
      return _activeStatement._sql.ColumnName(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the i of a column, given its name
    /// </summary>
    /// <param name="name">The name of the column to retrieve</param>
    /// <returns>The int i of the column</returns>
    public override int GetOrdinal(string name)
    {
      CheckClosed();
      int r = _activeStatement._sql.ColumnIndex(_activeStatement, name);
#if MONO_SUPPORT_KEYREADER
      if (r == -1 && _keyInfo != null)
      {
        r = _keyInfo.GetOrdinal(name);
        if (r > -1) r += VisibleFieldCount;
      }
#endif
      
      return r;
    }

    /// <summary>
    /// Schema information in Sqlite is difficult to map into .NET conventions, so a lot of work must be done
    /// to gather the necessary information so it can be represented in an ADO.NET manner.
    /// </summary>
    /// <returns>Returns a DataTable containing the schema information for the active SELECT statement being processed.</returns>
    public override DataTable GetSchemaTable()
    {
      return GetSchemaTable(true, false);
    }

    internal DataTable GetSchemaTable(bool wantUniqueInfo, bool wantDefaultValue)
    {
      CheckClosed();

      DataTable tbl = new DataTable("SchemaTable");
      DataTable tblIndexes = null;
      DataTable tblIndexColumns;
      DataRow row;
      string temp;
      string strCatalog = "";
      string strTable = "";
      string strColumn = "";

      tbl.Locale = CultureInfo.InvariantCulture;
      tbl.Columns.Add(SchemaTableColumn.ColumnName, typeof(String));
      tbl.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int));
      tbl.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int));
      tbl.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short));
      tbl.Columns.Add(SchemaTableColumn.NumericScale, typeof(short));
      tbl.Columns.Add(SchemaTableColumn.IsUnique, typeof(Boolean));
      tbl.Columns.Add(SchemaTableColumn.IsKey, typeof(Boolean));
      tbl.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string));
      tbl.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(String));
      tbl.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(String));
      tbl.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(String));
      tbl.Columns.Add(SchemaTableColumn.BaseTableName, typeof(String));
      tbl.Columns.Add(SchemaTableColumn.DataType, typeof(Type));
      tbl.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(Boolean));
      tbl.Columns.Add(SchemaTableColumn.ProviderType, typeof(int));
      tbl.Columns.Add(SchemaTableColumn.IsAliased, typeof(Boolean));
      tbl.Columns.Add(SchemaTableColumn.IsExpression, typeof(Boolean));
      tbl.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(Boolean));
      tbl.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(Boolean));
      tbl.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(Boolean));
      tbl.Columns.Add(SchemaTableColumn.IsLong, typeof(Boolean));
      tbl.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(Boolean));
      tbl.Columns.Add(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
      tbl.Columns.Add(SchemaTableOptionalColumn.DefaultValue, typeof(object));
      tbl.Columns.Add("DataTypeName", typeof(string));

      tbl.BeginLoadData();

      for (int n = 0; n < _fieldCount; n++)
      {
        row = tbl.NewRow();

        // Default settings for the column
        row[SchemaTableColumn.ColumnName] = GetName(n);
        row[SchemaTableColumn.ColumnOrdinal] = n;
        row[SchemaTableColumn.ColumnSize] = SqliteConvert.DbTypeToColumnSize(GetSqliteType(n).Type);
        row[SchemaTableColumn.NumericPrecision] = 255;
        row[SchemaTableColumn.NumericScale] = 255;
        row[SchemaTableColumn.ProviderType] = GetSqliteType(n).Type;
        row[SchemaTableColumn.IsLong] = (GetSqliteType(n).Type == DbType.Binary);
        row[SchemaTableColumn.AllowDBNull] = true;
        row[SchemaTableOptionalColumn.IsReadOnly] = false;
        row[SchemaTableOptionalColumn.IsRowVersion] = false;
        row[SchemaTableColumn.IsUnique] = false;
        row[SchemaTableColumn.IsKey] = false;
        row[SchemaTableOptionalColumn.IsAutoIncrement] = false;
        row[SchemaTableOptionalColumn.IsReadOnly] = false;
        row[SchemaTableColumn.DataType] = GetFieldType(n);
        row[SchemaTableOptionalColumn.IsHidden] = false;

        strColumn = _command.Connection._sql.ColumnOriginalName(_activeStatement, n);
        if (String.IsNullOrEmpty(strColumn) == false) row[SchemaTableColumn.BaseColumnName] = strColumn;
        
        row[SchemaTableColumn.IsExpression] = String.IsNullOrEmpty(strColumn);
        row[SchemaTableColumn.IsAliased] = (String.Compare(GetName(n), strColumn, true, CultureInfo.InvariantCulture) != 0);

        temp = _command.Connection._sql.ColumnTableName(_activeStatement, n);
        if (String.IsNullOrEmpty(temp) == false) row[SchemaTableColumn.BaseTableName] = temp;

        temp = _command.Connection._sql.ColumnDatabaseName(_activeStatement, n);
        if (String.IsNullOrEmpty(temp) == false) row[SchemaTableOptionalColumn.BaseCatalogName] = temp;

        string dataType = null;
        // If we have a table-bound column, extract the extra information from it
        if (String.IsNullOrEmpty(strColumn) == false)
        {
          string collSeq;
          bool bNotNull;
          bool bPrimaryKey;
          bool bAutoIncrement;
          string[] arSize;

          // Get the column meta data
          _command.Connection._sql.ColumnMetaData(
            (string)row[SchemaTableOptionalColumn.BaseCatalogName],
            (string)row[SchemaTableColumn.BaseTableName],
            strColumn,
            out dataType, out collSeq, out bNotNull, out bPrimaryKey, out bAutoIncrement);

          if (bNotNull || bPrimaryKey) row[SchemaTableColumn.AllowDBNull] = false;

          row[SchemaTableColumn.IsKey] = bPrimaryKey;
          row[SchemaTableOptionalColumn.IsAutoIncrement] = bAutoIncrement;

          // For types like varchar(50) and such, extract the size
          arSize = dataType.Split('(');
          if (arSize.Length > 1)
          {
            dataType = arSize[0];
            arSize = arSize[1].Split(')');
            if (arSize.Length > 1)
            {
              arSize = arSize[0].Split(',', '.');
              if (GetSqliteType(n).Type == DbType.String || GetSqliteType(n).Type == DbType.Binary)
              {
                row[SchemaTableColumn.ColumnSize] = Convert.ToInt32(arSize[0], CultureInfo.InvariantCulture);
              }
              else
              {
                row[SchemaTableColumn.NumericPrecision] = Convert.ToInt32(arSize[0], CultureInfo.InvariantCulture);
                if (arSize.Length > 1)
                  row[SchemaTableColumn.NumericScale] = Convert.ToInt32(arSize[1], CultureInfo.InvariantCulture);
              }
            }
          }

          if (wantDefaultValue)
          {
            // Determine the default value for the column, which sucks because we have to query the schema for each column
            using (SqliteCommand cmdTable = new SqliteCommand(String.Format(CultureInfo.InvariantCulture, "PRAGMA [{0}].TABLE_INFO([{1}])",
              row[SchemaTableOptionalColumn.BaseCatalogName],
              row[SchemaTableColumn.BaseTableName]
              ), _command.Connection))
            using (DbDataReader rdTable = cmdTable.ExecuteReader())
            {
              // Find the matching column
              while (rdTable.Read())
              {
                if (String.Compare((string)row[SchemaTableColumn.BaseColumnName], rdTable.GetString(1), true, CultureInfo.InvariantCulture) == 0)
                {
                  if (rdTable.IsDBNull(4) == false)
                    row[SchemaTableOptionalColumn.DefaultValue] = rdTable[4];

                  break;
                }
              }
            }
          }

          // Determine IsUnique properly, which is a pain in the butt!
          if (wantUniqueInfo)
          {
            if ((string)row[SchemaTableOptionalColumn.BaseCatalogName] != strCatalog
              || (string)row[SchemaTableColumn.BaseTableName] != strTable)
            {
              strCatalog = (string)row[SchemaTableOptionalColumn.BaseCatalogName];
              strTable = (string)row[SchemaTableColumn.BaseTableName];

              tblIndexes = _command.Connection.GetSchema("Indexes", new string[] {
                (string)row[SchemaTableOptionalColumn.BaseCatalogName],
                null,
                (string)row[SchemaTableColumn.BaseTableName],
                null });
            }

            foreach (DataRow rowIndexes in tblIndexes.Rows)
            {
              tblIndexColumns = _command.Connection.GetSchema("IndexColumns", new string[] {
                (string)row[SchemaTableOptionalColumn.BaseCatalogName],
                null,
                (string)row[SchemaTableColumn.BaseTableName],
                (string)rowIndexes["INDEX_NAME"],
                null
                });
              foreach (DataRow rowColumnIndex in tblIndexColumns.Rows)
              {
                if (String.Compare((string)rowColumnIndex["COLUMN_NAME"], strColumn, true, CultureInfo.InvariantCulture) == 0)
                {
                  if (tblIndexColumns.Rows.Count == 1 && (bool)row[SchemaTableColumn.AllowDBNull] == false)
                    row[SchemaTableColumn.IsUnique] = rowIndexes["UNIQUE"];

                  break;
                }
              }
            }
          }
        }
        
        if (String.IsNullOrEmpty(dataType))
        {
          TypeAffinity affin;
          dataType = _activeStatement._sql.ColumnType(_activeStatement, n, out affin);
        }

        if (String.IsNullOrEmpty(dataType) == false)
          row["DataTypeName"] = dataType;

        tbl.Rows.Add(row);
      }

#if MONO_SUPPORT_KEYREADER
      if (_keyInfo != null)
        _keyInfo.AppendSchemaTable(tbl);
#endif
      
      tbl.AcceptChanges();
      tbl.EndLoadData();

      return tbl;
    }

    /// <summary>
    /// Retrieves the column as a string
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>string</returns>
    public override string GetString(int i)
    {
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetString(i - VisibleFieldCount);
#endif
      
      VerifyType(i, DbType.String);
      return _activeStatement._sql.GetText(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the column as an object corresponding to the underlying datatype of the column
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>object</returns>
    public override object GetValue(int i)
    {
      CheckClosed();

#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.GetValue(i - VisibleFieldCount);
#endif
      SqliteType typ = GetSqliteType(i);
      typ.Affinity = _activeStatement._sql.ColumnAffinity(_activeStatement, i);
      return _activeStatement._sql.GetValue(_activeStatement, i, ref typ);
    }

    /// <summary>
    /// Retreives the values of multiple columns, up to the size of the supplied array
    /// </summary>
    /// <param name="values">The array to fill with values from the columns in the current resultset</param>
    /// <returns>The number of columns retrieved</returns>
    public override int GetValues(object[] values)
    {
      int nMax = FieldCount;
      if (values.Length < nMax) nMax = values.Length;

      for (int n = 0; n < nMax; n++)
      {
        values[n] = GetValue(n);
      }

      return nMax;
    }

    /// <summary>
    /// Returns True if the resultset has rows that can be fetched
    /// </summary>
    public override bool HasRows
    {
      get
      {
        CheckClosed();
        return (_readingState != 1);
      }
    }

    /// <summary>
    /// Returns True if the data reader is closed
    /// </summary>
    public override bool IsClosed
    {
      get { return (_command == null); }
    }

    /// <summary>
    /// Returns True if the specified column is null
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>True or False</returns>
    public override bool IsDBNull(int i)
    {
      CheckClosed();
#if MONO_SUPPORT_KEYREADER
      if (i >= VisibleFieldCount && _keyInfo != null)
        return _keyInfo.IsDBNull(i - VisibleFieldCount);
#endif
      return _activeStatement._sql.IsNull(_activeStatement, i);
    }

    /// <summary>
    /// Moves to the next resultset in multiple row-returning SQL command.
    /// </summary>
    /// <returns>True if the command was successful and a new resultset is available, False otherwise.</returns>
    public override bool NextResult()
    {
      CheckClosed();

      SqliteStatement stmt = null;
      int fieldCount;

      while (true)
      {
        if (_activeStatement != null && stmt == null)
        {
          // Reset the previously-executed statement
          _activeStatement._sql.Reset(_activeStatement);
          
          // If we're only supposed to return a single rowset, step through all remaining statements once until
          // they are all done and return false to indicate no more resultsets exist.
          if ((_commandBehavior & CommandBehavior.SingleResult) != 0)
          {
            for (; ; )
            {
              stmt = _command.GetStatement(_activeStatementIndex + 1);
              if (stmt == null) break;
              _activeStatementIndex++;

              stmt._sql.Step(stmt);
              if (stmt._sql.ColumnCount(stmt) == 0)
              {
                if (_rowsAffected == -1) _rowsAffected = 0;
                _rowsAffected += stmt._sql.Changes;
              }
              stmt._sql.Reset(stmt); // Gotta reset after every step to release any locks and such!
            }
            return false;
          }
        }

        // Get the next statement to execute
        stmt = _command.GetStatement(_activeStatementIndex + 1);

        // If we've reached the end of the statements, return false, no more resultsets
        if (stmt == null)
          return false;

        // If we were on a current resultset, set the state to "done reading" for it
        if (_readingState < 1)
          _readingState = 1;

        _activeStatementIndex++;

        fieldCount = stmt._sql.ColumnCount(stmt);

        // If the statement is not a select statement or we're not retrieving schema only, then perform the initial step
        if ((_commandBehavior & CommandBehavior.SchemaOnly) == 0 || fieldCount == 0)
        {
          if (stmt._sql.Step(stmt))
          {
            _readingState = -1;
          }
          else if (fieldCount == 0) // No rows returned, if fieldCount is zero, skip to the next statement
          {
            if (_rowsAffected == -1) _rowsAffected = 0;
            _rowsAffected += stmt._sql.Changes;
            stmt._sql.Reset(stmt);
            continue; // Skip this command and move to the next, it was not a row-returning resultset
          }
          else // No rows, fieldCount is non-zero so stop here
          {
            _readingState = 1; // This command returned columns but no rows, so return true, but HasRows = false and Read() returns false
          }
        }

        // Ahh, we found a row-returning resultset eligible to be returned!
        _activeStatement = stmt;
        _fieldCount = fieldCount;
        _fieldTypeArray = null;

        if ((_commandBehavior & CommandBehavior.KeyInfo) != 0)
          LoadKeyInfo();

        return true;
      }
    }

    /// <summary>
    /// Retrieves the SqliteType for a given column, and caches it to avoid repetetive interop calls.
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>A SqliteType structure</returns>
    private SqliteType GetSqliteType(int i)
    {
      if (_fieldTypeArray == null) _fieldTypeArray = new SqliteType[VisibleFieldCount];

      if (_fieldTypeArray[i].Affinity == TypeAffinity.Uninitialized || _fieldTypeArray[i].Affinity == TypeAffinity.Null)
        _fieldTypeArray[i].Type = SqliteConvert.TypeNameToDbType(_activeStatement._sql.ColumnType(_activeStatement, i, out _fieldTypeArray[i].Affinity));
      return _fieldTypeArray[i];
    }

    /// <summary>
    /// Reads the next row from the resultset
    /// </summary>
    /// <returns>True if a new row was successfully loaded and is ready for processing</returns>
    public override bool Read()
    {
      CheckClosed();

      if (_readingState == -1) // First step was already done at the NextResult() level, so don't step again, just return true.
      {
        _readingState = 0;
        return true;
      }
      else if (_readingState == 0) // Actively reading rows
      {
        if (_activeStatement._sql.Step(_activeStatement) == true)
        {
#if MONO_SUPPORT_KEYREADER
          if (_keyInfo != null)
            _keyInfo.Reset();
#endif
	  
          return true;
        }

        _readingState = 1; // Finished reading rows
      }

      return false;
    }

    /// <summary>
    /// Retrieve the count of records affected by an update/insert command.  Only valid once the data reader is closed!
    /// </summary>
    public override int RecordsAffected
    {
      get { return _rowsAffected; }
    }

    /// <summary>
    /// Indexer to retrieve data from a column given its name
    /// </summary>
    /// <param name="name">The name of the column to retrieve data for</param>
    /// <returns>The value contained in the column</returns>
    public override object this[string name]
    {
      get { return GetValue(GetOrdinal(name)); }
    }

    /// <summary>
    /// Indexer to retrieve data from a column given its i
    /// </summary>
    /// <param name="i">The index of the column to retrieve</param>
    /// <returns>The value contained in the column</returns>
    public override object this[int i]
    {
      get { return GetValue(i); }
    }

    private void LoadKeyInfo()
    {
#if MONO_SUPPORT_KEYREADER
      if (_keyInfo != null)
        _keyInfo.Dispose();

      _keyInfo = new SqliteKeyReader(_command.Connection, this, _activeStatement);
#endif
    }
  }
}
#endif
