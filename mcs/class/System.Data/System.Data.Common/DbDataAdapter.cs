//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) 2002 Tim Coleman
//

using System.Data.SqlClient;

namespace System.Data.Common
{
	/// <summary>
	/// Aids implementation of the IDbDataAdapter interface. Inheritors of DbDataAdapter  implement a set of functions to provide strong typing, but inherit most of the functionality needed to fully implement a DataAdapter.
	/// </summary>
	public abstract class DbDataAdapter : DataAdapter, ICloneable
	{
		#region Fields

		public const string DefaultSourceTableName = "default";

		IDbCommand selectCommand;
		IDbCommand insertCommand;
		IDbCommand deleteCommand;
		IDbCommand updateCommand;

		bool isDirty;

		#endregion

		#region Constructors

		protected DbDataAdapter() 
		{
			isDirty = true;
		}

		#endregion

		#region Properties

		public IDbCommand SelectCommand {
			get { return selectCommand; }
			set { 
				isDirty = true;
				selectCommand = value; 
			}
		}

		public IDbCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		public IDbCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		public IDbCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		#endregion

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
                        IDataReader dataReader = selectCommand.ExecuteReader ();


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
		public int Fill (DataTable dt) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (DataSet ds, string s) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataTable dt, IDataReader idr) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataTable dt, IDbCommand idc, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (DataSet ds, int i, int j, string s) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataSet ds, string s, IDataReader idr, int i, int j) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataSet ds, int i, int j, string s, IDbCommand idc, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable[] FillSchema (DataSet ds, SchemaType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable FillSchema (DataTable dt, SchemaType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable[] FillSchema (DataSet ds, SchemaType type, string s) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable FillSchema (DataTable dt, SchemaType type, IDbCommand idc, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema (DataSet ds, SchemaType type, IDbCommand idc, string s, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IDataParameter[] GetFillParameters () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataRow[] row) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int Update (DataSet ds) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataTable dt) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Update (DataRow[] row, DataTableMapping dtm) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataSet ds, string s) 
		{
			throw new NotImplementedException ();
		}

		protected abstract RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping);

		protected abstract RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping);

		[MonoTODO]
		protected virtual void OnFillError (FillErrorEventArgs value) 
		{
			throw new NotImplementedException ();
		}

		protected abstract void OnRowUpdated (RowUpdatedEventArgs value);
		protected abstract void OnRowUpdating (RowUpdatingEventArgs value);
		
		public event FillErrorEventHandler FillError;

		[MonoTODO]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
