// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
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

using System.Data;
using System.Data.Common;

namespace ByteFX.Data.MySQLClient
{
#if WINDOWS
	[System.Drawing.ToolboxBitmap( typeof(MySQLDataAdapter), "Designers.dataadapter.bmp")]
#endif
	public sealed class MySQLDataAdapter : DbDataAdapter, IDbDataAdapter
  {
    private MySQLCommand m_selectCommand;
    private MySQLCommand m_insertCommand;
    private MySQLCommand m_updateCommand;
    private MySQLCommand m_deleteCommand;

    /*
     * Inherit from Component through DbDataAdapter. The event
     * mechanism is designed to work with the Component.Events
     * property. These variables are the keys used to find the
     * events in the components list of events.
     */
    static private readonly object EventRowUpdated = new object(); 
    static private readonly object EventRowUpdating = new object(); 


    public MySQLDataAdapter()
    {
    }

	public MySQLDataAdapter( MySQLCommand selectCommand ) 
	{
		SelectCommand = selectCommand;
	}

	public MySQLDataAdapter( string selectCommandText, string selectConnString) 
	{
		SelectCommand = new MySQLCommand( selectCommandText, 
			new MySQLConnection(selectConnString) );
	}

	public MySQLDataAdapter( string selectCommandText, MySQLConnection conn) 
	{
		SelectCommand = new MySQLCommand( selectCommandText, conn );
	}

    public MySQLCommand SelectCommand 
    {
      get { return m_selectCommand; }
      set { m_selectCommand = value; }
    }

    IDbCommand IDbDataAdapter.SelectCommand 
    {
      get { return m_selectCommand; }
      set { m_selectCommand = (MySQLCommand)value; }
    }

    public MySQLCommand InsertCommand 
    {
      get { return m_insertCommand; }
      set { m_insertCommand = value; }
    }

    IDbCommand IDbDataAdapter.InsertCommand 
    {
      get { return m_insertCommand; }
      set { m_insertCommand = (MySQLCommand)value; }
    }

    public MySQLCommand UpdateCommand 
    {
      get { return m_updateCommand; }
      set { m_updateCommand = value; }
    }

    IDbCommand IDbDataAdapter.UpdateCommand 
    {
      get { return m_updateCommand; }
      set { m_updateCommand = (MySQLCommand)value; }
    }

    public MySQLCommand DeleteCommand 
    {
      get { return m_deleteCommand; }
      set { m_deleteCommand = value; }
    }

    IDbCommand IDbDataAdapter.DeleteCommand 
    {
      get { return m_deleteCommand; }
      set { m_deleteCommand = (MySQLCommand)value; }
    }

    /*
     * Implement abstract methods inherited from DbDataAdapter.
     */
    override protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
    {
      return new MySQLRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
    }

    override protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
    {
      return new MySQLRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
    }

    override protected void OnRowUpdating(RowUpdatingEventArgs value)
    {
      MySQLRowUpdatingEventHandler handler = (MySQLRowUpdatingEventHandler) Events[EventRowUpdating];
      if ((null != handler) && (value is MySQLRowUpdatingEventArgs)) 
      {
        handler(this, (MySQLRowUpdatingEventArgs) value);
      }
    }

    override protected void OnRowUpdated(RowUpdatedEventArgs value)
    {
      MySQLRowUpdatedEventHandler handler = (MySQLRowUpdatedEventHandler) Events[EventRowUpdated];
      if ((null != handler) && (value is MySQLRowUpdatedEventArgs)) 
      {
        handler(this, (MySQLRowUpdatedEventArgs) value);
      }
    }

    public event MySQLRowUpdatingEventHandler RowUpdating
    {
      add { Events.AddHandler(EventRowUpdating, value); }
      remove { Events.RemoveHandler(EventRowUpdating, value); }
    }

    public event MySQLRowUpdatedEventHandler RowUpdated
    {
      add { Events.AddHandler(EventRowUpdated, value); }
      remove { Events.RemoveHandler(EventRowUpdated, value); }
    }
  }

  public delegate void MySQLRowUpdatingEventHandler(object sender, MySQLRowUpdatingEventArgs e);
  public delegate void MySQLRowUpdatedEventHandler(object sender, MySQLRowUpdatedEventArgs e);

  public class MySQLRowUpdatingEventArgs : RowUpdatingEventArgs
  {
    public MySQLRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
      : base(row, command, statementType, tableMapping) 
    {
    }

    // Hide the inherited implementation of the command property.
    new public MySQLCommand Command
    {
      get  { return (MySQLCommand)base.Command; }
      set  { base.Command = value; }
    }
  }

  public class MySQLRowUpdatedEventArgs : RowUpdatedEventArgs
  {
    public MySQLRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
      : base(row, command, statementType, tableMapping) 
    {
    }

    // Hide the inherited implementation of the command property.
    new public MySQLCommand Command
    {
      get  { return (MySQLCommand)base.Command; }
    }
  }
}
