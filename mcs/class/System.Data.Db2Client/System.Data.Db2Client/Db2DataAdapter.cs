using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Db2Client {
	
	public sealed class Db2DataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields

		bool disposed = false;	
		Db2Command deleteCommand;
		Db2Command insertCommand;
		Db2Command selectCommand;
		Db2Command updateCommand;

		#endregion

		#region Constructors
		
		public Db2DataAdapter () 	
			: this (new Db2Command ())
		{
		}

		public Db2DataAdapter (Db2Command selectCommand) 
		{
			DeleteCommand = null;
			InsertCommand = null;
			SelectCommand = selectCommand;
			UpdateCommand = null;
		}

		public Db2DataAdapter (string selectCommandText, Db2Connection selectConnection) 
			: this (new Db2Command (selectCommandText, selectConnection))
		{ 
		}

		public Db2DataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new Db2Connection (selectConnectionString))
		{
		}

		#endregion

		#region Properties


		public Db2Command DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}


		public Db2Command InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}


		public Db2Command SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}


		public Db2Command UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is Db2Command)) 
					throw new ArgumentException ();
				DeleteCommand = (Db2Command)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is Db2Command)) 
					throw new ArgumentException ();
				InsertCommand = (Db2Command)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				if (!(value is Db2Command)) 
					throw new ArgumentException ();
				SelectCommand = (Db2Command)value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is Db2Command)) 
					throw new ArgumentException ();
				UpdateCommand = (Db2Command)value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion 

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new Db2RowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new Db2RowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					
				}
				
				disposed = true;
			}
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			if (RowUpdated != null)
				RowUpdated (this, (Db2RowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (Db2RowUpdatingEventArgs) value);
		}

		#endregion 

		#region Events and Delegates

		public event Db2RowUpdatedEventHandler RowUpdated;

		public event Db2RowUpdatingEventHandler RowUpdating;

		#endregion 

	}
}
