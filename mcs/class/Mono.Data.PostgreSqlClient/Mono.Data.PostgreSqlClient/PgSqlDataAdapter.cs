//
// System.Data.SqlClient.SqlDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a set of command-related properties that are used 
	/// to fill the DataSet and update a data source, all this 
	/// from a SQL database.
	/// </summary>
	public sealed class SqlDataAdapter : DbDataAdapter 
	{
		#region Fields

		SqlCommand deleteCommand;
		SqlCommand insertCommand;
		SqlCommand selectCommand;
		SqlCommand updateCommand;

		bool isDirty;  // indicates if query has changed since last SELECT

		#endregion

		#region Constructors
		
		public SqlDataAdapter () 	
			: this (new SqlCommand ())
		{
		}

		public SqlDataAdapter (SqlCommand selectCommand)
		{
			this.deleteCommand = new SqlCommand ();
			this.insertCommand = new SqlCommand ();
			this.selectCommand = selectCommand;
			this.updateCommand = new SqlCommand ();
			this.isDirty = true;
		}

		public SqlDataAdapter (string selectCommandText, SqlConnection selectConnection) 
			: this (new SqlCommand (selectCommandText, selectConnection))
		{ 
		}

		public SqlDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new SqlConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		public new SqlCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		public new SqlCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		public new SqlCommand SelectCommand {
			get { return selectCommand; }
			set { 
				this.isDirty = true;
				selectCommand = value; 
			}
		}

		public new SqlCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		#endregion // Properties

		#region Methods

		public override int Fill (DataSet dataSet) 
		{
			// Adds or refreshes rows in the DataSet to match those in 
			// the data source using the DataSet name, and creates
			// a DataTable named "Table"

			// If the SELECT query has changed, then clear the results
			// that we previously retrieved.

			int changeCount = 0;

			if (this.isDirty) 
			{
				dataSet.Tables.Clear ();
			}

			// Run the SELECT query and get the results in a datareader
			SqlDataReader dataReader = selectCommand.ExecuteReader ();


			// The results table in dataSet is called "Table"
			string tableName = "Table";
			DataTable table;

			int i = 0;
			do 
			{
				if (!this.isDirty)  // table already exists
				{
					table = dataSet.Tables[tableName];
				}
				else // create a new table
				{
					table = new DataTable (tableName);
					for (int j = 0; j < dataReader.FieldCount; j += 1) 
					{
						string baseColumnName = dataReader.GetName (j);
						string columnName = "";

						if (baseColumnName == "")
							baseColumnName = "Column";
						else
							columnName = baseColumnName;

						for (int k = 1; table.Columns.Contains (columnName) || columnName == ""; k += 1)
							columnName = String.Format ("{0}{1}", baseColumnName, k);
							
						table.Columns.Add (new DataColumn (columnName, dataReader.GetFieldType (j)));
					}
					dataSet.Tables.Add (table);
				} 

				DataRow thisRow;
				object[] itemArray = new object[dataReader.FieldCount];

				while (dataReader.Read ())
				{
					// need to check for existing rows to reconcile if we have key
					// information.  skip this step for now

					// append rows to the end of the current table.
					dataReader.GetValues (itemArray);
					thisRow = table.NewRow ();
					thisRow.ItemArray = itemArray;
					table.ImportRow (thisRow);
					changeCount += 1;
				}

				i += 1;
				tableName = String.Format ("Table{0}", i);
			} while (dataReader.NextResult ());

			dataReader.Close ();
			this.isDirty = false;
			return changeCount;
		}

		[MonoTODO]
		public override DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IDataParameter[] GetFillParameters () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int Update (DataSet dataSet) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			throw new NotImplementedException ();
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			throw new NotImplementedException ();
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Events and Delegates

		public event SqlRowUpdatedEventHandler RowUpdated;
		public event SqlRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates

	}
}
