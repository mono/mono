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
using System.ComponentModel;

namespace ByteFX.Data.MySqlClient
{
	[System.Drawing.ToolboxBitmap( typeof(MySqlDataAdapter), "Designers.dataadapter.bmp")]
	[System.ComponentModel.DesignerCategory("Code")]
	public sealed class MySqlDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		private MySqlCommand m_selectCommand;
		private MySqlCommand m_insertCommand;
		private MySqlCommand m_updateCommand;
		private MySqlCommand m_deleteCommand;

		/*
			* Inherit from Component through DbDataAdapter. The event
			* mechanism is designed to work with the Component.Events
			* property. These variables are the keys used to find the
			* events in the components list of events.
			*/
		static private readonly object EventRowUpdated = new object(); 
		static private readonly object EventRowUpdating = new object(); 


		public MySqlDataAdapter()
		{
		}

		public MySqlDataAdapter( MySqlCommand selectCommand ) 
		{
			SelectCommand = selectCommand;
		}

		public MySqlDataAdapter( string selectCommandText, string selectConnString) 
		{
			SelectCommand = new MySqlCommand( selectCommandText, 
				new MySqlConnection(selectConnString) );
		}

		public MySqlDataAdapter( string selectCommandText, MySqlConnection conn) 
		{
			SelectCommand = new MySqlCommand( selectCommandText, conn );
		}

		#region Properties
		[DataSysDescription("Used during Fill/FillSchema")]
		[Category("Fill")]
		public MySqlCommand SelectCommand 
		{
			get { return m_selectCommand; }
			set { m_selectCommand = value; }
		}

		IDbCommand IDbDataAdapter.SelectCommand 
		{
			get { return m_selectCommand; }
			set { m_selectCommand = (MySqlCommand)value; }
		}

		[DataSysDescription("Used during Update for new rows in Dataset.")]
		public MySqlCommand InsertCommand 
		{
			get { return m_insertCommand; }
			set { m_insertCommand = value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand 
		{
			get { return m_insertCommand; }
			set { m_insertCommand = (MySqlCommand)value; }
		}

		[DataSysDescription("Used during Update for modified rows in Dataset.")]
		public MySqlCommand UpdateCommand 
		{
			get { return m_updateCommand; }
			set { m_updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand 
		{
			get { return m_updateCommand; }
			set { m_updateCommand = (MySqlCommand)value; }
		}

		[DataSysDescription("Used during Update for deleted rows in Dataset.")]
		public MySqlCommand DeleteCommand 
		{
			get { return m_deleteCommand; }
			set { m_deleteCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand 
		{
			get { return m_deleteCommand; }
			set { m_deleteCommand = (MySqlCommand)value; }
		}
		#endregion

		/*
			* Implement abstract methods inherited from DbDataAdapter.
			*/
		override protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new MySqlRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
		}

		override protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new MySqlRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}

		override protected void OnRowUpdating(RowUpdatingEventArgs value)
		{
			MySqlRowUpdatingEventHandler handler = (MySqlRowUpdatingEventHandler) Events[EventRowUpdating];
			if ((null != handler) && (value is MySqlRowUpdatingEventArgs)) 
			{
				handler(this, (MySqlRowUpdatingEventArgs) value);
			}
		}

		override protected void OnRowUpdated(RowUpdatedEventArgs value)
		{
			MySqlRowUpdatedEventHandler handler = (MySqlRowUpdatedEventHandler) Events[EventRowUpdated];
			if ((null != handler) && (value is MySqlRowUpdatedEventArgs)) 
			{
				handler(this, (MySqlRowUpdatedEventArgs) value);
			}
		}

		public event MySqlRowUpdatingEventHandler RowUpdating
		{
			add { Events.AddHandler(EventRowUpdating, value); }
			remove { Events.RemoveHandler(EventRowUpdating, value); }
		}

		public event MySqlRowUpdatedEventHandler RowUpdated
		{
			add { Events.AddHandler(EventRowUpdated, value); }
			remove { Events.RemoveHandler(EventRowUpdated, value); }
		}
	}

	public delegate void MySqlRowUpdatingEventHandler(object sender, MySqlRowUpdatingEventArgs e);
	public delegate void MySqlRowUpdatedEventHandler(object sender, MySqlRowUpdatedEventArgs e);

	public class MySqlRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		public MySqlRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base(row, command, statementType, tableMapping) 
		{
		}

		// Hide the inherited implementation of the command property.
		new public MySqlCommand Command
		{
			get  { return (MySqlCommand)base.Command; }
			set  { base.Command = value; }
		}
	}

	public class MySqlRowUpdatedEventArgs : RowUpdatedEventArgs
	{
		public MySqlRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			: base(row, command, statementType, tableMapping) 
		{
		}

		// Hide the inherited implementation of the command property.
		new public MySqlCommand Command
		{
			get  { return (MySqlCommand)base.Command; }
		}
	}
}
