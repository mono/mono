//
// Mono.Data.SybaseClient.SybaseDataAdapter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseDataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields
	
		SybaseCommand deleteCommand;
		SybaseCommand insertCommand;
		SybaseCommand selectCommand;
		SybaseCommand updateCommand;

		static readonly object EventRowUpdated = new object(); 
		static readonly object EventRowUpdating = new object(); 

		#endregion

		#region Constructors
		
		public SybaseDataAdapter () 	
			: this (new SybaseCommand ())
		{
		}

		public SybaseDataAdapter (SybaseCommand selectCommand) 
		{
			DeleteCommand = new SybaseCommand ();
			InsertCommand = new SybaseCommand ();
			SelectCommand = selectCommand;
			UpdateCommand = new SybaseCommand ();
		}

		public SybaseDataAdapter (string selectCommandText, SybaseConnection selectConnection) 
			: this (new SybaseCommand (selectCommandText, selectConnection))
		{ 
		}

		public SybaseDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new SybaseConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		public SybaseCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		public SybaseCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		public SybaseCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		public SybaseCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is SybaseCommand)) 
					throw new ArgumentException ();
				DeleteCommand = (SybaseCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is SybaseCommand)) 
					throw new ArgumentException ();
				InsertCommand = (SybaseCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				if (!(value is SybaseCommand)) 
					throw new ArgumentException ();
				SelectCommand = (SybaseCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is SybaseCommand)) 
					throw new ArgumentException ();
				UpdateCommand = (SybaseCommand)value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SybaseRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SybaseRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
         		SybaseRowUpdatedEventHandler handler = (SybaseRowUpdatedEventHandler) Events[EventRowUpdated];
			if ((handler != null) && (value is SybaseRowUpdatedEventArgs))
            			handler(this, (SybaseRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
         		SybaseRowUpdatingEventHandler handler = (SybaseRowUpdatingEventHandler) Events[EventRowUpdating];
			if ((handler != null) && (value is SybaseRowUpdatingEventArgs))
            			handler(this, (SybaseRowUpdatingEventArgs) value);
		}

		#endregion // Methods

		#region Events and Delegates

		public event SybaseRowUpdatedEventHandler RowUpdated {
			add { Events.AddHandler (EventRowUpdated, value); }
			remove { Events.RemoveHandler (EventRowUpdated, value); }
		}

		public event SybaseRowUpdatingEventHandler RowUpdating {
			add { Events.AddHandler (EventRowUpdating, value); }
			remove { Events.RemoveHandler (EventRowUpdating, value); }
		}

		#endregion // Events and Delegates

	}
}
