//
// System.Data.OleDb.OleDbDataAdapter
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		#region Fields

		OleDbCommand deleteCommand;
		OleDbCommand insertCommand;
		OleDbCommand selectCommand;
		OleDbCommand updateCommand;
		MissingMappingAction missingMappingAction;
		MissingSchemaAction missingSchemaAction;

		#endregion

		#region Constructors

		public OleDbDataAdapter ()
			: this (new OleDbCommand ())
		{
		}

		public OleDbDataAdapter (OleDbCommand selectCommand)
		{
			DeleteCommand = new OleDbCommand ();
			InsertCommand = new OleDbCommand ();
			SelectCommand = selectCommand;
			UpdateCommand = new OleDbCommand ();
		}

		public OleDbDataAdapter (string selectCommandText, OleDbConnection selectConnection)
			: this (new OleDbCommand (selectCommandText, selectConnection))
		{
		}

		public OleDbDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new OleDbConnection (selectConnectionString))
		{
		}

		#endregion // Fields

		#region Properties

		public OleDbCommand DeleteCommand {
			get {
				return deleteCommand;
			}
			set {
				deleteCommand = value;
			}
		}

		public OleDbCommand InsertCommand {
			get {
				return insertCommand;
			}
			set {
				insertCommand = value;
			}
		}

		public OleDbCommand SelectCommand {
			get {
				return selectCommand;
			}
			set {
				selectCommand = value;
			}
		}

		public OleDbCommand UpdateCommand {
			get {
				return updateCommand;
			}
			set {
				updateCommand = value;
			}
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get {
				return DeleteCommand;
			}
			set { 
				if (!(value is OleDbCommand))
					throw new ArgumentException ();
				DeleteCommand = (OleDbCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get {
				return InsertCommand;
			}
			set { 
				if (!(value is OleDbCommand))
					throw new ArgumentException ();
				InsertCommand = (OleDbCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get {
				return SelectCommand;
			}
			set { 
				if (!(value is OleDbCommand))
					throw new ArgumentException ();
				SelectCommand = (OleDbCommand)value;
			}
		}

		MissingMappingAction IDataAdapter.MissingMappingAction {
			get {
				return missingMappingAction;
			}
			set {
				missingMappingAction = value;
			}
		}

		MissingSchemaAction IDataAdapter.MissingSchemaAction {
			get {
				return missingSchemaAction;
			}
			set {
				missingSchemaAction = value;
			}
		}
		
		IDbCommand IDbDataAdapter.UpdateCommand {
			get {
				return UpdateCommand;
			}
			set { 
				if (!(value is OleDbCommand))
					throw new ArgumentException ();
				UpdateCommand = (OleDbCommand)value;
			}
		}

		ITableMappingCollection IDataAdapter.TableMappings {
			get {
				return TableMappings;
			}
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow,
									      IDbCommand command,
									      StatementType statementType,
									      DataTableMapping tableMapping) 
		{
			return new OleDbRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow,
										IDbCommand command,
										StatementType statementType,
										DataTableMapping tableMapping) 
		{
			return new OleDbRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			if (RowUpdated != null)
				RowUpdated (this, (OleDbRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (OleDbRowUpdatingEventArgs) value);
		}
		
		#endregion // Methods

		#region Events and Delegates

		public event OleDbRowUpdatedEventHandler RowUpdated;
		public event OleDbRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates

	}
}
