//
// Mono.Data.Sqlite.SQLiteCommand.cs
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
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
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
  using System.Collections.Generic;
  using System.ComponentModel;

  /// <summary>
  /// Sqlite implementation of DbCommand.
  /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
  [Designer("Sqlite.Designer.SqliteCommandDesigner, Sqlite.Designer, Version=1.0.31.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139"), ToolboxItem(true)]
#endif
  public sealed class SqliteCommand : DbCommand, ICloneable
  {
    /// <summary>
    /// The command text this command is based on
    /// </summary>
    private string _commandText;
    /// <summary>
    /// The connection the command is associated with
    /// </summary>
    private SqliteConnection _cnn;
    /// <summary>
    /// Indicates whether or not a DataReader is active on the command.
    /// </summary>
    private SqliteDataReader _activeReader;
    /// <summary>
    /// The timeout for the command, kludged because Sqlite doesn't support per-command timeout values
    /// </summary>
    internal int _commandTimeout;
    /// <summary>
    /// Designer support
    /// </summary>
    private bool _designTimeVisible;
    /// <summary>
    /// Used by DbDataAdapter to determine updating behavior
    /// </summary>
    private UpdateRowSource _updateRowSource;
    /// <summary>
    /// The collection of parameters for the command
    /// </summary>
    private SqliteParameterCollection _parameterCollection;
    /// <summary>
    /// The SQL command text, broken into individual SQL statements as they are executed
    /// </summary>
    internal List<SqliteStatement> _statementList;
    /// <summary>
    /// Unprocessed SQL text that has not been executed
    /// </summary>
    internal string _remainingText;
    /// <summary>
    /// Transaction associated with this command
    /// </summary>
    private SqliteTransaction _transaction;

    ///<overloads>
    /// Constructs a new SqliteCommand
    /// </overloads>
    /// <summary>
    /// Default constructor
    /// </summary>
    public SqliteCommand() :this(null, null)
    {
    }

    /// <summary>
    /// Initializes the command with the given command text
    /// </summary>
    /// <param name="commandText">The SQL command text</param>
    public SqliteCommand(string commandText) 
      : this(commandText, null, null)
    {
    }

    /// <summary>
    /// Initializes the command with the given SQL command text and attach the command to the specified
    /// connection.
    /// </summary>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="connection">The connection to associate with the command</param>
    public SqliteCommand(string commandText, SqliteConnection connection)
      : this(commandText, connection, null)
    {
    }

    /// <summary>
    /// Initializes the command and associates it with the specified connection.
    /// </summary>
    /// <param name="connection">The connection to associate with the command</param>
    public SqliteCommand(SqliteConnection connection) 
      : this(null, connection, null)
    {
    }

    private SqliteCommand(SqliteCommand source) : this(source.CommandText, source.Connection, source.Transaction)
    {
      CommandTimeout = source.CommandTimeout;
      DesignTimeVisible = source.DesignTimeVisible;
      UpdatedRowSource = source.UpdatedRowSource;

      foreach (SqliteParameter param in source._parameterCollection)
      {
        Parameters.Add(param.Clone());
      }
    }

    /// <summary>
    /// Initializes a command with the given SQL, connection and transaction
    /// </summary>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="connection">The connection to associate with the command</param>
    /// <param name="transaction">The transaction the command should be associated with</param>
    public SqliteCommand(string commandText, SqliteConnection connection, SqliteTransaction transaction)
    {
      _statementList = null;
      _activeReader = null;
      _commandTimeout = connection != null ? connection._busyTimeout : 30;
      _parameterCollection = new SqliteParameterCollection(this);
      _designTimeVisible = true;
      _updateRowSource = UpdateRowSource.FirstReturnedRecord;
      _transaction = null;

      if (commandText != null)
        CommandText = commandText;

      if (connection != null)
        DbConnection = connection;

      if (transaction != null)
        Transaction = transaction;
    }

    /// <summary>
    /// Disposes of the command and clears all member variables
    /// </summary>
    /// <param name="disposing">Whether or not the class is being explicitly or implicitly disposed</param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      // If a reader is active on this command, don't destroy it completely
      if (_activeReader != null)
      {
        _activeReader._disposeCommand = true;
        return;
      }

      Connection = null;
      _parameterCollection.Clear();
      _commandText = null;
    }

    /// <summary>
    /// Clears and destroys all statements currently prepared
    /// </summary>
    internal void ClearCommands()
    {
      if (_activeReader != null)
        _activeReader.Close();

      if (_statementList == null) return;

      int x = _statementList.Count;
      for (int n = 0; n < x; n++)
        _statementList[n].Dispose();

      _statementList = null;

      _parameterCollection.Unbind();
    }

    /// <summary>
    /// Builds an array of prepared statements for each complete SQL statement in the command text
    /// </summary>
    internal SqliteStatement BuildNextCommand()
    {
      SqliteStatement stmt = null;

      try
      {
        if (_statementList == null)
          _remainingText = _commandText;

        stmt = _cnn._sql.Prepare(_remainingText, (_statementList == null) ? null : _statementList[_statementList.Count - 1], out _remainingText);
        if (stmt != null)
        {
          stmt._command = this;
          if (_statementList == null)
            _statementList = new List<SqliteStatement>();

          _statementList.Add(stmt);

          _parameterCollection.MapParameters(stmt);
          stmt.BindParameters();
        }        
        return stmt;
      }
      catch (Exception)
      {
        if (stmt != null)
        {
          if (_statementList.Contains(stmt))
            _statementList.Remove(stmt);

          stmt.Dispose();
        }

        // If we threw an error compiling the statement, we cannot continue on so set the remaining text to null.
        _remainingText = null;

        throw;
      }
    }

    internal SqliteStatement GetStatement(int index)
    {
      // Haven't built any statements yet
      if (_statementList == null) return BuildNextCommand();

      // If we're at the last built statement and want the next unbuilt statement, then build it
      if (index == _statementList.Count)
      {
        if (String.IsNullOrEmpty(_remainingText) == false) return BuildNextCommand();
        else return null; // No more commands
      }

      SqliteStatement stmt = _statementList[index];
      stmt.BindParameters();

      return stmt;
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public override void Cancel()
    {
    }

    /// <summary>
    /// The SQL command text associated with the command
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue(""), RefreshProperties(RefreshProperties.All), Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlCommandTextEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
    public override string CommandText
    {
      get
      {
        return _commandText;
      }
      set
      {
        if (_commandText == value) return;

        if (_activeReader != null)
        {
          throw new InvalidOperationException("Cannot set CommandText while a DataReader is active");
        }

        ClearCommands();
        _commandText = value;

        if (_cnn == null) return;
      }
    }

    /// <summary>
    /// The amount of time to wait for the connection to become available before erroring out
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((int)30)]
#endif
    public override int CommandTimeout
    {
      get
      {
        return _commandTimeout;
      }
      set
      {
        _commandTimeout = value;
      }
    }

    /// <summary>
    /// The type of the command.  Sqlite only supports CommandType.Text
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [RefreshProperties(RefreshProperties.All), DefaultValue(CommandType.Text)]
#endif
    public override CommandType CommandType
    {
      get
      {
        return CommandType.Text;
      }
      set
      {
        if (value != CommandType.Text)
        {
          throw new NotSupportedException();
        }
      }
    }

    /// <summary>
    /// Forwards to the local CreateParameter() function
    /// </summary>
    /// <returns></returns>
    protected override DbParameter CreateDbParameter()
    {
      return CreateParameter();
    }

    /// <summary>
    /// Create a new parameter
    /// </summary>
    /// <returns></returns>
    public new SqliteParameter CreateParameter()
    {
      return new SqliteParameter();
    }

    /// <summary>
    /// The connection associated with this command
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((string)null), Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
    public new SqliteConnection Connection
    {
      get { return _cnn; }
      set
      {
        if (_activeReader != null)
          throw new InvalidOperationException("Cannot set Connection while a DataReader is active");

        if (_cnn != null)
        {
          ClearCommands();
          _cnn.RemoveCommand(this);
        }

        _cnn = value;

        if (_cnn != null)
          _cnn.AddCommand(this);
      }
    }

    /// <summary>
    /// Forwards to the local Connection property
    /// </summary>
    protected override DbConnection DbConnection
    {
      get
      {
        return Connection;
      }
      set
      {
        Connection = (SqliteConnection)value;
      }
    }

    /// <summary>
    /// Returns the SqliteParameterCollection for the given command
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
#endif
    public new SqliteParameterCollection Parameters
    {
      get { return _parameterCollection; }
    }

    /// <summary>
    /// Forwards to the local Parameters property
    /// </summary>
    protected override DbParameterCollection DbParameterCollection
    {
      get
      {
        return Parameters;
      }
    }

    /// <summary>
    /// The transaction associated with this command.  Sqlite only supports one transaction per connection, so this property forwards to the
    /// command's underlying connection.
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
    public new SqliteTransaction Transaction
    {
      get { return _transaction; }
      set
      {
        if (_cnn != null)
        {
          if (_activeReader != null)
            throw new InvalidOperationException("Cannot set Transaction while a DataReader is active");

          if (value != null)
          {
            if (value._cnn != _cnn)
              throw new ArgumentException("Transaction is not associated with the command's connection");
          }
          _transaction = value;
        }
        else if (value != null)
          throw new ArgumentOutOfRangeException("SqliteTransaction", "Not associated with a connection");
      }
    }

    /// <summary>
    /// Forwards to the local Transaction property
    /// </summary>
    protected override DbTransaction DbTransaction
    {
      get
      {
        return Transaction;
      }
      set
      {
        Transaction = (SqliteTransaction)value;
      }
    }

    /// <summary>
    /// This function ensures there are no active readers, that we have a valid connection,
    /// that the connection is open, that all statements are prepared and all parameters are assigned
    /// in preparation for allocating a data reader.
    /// </summary>
    private void InitializeForReader()
    {
      if (_activeReader != null)
        throw new InvalidOperationException("DataReader already active on this command");

      if (_cnn == null)
        throw new InvalidOperationException("No connection associated with this command");

      if (_cnn.State != ConnectionState.Open)
        throw new InvalidOperationException("Database is not open");

      // Map all parameters for statements already built
      _parameterCollection.MapParameters(null);

      // Set the default command timeout
      _cnn._sql.SetTimeout(_commandTimeout * 1000);
    }

    /// <summary>
    /// Creates a new SqliteDataReader to execute/iterate the array of Sqlite prepared statements
    /// </summary>
    /// <param name="behavior">The behavior the data reader should adopt</param>
    /// <returns>Returns a SqliteDataReader object</returns>
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      return ExecuteReader(behavior);
    }

    /// <summary>
    /// Overrides the default behavior to return a SqliteDataReader specialization class
    /// </summary>
    /// <param name="behavior">The flags to be associated with the reader</param>
    /// <returns>A SqliteDataReader</returns>
    public new SqliteDataReader ExecuteReader(CommandBehavior behavior)
    {
      InitializeForReader();

      SqliteDataReader rd = new SqliteDataReader(this, behavior);
      _activeReader = rd;

      return rd;
    }

    /// <summary>
    /// Overrides the default behavior of DbDataReader to return a specialized SqliteDataReader class
    /// </summary>
    /// <returns>A SqliteDataReader</returns>
    public new SqliteDataReader ExecuteReader()
    {
      return ExecuteReader(CommandBehavior.Default);
    }

    /// <summary>
    /// Called by the SqliteDataReader when the data reader is closed.
    /// </summary>
    internal void ClearDataReader()
    {
      _activeReader = null;
    }

    /// <summary>
    /// Execute the command and return the number of rows inserted/updated affected by it.
    /// </summary>
    /// <returns></returns>
    public override int ExecuteNonQuery()
    {
      InitializeForReader();

      int nAffected = 0;
      int x = 0;
      SqliteStatement stmt;

      for(;;)
      {
        stmt = GetStatement(x);
        x++;
        if (stmt == null) break;

        _cnn._sql.Step(stmt);
        nAffected += _cnn._sql.Changes;
        _cnn._sql.Reset(stmt);
      }

      return nAffected;
    }

    /// <summary>
    /// Execute the command and return the first column of the first row of the resultset
    /// (if present), or null if no resultset was returned.
    /// </summary>
    /// <returns>The first column of the first row of the first resultset from the query</returns>
    public override object ExecuteScalar()
    {
      InitializeForReader();

      int x = 0;
      object ret = null;
      SqliteType typ = new SqliteType();
      SqliteStatement stmt;

      // We step through every statement in the command, but only grab the first row of the first resultset.
      // We keep going even after obtaining it.
      for (;;)
      {
        stmt = GetStatement(x);
        x++;
        if (stmt == null) break;

        if (_cnn._sql.Step(stmt) == true && ret == null)
        {
          ret = _cnn._sql.GetValue(stmt, 0, ref typ);
        }
        _cnn._sql.Reset(stmt);
      }

      return ret;
    }

    /// <summary>
    /// Does nothing.  Commands are prepared as they are executed the first time, and kept in prepared state afterwards.
    /// </summary>
    public override void Prepare()
    {
    }

    /// <summary>
    /// Sets the method the SqliteCommandBuilder uses to determine how to update inserted or updated rows in a DataTable.
    /// </summary>
    [DefaultValue(UpdateRowSource.FirstReturnedRecord)]
    public override UpdateRowSource UpdatedRowSource
    {
      get
      {
        return _updateRowSource;
      }
      set
      {
        _updateRowSource = value;
      }
    }

    /// <summary>
    /// Determines if the command is visible at design time.  Defaults to True.
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DesignOnly(true), Browsable(false), DefaultValue(true), EditorBrowsable(EditorBrowsableState.Never)]
#endif
    public override bool DesignTimeVisible
    {
      get
      {
        return _designTimeVisible;
      }
      set
      {
        _designTimeVisible = value;
#if !PLATFORM_COMPACTFRAMEWORK
        TypeDescriptor.Refresh(this);
#endif
      }
    }

    /// <summary>
    /// Clones a command, including all its parameters
    /// </summary>
    /// <returns>A new SqliteCommand with the same commandtext, connection and parameters</returns>
    public object Clone()
    {
      return new SqliteCommand(this);
    }

    public int LastInsertRowID ()
    {
	    return _cnn.LastInsertRowId;
    }
  }
}
#endif
