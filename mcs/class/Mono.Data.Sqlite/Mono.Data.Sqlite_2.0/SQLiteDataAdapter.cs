//
// Mono.Data.Sqlite.SQLiteDataAdapter.cs
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
  using System.ComponentModel;

  /// <summary>
  /// Sqlite implementation of DbDataAdapter.
  /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
  [DefaultEvent("RowUpdated")]
  [ToolboxItem("Sqlite.Designer.SqliteDataAdapterToolboxItem, Sqlite.Designer, Version=1.0.31.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139")]
  [Designer("Microsoft.VSDesigner.Data.VS.SqlDataAdapterDesigner, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
  public sealed class SqliteDataAdapter : DbDataAdapter
  {
    private static object _updatingEventPH = new object();
    private static object _updatedEventPH = new object();

    /// <overloads>
    /// This class is just a shell around the DbDataAdapter.  Nothing from DbDataAdapter is overridden here, just a few constructors are defined.
    /// </overloads>
    /// <summary>
    /// Default constructor.
    /// </summary>
    public SqliteDataAdapter()
    {
    }

    /// <summary>
    /// Constructs a data adapter using the specified select command.
    /// </summary>
    /// <param name="cmd">The select command to associate with the adapter.</param>
    public SqliteDataAdapter(SqliteCommand cmd)
    {
      SelectCommand = cmd;
    }

    /// <summary>
    /// Constructs a data adapter with the supplied select command text and associated with the specified connection.
    /// </summary>
    /// <param name="commandText">The select command text to associate with the data adapter.</param>
    /// <param name="connection">The connection to associate with the select command.</param>
    public SqliteDataAdapter(string commandText, SqliteConnection connection)
    {
      SelectCommand = new SqliteCommand(commandText, connection);
    }

    /// <summary>
    /// Constructs a data adapter with the specified select command text, and using the specified database connection string.
    /// </summary>
    /// <param name="commandText">The select command text to use to construct a select command.</param>
    /// <param name="connectionString">A connection string suitable for passing to a new SqliteConnection, which is associated with the select command.</param>
    public SqliteDataAdapter(string commandText, string connectionString)
    {
      SqliteConnection cnn = new SqliteConnection(connectionString);
      SelectCommand = new SqliteCommand(commandText, cnn);
    }

    /// <summary>
    /// Row updating event handler
    /// </summary>
    public event EventHandler<RowUpdatingEventArgs> RowUpdating
    {
      add { base.Events.AddHandler(_updatingEventPH, value); }
      remove { base.Events.RemoveHandler(_updatingEventPH, value); }
    }

    /// <summary>
    /// Row updated event handler
    /// </summary>
    public event EventHandler<RowUpdatedEventArgs> RowUpdated
    {
      add { base.Events.AddHandler(_updatedEventPH, value); }
      remove { base.Events.RemoveHandler(_updatedEventPH, value); }
    }

    /// <summary>
    /// Raised by the underlying DbDataAdapter when a row is being updated
    /// </summary>
    /// <param name="value">The event's specifics</param>
    protected override void OnRowUpdating(RowUpdatingEventArgs value)
    {
      EventHandler<RowUpdatingEventArgs> handler = base.Events[_updatingEventPH] as EventHandler<RowUpdatingEventArgs>;

      if (handler != null)
        handler(this, value);
    }

    /// <summary>
    /// Raised by DbDataAdapter after a row is updated
    /// </summary>
    /// <param name="value">The event's specifics</param>
    protected override void OnRowUpdated(RowUpdatedEventArgs value)
    {
      EventHandler<RowUpdatedEventArgs> handler = base.Events[_updatedEventPH] as EventHandler<RowUpdatedEventArgs>;

      if (handler != null)
        handler(this, value);
    }

    /// <summary>
    /// Gets/sets the select command for this DataAdapter
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((string)null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
    public new SqliteCommand SelectCommand
    {
      get { return (SqliteCommand)base.SelectCommand; }
      set { base.SelectCommand = value; }
    }

    /// <summary>
    /// Gets/sets the insert command for this DataAdapter
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((string)null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
    public new SqliteCommand InsertCommand
    {
      get { return (SqliteCommand)base.InsertCommand; }
      set { base.InsertCommand = value; }
    }

    /// <summary>
    /// Gets/sets the update command for this DataAdapter
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((string)null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
    public new SqliteCommand UpdateCommand
    {
      get { return (SqliteCommand)base.UpdateCommand; }
      set { base.UpdateCommand = value; }
    }

    /// <summary>
    /// Gets/sets the delete command for this DataAdapter
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((string)null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
    public new SqliteCommand DeleteCommand
    {
      get { return (SqliteCommand)base.DeleteCommand; }
      set { base.DeleteCommand = value; }
    }
  }
}
#endif
