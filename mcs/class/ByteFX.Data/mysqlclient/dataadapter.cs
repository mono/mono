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
	/// <summary>
	/// Represents a set of data commands and a database connection that are used to fill a dataset and update a MySQL database. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlDataAdapter.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
	[System.Drawing.ToolboxBitmap( typeof(MySqlDataAdapter), "MySqlClient.resources.dataadapter.bmp")]
	[System.ComponentModel.DesignerCategory("Code")]
	[Designer("ByteFX.Data.MySqlClient.Design.MySqlDataAdapterDesigner,MySqlClient.Design")]
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


		/// <summary>
		/// Initializes a new instance of the MySqlDataAdapter class.
		/// </summary>
		public MySqlDataAdapter()
		{
		}

		/// <summary>
		/// Initializes a new instance of the MySqlDataAdapter class with the specified MySqlCommand as the SelectCommand property.
		/// </summary>
		/// <param name="selectCommand"></param>
		public MySqlDataAdapter( MySqlCommand selectCommand ) 
		{
			SelectCommand = selectCommand;
		}

		/// <summary>
		/// Initializes a new instance of the MySqlDataAdapter class with a SelectCommand and a MySqlConnection object.
		/// </summary>
		/// <param name="selectCommandText"></param>
		/// <param name="conn"></param>
		public MySqlDataAdapter( string selectCommandText, MySqlConnection conn) 
		{
			SelectCommand = new MySqlCommand( selectCommandText, conn );
		}

		/// <summary>
		/// Initializes a new instance of the MySqlDataAdapter class with a SelectCommand and a connection string.
		/// </summary>
		/// <param name="selectCommandText"></param>
		/// <param name="selectConnString"></param>
		public MySqlDataAdapter( string selectCommandText, string selectConnString) 
		{
			SelectCommand = new MySqlCommand( selectCommandText, 
				new MySqlConnection(selectConnString) );
		}

		#region Properties
		/// <summary>
		/// Gets or sets a SQL statement to delete records from the data set.
		/// </summary>
		[Description("Used during Update for deleted rows in Dataset.")]
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

		/// <summary>
		/// Gets or sets a SQL statement to insert new records into the data source.
		/// </summary>
		[Description("Used during Update for new rows in Dataset.")]
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

		/// <summary>
		/// Gets or sets a SQL statement used to select records in the data source.
		/// </summary>
		[Description("Used during Fill/FillSchema")]
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

		/// <summary>
		/// Gets or sets a SQL statement used to update records in the data source.
		/// </summary>
		[Description("Used during Update for modified rows in Dataset.")]
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

		#endregion

		/*
			* Implement abstract methods inherited from DbDataAdapter.
			*/
		/// <summary>
		/// Overridden. See <see cref="DbDataAdapter.CreateRowUpdatedEvent"/>.
		/// </summary>
		/// <param name="dataRow"></param>
		/// <param name="command"></param>
		/// <param name="statementType"></param>
		/// <param name="tableMapping"></param>
		/// <returns></returns>
		override protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new MySqlRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
		}

		/// <summary>
		/// Overridden. See <see cref="DbDataAdapter.CreateRowUpdatingEvent"/>.
		/// </summary>
		/// <param name="dataRow"></param>
		/// <param name="command"></param>
		/// <param name="statementType"></param>
		/// <param name="tableMapping"></param>
		/// <returns></returns>
		override protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new MySqlRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}

		/// <summary>
		/// Overridden. Raises the RowUpdating event.
		/// </summary>
		/// <param name="value">A MySqlRowUpdatingEventArgs that contains the event data.</param>
		override protected void OnRowUpdating(RowUpdatingEventArgs value)
		{
			MySqlRowUpdatingEventHandler handler = (MySqlRowUpdatingEventHandler) Events[EventRowUpdating];
			if ((null != handler) && (value is MySqlRowUpdatingEventArgs)) 
			{
				handler(this, (MySqlRowUpdatingEventArgs) value);
			}
		}

		/// <summary>
		/// Overridden. Raises the RowUpdated event.
		/// </summary>
		/// <param name="value">A MySqlRowUpdatedEventArgs that contains the event data. </param>
		override protected void OnRowUpdated(RowUpdatedEventArgs value)
		{
			MySqlRowUpdatedEventHandler handler = (MySqlRowUpdatedEventHandler) Events[EventRowUpdated];
			if ((null != handler) && (value is MySqlRowUpdatedEventArgs)) 
			{
				handler(this, (MySqlRowUpdatedEventArgs) value);
			}
		}

		/// <summary>
		/// Occurs during Update before a command is executed against the data source. The attempt to update is made, so the event fires.
		/// </summary>
		public event MySqlRowUpdatingEventHandler RowUpdating
		{
			add { Events.AddHandler(EventRowUpdating, value); }
			remove { Events.RemoveHandler(EventRowUpdating, value); }
		}

		/// <summary>
		/// Occurs during Update after a command is executed against the data source. The attempt to update is made, so the event fires.
		/// </summary>
		public event MySqlRowUpdatedEventHandler RowUpdated
		{
			add { Events.AddHandler(EventRowUpdated, value); }
			remove { Events.RemoveHandler(EventRowUpdated, value); }
		}
	}

	/// <summary>
	/// Represents the method that will handle the <see cref="MySqlDataAdapter.RowUpdating"/> event of a <see cref="MySqlDataAdapter"/>.
	/// </summary>
	public delegate void MySqlRowUpdatingEventHandler(object sender, MySqlRowUpdatingEventArgs e);

	/// <summary>
	/// Represents the method that will handle the <see cref="MySqlDataAdapter.RowUpdated"/> event of a <see cref="MySqlDataAdapter"/>.
	/// </summary>
	public delegate void MySqlRowUpdatedEventHandler(object sender, MySqlRowUpdatedEventArgs e);

	/// <summary>
	/// Provides data for the RowUpdating event. This class cannot be inherited.
	/// </summary>
	public sealed class MySqlRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the MySqlRowUpdatingEventArgs class.
		/// </summary>
		/// <param name="row">The <see cref="DataRow"/> to <see cref="DbDataAdapter.Update"/>.</param>
		/// <param name="command">The <see cref="IDbCommand"/> to execute during <see cref="DbDataAdapter.Update"/>.</param>
		/// <param name="statementType">One of the <see cref="StatementType"/> values that specifies the type of query executed.</param>
		/// <param name="tableMapping">The <see cref="DataTableMapping"/> sent through an <see cref="DbDataAdapter.Update"/>.</param>
		public MySqlRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base(row, command, statementType, tableMapping) 
		{
		}

		/// <summary>
		/// Gets or sets the MySqlCommand to execute when performing the Update.
		/// </summary>
		new public MySqlCommand Command
		{
			get  { return (MySqlCommand)base.Command; }
			set  { base.Command = value; }
		}
	}

	/// <summary>
	/// Provides data for the RowUpdated event. This class cannot be inherited.
	/// </summary>
	public sealed class MySqlRowUpdatedEventArgs : RowUpdatedEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the MySqlRowUpdatedEventArgs class.
		/// </summary>
		/// <param name="row">The <see cref="DataRow"/> sent through an <see cref="DbDataAdapter.Update"/>.</param>
		/// <param name="command">The <see cref="IDbCommand"/> executed when <see cref="DbDataAdapter.Update"/> is called.</param>
		/// <param name="statementType">One of the <see cref="StatementType"/> values that specifies the type of query executed.</param>
		/// <param name="tableMapping">The <see cref="DataTableMapping"/> sent through an <see cref="DbDataAdapter.Update"/>.</param>
		public MySqlRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			: base(row, command, statementType, tableMapping) 
		{
		}

		/// <summary>
		/// Gets or sets the MySqlCommand executed when Update is called.
		/// </summary>
		new public MySqlCommand Command
		{
			get  { return (MySqlCommand)base.Command; }
		}
	}
}
